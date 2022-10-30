using Leosac.KeyManager.Library.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    public class AES128SAMKeyEntryFactory : SAMKeyEntryFactory
    {
        public override string Name => "SAM AES128";

        public override KeyEntry CreateKeyEntry()
        {
            return new AES128SAMKeyEntry();
        }
    }
}
