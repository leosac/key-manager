using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
