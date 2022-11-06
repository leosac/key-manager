using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class DESSAMKeyEntry : SAMKeyEntry
    {
        public DESSAMKeyEntry() : base()
        {
            KeyVersions.Add(new KeyVersion("Key Version A", 0, new Key(KeyTag.DES, 16)));
            KeyVersions.Add(new KeyVersion("Key Version B", 0, new Key(KeyTag.DES, 16)));
            KeyVersions.Add(new KeyVersion("Key Version C", 0, new Key(KeyTag.DES, 16)));
        }
    }
}
