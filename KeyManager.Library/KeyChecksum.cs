﻿using System;
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

        public string ComputeKCV(KeyTag keytags, string key, string? iv = null)
        {
            byte[]? ivb = null;
            if (!string.IsNullOrEmpty(iv))
            {
                ivb = Convert.FromHexString(iv);
            }
            return Convert.ToHexString(ComputeKCV(keytags, Convert.FromHexString(key), ivb));
        }

        public abstract byte[] ComputeKCV(KeyTag keytags, byte[] key, byte[]? iv = null);
    }
}
