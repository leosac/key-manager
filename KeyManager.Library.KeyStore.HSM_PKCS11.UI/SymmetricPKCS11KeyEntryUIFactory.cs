using Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI.Domain;
using Leosac.KeyManager.Library.Plugin;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11.UI
{
    public class SymmetricPKCS11KeyEntryUIFactory : KeyEntryUIFactory
    {
        public SymmetricPKCS11KeyEntryUIFactory()
        {
            targetFactory = new SymmetricPKCS11KeyEntryFactory();
        }

        public override string Name => "PKCS#11 Symmetric Key Entry";

        public override Type GetPropertiesType()
        {
            return typeof(SymmetricPKCS11KeyEntryProperties);
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
