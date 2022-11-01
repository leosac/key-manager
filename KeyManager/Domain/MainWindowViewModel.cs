using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf.Transitions;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Domain
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            HomeCommand = new KeyManagerCommandImplementation(
                _ =>
                {
                    SelectedIndex = 0;
                });
            FavoritesCommand = new KeyManagerCommandImplementation(
                _ =>
                {
                    SelectedIndex = 1;
                });
            KeyStoreCommand = new KeyManagerCommandImplementation(
                newKeyStore =>
                {
                    SelectedIndex = 2;
                    var editModel = _selectedItem?.DataContext as EditKeyStoreControlViewModel;
                    if (editModel != null)
                    {
                        if (newKeyStore != null)
                        {
                            editModel.KeyStore = newKeyStore as KeyStore;
                            editModel.KeyStore?.Open();
                            editModel.RefreshKeyEntries();
                        }
                    }
                });

            MenuItems = new ObservableCollection<NavItem>(new[]
            {
                new NavItem(
                    "Home",
                    typeof(HomeControl),
                    "House",
                    new HomeControlViewModel(snackbarMessageQueue)
                    {
                        KeyStoreCommand = KeyStoreCommand,
                        FavoritesCommand = FavoritesCommand
                    }
                ),
                new NavItem(
                    "Favorites",
                    typeof(FavoritesControl),
                    "Star"
                ),
                new NavItem(
                    "Current Key Store",
                    typeof(EditKeyStoreControl),
                    "ShieldKeyOutline",
                    new EditKeyStoreControlViewModel(snackbarMessageQueue)
                )
            });
            SelectedItem = MenuItems[0];
            SelectedIndex = 0;

            _navItemsView = CollectionViewSource.GetDefaultView(MenuItems);
        }

        private readonly ICollectionView _navItemsView;
        private NavItem? _selectedItem;
        private int _selectedIndex;

        public ObservableCollection<NavItem> MenuItems { get; }

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

        public KeyManagerCommandImplementation HomeCommand { get; }
        public KeyManagerCommandImplementation FavoritesCommand { get; }
        public KeyManagerCommandImplementation KeyStoreCommand { get; }
    }
}
