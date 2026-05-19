using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library.Plugin
{
    /// <summary>
    /// The base class for Key Store Factory implementation.
    /// </summary>
    public abstract class KeyStoreFactory : KMFactory<KeyStoreFactory>
    {
        /// <summary>
        /// Create a new key store instance.
        /// </summary>
        /// <returns>The new key store instance.</returns>
        public abstract Leosac.KeyManager.Library.KeyStore.KeyStore CreateKeyStore();

        /// <summary>
        /// Create a new key store properties instance.
        /// </summary>
        /// <returns>The key store properties instance.</returns>
        public abstract Leosac.KeyManager.Library.KeyStore.KeyStoreProperties CreateKeyStoreProperties();

        /// <summary>
        /// Create a new key store properties instance from a serialized content.
        /// </summary>
        /// <param name="serialized">The serialized key store properties content</param>
        /// <returns>The key store properties instance.</returns>
        public abstract Leosac.KeyManager.Library.KeyStore.KeyStoreProperties? CreateKeyStoreProperties(string serialized);

        /// <summary>
        /// Create a card device associated with the given key store.
        /// </summary>
        /// <param name="keyStore">The key store instance for which to create a card device.</param>
        /// <returns> "ICardDevice" instance if supported; otherwise, null.</returns>
        public virtual Device.ICardDevice? CreateCardDevice(KeyStore.KeyStore keyStore)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Orders the specified key entries according to the target key store publishing requirements.
        /// </summary>
        /// <param name="entries">The collection of key entry identifiers to order.</param>
        /// <param name="keyStore">The key store for which the entries will be published.</param>
        /// <returns>An ordered list of key entry identifiers.</returns>
        public virtual IEnumerable<IChangeKeyEntry> OrderKeyEntries(IList<IChangeKeyEntry> entries, KeyStore.KeyStore keyStore)
        {
            return entries;
        }
    }
}
