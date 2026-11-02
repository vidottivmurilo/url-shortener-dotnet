namespace Shortener.Domain.Common;

public static class Base62Encoder
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly sbyte[] LookupTable = CreateLookupTable();

    private static sbyte[] CreateLookupTable()
    {
        var table = new sbyte[128];
        Array.Fill(table, (sbyte)-1);

        for (int i = 0; i < Alphabet.Length; i++)
        {
            table[Alphabet[i]] = (sbyte)i;
        }

        return table;
    }

    public static string Encode(long number)
    {
        if (number < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "O número deve ser não negativo.");
        }

        if (number == 0)
        {
            return "0";
        }

        // 62^11 > long.MaxValue, logo 11 posições são suficientes
        Span<char> buffer = stackalloc char[11];
        int position = buffer.Length;

        while (number > 0)
        {
            int remainder = (int)(number % 62);
            buffer[--position] = Alphabet[remainder];
            number /= 62;
        }

        return new string(buffer[position..]);
    }

    public static long Decode(string base62)
    {
        if (string.IsNullOrEmpty(base62))
        {
            throw new ArgumentException("A entrada em Base62 não pode ser nula ou vazia.", nameof(base62));
        }

        long result = 0;

        foreach (char c in base62)
        {
            if (c >= 128 || LookupTable[c] == -1)
            {
                throw new ArgumentException($"Caractere inválido '{c}' encontrado na string Base62.", nameof(base62));
            }

            result = checked(result * 62 + LookupTable[c]);
        }

        return result;
    }
}
