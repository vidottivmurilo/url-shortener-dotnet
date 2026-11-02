namespace Shortener.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Shortener.Application.Common.Interfaces;

public class InMemoryClickMetricRepository : IClickMetricRepository
{
    private class MetricRecord
    {
        public long TotalClicks { get; set; }
        public DateTime? LastClickedAt { get; set; }
    }

    private readonly ConcurrentDictionary<string, MetricRecord> _metricsByCode = new();

    public Task RecordClickAsync(string shortCode, DateTime clickedAt, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        _metricsByCode.AddOrUpdate(
            shortCode,
            _ => new MetricRecord { TotalClicks = 1, LastClickedAt = clickedAt },
            (_, record) =>
            {
                record.TotalClicks++;
                record.LastClickedAt = clickedAt;
                return record;
            }
        );
        return Task.CompletedTask;
    }

    public Task<(long totalClicks, DateTime? lastClickedAt)> GetStatsAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        if (_metricsByCode.TryGetValue(shortCode, out var record))
        {
            return Task.FromResult((record.TotalClicks, record.LastClickedAt));
        }

        return Task.FromResult((0L, (DateTime?)null));
    }
}
