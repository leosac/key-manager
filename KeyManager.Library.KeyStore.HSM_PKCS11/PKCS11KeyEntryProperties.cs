using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class PKCS11KeyEntryProperties : KeyEntryProperties
    {
        public bool Encrypt { get; set; }

        public bool Decrypt { get; set; }

        public bool Derive { get; set; }

        public bool Extractable { get; set; }
    }
}
