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

namespace Leosac.KeyManager.Domain
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            MenuItems = new ObservableCollection<NavItem>(new[]
            {
                new NavItem(
                    "Home",
                    typeof(HomeControl)
                )
            });

            _navItemsView = CollectionViewSource.GetDefaultView(MenuItems);

            HomeCommand = new KeyManagerCommandImplementation(
                _ =>
                {
                    SelectedIndex = 0;
                });

            MovePrevCommand = new KeyManagerCommandImplementation(
                _ =>
                {
                    SelectedIndex--;
                },
                _ => SelectedIndex > 0);

            MoveNextCommand = new KeyManagerCommandImplementation(
               _ =>
               {
                   SelectedIndex++;
               },
               _ => SelectedIndex < MenuItems.Count - 1);
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
        public KeyManagerCommandImplementation MovePrevCommand { get; }
        public KeyManagerCommandImplementation MoveNextCommand { get; }
    }
}
