using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
