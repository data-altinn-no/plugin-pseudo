using System.Security.Cryptography;
using System.Text;
using System;

namespace Dan.Pseudo.Models;


public static class Pseudonymizer
{
    public static string HashIdentifier(string identifier, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        var data = Encoding.UTF8.GetBytes(identifier);
        var hash = hmac.ComputeHash(data);
        return Convert.ToBase64String(hash);
    }
}
