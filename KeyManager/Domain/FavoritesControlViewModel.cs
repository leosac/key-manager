using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp.Domain;
using MaterialDesignThemes.Wpf;
using System;

namespace Leosac.KeyManager.Domain
{
    public class FavoritesControlViewModel : ObservableValidator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public FavoritesControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _favorites = Favorites.GetSingletonInstance();
            RefreshFavoritesCommand = new RelayCommand(
                () =>
                {
                    RefreshFavorites();
                });
            CreateFavoriteCommand = new AsyncRelayCommand(
                async () =>
                {
                    var model = new KeyStoreSelectorDialogViewModel() { Message = "Save a new Favorite Key Store" };
                    var dialog = new KeyStoreSelectorDialog
                    {
                        DataContext = model
                    };

                    object? ret = await DialogHost.Show(dialog, "RootDialog");
                    if (ret != null)
                    {
                        var store = model.CreateKeyStore();
                        if (store != null)
                        {
                            Favorites?.CreateFromKeyStore(store);
                        }
                    }
                });
            RemoveFavoriteCommand = new RelayCommand<Favorite>(
                fav =>
                {
                    Favorites?.KeyStores.Remove(fav);
                    Favorites?.SaveToFile();
                    log.Info(String.Format("Favorite `{0}` removed.", fav.Name));
                });
            EditFavoriteCommand = new AsyncRelayCommand<Favorite>(
                async fav =>
                {
                    if (Favorites != null && fav != null)
                    {
                        await EditKeyStoreControlViewModel.EditFavorite(Favorites, fav);
                        RefreshFavorites();
                    }
                });
        }

        private Favorites? _favorites;

        public Favorites? Favorites
        {
            get => _favorites;
            set => SetProperty(ref _favorites, value);
        }

        public void RefreshFavorites()
        {
            Favorites = Favorites.GetSingletonInstance(true);
        }

        public RelayCommand? RefreshFavoritesCommand { get; set; }
        public AsyncRelayCommand? CreateFavoriteCommand { get; set; }
        public RelayCommand<Favorite>? RemoveFavoriteCommand { get; set; }
        public AsyncRelayCommand<Favorite>? EditFavoriteCommand { get; }
        public AsyncRelayCommand<object>? KeyStoreCommand { get; set; }
    }
}
