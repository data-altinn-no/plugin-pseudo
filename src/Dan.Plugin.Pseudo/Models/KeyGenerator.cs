using System.Security.Cryptography;

namespace Dan.Pseudo.Models;

public static class KeyGenerator
{
    public static byte[] GenerateKey()
    {
        var key = new byte[32]; // 256-bit key
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return key;
    }
}
