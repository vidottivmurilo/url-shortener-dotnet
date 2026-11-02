namespace Shortener.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Shortener.Application.Common.Interfaces;
using Shortener.Domain.Common;
using Shortener.Domain.Entities;

public class InMemoryShortenedUrlRepository : IShortenedUrlRepository
{
    private readonly ConcurrentDictionary<string, ShortenedUrl> _urlsByCode = new();
    private long _sequence = 100000;

    public Task<ShortenedUrl> AddAsync(ShortenedUrl entity, CancellationToken cancellationToken = default)
    {
        var id = Interlocked.Increment(ref _sequence);
        entity.Id = id;
        entity.ShortCode = Base62Encoder.Encode(id);

        _urlsByCode[entity.ShortCode] = entity;
        return Task.FromResult(entity);
    }

    public Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        _urlsByCode.TryGetValue(shortCode, out var entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(ShortenedUrl entity, CancellationToken cancellationToken = default)
    {
        _urlsByCode[entity.ShortCode] = entity;
        return Task.CompletedTask;
    }
}
