using Leosac.KeyManager.Library.Plugin.UI.Domain;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI.Domain
{
    public class MemoryKeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public MemoryKeyEntryPropertiesControlViewModel()
        {
            _properties = new MemoryKeyEntryProperties();
        }

        public MemoryKeyEntryProperties? MemoryProperties
        {
            get { return Properties as MemoryKeyEntryProperties; }
        }
    }
}
