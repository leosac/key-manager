using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using MaterialDesignThemes.Wpf;
using System;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Domain
{
    public class FavoritesControlViewModel : ObservableValidator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public FavoritesControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _snackbarMessageQueue = snackbarMessageQueue;
            RefreshFavoritesCommand = new RelayCommand(
                () =>
                {
                    RefreshFavorites();
                });
            OpenFavoriteCommand = new AsyncRelayCommand<Favorite>(
                async fav =>
                {
                    DialogHost.CloseDialogCommand.Execute(null, null);
                    if (KeyStoreCommand != null)
                    {
                        await KeyStoreCommand.ExecuteAsync(fav);
                    }
                });
            CreateFavoriteCommand = new AsyncRelayCommand(
                async () =>
                {
                    var model = new KeyStoreSelectorDialogViewModel { Message = "Save a new Favorite Key Store" };
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
                    if (fav != null)
                    {
                        DialogHost.CloseDialogCommand.Execute(null, null);

                        Favorites?.KeyStores.Remove(fav);
                        Favorites?.SaveToFile();
                        log.Info(String.Format("Favorite `{0}` removed.", fav.Name));
                    }
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
            ChangeMasterKeyCommand = new RelayCommand(() =>
                {
                    if (!string.IsNullOrEmpty(NewMasterKey))
                    {
                        EncryptJsonConverter.ChangeMasterKey(NewMasterKey);
                        NewMasterKey = null;
                    }
                    else
                    {
                        EncryptJsonConverter.ResetToDefaultMasterKey();
                    }

                    RefreshMasterKeyState();
                    DialogHost.CloseDialogCommand.Execute(null, null);
                    RefreshFavorites();
                });
        }

        public void RefreshMasterKeyState()
        {
            IsDefaultMasterKey = EncryptJsonConverter.IsDefaultMasterKey();
        }

        protected ISnackbarMessageQueue _snackbarMessageQueue;

        private Favorites? _favorites;

        public Favorites? Favorites
        {
            get => _favorites;
            set => SetProperty(ref _favorites, value);
        }

        private bool _isLoadingFavorites;

        public bool IsLoadingFavorites
        {
            get => _isLoadingFavorites;
            set => SetProperty(ref _isLoadingFavorites, value);
        }

        private bool _isDefaultMasterKey;

        public bool IsDefaultMasterKey
        {
            get => _isDefaultMasterKey;
            set => SetProperty(ref _isDefaultMasterKey, value);
        }

        private string? _newMasterKey;

        public string? NewMasterKey
        {
            get => _newMasterKey;
            set => SetProperty(ref _newMasterKey, value);
        }

        public void RefreshFavorites()
        {
            IsLoadingFavorites = true;
            Favorites = Favorites.GetSingletonInstance(true);
            Task.Run(async () =>
            {
                // Just to be sure the animation is visible long enough...
                await Task.Delay(500);
                IsLoadingFavorites = false;
            });
        }

        public RelayCommand? RefreshFavoritesCommand { get; set; }
        public AsyncRelayCommand? CreateFavoriteCommand { get; set; }
        public RelayCommand<Favorite>? RemoveFavoriteCommand { get; set; }
        public AsyncRelayCommand<Favorite>? EditFavoriteCommand { get; }
        public AsyncRelayCommand<Favorite>? OpenFavoriteCommand { get; }
        public RelayCommand? ChangeMasterKeyCommand { get; }

        public AsyncRelayCommand<object>? KeyStoreCommand { private get; set; }
    }
}
