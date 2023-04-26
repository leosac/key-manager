using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore
{
    public abstract class KeyStoreProperties : KMObject
    {
        public KeyStoreProperties()
        {

        }

        private string? _secret;

        [JsonIgnore]
        public string? Secret
        {
            get => _secret;
            set => SetProperty(ref _secret, value);
        }
    }
}
