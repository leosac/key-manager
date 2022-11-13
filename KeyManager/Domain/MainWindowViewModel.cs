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
using System.Windows;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library;

namespace Leosac.KeyManager.Domain
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            HomeCommand = new KeyManagerCommand(
                _ =>
                {
                    SelectedIndex = 0;
                });
            FavoritesCommand = new KeyManagerCommand(
                _ =>
                {
                    SelectedIndex = 1;
                });
            KeyStoreCommand = new KeyManagerCommand(
                parameter =>
                {
                    if (parameter != null)
                    {
                        KeyStore? ks = null;
                        Favorite? fav = null;
                        if (parameter is KeyStore)
                        {
                            ks = parameter as KeyStore;
                        }
                        else if(parameter is Favorite)
                        {
                            fav = parameter as Favorite;
                            var factory = KeyStoreFactory.GetFactoryFromPropertyType(fav!.Properties!.GetType());
                            if (factory != null)
                            {
                                ks = factory.CreateKeyStore();
                                ks.Properties = fav.Properties;
                            }
                        }

                        if (ks != null)
                        {
                            try
                            {
                                ks?.Open();

                                SelectedIndex = 2;
                                var editModel = _selectedItem?.DataContext as EditKeyStoreControlViewModel;
                                if (editModel != null)
                                {
                                    editModel.KeyStore = ks;
                                    editModel.Favorite = fav;
                                    editModel.RefreshKeyEntries();
                                }
                            }
                            catch (KeyStoreException ex)
                            {
                                SnackbarHelper.EnqueueError(snackbarMessageQueue, ex, "Key Store Error");
                            }
                            catch (Exception ex)
                            {
                                SnackbarHelper.EnqueueError(snackbarMessageQueue, ex);
                            }
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
                    "Star",
                    new FavoritesControlViewModel(snackbarMessageQueue)
                    {
                        KeyStoreCommand = KeyStoreCommand
                    }
                ),
                new NavItem(
                    "Current Key Store",
                    typeof(EditKeyStoreControl),
                    "ShieldKeyOutline",
                    new EditKeyStoreControlViewModel(snackbarMessageQueue)
                    {
                        HomeCommand = HomeCommand
                    }
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

        public KeyManagerCommand HomeCommand { get; }
        public KeyManagerCommand FavoritesCommand { get; }
        public KeyManagerCommand KeyStoreCommand { get; }
    }
}
