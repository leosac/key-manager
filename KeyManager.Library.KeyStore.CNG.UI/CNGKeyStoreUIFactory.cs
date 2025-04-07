using Leosac.KeyManager.Library.KeyStore.CNG.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.CNG.UI
{
    public class CNGKeyStoreUIFactory : KeyStoreUIFactory
    {
        public CNGKeyStoreUIFactory()
        {
            targetFactory = new CNGKeyStoreFactory();
        }

        public override string Name => "Cryptography API: Next Generation (CNG)";

        public override Type GetPropertiesType()
        {
            return typeof(CNGKeyStoreProperties);
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new CNGKeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel? CreateKeyStorePropertiesControlViewModel()
        {
            return new CNGKeyStorePropertiesControlViewModel();
        }

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            return new Dictionary<string, UserControl>();
        }
    }
}
