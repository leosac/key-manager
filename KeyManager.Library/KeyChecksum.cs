using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public abstract class KeyChecksum
    {
        public abstract string Name { get; }

        public string ComputeKCV(Key key, string? iv = null)
        {
            return ComputeKCV(key.Tags, key.Value, iv);
        }

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
            return Convert.ToHexString(ComputeKCV(tags, Convert.FromHexString(key), ivb));
        }

        public byte[] ComputeKCV(string tag, byte[] key, byte[]? iv = null)
        {
            return ComputeKCV(new string[] { tag }, key, iv);
        }

        public abstract byte[] ComputeKCV(IEnumerable<string> tags, byte[] key, byte[]? iv = null);
    }
}
