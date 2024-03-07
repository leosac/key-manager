namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class AsymmetricPKCS11KeyEntry : PKCS11KeyEntry
    {
        public AsymmetricPKCS11KeyEntry() : this(KeyEntryClass.Asymmetric)
        {

        }

        public AsymmetricPKCS11KeyEntry(KeyEntryClass kclass)
        {
            Properties = new AsymmetricPKCS11KeyEntryProperties();
            _kclass = kclass;
        }

        private readonly KeyEntryClass _kclass;

        public override KeyEntryClass KClass => _kclass;
    }
}
