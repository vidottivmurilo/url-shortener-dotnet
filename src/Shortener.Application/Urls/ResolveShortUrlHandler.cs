namespace Shortener.Application.Urls;

using System.Text.Json;
using Shortener.Application.Common.Interfaces;
using Shortener.Domain.Events;

public record ResolveShortUrlQuery(string ShortCode, string? IpAddress = null, string? UserAgent = null);

public record ResolveShortUrlResult(string OriginalUrl);

public record CachedShortUrl(string OriginalUrl, bool IsActive, DateTime? ExpiresAt);

public class ResolveShortUrlHandler
{
    private readonly IShortenedUrlRepository _repository;
    private readonly ICacheService _cache;
    private readonly IMessagePublisher _publisher;

    public ResolveShortUrlHandler(
        IShortenedUrlRepository repository,
        ICacheService cache,
        IMessagePublisher publisher)
    {
        _repository = repository;
        _cache = cache;
        _publisher = publisher;
    }

    public async Task<ResolveShortUrlResult> HandleAsync(ResolveShortUrlQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"short:{query.ShortCode}";

        // 1. Consulta Cache-Aside
        var cached = await _cache.GetAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            var cachedObj = JsonSerializer.Deserialize<CachedShortUrl>(cached);
            if (cachedObj != null)
            {
                if (!cachedObj.IsActive || (cachedObj.ExpiresAt.HasValue && cachedObj.ExpiresAt.Value <= DateTime.UtcNow))
                {
                    throw new InvalidOperationException("Link está inativo ou expirado.");
                }

                // Publica evento assíncrono de clique
                await _publisher.PublishAsync(
                    new LinkClickedEvent(query.ShortCode, DateTime.UtcNow, query.IpAddress, query.UserAgent),
                    cancellationToken
                );

                return new ResolveShortUrlResult(cachedObj.OriginalUrl);
            }
        }

        // 2. Consulta Repositório (PostgreSQL / Store)
        var entity = await _repository.GetByShortCodeAsync(query.ShortCode, cancellationToken);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Código encurtado '{query.ShortCode}' não encontrado.");
        }

        if (!entity.IsActive || (entity.ExpiresAt.HasValue && entity.ExpiresAt.Value <= DateTime.UtcNow))
        {
            throw new InvalidOperationException("Link está inativo ou expirado.");
        }

        // 3. Popula Cache Redis
        var toCache = new CachedShortUrl(entity.OriginalUrl, entity.IsActive, entity.ExpiresAt);
        await _cache.SetAsync(cacheKey, JsonSerializer.Serialize(toCache), TimeSpan.FromHours(24), cancellationToken);

        // 4. Publica evento assíncrono de clique
        await _publisher.PublishAsync(
            new LinkClickedEvent(query.ShortCode, DateTime.UtcNow, query.IpAddress, query.UserAgent),
            cancellationToken
        );

        return new ResolveShortUrlResult(entity.OriginalUrl);
    }
}
