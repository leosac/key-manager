using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain;
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
            return new SAMKeyStore();
        }

        public override Type GetKeyStorePropertiesType()
        {
            return typeof(SAMKeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new SAMKeyStoreProperties();
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new SAMKeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel CreateKeyStorePropertiesControlViewModel()
        {
            return new SAMKeyStorePropertiesControlViewModel();
        }
    }
}
