using Leosac.KeyManager.Library.KeyStore;
using System.Windows;

namespace Leosac.KeyManager.Library.Plugin.UI
{
    public abstract class WizardFactory : KMFactory<WizardFactory>
    {
        public abstract Window CreateWizardWindow();

        public abstract IList<KeyEntry> GetKeyEntries(Window window);

        public abstract KeyEntryClass[] KeyEntryClasses { get; }
    }
}
