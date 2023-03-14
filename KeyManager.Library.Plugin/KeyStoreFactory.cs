using Leosac.KeyManager.Library.Plugin.Domain;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class KeyStoreFactory : KMFactory<KeyStoreFactory>
    {
        public abstract Leosac.KeyManager.Library.KeyStore.KeyStore CreateKeyStore();

        public abstract Type GetKeyStorePropertiesType();

        public abstract Leosac.KeyManager.Library.KeyStore.KeyStoreProperties CreateKeyStoreProperties();

        public abstract UserControl CreateKeyStorePropertiesControl();

        public abstract KeyStorePropertiesControlViewModel CreateKeyStorePropertiesControlViewModel();

        public abstract IDictionary<string, UserControl> CreateKeyStoreAdditionalControls();

        public static KeyStoreFactory? GetFactoryFromPropertyType(Type? type)
        {
            if (type == null)
                return null;

            lock (RegisteredFactories)
            {
                foreach (var factory in RegisteredFactories)
                {
                    if (factory.GetKeyStorePropertiesType() == type)
                    {
                        return factory;
                    }
                }
            }

            return null;
        }
    }
}
