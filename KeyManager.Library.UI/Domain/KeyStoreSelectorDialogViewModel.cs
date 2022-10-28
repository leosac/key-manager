using Leosac.KeyManager.Library.UI;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyStoreSelectorDialogViewModel : ViewModelBase
    {
        public KeyStoreSelectorDialogViewModel()
        {
            KeyStores = new ObservableCollection<KeyStoreItem>();
            foreach (var factory in KeyStoreFactory.RegisteredFactories)
            {
                KeyStores.Add(new KeyStoreItem(factory));
            }
        }

        private KeyStoreItem? _selectedItem;
        private int _selectedIndex;
        private string? _message;

        public ObservableCollection<KeyStoreItem> KeyStores { get; }

        public KeyStoreItem? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        public string? Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
    }
}
