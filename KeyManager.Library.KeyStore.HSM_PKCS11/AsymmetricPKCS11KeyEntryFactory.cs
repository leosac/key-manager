using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class AsymmetricPKCS11KeyEntryFactory : GenericKeyEntryFactory<AsymmetricPKCS11KeyEntry, AsymmetricPKCS11KeyEntryProperties>
    {
        public override string Name => "PKCS#11 Asymmetric Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Asymmetric];
    }
}
