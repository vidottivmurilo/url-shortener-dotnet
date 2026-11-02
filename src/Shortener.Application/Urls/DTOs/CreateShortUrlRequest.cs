namespace Shortener.Application.Urls.DTOs;

public record CreateShortUrlRequest(string OriginalUrl, DateTime? ExpiresAt = null);
