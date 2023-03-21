using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;

namespace Leosac.KeyManager.Domain
{
    public class FavoritesControlViewModel : ViewModelBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public FavoritesControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _favorites = Favorites.GetSingletonInstance();
            RefreshFavoritesCommand = new KeyManagerCommand(
                parameter =>
                {
                    Favorites = Favorites.GetSingletonInstance(true);
                });
            CreateFavoriteCommand = new KeyManagerAsyncCommand<object>(async
                parameter =>
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
            RemoveFavoriteCommand = new KeyManagerCommand(
                parameter =>
                {
                    if (parameter is Favorite favorite)
                    {
                        Favorites?.KeyStores.Remove(favorite);
                        Favorites?.SaveToFile();
                        log.Info(String.Format("Favorite `{0}` removed.", favorite.Name));
                    }
                });
        }

        private Favorites? _favorites;

        public Favorites? Favorites
        {
            get => _favorites;
            set => SetProperty(ref _favorites, value);
        }

        public KeyManagerCommand? RefreshFavoritesCommand { get; set; }
        public KeyManagerAsyncCommand<object>? CreateFavoriteCommand { get; set; }
        public KeyManagerCommand? RemoveFavoriteCommand { get; set; }
        public KeyManagerCommand? KeyStoreCommand { get; set; }
    }
}
