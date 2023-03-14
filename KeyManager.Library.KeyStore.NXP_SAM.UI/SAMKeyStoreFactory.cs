using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.Domain;
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

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            var controls = new Dictionary<string, UserControl>();
            controls.Add("Key Usage Counters", new SAMKeyStoreKeyCounterControl());
            controls.Add("Tools", new SAMKeyStoreToolsControl());
            return controls;
        }
    }
}
