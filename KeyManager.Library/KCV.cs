using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;

namespace Leosac.KeyManager.Library
{
    /// <summary>
    /// Key Checksum Value calculation.
    /// This KCV algorithm is something used by the security industry manufacturers (SIM/SmartCard/HSM/...) but not realy standardized afaik (sigh...).
    /// The logic is simple: we encrypt a block of bytes, each set to value 0x00 or 0x01, using the cryptographic key and keep only the first 3 resulting bytes as the KCV.
    /// </summary>
    public class KCV : KeyChecksum
    {
        public override string Name => "KCV";

        public override byte[] ComputeKCV(Key key, byte[]? iv)
        {
            byte[] result;
            var data = new byte[KeyHelper.GetBlockSize(key.Tags)];
            var paddediv = new byte[KeyHelper.GetBlockSize(key.Tags)];
            if (key.Tags.Contains("AES"))
            {
                // For AES, GlobalPlatform specification is using a default byte value set to 0x01 and not 0x00
                for (var i = 0; i < paddediv.Length; i++)
                {
                    paddediv[i] = 0x01;
                }
            }
            if (iv != null)
            {
                Array.Copy(iv, paddediv, iv.Length > paddediv.Length ? paddediv.Length : iv.Length);
            }
            using (var ms = new MemoryStream())
            {
                var crypto = KeyHelper.GetSymmetricAlgorithm(key, CipherMode.CBC, true) ?? throw new Exception("Unsupported key for KCV calcul.");
                var parameters = new ParametersWithIV(new KeyParameter(key.GetAggregatedValueAsBinary()), paddediv);
                crypto.Init(true, parameters);
                result = crypto.DoFinal(data);
                Array.Resize(ref result, 3);
            }

            return result;
        }
    }
}
