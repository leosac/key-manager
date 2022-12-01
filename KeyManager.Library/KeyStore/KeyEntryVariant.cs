using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryVariant : KMObject
    {
        public KeyEntryVariant()
        {
            _name = string.Empty;
            KeyContainers = new ObservableCollection<KeyContainer>();
        }

        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<KeyContainer> KeyContainers { get; set; }
    }
}
