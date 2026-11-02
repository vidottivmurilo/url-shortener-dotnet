namespace Shortener.Domain.Entities;

public class ShortenedUrl
{
    public long Id { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public static ShortenedUrl Create(string originalUrl, DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
        {
            throw new ArgumentException("A URL original não pode ser nula ou vazia.", nameof(originalUrl));
        }

        if (originalUrl.Length > 2048)
        {
            throw new ArgumentException("A URL original excede o tamanho máximo de 2048 caracteres.", nameof(originalUrl));
        }

        if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("A URL original deve possuir esquema HTTP ou HTTPS válido.", nameof(originalUrl));
        }

        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
        {
            throw new ArgumentException("A data de expiração deve ser estritamente posterior à data e hora atual.", nameof(expiresAt));
        }

        return new ShortenedUrl
        {
            OriginalUrl = originalUrl,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = true
        };
    }
}
