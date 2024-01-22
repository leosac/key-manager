using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    public class SAMKeyStoreUIFactory : KeyStoreUIFactory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SAMKeyStoreUIFactory()
        {
            targetFactory = new SAMKeyStoreFactory();
        }

        public override string Name => "NXP SAM AV2/AV3";

        public override Type GetPropertiesType()
        {
            return typeof(SAMKeyStoreProperties);
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new SAMKeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel? CreateKeyStorePropertiesControlViewModel()
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
            var controls = new Dictionary<string, UserControl>
            {
                { "Key Usage Counters", new SAMKeyStoreKeyCounterControl() },
                { "Tools", new SAMKeyStoreToolsControl() }
            };
            return controls;
        }
    }
}
