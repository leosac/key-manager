using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    public class T3KDESSAMKeyEntryFactory : SAMKeyEntryFactory
    {
        public override string Name => "SAM T3KDES";

        public override KeyEntry CreateKeyEntry()
        {
            return new T3KDESSAMKeyEntry();
        }
    }
}
