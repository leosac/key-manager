namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class SymmetricPKCS11KeyEntry : PKCS11KeyEntry
    {
        public SymmetricPKCS11KeyEntry()
        {
            Properties = new SymmetricPKCS11KeyEntryProperties();
        }

        public override KeyEntryClass KClass => KeyEntryClass.Symmetric;
    }
}
