namespace Leosac.KeyManager.Library.Plugin
{
    public abstract class KeyEntryFactory : KMFactory<KeyEntryFactory>
    {
        public abstract IEnumerable<Leosac.KeyManager.Library.KeyStore.KeyEntryClass> KClasses { get; }

        public abstract Leosac.KeyManager.Library.KeyStore.KeyEntry CreateKeyEntry();

        public abstract Leosac.KeyManager.Library.KeyStore.KeyEntryProperties CreateKeyEntryProperties();

        public abstract Leosac.KeyManager.Library.KeyStore.KeyEntryProperties? CreateKeyEntryProperties(string serialized);
    }
}
