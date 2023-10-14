using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

namespace Leosac.KeyManager.Library
{
    public static class KeyHelper
    {
        public static uint GetBlockSize(IEnumerable<string> tags)
        {
            uint size = 8;
            if (tags.Contains("AES"))
            {
                size = 16;
            }
            return size;
        }

        public static string? GetAlgorithmName(Key key)
        {
            string? algoName = null;
            if (key.Tags.Contains("AES"))
            {
                algoName = "AES";
            }
            else if (key.Tags.Contains("DES") && key.KeySize > 8)
            {
                algoName = "DESEDE";
            }
            else if (key.Tags.Contains("DES"))
            {
                algoName = "DES";
            }
            else if (key.Tags.Contains("Blowfish"))
            {
                algoName = "Blowfish";
            }
            return algoName;
        }

        public static IBufferedCipher? GetSymmetricAlgorithm(Key key)
        {
            return GetSymmetricAlgorithm(key, CipherMode.CBC);
        }

        public static IBufferedCipher? GetSymmetricAlgorithm(Key key, CipherMode cipherMode)
        {
            return GetSymmetricAlgorithm(key, cipherMode, false);
        }

        public static IBufferedCipher? GetSymmetricAlgorithm(Key key, CipherMode cipherMode, bool noPadding)
        {
            IBufferedCipher? crypto = null;

            var algoName = GetAlgorithmName(key);
            if (!string.IsNullOrEmpty(algoName))
            {
                var fullName = algoName + "/" + cipherMode.ToString();
                if (noPadding)
                {
                    fullName += "/" + "NoPadding";
                }
                crypto = CipherUtilities.GetCipher(fullName);
            }

            return crypto;
        }
    }
}
