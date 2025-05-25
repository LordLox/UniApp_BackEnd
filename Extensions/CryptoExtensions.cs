using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

public static class CryptoExtensions
{
    // Generate a hash using HMAC-SHA256
    public static string HMACSHA256(string str, byte[] salt)
    {
        return Convert.ToBase64String(
            KeyDerivation.Pbkdf2
            (
                password: str,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            )
        );
    }

    // Generate a random salt
    public static byte[] GenerateSalt() =>
        RandomNumberGenerator.GetBytes(128 / 8);

    // Generate a random string of specified length
    public static string RandomString(int length)
    {
        Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}