namespace Leosac.KeyManager.Library
{
    public abstract class KeyChecksum
    {
        public abstract string Name { get; }

        public string ComputeKCV(string tag, string key, string? iv = null)
        {
            return ComputeKCV(new string[] { tag }, key, iv);
        }

        public string ComputeKCV(IEnumerable<string> tags, string key, string? iv = null)
        {
            byte[]? ivb = null;
            if (!string.IsNullOrEmpty(iv))
            {
                ivb = Convert.FromHexString(iv);
            }
            return Convert.ToHexString(ComputeKCV(new Key(tags, key), ivb));
        }

        public string ComputeKCV(Key key, string? iv)
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
