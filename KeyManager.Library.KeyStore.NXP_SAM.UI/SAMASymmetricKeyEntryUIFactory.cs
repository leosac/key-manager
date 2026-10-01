using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    public class SAMAsymmetricKeyEntryUIFactory : KeyEntryUIFactory
    {
        public SAMAsymmetricKeyEntryUIFactory()
        {
            targetFactory = new SAMAsymmetricKeyEntryFactory();
        }

        public override string Name => "NXP SAM Asymmetric Key Entry";

        public override Type? GetPropertiesType()
        {
            return typeof(SAMAsymmetricKeyEntryProperties);
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new SAMAsymmetricKeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new SAMAsymmetricKeyEntryPropertiesControlViewModel();
        }
    }
}
