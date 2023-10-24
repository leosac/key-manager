using System.Security.Cryptography;
using System.Text;

namespace Leosac.KeyManager.Library
{
    public static class KeyGeneration
    {
        public static byte[] Random(uint keySize)
        {
            using var rng = RandomNumberGenerator.Create();
            var key = new byte[keySize];
            rng.GetBytes(key);
            return key;
        }

        public static byte[] FromPassword(string password, string salt, int keySize)
        {
            var deriv = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000, HashAlgorithmName.SHA256);
            var key = deriv.GetBytes(keySize);
            return key;
        }

        public static byte[] CreateRandomSalt(uint length)
        {
            return Random(length >= 1 ? length : 1);
        }
    }
}
