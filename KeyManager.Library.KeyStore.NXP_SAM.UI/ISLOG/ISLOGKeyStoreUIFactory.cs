using Leosac.KeyManager.Library.KeyStore.NXP_SAM.ISLOG;
using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.ISLOG.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.ISLOG
{
    public class ISLOGKeyStoreUIFactory : KeyStoreUIFactory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public ISLOGKeyStoreUIFactory()
        {
            targetFactory = new ISLOGKeyStoreFactory();
        }

        public override string Name => "ISLOG SAM Manager Template";

        public override Type GetPropertiesType()
        {
            return typeof(ISLOGKeyStoreProperties);
        }

        public override UserControl CreateKeyStorePropertiesControl()
        {
            return new ISLOGKeyStorePropertiesControl();
        }

        public override KeyStorePropertiesControlViewModel? CreateKeyStorePropertiesControlViewModel()
        {
            try
            {
                return new ISLOGKeyStorePropertiesControlViewModel();
            }
            catch (Exception ex)
            {
                log.Error("Error when initializing the key store view model", ex);
                return null;
            }
        }

        public override IDictionary<string, UserControl> CreateKeyStoreAdditionalControls()
        {
            return new Dictionary<string, UserControl>();
        }
    }
}
