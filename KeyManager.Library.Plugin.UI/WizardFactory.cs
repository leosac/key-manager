using System.Windows;

namespace Leosac.KeyManager.Library.Plugin.UI
{
    public abstract class WizardFactory : KMFactory<WizardFactory>
    {
        public abstract Window CreateWizardWindow();

        public abstract IList<Leosac.KeyManager.Library.KeyStore.KeyEntry> GetKeyEntries(Window window);
    }
}
