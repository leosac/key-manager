using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyVersion : KMObject
    {
        public KeyVersion()
        {
            _version = 0;
            _key = new Key();
        }

        public KeyVersion(int version)
        {
            _version = version;
            _key = new Key();
        }

        public KeyVersion(int version, Key key)
        {
            _version = version;
            _key = key;
        }

        private int _version;
        private Key _key;

        public int Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        public Key Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }
    }
}
