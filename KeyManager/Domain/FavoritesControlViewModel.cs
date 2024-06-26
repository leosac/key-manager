using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Data;

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
            ImportFavoritesCommand = new RelayCommand(
                () =>
                {
                    ImportFavorites();
                });
            ExportFavoritesCommand = new RelayCommand(
                () =>
                {
                    ExportFavorites();
                });
            OpenFavoriteCommand = new AsyncRelayCommand<Favorite>(
                async fav =>
                {
                    DialogHost.CloseDialogCommand.Execute(null, null);
                    if (KeyStoreCommand != null)
                    {
                        await KeyStoreCommand.ExecuteAsync(fav);
                        SearchTerms = string.Empty;
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
                        log.Info(string.Format("Favorite `{0}` removed.", fav.Name));
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

        private string? _searchTerms;

        public string? SearchTerms
        {
            get => _searchTerms;
            set
            {
                if (SetProperty(ref _searchTerms, value))
                {
                    RefreshFavoritesView();
                }
            }
        }

        public void RefreshFavorites()
        {
            IsLoadingFavorites = true;
            SearchTerms = string.Empty;
            Favorites = Favorites.GetSingletonInstance(true);
            Task.Run(async () =>
            {
                // Just to be sure the animation is visible long enough...
                await Task.Delay(500);
                IsLoadingFavorites = false;
            });
            if (Favorites != null)
            {
                FavoritesView = CollectionViewSource.GetDefaultView(Favorites.KeyStores);
                FavoritesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                FavoritesView.Filter = FavoritesFilter;
            }
        }

        public void RefreshFavoritesView()
        {
            FavoritesView?.Refresh();
        }

        private ICollectionView? _favoritesView;
        public ICollectionView? FavoritesView
        {
            get => _favoritesView;
            set => SetProperty(ref _favoritesView, value);
        }

        protected void ImportFavorites()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "JSON Files (*.json)|*.json";
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var favorites = Favorites.LoadFromFile(ofd.FileName);
                    if (favorites != null)
                    {
                        favorites.SaveToFile();
                        RefreshFavorites();
                    }
                }
                catch(Exception ex)
                {
                    SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                }
            }
        }

        protected void ExportFavorites()
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "JSON Files (*.json)|*.json";
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    Favorites.GetSingletonInstance()?.SaveToFile(sfd.FileName);
                }
                catch (Exception ex)
                {
                    SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
                }
            }
        }

        private bool FavoritesFilter(object obj)
        {
            if (string.IsNullOrWhiteSpace(_searchTerms))
            {
                return true;
            }

            if (obj is Favorite item)
            {
                var terms = _searchTerms.ToLowerInvariant();

                if (item.Name != null && item.Name.ToLowerInvariant().Contains(terms))
                {
                    return true;
                }
            }

            return false;
        }

        public RelayCommand? RefreshFavoritesCommand { get; set; }
        public RelayCommand? ImportFavoritesCommand { get; set; }
        public RelayCommand? ExportFavoritesCommand { get; set; }
        public AsyncRelayCommand? CreateFavoriteCommand { get; set; }
        public RelayCommand<Favorite>? RemoveFavoriteCommand { get; set; }
        public AsyncRelayCommand<Favorite>? EditFavoriteCommand { get; }
        public AsyncRelayCommand<Favorite>? OpenFavoriteCommand { get; }
        public RelayCommand? ChangeMasterKeyCommand { get; }
        public AsyncRelayCommand<object>? KeyStoreCommand { private get; set; }
    }
}
