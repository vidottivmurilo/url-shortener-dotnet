namespace Shortener.Application.Common.Interfaces;

public interface IClickMetricRepository
{
    Task RecordClickAsync(string shortCode, DateTime clickedAt, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<(long totalClicks, DateTime? lastClickedAt)> GetStatsAsync(string shortCode, CancellationToken cancellationToken = default);
}
