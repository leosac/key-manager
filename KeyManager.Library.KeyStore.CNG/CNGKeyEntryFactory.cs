using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.CNG
{
    public class CNGKeyEntryFactory : GenericKeyEntryFactory<CNGKeyEntry, CNGKeyEntryProperties>
    {
        public override string Name => "CNG Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric];
    }
}
