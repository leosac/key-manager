using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyVersion : KMObject
    {
        public KeyVersion()
        {
            _name = "Key Version";
            _version = 0;
            _key = new Key();
        }

        public KeyVersion(string name, byte version)
        {
            _name = name;
            _version = version;
            _key = new Key();
        }

        public KeyVersion(string name, byte version, Key key)
        {
            _name = name;
            _version = version;
            _key = key;
        }

        private string _name;
        private byte _version;
        private Key _key;

        public byte Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        public Key Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
