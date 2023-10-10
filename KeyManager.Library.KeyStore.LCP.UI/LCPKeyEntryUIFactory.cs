using Leosac.KeyManager.Library.KeyStore.LCP.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.LCP.UI
{
    public class LCPKeyEntryUIFactory : KeyEntryUIFactory
    {
        public LCPKeyEntryUIFactory()
        {
            targetFactory = new LCPKeyEntryFactory();
        }

        public override string Name => "LCP Key Entry";

        public override Type GetPropertiesType()
        {
            return typeof(LCPKeyEntryProperties);
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new LCPKeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new LCPKeyEntryPropertiesControlViewModel();
        }
    }
}
