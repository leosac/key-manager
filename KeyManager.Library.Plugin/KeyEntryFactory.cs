using Leosac.KeyManager.Library.Plugin.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class KeyEntryFactory : KMFactory<KeyEntryFactory>
    {
        public abstract IEnumerable<Leosac.KeyManager.Library.KeyStore.KeyEntryClass> KClasses { get; }

        public abstract Leosac.KeyManager.Library.KeyStore.KeyEntry CreateKeyEntry();

        public abstract Type GetKeyEntryPropertiesType();

        public abstract Leosac.KeyManager.Library.KeyStore.KeyEntryProperties CreateKeyEntryProperties();

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
