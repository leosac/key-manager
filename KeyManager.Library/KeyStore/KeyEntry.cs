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
            _identifier = Guid.NewGuid().ToString();
            KeyVersions = new ObservableCollection<KeyVersion>();
        }

        private string _identifier;
        private string? _label;
        private KeyEntryProperties? _properties;

        public ObservableCollection<KeyVersion> KeyVersions { get; set; }

        public string Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
        }

        public string? Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public KeyEntryProperties? Properties
        {
            get => _properties;
            set => SetProperty(ref _properties, value);
        }
    }
}
