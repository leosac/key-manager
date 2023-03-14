using Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class PKCSC11KeyStoreFactory : KeyStoreFactory
    {
        public override string Name => "HSM PKCS#11";

        public override KeyStore CreateKeyStore()
        {
            return new PKCS11KeyStore();
        }

        public override Type GetKeyStorePropertiesType()
        {
            return typeof(PKCS11KeyStoreProperties);
        }

        public override KeyStoreProperties CreateKeyStoreProperties()
        {
            return new PKCS11KeyStoreProperties();
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new PKCS11KeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel CreateKeyStorePropertiesControlViewModel()
        {
            return new PKCS11KeyStorePropertiesControlViewModel();
        }

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            return new Dictionary<string, UserControl>();
        }
    }
}
