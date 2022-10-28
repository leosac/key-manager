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

        private Key? _wrappingKey;

        public Key? WrappingKey
        {
            get => _wrappingKey;
            set => SetProperty(ref _wrappingKey, value);
        }
    }
}
