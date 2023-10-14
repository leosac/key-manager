using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore
{
    /// <summary>
    /// The base class for a Key Store Properties implementation.
    /// </summary>
    public abstract class KeyStoreProperties : ObservableValidator
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

        [JsonIgnore]
        public virtual int? SecretMaxLength => null;

        private bool _storeSecret;
        public bool StoreSecret
        {
            get => _storeSecret;
            set => SetProperty(ref _storeSecret, value);
        }

        [JsonConverter(typeof(EncryptJsonConverter))]
        public string? StoredSecret
        {
            get => _storeSecret ? _secret : null;
            set => SetProperty(ref _secret, value);
        }
    }
}
