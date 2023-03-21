using Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class AsymmetricPKCS11KeyEntryUIFactory : KeyEntryUIFactory
    {
        public AsymmetricPKCS11KeyEntryUIFactory()
        {
            targetFactory = new AsymmetricPKCS11KeyEntryFactory();
        }

        public override string Name => "PKCS#11 Asymmetric Key Entry";

        public override Type GetPropertiesType()
        {
            return typeof(AsymmetricPKCS11KeyEntryProperties);
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
