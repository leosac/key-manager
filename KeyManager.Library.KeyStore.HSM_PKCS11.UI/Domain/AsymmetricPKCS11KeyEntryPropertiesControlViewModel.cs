using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
