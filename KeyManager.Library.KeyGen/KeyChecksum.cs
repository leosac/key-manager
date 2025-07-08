namespace Leosac.KeyManager.Library.KeyGen
{
    public abstract class KeyChecksum
    {
        public abstract string Name { get; }

        public string ComputeKCV(string tag, string key)
        {
            return ComputeKCV(tag, key, null);
        }

        public string ComputeKCV(string tag, string key, string? iv)
        {
            return ComputeKCV(new[] { tag }, key, iv);
        }

        public string ComputeKCV(IEnumerable<string> tags, string key)
        {
            return ComputeKCV(tags, key, null);
        }

        public string ComputeKCV(IEnumerable<string> tags, string key, string? iv)
        {
            byte[]? ivb = null;
            if (!string.IsNullOrEmpty(iv))
            {
                ivb = Convert.FromHexString(iv);
            }
            uint keySize = 0;
            if (!string.IsNullOrEmpty(key))
            {
                keySize = (uint)key.Length / 2;
            }
            return Convert.ToHexString(ComputeKCV(new Key(tags, keySize, key), ivb));
        }

        public string ComputeKCV(Key key, string? iv = null)
        {
            byte[]? ivb = null;
            if (!string.IsNullOrEmpty(iv))
            {
                ivb = Convert.FromHexString(iv);
            }
            return Convert.ToHexString(ComputeKCV(key, ivb));
        }

        public abstract byte[] ComputeKCV(Key key, byte[]? iv);
    }
}
