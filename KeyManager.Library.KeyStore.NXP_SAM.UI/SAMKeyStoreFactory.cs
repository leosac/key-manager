using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.Domain;
using log4net;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    public class SAMKeyStoreFactory : KeyStoreFactory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

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
            try
            {
                return new SAMKeyStorePropertiesControlViewModel();
            }
            catch (Exception ex)
            {
                log.Error("Error when initializing the key store view model", ex);
                return null;
            }
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
