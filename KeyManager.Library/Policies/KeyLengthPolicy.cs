using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.Policies
{
    public class KeyLengthPolicy : IKeyPolicy
    {
        public KeyLengthPolicy(int byteLength)
        {
            ByteLength = byteLength;
        }

        public void Validate(Key key)
        {
            if (key.Value.Length % 2 != 0)
                throw new KeyPolicyException("Key is not correctly formated to be parsed to a byte array.");

            if (key.Value.Length / 2 != ByteLength)
                throw new KeyPolicyException("Wrong key length.");
        }

        public int ByteLength { get; set; }
    }
}
