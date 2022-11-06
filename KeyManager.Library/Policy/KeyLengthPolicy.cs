﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.Policy
{
    public class KeyLengthPolicy : IKeyPolicy
    {
        public KeyLengthPolicy(uint byteLength)
        {
            ByteLength = byteLength;
        }

        public void Validate(string? key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (key.Length % 2 != 0)
                    throw new KeyPolicyException("Key is not correctly formated to be parsed to a byte array.");

                if (key.Length / 2 != ByteLength)
                    throw new KeyPolicyException("Wrong key length.");
            }
        }

        public uint ByteLength { get; set; }
    }
}
