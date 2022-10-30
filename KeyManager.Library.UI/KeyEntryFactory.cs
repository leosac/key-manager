using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    public abstract class KeyEntryFactory
    {
        public abstract string Name { get; }

        public abstract KeyEntry CreateKeyEntry();

        public abstract KeyEntryProperties CreateKeyEntryProperties();

        public abstract UserControl CreateKeyEntryPropertiesControl();

        public static IList<KeyEntryFactory> RegisteredFactories { get; } = new List<KeyEntryFactory>();
        public static void Register(KeyEntryFactory factory)
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
