using System;
using System.Linq;

namespace RefScout.Analyzer;

public readonly struct PublicKeyToken
{
    private const int TokenSize = 8;
    public static readonly PublicKeyToken Empty = new();

    private readonly byte[]? _data;

    public PublicKeyToken(byte[]? data)
    {
        if (data is null || data.Length != TokenSize)
        {
            this = Empty;
        }
        else
        {
            _data = data;
        }
    }

    public override string ToString()
    {
        const string nullString = "null";

        if (_data is not { Length: TokenSize })
        {
            return nullString;
        }

        return string.Create(TokenSize * 2, _data, (chars, buffer) =>
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                var high = buffer[i] >> 4;
                chars[i * 2] = (char)(87 + high + (((high - 10) >> 31) & -39));
                var low = buffer[i] & 0xF;
                chars[i * 2 + 1] = (char)(87 + low + (((low - 10) >> 31) & -39));
            }
        });
    }

    public static bool operator ==(PublicKeyToken a, PublicKeyToken b)
    {
        if (a._data == null && b._data == null)
        {
            return true;
        }

        if (a._data == null || b._data == null || a._data.Length != b._data.Length)
        {
            return false;
        }

        for (var i = 0; i < TokenSize; i++)
        {
            if (a._data[i] != b._data[i])
            {
                return false;
            }
        }

        return true;
    }

    public static bool operator !=(PublicKeyToken a, PublicKeyToken b) => !(a == b);

    public static PublicKeyToken Parse(string hexString) => Parse(hexString.AsSpan());

    public static PublicKeyToken Parse(ReadOnlySpan<char> hexString)
    {
        var exception = TryParseInternal(hexString, out var token);
        return exception != null
            ? throw exception
            : token;
    }

    public static bool TryParse(string hexString, out PublicKeyToken token) =>
        TryParse(hexString.AsSpan(), out token);

    public static bool TryParse(ReadOnlySpan<char> hexString, out PublicKeyToken token)
    {
        var exception = TryParseInternal(hexString, out token);
        return exception == null;
    }

    private static Exception? TryParseInternal(ReadOnlySpan<char> hexString, out PublicKeyToken token)
    {
        token = Empty;
        if (hexString.Length != TokenSize * 2)
        {
            return new ArgumentOutOfRangeException(nameof(hexString),
                $"PublicKeyToken should always be {TokenSize * 2} hexadecimal characters");
        }

        var bytes = new byte[TokenSize];
        for (var i = 0; i < bytes.Length; i++)
        {
            int high = hexString[i * 2];
            int low = hexString[i * 2 + 1];

            if (!IsValidHexCharacter(high) || !IsValidHexCharacter(low))
            {
                return new ArgumentException("A public key token can only consist of hexadecimal characters",
                    nameof(hexString));
            }

            high = (high & 0xf) + ((high & 0x40) >> 6) * 9;
            low = (low & 0xf) + ((low & 0x40) >> 6) * 9;
            bytes[i] = (byte)((high << 4) | low);
        }

        token = new PublicKeyToken(bytes);
        return null;
    }

    private static bool IsValidHexCharacter(int character) =>
        character is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F');

    public override bool Equals(object? obj)
    {
        if (obj is not PublicKeyToken other)
        {
            return false;
        }

        return this == other;
    }

    public override int GetHashCode()
    {
        if (_data == null)
        {
            return 0;
        }

        unchecked
        {
            return _data.Aggregate(0, (current, b) => (current * 31) ^ b);
        }
    }
}