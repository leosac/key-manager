using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public abstract class KeyEntry : KMObject
    {
        public KeyEntry()
        {
            _identifier = new Guid().ToString();
            KeyVersions = new ObservableCollection<KeyVersion>();
        }

        private string _identifier;

        private KeyEntryProperties? _properties;

        public abstract string Name { get; }

        public ObservableCollection<KeyVersion> KeyVersions { get; set; }

        public string Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
        }

        public KeyEntryProperties? Properties
        {
            get => _properties;
            set => SetProperty(ref _properties, value);
        }
    }
}
