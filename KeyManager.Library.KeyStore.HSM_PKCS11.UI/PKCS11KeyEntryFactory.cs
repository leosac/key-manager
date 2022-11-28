using Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class PKCS11KeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "PKCS#11 Key Entry";

        public override KeyEntry CreateKeyEntry()
        {
            return new PKCS11KeyEntry();
        }

        public override Type GetKeyEntryPropertiesType()
        {
            return typeof(PKCS11KeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new PKCS11KeyEntryProperties();
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new PKCS11KeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new PKCS11KeyEntryPropertiesControlViewModel();
        }
    }
}
