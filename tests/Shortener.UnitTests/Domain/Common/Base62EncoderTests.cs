using Shortener.Domain.Common;

namespace Shortener.UnitTests.Domain.Common;

public class Base62EncoderTests
{
    // Cenário de testes 1: Números negativos
    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Encode_WhenNumberIsNegative_ThrowArguentOutOfRangeException(long negativeNumber)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Base62Encoder.Encode(negativeNumber));
    }

    // Cenário de testes 2: Números válidos conhecidos
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(10, "a")]
    [InlineData(35, "z")]
    [InlineData(36, "A")]
    [InlineData(61, "Z")]
    [InlineData(62, "10")]
    [InlineData(125, "21")]
    [InlineData(3844, "100")]
    public void Encode_WhenValidNumber_ReturnsExpectedBase62String(long number, string expected)
    {
        // Act
        string result = Base62Encoder.Encode(number);

        // Assert
        Assert.Equal(expected, result);
    }

    // Cenário de testes 3: Decodificação de códigos válidos de volta para números
    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("a", 10)]
    [InlineData("z", 35)]
    [InlineData("A", 36)]
    [InlineData("Z", 61)]
    [InlineData("10", 62)]
    [InlineData("21", 125)]
    [InlineData("100", 3844)]
    public void Decode_WhenValidBase62String_ReturnsExpectedNumber(string base62, long expected)
    {
        // Act
        long result = Base62Encoder.Decode(base62);

        // Assert
        Assert.Equal(expected, result);
    }

    // Cenário de testes 4: Caracteres inválidos na decodificação
    [Theory]
    [InlineData("abc-123")]
    [InlineData("!@#")]
    [InlineData("a b c")]
    public void Decode_WhenInvalidCharacters_ThrowsArgumentException(string invalidInput)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Base62Encoder.Decode(invalidInput));
    }

    // Cenário de testes 5: Teste de "Ida e Volta"
    [Theory]
    [InlineData(123456789L)]
    [InlineData(9876543210L)]
    public void EncodeAndDecode_RoundTrip_PreservesOriginalNumber(long originalNumber)
    {
        // Act: Codifica para Base62 e decodifica de volta
        string encoded = Base62Encoder.Encode(originalNumber);
        long decoded = Base62Encoder.Decode(encoded);
        // Assert: O valor recuperado deve ser idêntico ao original
        Assert.Equal(originalNumber, decoded);
    }

}
