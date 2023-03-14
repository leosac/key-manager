using Leosac.KeyManager.Library.Plugin.Domain;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain
{
    public class SymmetricPKCS11KeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public SymmetricPKCS11KeyEntryPropertiesControlViewModel()
        {
            _properties = new SymmetricPKCS11KeyEntryProperties();
        }

        public SymmetricPKCS11KeyEntryProperties? SymmetricPKCS11Properties
        {
            get { return Properties as SymmetricPKCS11KeyEntryProperties; }
        }
    }
}
