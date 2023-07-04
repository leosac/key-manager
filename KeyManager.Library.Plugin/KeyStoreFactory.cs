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
    }
}
