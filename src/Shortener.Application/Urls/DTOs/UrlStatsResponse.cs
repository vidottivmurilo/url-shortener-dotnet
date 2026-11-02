namespace Shortener.Application.Urls.DTOs;

public record UrlStatsResponse(
    string ShortCode,
    string OriginalUrl,
    long TotalClicks,
    DateTime? LastClickedAt,
    DateTime CreatedAt
);
