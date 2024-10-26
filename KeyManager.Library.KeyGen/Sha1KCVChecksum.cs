using System.Security.Cryptography;

namespace Leosac.KeyManager.Library.KeyGen
{
    public class Sha1KCVChecksum : Sha1Checksum
    {
        public override string Name => "SHA1 KCV";

        public override byte[] ComputeKCV(Key key, byte[]? iv)
        {
            var data = base.ComputeKCV(key, iv);
            Array.Resize(ref data, 3);
            return data;
        }
    }
}
