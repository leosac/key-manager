using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class SymmetricPKCS11KeyEntryFactory : GenericKeyEntryFactory<SymmetricPKCS11KeyEntry, SymmetricPKCS11KeyEntryProperties>
    {
        public override string Name => "PKCS#11 Symmetric Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Symmetric];
    }
}
