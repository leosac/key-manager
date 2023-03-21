using Leosac.KeyManager.Library.Plugin;
using Newtonsoft.Json;

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

        public override Type GetPropertiesType()
        {
            return typeof(MemoryKeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new MemoryKeyEntryProperties();
        }

        public override KeyEntryProperties? CreateKeyEntryProperties(string serialized)
        {
            return JsonConvert.DeserializeObject<MemoryKeyEntryProperties>(serialized);
        }
    }
}
