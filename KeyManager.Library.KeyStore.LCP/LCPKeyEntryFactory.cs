using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.LCP
{
    public class LCPKeyEntryFactory : GenericKeyEntryFactory<LCPKeyEntry, LCPKeyEntryProperties>
    {
        public override string Name => "LCP Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric];
    }
}
