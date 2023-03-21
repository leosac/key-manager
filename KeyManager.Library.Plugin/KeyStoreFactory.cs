namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class KeyStoreFactory : KMFactory<KeyStoreFactory>
    {
        public abstract Leosac.KeyManager.Library.KeyStore.KeyStore CreateKeyStore();

        public abstract Leosac.KeyManager.Library.KeyStore.KeyStoreProperties CreateKeyStoreProperties();

        public abstract Leosac.KeyManager.Library.KeyStore.KeyStoreProperties? CreateKeyStoreProperties(string serialized);
    }
}
