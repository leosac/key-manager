using Leosac.KeyManager.Library.KeyStore.LCP.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.LCP.UI
{
    public class LCPKeyStoreUIFactory : KeyStoreUIFactory
    {
        public LCPKeyStoreUIFactory()
        {
            targetFactory = new LCPKeyStoreFactory();
        }

        public override string Name => "Leosac Credential Provisioning Server";

        public override Type GetPropertiesType()
        {
            return typeof(LCPKeyStoreProperties);
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new LCPKeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel? CreateKeyStorePropertiesControlViewModel()
        {
            return new LCPKeyStorePropertiesControlViewModel();
        }

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            return new Dictionary<string, UserControl>();
        }
    }
}
