using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Leosac.KeyManager.Library.UI
{
    public abstract class WizardFactory
    {
        public abstract string Name { get; }

        public abstract Window CreateWizardWindow();

        public abstract IList<KeyStore.KeyEntry> GetKeyEntries(Window window);

        public static IList<WizardFactory> RegisteredFactories { get; } = new List<WizardFactory>();
        public static void Register(WizardFactory factory)
        {
            lock (RegisteredFactories)
            {
                if (!RegisteredFactories.Contains(factory))
                {
                    RegisteredFactories.Add(factory);
                }
            }
        }
    }
}
