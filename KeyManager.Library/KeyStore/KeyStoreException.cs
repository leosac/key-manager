namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyStoreException : Exception
    {
        public KeyStoreException(string? message) : base(message) { }

        public KeyStoreException(string? message, Exception? exception) : base(message, exception) { }
    }
}
