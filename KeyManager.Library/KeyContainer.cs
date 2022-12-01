using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public class KeyContainer : KMObject
    {
        public KeyContainer(string? name = null, Key? key = null)
        {
            _name = name ?? "Key Container";
            _key = key ?? new Key();
        }

        private string _name;
        private Key _key;

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
