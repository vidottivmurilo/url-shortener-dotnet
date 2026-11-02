namespace Shortener.Domain.Events;

public record LinkClickedEvent(
    string ShortCode,
    DateTime ClickedAt,
    string? IpAddress,
    string? UserAgent
);
