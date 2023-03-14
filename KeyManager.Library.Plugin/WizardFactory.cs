using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class WizardFactory : KMFactory<WizardFactory>
    {
        public abstract Window CreateWizardWindow();

        public abstract IList<Leosac.KeyManager.Library.KeyStore.KeyEntry> GetKeyEntries(Window window);
    }
}
