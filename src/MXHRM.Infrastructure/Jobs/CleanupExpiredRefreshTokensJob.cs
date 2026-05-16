using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MXHRM.Infrastructure.Data;

namespace MXHRM.Infrastructure.Jobs;

public class CleanupExpiredRefreshTokensJob
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CleanupExpiredRefreshTokensJob> _logger;

    public CleanupExpiredRefreshTokensJob(
        AppDbContext db,
        IConfiguration configuration,
        ILogger<CleanupExpiredRefreshTokensJob> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var retentionDays = _configuration.GetValue<int>(
            "RefreshTokenCleanup:RetentionDays",
            30);

        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        var tokensToDelete = await _db.RefreshTokens
            .Where(x =>
                x.ExpiresAt < cutoffDate ||
                (x.RevokedAt.HasValue && x.RevokedAt.Value < cutoffDate))
            .ToListAsync();

        if (tokensToDelete.Count == 0)
        {
            _logger.LogInformation(
                "Refresh token cleanup completed. No tokens to delete. RetentionDays: {RetentionDays}",
                retentionDays);

            return;
        }

        _db.RefreshTokens.RemoveRange(tokensToDelete);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Refresh token cleanup completed. DeletedCount: {DeletedCount}, RetentionDays: {RetentionDays}, CutoffDate: {CutoffDate}",
            tokensToDelete.Count,
            retentionDays,
            cutoffDate);
    }
}