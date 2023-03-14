using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI
{
    public abstract class KeyEntryFactory : KMFactory<KeyEntryFactory>
    {
        public abstract IEnumerable<KeyEntryClass> KClasses { get; }

        public abstract KeyEntry CreateKeyEntry();

        public abstract Type GetKeyEntryPropertiesType();

        public abstract KeyEntryProperties CreateKeyEntryProperties();

        public abstract UserControl CreateKeyEntryPropertiesControl();

        public abstract KeyEntryPropertiesControlViewModel CreateKeyEntryPropertiesControlViewModel();

        public static KeyEntryFactory? GetFactoryFromPropertyType(Type? type)
        {
            if (type == null)
                return null;

            lock (RegisteredFactories)
            {
                foreach (var factory in RegisteredFactories)
                {
                    if (factory.GetKeyEntryPropertiesType() == type)
                    {
                        return factory;
                    }
                }
            }

            return null;
        }
    }
}
