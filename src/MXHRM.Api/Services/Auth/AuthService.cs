using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MXHRM.Api.DTOs.Auth;
using MXHRM.Api.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using MXHRM.Api.Data;
using MXHRM.Api.Authorization;

namespace MXHRM.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;

    public AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    AppDbContext db)
    {
        _userManager = userManager;
        _configuration = configuration;
        _db = db;
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

        // Add role assignment logic here if needed (e.g., assign "Employee" role by default)
        await _userManager.AddToRoleAsync(user, "Employee");   // role "Admin" on test only, should be role "Employee"

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid username or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User is inactive.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Invalid username or password.");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task LogoutAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (refreshToken is null)
            return;

        refreshToken.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (refreshToken is null || !refreshToken.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var user = await _userManager.FindByIdAsync(refreshToken.UserId);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid user.");

        refreshToken.RevokedAt = DateTime.UtcNow;

        return await GenerateAuthResponseAsync(user);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user)
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
            new("company_id", user.CompanyID),
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
            claims.Add(new Claim(PermissionAuthorizationHandler.PermissionClaimType, permission));
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
            ExpiresAt = DateTime.UtcNow.AddDays(7)
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
            CompanyID = user.CompanyID,
            Roles = roles.ToList()
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