using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MXHRM.Application.Auth.DTOs;
using MXHRM.Infrastructure.Identity;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using MXHRM.Infrastructure.Data;
using MXHRM.Application.Authorization;
using MXHRM.Application.Auth;
using System.Text.Json;
using MXHRM.Application.Auditing;
using MXHRM.Application.Auditing.DTOs;

namespace MXHRM.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;
    private readonly IUserActivityLogService _userActivityLogService;

    public AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    AppDbContext db,
    IUserActivityLogService userActivityLogService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _db = db;
        _userActivityLogService = userActivityLogService;
    }

    private static string SerializeActivityMetadata(object value)
    {
        return JsonSerializer.Serialize(value);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByNameAsync(request.UserName);

        if (existingUser is not null)
            throw new InvalidOperationException("Username already exists.");

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            CompanyID = request.CompanyID,
            DisplayName = request.DisplayName,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "Employee");

        return await GenerateAuthResponseAsync(user, user.CompanyID);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user is null)
        {
            await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
            {
                ActivityType = "LoginFailed",
                Description = "Login failed because username was not found.",
                Metadata = SerializeActivityMetadata(new
                {
                    request.UserName,
                    Reason = "UserNotFound"
                })
            });

            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
            {
                ActivityType = "LoginFailed",
                Description = "Login failed because user is inactive.",
                Metadata = SerializeActivityMetadata(new
                {
                    UserId = user.Id,
                    user.UserName,
                    Reason = "InactiveUser"
                })
            });

            throw new UnauthorizedAccessException("User is inactive.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
            {
                ActivityType = "LoginFailed",
                Description = "Login failed because password was invalid.",
                Metadata = SerializeActivityMetadata(new
                {
                    UserId = user.Id,
                    user.UserName,
                    Reason = "InvalidPassword"
                })
            });

            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        // non-admin: บริษัทที่ระบุต้องตรงกับบริษัทของ user เอง
        if (!isAdmin &&
            !string.Equals(request.CompanyID, user.CompanyID, StringComparison.OrdinalIgnoreCase))
        {
            await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
            {
                ActivityType = "LoginFailed",
                Description = "Login failed because company did not match the user.",
                Metadata = SerializeActivityMetadata(new
                {
                    UserId = user.Id,
                    user.UserName,
                    RequestedCompanyId = request.CompanyID,
                    Reason = "CompanyMismatch"
                })
            });

            throw new UnauthorizedAccessException("Invalid username, password, or company.");
        }

        // admin เลือกบริษัทได้ / non-admin = บริษัทตัวเอง
        var effectiveCompanyId = isAdmin ? request.CompanyID : user.CompanyID;

        await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
        {
            ActivityType = "LoginSuccess",
            Description = "User logged in successfully.",
            Metadata = SerializeActivityMetadata(new
            {
                UserId = user.Id,
                user.UserName
            })
        });

        return await GenerateAuthResponseAsync(user, effectiveCompanyId);
    }

    public async Task LogoutAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (refreshToken is null)
        {
            await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
            {
                ActivityType = "LogoutFailed",
                Description = "Logout failed because refresh token was not found.",
                Metadata = SerializeActivityMetadata(new
                {
                    Reason = "RefreshTokenNotFound"
                })
            });

            return;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
        {
            ActivityType = "Logout",
            Description = "User logged out.",
            Metadata = SerializeActivityMetadata(new
            {
                refreshToken.UserId
            })
        });
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
            {
                ActivityType = "RefreshTokenFailed",
                Description = "Refresh token failed because token was invalid or inactive.",
                Metadata = SerializeActivityMetadata(new
                {
                    Reason = "InvalidOrInactiveRefreshToken"
                })
            });

            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = await _userManager.FindByIdAsync(refreshToken.UserId);

        if (user is null || !user.IsActive)
        {
            await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
            {
                ActivityType = "RefreshTokenFailed",
                Description = "Refresh token failed because user was invalid or inactive.",
                Metadata = SerializeActivityMetadata(new
                {
                    UserId = refreshToken.UserId,
                    Reason = "InvalidOrInactiveUser"
                })
            });

            throw new UnauthorizedAccessException("Invalid user.");
        }

        await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
        {
            ActivityType = "RefreshToken",
            Description = "User refreshed access token.",
            Metadata = SerializeActivityMetadata(new
            {
                UserId = user.Id,
                user.UserName
            })
        });

        refreshToken.RevokedAt = DateTime.UtcNow;

        return await GenerateAuthResponseAsync(user, refreshToken.CompanyID);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user, string companyId)
    {
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;
        var secretKey = _configuration["Jwt:SecretKey"]!;
        var accessTokenMinutes = int.Parse(_configuration["Jwt:AccessTokenMinutes"]!);

        var expiresAt = DateTime.UtcNow.AddMinutes(accessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new("company_id", companyId),
            new("display_name", user.DisplayName)
        };

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsByRolesAsync(roles);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim(AuthorizationClaimTypes.Permission, permission));
        }


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshTokenValue = GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CompanyID = companyId
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = expiresAt,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            CompanyID = companyId,
            Roles = roles.ToList(),
            Permissions = permissions.ToList()
        };
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    private async Task<IReadOnlyCollection<string>> GetPermissionsByRolesAsync(IEnumerable<string> roles)
    {
        var roleNames = roles.ToList();

        if (roleNames.Count == 0)
        {
            return Array.Empty<string>();
        }

        return await _db.RolePermissions
            .AsNoTracking()
            .Where(x => roleNames.Contains(x.Role.Name!))
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync();
    }

}
