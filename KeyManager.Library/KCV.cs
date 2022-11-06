using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        public override byte[] ComputeKCV(KeyTag tags, byte[] key, uint keySize, byte[]? iv = null)
        {
            var result = new byte[0];
            var data = new byte[KeyHelper.GetBlockSize(tags)];
            var paddediv = new byte[KeyHelper.GetBlockSize(tags)];
            if ((tags & KeyTag.AES) == KeyTag.AES)
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
                SymmetricAlgorithm? crypto = null;
                if ((tags & KeyTag.AES) == KeyTag.AES)
                {
                    crypto = Aes.Create();
                }
                else if ((tags & KeyTag.DES) == KeyTag.DES && keySize > 8)
                {
                    crypto = TripleDES.Create();
                }
                else if ((tags & KeyTag.DES) == KeyTag.DES)
                {
                    crypto = DES.Create();
                }
                else
                    throw new Exception("Unsupported key for KCV calcul.");

                if (crypto != null)
                {
                    crypto.Mode = CipherMode.ECB;
                    using (var encryptor = crypto.CreateEncryptor(key, paddediv))
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                            cs.FlushFinalBlock();
                            result = ms.ToArray();
                            Array.Resize(ref result, 3);
                            cs.Close();
                        }
                    }
                    crypto.Dispose();
                }
            }

            return result;
        }
    }
}
