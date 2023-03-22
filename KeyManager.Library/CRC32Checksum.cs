using Force.Crc32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public class CRC32Checksum : KeyChecksum
    {
        public override string Name => "CRC32";

        public override byte[] ComputeKCV(Key key, byte[]? iv = null)
        {
            var rawkey = key.GetAggregatedValue<byte[]>(KeyValueFormat.Binary);
            if (rawkey == null)
                throw new Exception("Key value is null");

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
            return BitConverter.GetBytes(Crc32Algorithm.ComputeAndWriteToEnd(data));
        }
    }
}
