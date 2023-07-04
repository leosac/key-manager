using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore
{
    /// <summary>
    /// The base class for a Key Store Properties implementation.
    /// </summary>
    public abstract class KeyStoreProperties : KMObject
    {
        private string? _secret;

        /// <summary>
        /// The key store secret. Volatile memory only.
        /// </summary>
        [JsonIgnore]
        public string? Secret
        {
            get => _secret;
            set => SetProperty(ref _secret, value);
        }
    }
}
