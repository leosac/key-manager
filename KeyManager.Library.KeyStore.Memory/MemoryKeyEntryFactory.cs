using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.Memory.UI
{
    public class MemoryKeyEntryFactory : GenericKeyEntryFactory<MemoryKeyEntry, MemoryKeyEntryProperties>
    {
        public override string Name => "Memory Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric];
    }
}
