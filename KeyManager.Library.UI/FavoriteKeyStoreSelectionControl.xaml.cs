using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Leosac.KeyManager.Library.UI
{
    /// <summary>
    /// Interaction logic for FavoriteSelectionControl.xaml
    /// </summary>
    public partial class FavoriteKeyStoreSelectionControl : UserControl
    {
        public FavoriteKeyStoreSelectionControl()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                Favorites = new Favorites(); 
            else
                Favorites = Favorites.LoadFromFile();

        }

        public Favorites Favorites
        {
            get { return (Favorites)GetValue(FavoritesProperty); }
            set { SetValue(FavoritesProperty, value); }
        }

        public static readonly DependencyProperty FavoritesProperty = DependencyProperty.Register(nameof(Favorites), typeof(Favorites), typeof(FavoriteKeyStoreSelectionControl));

        public Favorite SelectedKeyStoreFavorite
        {
            get { return (Favorite)GetValue(SelectedKeyStoreFavoriteProperty); }
            set { SetValue(SelectedKeyStoreFavoriteProperty, value); }
        }

        public static readonly DependencyProperty SelectedKeyStoreFavoriteProperty = DependencyProperty.Register(nameof(SelectedKeyStoreFavorite), typeof(Favorite), typeof(FavoriteKeyStoreSelectionControl));

        private async void btnNew_Click(object sender, RoutedEventArgs e)
        {
            var model = new KeyStoreSelectorDialogViewModel() { Message = "Save a new Favorite Key Store" };
            var dialog = new KeyStoreSelectorDialog
            {
                DataContext = model
            };

            object? ret = await DialogHost.Show(dialog, "FavSelectionDialog");
            if (ret != null)
            {
                var store = model.CreateKeyStore();
                if (store != null)
                {
                    var fav = Favorites?.CreateFromKeyStore(store);
                    SelectedKeyStoreFavorite = fav;
                }
            }
        }
    }
}
