using Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class PKCSC11KeyStoreUIFactory : KeyStoreUIFactory
    {
        public override string Name => "HSM PKCS#11";

        public override Type GetPropertiesType()
        {
            return typeof(PKCS11KeyStoreProperties);
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
