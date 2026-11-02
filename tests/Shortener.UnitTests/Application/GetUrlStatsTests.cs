namespace Shortener.UnitTests.Application;

using Shortener.Application.Common.Interfaces;
using Shortener.Application.Urls;
using Shortener.Domain.Entities;
using Xunit;

public class GetUrlStatsTests
{
    private class FakeUrlRepository : IShortenedUrlRepository
    {
        public readonly List<ShortenedUrl> Urls = new();

        public Task<ShortenedUrl> AddAsync(ShortenedUrl entity, CancellationToken cancellationToken = default)
        {
            Urls.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default)
        {
            var found = Urls.FirstOrDefault(u => u.ShortCode == shortCode);
            return Task.FromResult(found);
        }

        public Task UpdateAsync(ShortenedUrl entity, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private class FakeMetricRepository : IClickMetricRepository
    {
        public readonly Dictionary<string, (long Total, DateTime? Last)> Stats = new();

        public Task RecordClickAsync(string shortCode, DateTime clickedAt, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
        {
            if (Stats.TryGetValue(shortCode, out var existing))
            {
                Stats[shortCode] = (existing.Total + 1, clickedAt);
            }
            else
            {
                Stats[shortCode] = (1, clickedAt);
            }
            return Task.CompletedTask;
        }

        public Task<(long totalClicks, DateTime? lastClickedAt)> GetStatsAsync(string shortCode, CancellationToken cancellationToken = default)
        {
            if (Stats.TryGetValue(shortCode, out var stats))
            {
                return Task.FromResult(stats);
            }
            return Task.FromResult((0L, (DateTime?)null));
        }
    }

    [Fact]
    public async Task Should_ReturnUrlStats_When_LinkExists_WithClicks()
    {
        // Arrange
        var urlRepo = new FakeUrlRepository();
        var metricRepo = new FakeMetricRepository();

        var link = ShortenedUrl.Create("https://example.com/stats");
        link.ShortCode = "stats123";
        await urlRepo.AddAsync(link);

        var now = DateTime.UtcNow;
        await metricRepo.RecordClickAsync("stats123", now, "127.0.0.1", "Mozilla");

        var handler = new GetUrlStatsHandler(urlRepo, metricRepo);
        var query = new GetUrlStatsQuery("stats123");

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("stats123", result.ShortCode);
        Assert.Equal("https://example.com/stats", result.OriginalUrl);
        Assert.Equal(1, result.TotalClicks);
        Assert.Equal(now, result.LastClickedAt);
    }

    [Fact]
    public async Task Should_ReturnUrlStats_When_LinkExists_WithZeroClicks()
    {
        // Arrange
        var urlRepo = new FakeUrlRepository();
        var metricRepo = new FakeMetricRepository();

        var link = ShortenedUrl.Create("https://example.com/noclicks");
        link.ShortCode = "zeroClicks";
        await urlRepo.AddAsync(link);

        var handler = new GetUrlStatsHandler(urlRepo, metricRepo);
        var query = new GetUrlStatsQuery("zeroClicks");

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("zeroClicks", result.ShortCode);
        Assert.Equal(0, result.TotalClicks);
        Assert.Null(result.LastClickedAt);
    }

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_LinkDoesNotExist()
    {
        // Arrange
        var urlRepo = new FakeUrlRepository();
        var metricRepo = new FakeMetricRepository();

        var handler = new GetUrlStatsHandler(urlRepo, metricRepo);
        var query = new GetUrlStatsQuery("naoExiste");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.HandleAsync(query));
    }
}
