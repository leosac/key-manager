﻿using System.Security.Cryptography;

namespace Leosac.KeyManager.Library
{
    public class Sha256Checksum : KeyChecksum
    {
        public override string Name => "SHA256";

        public override byte[] ComputeKCV(Key key, byte[]? iv)
        {
            var rawkey = key.GetAggregatedValueBinary() ?? throw new Exception("Key value is null");

            // Use the IV as a Salt
            byte[] data;
            if (iv != null)
            {
                data = new byte[iv.Length + rawkey.Length];
                Array.Copy(iv, 0, data, 0, iv.Length);
                Array.Copy(rawkey, 0, data, iv.Length, rawkey.Length);
            }
            else
            {
                data = rawkey;
            }
            return SHA256.HashData(data);
        }
    }
}
