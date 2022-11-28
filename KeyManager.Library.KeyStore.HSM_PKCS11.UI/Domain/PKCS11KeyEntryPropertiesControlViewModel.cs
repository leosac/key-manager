using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain
{
    public class PKCS11KeyEntryPropertiesControlViewModel : KeyEntryPropertiesControlViewModel
    {
        public PKCS11KeyEntryPropertiesControlViewModel()
        {
            _properties = new PKCS11KeyEntryProperties();
        }

        public PKCS11KeyEntryProperties? PKCS11Properties
        {
            get { return Properties as PKCS11KeyEntryProperties; }
        }
    }
}
