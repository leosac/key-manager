using Leosac.KeyManager.Library.KeyStore.CNG.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.CNG.UI
{
    public class CNGKeyEntryUIFactory : KeyEntryUIFactory
    {
        public CNGKeyEntryUIFactory()
        {
            targetFactory = new CNGKeyEntryFactory();
        }

        public override string Name => "CNG Key Entry";

        public override Type GetPropertiesType()
        {
            return typeof(CNGKeyEntryProperties);
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new CNGKeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new CNGKeyEntryPropertiesControlViewModel();
        }
    }
}
