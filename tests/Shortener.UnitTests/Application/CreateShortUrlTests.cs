namespace Shortener.UnitTests.Application;

using Shortener.Application.Common.Interfaces;
using Shortener.Application.Urls;
using Shortener.Application.Urls.DTOs;
using Shortener.Domain.Common;
using Shortener.Domain.Entities;
using Xunit;

public class CreateShortUrlTests
{
    private class FakeShortenedUrlRepository : IShortenedUrlRepository
    {
        private readonly List<ShortenedUrl> _urls = new();
        private long _idSequence = 1000;

        public Task<ShortenedUrl> AddAsync(ShortenedUrl entity, CancellationToken cancellationToken = default)
        {
            entity.Id = ++_idSequence;
            entity.ShortCode = Base62Encoder.Encode(entity.Id);
            _urls.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default)
        {
            var found = _urls.FirstOrDefault(u => u.ShortCode == shortCode);
            return Task.FromResult(found);
        }

        public Task UpdateAsync(ShortenedUrl entity, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Should_ReturnCreatedResponse_With_Base62Code_When_RequestIsValid()
    {
        // Arrange
        var fakeRepo = new FakeShortenedUrlRepository();
        var handler = new CreateShortUrlHandler(fakeRepo, "https://encurta.do");
        var request = new CreateShortUrlRequest("https://google.com");

        // Act
        var response = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.ShortCode);
        Assert.Equal("https://google.com", response.OriginalUrl);
        Assert.Equal($"https://encurta.do/{response.ShortCode}", response.ShortUrl);
        Assert.True(response.IsActive);
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_UrlIsInvalid()
    {
        // Arrange
        var fakeRepo = new FakeShortenedUrlRepository();
        var handler = new CreateShortUrlHandler(fakeRepo, "https://encurta.do");
        var request = new CreateShortUrlRequest("invalido");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(request));
    }
}
