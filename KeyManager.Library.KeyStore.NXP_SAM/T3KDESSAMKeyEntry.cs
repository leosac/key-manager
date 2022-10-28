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
            KeyVersions.Add(new KeyVersion(0));
            KeyVersions.Add(new KeyVersion(0));
        }
    }
}
