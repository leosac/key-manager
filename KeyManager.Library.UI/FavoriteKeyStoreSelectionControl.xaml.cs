using Leosac.KeyManager.Library.UI.Domain;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

            ConfigureFavoritesView();
        }

        public Favorites? Favorites
        {
            get => (Favorites?)GetValue(FavoritesProperty);
            set => SetValue(FavoritesProperty, value);
        }

        public static readonly DependencyProperty FavoritesProperty = DependencyProperty.Register(nameof(Favorites), typeof(Favorites), typeof(FavoriteKeyStoreSelectionControl), new PropertyMetadata(null, OnFavoritesChanged));

        public Favorite? SelectedKeyStoreFavorite
        {
            get => (Favorite?)GetValue(SelectedKeyStoreFavoriteProperty);
            set => SetValue(SelectedKeyStoreFavoriteProperty, value);
        }

        public static readonly DependencyProperty SelectedKeyStoreFavoriteProperty = DependencyProperty.Register(nameof(SelectedKeyStoreFavorite), typeof(Favorite), typeof(FavoriteKeyStoreSelectionControl));

        public ICollectionView? ResolvedFavoritesView
        {
            get => (ICollectionView?)GetValue(ResolvedFavoritesViewProperty);
            private set => SetValue(ResolvedFavoritesViewPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ResolvedFavoritesViewPropertyKey = DependencyProperty.RegisterReadOnly(
                nameof(ResolvedFavoritesView),
                typeof(ICollectionView),
                typeof(FavoriteKeyStoreSelectionControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ResolvedFavoritesViewProperty = ResolvedFavoritesViewPropertyKey.DependencyProperty;

        private static void OnFavoritesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FavoriteKeyStoreSelectionControl control)
                control.ConfigureFavoritesView();
        }

        private void ConfigureFavoritesView()
        {
            if (Favorites == null)
            {
                ResolvedFavoritesView = null;
                return;
            }
            var view = CollectionViewSource.GetDefaultView(Favorites.KeyStores);
            view.Filter = static obj => obj is Favorite fav && fav.IsResolved;
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(nameof(Favorite.Name), ListSortDirection.Ascending));
            ResolvedFavoritesView = view;
        }

        private async void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            var model = new KeyStoreSelectorDialogViewModel { Message = "Save a new Favorite Key Store" };
            var dialog = new KeyStoreSelectorDialog { DataContext = model };

            object? ret = await DialogHelper.ForceShow(dialog, "FavSelectionDialog");
            if (ret == null)
                return;

            var store = model.CreateKeyStore();
            if (store == null)
                return;

            var favorite = Favorites?.CreateFromKeyStore(store);
            if (favorite?.IsResolved == true)
            {
                SelectedKeyStoreFavorite = favorite;
                ResolvedFavoritesView?.Refresh();
            }
        }
    }
}
