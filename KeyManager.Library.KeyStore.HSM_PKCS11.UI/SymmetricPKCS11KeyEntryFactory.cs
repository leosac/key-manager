using Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class SymmetricPKCS11KeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "PKCS#11 Symmetric Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => new KeyEntryClass[] { KeyEntryClass.Symmetric };

        public override KeyEntry CreateKeyEntry()
        {
            return new SymmetricPKCS11KeyEntry();
        }

        public override Type GetKeyEntryPropertiesType()
        {
            return typeof(SymmetricPKCS11KeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new SymmetricPKCS11KeyEntryProperties();
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new SymmetricPKCS11KeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new SymmetricPKCS11KeyEntryPropertiesControlViewModel();
        }
    }
}
