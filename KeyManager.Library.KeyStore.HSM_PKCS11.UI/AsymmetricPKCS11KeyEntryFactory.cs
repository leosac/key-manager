using Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class AsymmetricPKCS11KeyEntryFactory : KeyEntryFactory
    {
        public override string Name => "PKCS#11 Asymmetric Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => new KeyEntryClass[] { KeyEntryClass.Asymmetric };

        public override KeyEntry CreateKeyEntry()
        {
            return new AsymmetricPKCS11KeyEntry();
        }

        public override Type GetKeyEntryPropertiesType()
        {
            return typeof(AsymmetricPKCS11KeyEntryProperties);
        }

        public override KeyEntryProperties CreateKeyEntryProperties()
        {
            return new AsymmetricPKCS11KeyEntryProperties();
        }

        public override UserControl CreateKeyEntryPropertiesControl()
        {
            return new AsymmetricPKCS11KeyEntryPropertiesControl();
        }

        public override KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel()
        {
            return new AsymmetricPKCS11KeyEntryPropertiesControlViewModel();
        }
    }
}
