using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class T3KDESSAMKeyEntry : SAMKeyEntry
    {
        public T3KDESSAMKeyEntry() : base()
        {
            KeyVersions.Add(new KeyVersion("Key Version A", 0, new Key(KeyTag.DES, 24)));
            KeyVersions.Add(new KeyVersion("Key Version B", 0, new Key(KeyTag.DES, 24)));
        }
    }
}
