using System;
using System.IO.Hashing;
using System.Text;

namespace PA.CacheAside.Core;

public static class StringExtensions
{
    public static string GenerateCompactCacheKey(this string text)
    {
        if (text == null)
        {
            return "";
        }

        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] hashBytes = XxHash64.Hash(bytes);

        return hashBytes.ToHexString();
    }

    private static string ToHexString(this ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != sizeof(ulong))
            throw new ArgumentException("The span must be exactly 8 bytes.");

        Span<char> c = stackalloc char[bytes.Length * 2];

        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];

            int pos = i * 2;

            int bSup = b >> 4;
            c[pos] = (char)(bSup < 10 ? bSup + '0' : bSup - 10 + 'A');

            int bInf = b & 0xF;
            c[pos + 1] = (char)(bInf < 10 ? bInf + '0' : bInf - 10 + 'A');
        }

        return new string(c);
    }
}
