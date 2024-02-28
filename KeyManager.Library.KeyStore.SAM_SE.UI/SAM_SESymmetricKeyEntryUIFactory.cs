using Leosac.KeyManager.Library.KeyStore.SAM_SE.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE.UI
{
    public class SAM_SESymmetricKeyEntryUIFactory : KeyEntryUIFactory
    {
        public SAM_SESymmetricKeyEntryUIFactory()
        {
            targetFactory = new SAM_SESymmetricKeyEntryFactory();
        }

        public override string Name => "SAM-SE Key Entry";

        public override Type? GetPropertiesType()
        {
            return typeof(SAM_SESymmetricKeyEntryProperties);
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new SAM_SESymmetricKeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new SAM_SESymmetricKeyEntryPropertiesControlViewModel();
        }
    }
}
