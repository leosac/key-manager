﻿using System.Security.Cryptography;
using System.Text;

namespace Leosac.KeyManager.Library
{
    public static class KeyGeneration
    {
        public static string Random(uint keySize)
        {
            using var rng = RandomNumberGenerator.Create();
            var key = new byte[keySize];
            rng.GetBytes(key);
            return Convert.ToHexString(key);
        }

        public static string FromPassword(string password, string salt, int keySize)
        {
            var deriv = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000, HashAlgorithmName.SHA256);
            var key = deriv.GetBytes(keySize);
            return Convert.ToHexString(key);
        }

        public static byte[] CreateRandomSalt(uint length)
        {
            byte[] randBytes = (length >= 1) ? new byte[length] : new byte[1];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randBytes);
            }
            return randBytes;
        }
    }
}
