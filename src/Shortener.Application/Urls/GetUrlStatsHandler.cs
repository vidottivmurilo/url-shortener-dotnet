namespace Shortener.Application.Urls;

using Shortener.Application.Common.Interfaces;
using Shortener.Application.Urls.DTOs;

public record GetUrlStatsQuery(string ShortCode);

public class GetUrlStatsHandler
{
    private readonly IShortenedUrlRepository _urlRepository;
    private readonly IClickMetricRepository _metricRepository;

    public GetUrlStatsHandler(
        IShortenedUrlRepository urlRepository,
        IClickMetricRepository metricRepository)
    {
        _urlRepository = urlRepository;
        _metricRepository = metricRepository;
    }

    public async Task<UrlStatsResponse> HandleAsync(GetUrlStatsQuery query, CancellationToken cancellationToken = default)
    {
        var link = await _urlRepository.GetByShortCodeAsync(query.ShortCode, cancellationToken);
        if (link == null)
        {
            throw new KeyNotFoundException($"Código '{query.ShortCode}' não encontrado.");
        }

        var (totalClicks, lastClickedAt) = await _metricRepository.GetStatsAsync(query.ShortCode, cancellationToken);

        return new UrlStatsResponse(
            ShortCode: link.ShortCode,
            OriginalUrl: link.OriginalUrl,
            TotalClicks: totalClicks,
            LastClickedAt: lastClickedAt,
            CreatedAt: link.CreatedAt
        );
    }
}
