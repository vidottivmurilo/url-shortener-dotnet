namespace Shortener.Application.Urls;

using Shortener.Application.Common.Interfaces;

public record DeactivateShortUrlCommand(string ShortCode);

public class DeactivateShortUrlHandler
{
    private readonly IShortenedUrlRepository _repository;
    private readonly ICacheService _cache;

    public DeactivateShortUrlHandler(IShortenedUrlRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task HandleAsync(DeactivateShortUrlCommand command, CancellationToken cancellationToken = default)
    {
        var link = await _repository.GetByShortCodeAsync(command.ShortCode, cancellationToken);
        if (link == null)
        {
            throw new KeyNotFoundException($"Link com código '{command.ShortCode}' não encontrado.");
        }

        link.IsActive = false;
        await _repository.UpdateAsync(link, cancellationToken);
        await _cache.RemoveAsync($"short:{command.ShortCode}", cancellationToken);
    }
}
