using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class PKCSC11KeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "HSM PKCS#11";

        public override KeyStore CreateKeyStore()
        {
            return null;
        }

        public override Type GetKeyStorePropertiesType()
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

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            throw new NotImplementedException();
        }
    }
}
