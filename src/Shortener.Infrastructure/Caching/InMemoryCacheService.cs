namespace Shortener.Infrastructure.Caching;

using System.Collections.Concurrent;
using Shortener.Application.Common.Interfaces;

public class InMemoryCacheService : ICacheService
{
    private class CacheEntry
    {
        public string Value { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }

    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value <= DateTime.UtcNow)
            {
                _cache.TryRemove(key, out _);
                return Task.FromResult<string?>(null);
            }
            return Task.FromResult<string?>(entry.Value);
        }
        return Task.FromResult<string?>(null);
    }

    public Task SetAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var entry = new CacheEntry
        {
            Value = value,
            ExpiresAt = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : null
        };
        _cache[key] = entry;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
