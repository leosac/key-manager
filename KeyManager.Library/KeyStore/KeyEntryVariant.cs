using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryVariant : ObservableObject
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
