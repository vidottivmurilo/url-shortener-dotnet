namespace Shortener.Application.Urls;

using Shortener.Application.Common.Interfaces;
using Shortener.Application.Urls.DTOs;
using Shortener.Domain.Entities;

public class CreateShortUrlHandler
{
    private readonly IShortenedUrlRepository _repository;
    private readonly string _baseUrl;

    public CreateShortUrlHandler(IShortenedUrlRepository repository, string baseUrl = "http://localhost:5000")
    {
        _repository = repository;
        _baseUrl = baseUrl;
    }

    public async Task<ShortenedUrlResponse> HandleAsync(CreateShortUrlRequest request, CancellationToken cancellationToken = default)
    {
        var entity = ShortenedUrl.Create(request.OriginalUrl, request.ExpiresAt);
        var saved = await _repository.AddAsync(entity, cancellationToken);
        var shortUrl = $"{_baseUrl.TrimEnd('/')}/{saved.ShortCode}";

        return new ShortenedUrlResponse(
            ShortCode: saved.ShortCode,
            OriginalUrl: saved.OriginalUrl,
            ShortUrl: shortUrl,
            CreatedAt: saved.CreatedAt,
            ExpiresAt: saved.ExpiresAt,
            IsActive: saved.IsActive
        );
    }
}
