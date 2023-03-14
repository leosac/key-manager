using Leosac.KeyManager.Library.Plugin.Domain;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain
{
    public abstract class PKCS11KeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public PKCS11KeyEntryProperties? PKCS11Properties
        {
            get { return Properties as PKCS11KeyEntryProperties; }
        }
    }
}
