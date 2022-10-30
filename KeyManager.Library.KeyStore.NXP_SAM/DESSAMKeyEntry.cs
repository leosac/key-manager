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
            KeyVersions.Add(new KeyVersion(0));
            KeyVersions.Add(new KeyVersion(0));
            KeyVersions.Add(new KeyVersion(0));
        }

        public override string Name => "SAM DES";
    }
}
