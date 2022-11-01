using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    public class SAMKeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "NXP SAM AV2";

        public override KeyStore CreateKeyStore()
        {
            return null;
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return null;
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return null;
        }

        public override KeyStorePropertiesControlViewModel CreateKeyStorePropertiesControlViewModel()
        {
            return null;
        }
    }
}
