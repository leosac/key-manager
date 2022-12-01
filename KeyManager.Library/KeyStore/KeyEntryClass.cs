using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    [Flags]
    public enum KeyEntryClass : byte
    {
        Symmetric = 0x10,
        Asymmetric = 0x20,
        PrivateKey = Asymmetric | 0x01,
        PublicKey = Asymmetric | 0x02
    }
}
