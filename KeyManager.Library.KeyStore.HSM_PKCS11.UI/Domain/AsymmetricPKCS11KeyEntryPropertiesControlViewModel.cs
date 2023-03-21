namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain
{
    public class AsymmetricPKCS11KeyEntryPropertiesControlViewModel : PKCS11KeyEntryPropertiesControlViewModel
    {
        public AsymmetricPKCS11KeyEntryPropertiesControlViewModel()
        {
            _properties = new AsymmetricPKCS11KeyEntryProperties();
        }

        public AsymmetricPKCS11KeyEntryProperties? AsymmetricPKCS11Properties
        {
            get { return Properties as AsymmetricPKCS11KeyEntryProperties; }
        }
    }
}
