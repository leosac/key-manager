using Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI
{
    public class SAMSymmetricKeyEntryUIFactory : KeyEntryUIFactory
    {
        public SAMSymmetricKeyEntryUIFactory()
        {
            targetFactory = new SAMSymmetricKeyEntryFactory();
        }

        public override string Name => "NXP SAM Key Entry";

        public override Type? GetPropertiesType()
        {
            return typeof(SAMSymmetricKeyEntryProperties);
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new SAMSymmetricKeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new SAMSymmetricKeyEntryPropertiesControlViewModel();
        }
    }
}
