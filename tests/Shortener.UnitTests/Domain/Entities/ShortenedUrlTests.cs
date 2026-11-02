namespace Shortener.UnitTests.Domain.Entities;

using Shortener.Domain.Entities;
using Xunit;

public class ShortenedUrlTests
{
    [Fact]
    public void Should_Create_ShortenedUrl_When_UrlIsValid()
    {
        // Arrange
        var validUrl = "https://example.com/some/deep/resource";

        // Act
        var result = ShortenedUrl.Create(validUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(validUrl, result.OriginalUrl);
        Assert.True(result.IsActive);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
        Assert.Null(result.ExpiresAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_ThrowArgumentException_When_UrlIsEmptyOrWhitespace(string? invalidUrl)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ShortenedUrl.Create(invalidUrl!));
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("not-a-valid-uri")]
    [InlineData("mailto:user@example.com")]
    public void Should_ThrowArgumentException_When_UrlIsNotHttpOrHttps(string nonHttpUrl)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ShortenedUrl.Create(nonHttpUrl));
    }

    [Fact]
    public void Should_ThrowArgumentException_When_ExpiresAtIsInThePast()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddMinutes(-5);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ShortenedUrl.Create("https://example.com", pastDate));
    }
}
