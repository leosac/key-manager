using Leosac.KeyManager.Library.KeyStore.Memory.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI
{
    public class MemoryKeyEntryUIFactory : KeyEntryUIFactory
    {
        public MemoryKeyEntryUIFactory()
        {
            targetFactory = new MemoryKeyEntryFactory();
        }

        public override string Name => "Memory Key Entry";

        public override Type GetPropertiesType()
        {
            return typeof(MemoryKeyEntryProperties);
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new MemoryKeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new MemoryKeyEntryPropertiesControlViewModel();
        }
    }
}
