using Leosac.KeyManager.Library.UI.Domain;
using Leosac.KeyManager.Library.UI.Helpers;
using System.Collections.Generic;
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

            AvailableFavorites = DesignerProperties.GetIsInDesignMode(this) ? new Favorite[0] : FavoritesManager.AvailableKeyStores;
            ConfigureFavoritesView();
        }

        public IEnumerable<Favorite>? AvailableFavorites
        {
            get => (IEnumerable<Favorite>?)GetValue(AvailableFavoritesProperty);
            private set => SetValue(AvailableFavoritesPropertyKey, value);
        }

        private static readonly DependencyPropertyKey AvailableFavoritesPropertyKey = DependencyProperty.RegisterReadOnly(
                nameof(AvailableFavorites),
                typeof(IEnumerable<Favorite>),
                typeof(FavoriteKeyStoreSelectionControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty AvailableFavoritesProperty = AvailableFavoritesPropertyKey.DependencyProperty;

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
            var view = CollectionViewSource.GetDefaultView(AvailableFavorites);
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

            var favorite = FavoritesManager.User?.CreateFromKeyStore(store);
            if (favorite?.IsResolved == true)
            {
                SelectedKeyStoreFavorite = favorite;
                ResolvedFavoritesView?.Refresh();
            }
        }
    }
}
