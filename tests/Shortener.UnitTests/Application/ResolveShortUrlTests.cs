namespace Shortener.UnitTests.Application;

using System.Text.Json;
using Shortener.Application.Common.Interfaces;
using Shortener.Application.Urls;
using Shortener.Domain.Entities;
using Shortener.Domain.Events;
using Xunit;

public class ResolveShortUrlTests
{
    private class FakeCacheService : ICacheService
    {
        public readonly Dictionary<string, string> Store = new();

        public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            Store.TryGetValue(key, out var val);
            return Task.FromResult<string?>(val);
        }

        public Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            Store[key] = value;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            Store.Remove(key);
            return Task.CompletedTask;
        }
    }

    private class FakeMessagePublisher : IMessagePublisher
    {
        public readonly List<object> PublishedMessages = new();

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            if (message != null)
            {
                PublishedMessages.Add(message);
            }
            return Task.CompletedTask;
        }
    }

    private class FakeShortenedUrlRepository : IShortenedUrlRepository
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

    [Fact]
    public async Task Should_ReturnRedirectUrl_And_PublishMetric_When_LinkIsInCache()
    {
        // Arrange
        var repo = new FakeShortenedUrlRepository();
        var cache = new FakeCacheService();
        var publisher = new FakeMessagePublisher();

        var cachedPayload = JsonSerializer.Serialize(new
        {
            OriginalUrl = "https://example.com/cached",
            IsActive = true,
            ExpiresAt = (DateTime?)null
        });
        await cache.SetAsync("short:cached123", cachedPayload);

        var handler = new ResolveShortUrlHandler(repo, cache, publisher);
        var query = new ResolveShortUrlQuery("cached123", "192.168.1.1", "Mozilla/5.0");

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://example.com/cached", result.OriginalUrl);
        Assert.Single(publisher.PublishedMessages);
        var publishedEvent = Assert.IsType<LinkClickedEvent>(publisher.PublishedMessages[0]);
        Assert.Equal("cached123", publishedEvent.ShortCode);
        Assert.Equal("192.168.1.1", publishedEvent.IpAddress);
    }

    [Fact]
    public async Task Should_FetchFromRepo_PopulateCache_And_Return_When_CacheMiss()
    {
        // Arrange
        var repo = new FakeShortenedUrlRepository();
        var cache = new FakeCacheService();
        var publisher = new FakeMessagePublisher();

        var entity = ShortenedUrl.Create("https://example.com/from-db");
        entity.ShortCode = "dbcode99";
        await repo.AddAsync(entity);

        var handler = new ResolveShortUrlHandler(repo, cache, publisher);
        var query = new ResolveShortUrlQuery("dbcode99");

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://example.com/from-db", result.OriginalUrl);
        Assert.True(cache.Store.ContainsKey("short:dbcode99"));
        Assert.Single(publisher.PublishedMessages);
    }

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_ShortCodeDoesNotExist()
    {
        // Arrange
        var repo = new FakeShortenedUrlRepository();
        var cache = new FakeCacheService();
        var publisher = new FakeMessagePublisher();

        var handler = new ResolveShortUrlHandler(repo, cache, publisher);
        var query = new ResolveShortUrlQuery("inexistente");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.HandleAsync(query));
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_LinkIsInactiveOrExpired()
    {
        // Arrange
        var repo = new FakeShortenedUrlRepository();
        var cache = new FakeCacheService();
        var publisher = new FakeMessagePublisher();

        var entity = ShortenedUrl.Create("https://example.com/inactive");
        entity.ShortCode = "inativo1";
        entity.IsActive = false;
        await repo.AddAsync(entity);

        var handler = new ResolveShortUrlHandler(repo, cache, publisher);
        var query = new ResolveShortUrlQuery("inativo1");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(query));
    }
}
