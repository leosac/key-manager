using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    public abstract class KeyStoreFactory
    {
        public abstract string Name { get; }

        public abstract KeyStore.KeyStore CreateKeyStore();

        public abstract KeyStore.KeyStoreProperties CreateKeyStoreProperties();

        public abstract UserControl CreateKeyStorePropertiesControl();

        public static IList<KeyStoreFactory> RegisteredFactories { get; } = new List<KeyStoreFactory>();
        public static void Register(KeyStoreFactory factory)
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
