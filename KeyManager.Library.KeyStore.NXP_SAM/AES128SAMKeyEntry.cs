using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class AES128SAMKeyEntry : SAMKeyEntry
    {
        public AES128SAMKeyEntry() : base()
        {
            KeyVersions.Add(new KeyVersion("Key Version A", 0));
            KeyVersions.Add(new KeyVersion("Key Version B", 0));
            KeyVersions.Add(new KeyVersion("Key Version C", 0));
        }
    }
}
