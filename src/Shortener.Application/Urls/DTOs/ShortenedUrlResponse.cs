namespace Shortener.Application.Urls.DTOs;

public record ShortenedUrlResponse(
    string ShortCode,
    string OriginalUrl,
    string ShortUrl,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    bool IsActive
);
