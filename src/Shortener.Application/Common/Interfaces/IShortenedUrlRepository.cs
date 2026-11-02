namespace Shortener.Application.Common.Interfaces;

using Shortener.Domain.Entities;

public interface IShortenedUrlRepository
{
    Task<ShortenedUrl> AddAsync(ShortenedUrl entity, CancellationToken cancellationToken = default);
    Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default);
    Task UpdateAsync(ShortenedUrl entity, CancellationToken cancellationToken = default);
}
