using Leosac.KeyManager.Library.KeyStore.Memory.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI
{
    public class MemoryKeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "Memory Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => new KeyEntryClass[] { KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric };

        public override KeyEntry CreateKeyEntry()
        {
            return new MemoryKeyEntry();
        }

        public override Type GetKeyEntryPropertiesType()
        {
            return typeof(MemoryKeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new MemoryKeyEntryProperties();
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
