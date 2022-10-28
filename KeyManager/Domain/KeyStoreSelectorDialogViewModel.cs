using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Domain
{
    public class KeyStoreSelectorDialogViewModel : ViewModelBase
    {
        public KeyStoreSelectorDialogViewModel()
        {
            KeyStores = new ObservableCollection<NavItem>(new[]
            {
                new NavItem(
                    "File Key Store",
                    typeof(KeyStore.FileKeyStore)
                )
            });
        }

        private NavItem? _selectedItem;
        private int _selectedIndex;
        private string? _message;

        public ObservableCollection<NavItem> KeyStores { get; }

        public NavItem? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
    }
}
