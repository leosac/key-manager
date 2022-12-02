using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
