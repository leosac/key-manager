using Leosac.KeyManager.Library.UI.Domain;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

            Favorites = DesignerProperties.GetIsInDesignMode(this) ? new Favorites() : Favorites.GetSingletonInstance();
        }

        public Favorites? Favorites
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

        private async void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            var model = new KeyStoreSelectorDialogViewModel { Message = "Save a new Favorite Key Store" };
            var dialog = new KeyStoreSelectorDialog
            {
                DataContext = model
            };

            object? ret = await DialogHelper.ForceShow(dialog, "FavSelectionDialog");
            if (ret != null)
            {
                var store = model.CreateKeyStore();
                if (store != null)
                {
                    var fav = Favorites?.CreateFromKeyStore(store);
                    if (fav != null)
                    {
                        SelectedKeyStoreFavorite = fav;
                    }
                }
            }
        }
    }
}
