namespace Shortener.UnitTests.Application;

using Shortener.Application.Common.Interfaces;
using Shortener.Application.Urls;
using Shortener.Domain.Entities;
using Xunit;

public class DeactivateShortUrlTests
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
            var existing = Urls.FirstOrDefault(u => u.ShortCode == entity.ShortCode);
            if (existing != null)
            {
                existing.IsActive = entity.IsActive;
            }
            return Task.CompletedTask;
        }
    }

    private class FakeCacheService : ICacheService
    {
        public readonly HashSet<string> RemovedKeys = new();

        public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(null);
        }

        public Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            RemovedKeys.Add(key);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Should_DeactivateLink_And_RemoveFromCache_When_LinkExists()
    {
        // Arrange
        var repo = new FakeUrlRepository();
        var cache = new FakeCacheService();

        var link = ShortenedUrl.Create("https://example.com/to-deactivate");
        link.ShortCode = "deact123";
        link.IsActive = true;
        await repo.AddAsync(link);

        var handler = new DeactivateShortUrlHandler(repo, cache);
        var command = new DeactivateShortUrlCommand("deact123");

        // Act
        await handler.HandleAsync(command);

        // Assert
        var updated = await repo.GetByShortCodeAsync("deact123");
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
        Assert.Contains("short:deact123", cache.RemovedKeys);
    }

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_LinkDoesNotExist()
    {
        // Arrange
        var repo = new FakeUrlRepository();
        var cache = new FakeCacheService();

        var handler = new DeactivateShortUrlHandler(repo, cache);
        var command = new DeactivateShortUrlCommand("naoExiste");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.HandleAsync(command));
    }
}
