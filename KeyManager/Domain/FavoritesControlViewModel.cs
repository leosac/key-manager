using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Leosac.KeyManager.Domain
{
    public class FavoritesControlViewModel : ObservableValidator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        protected ISnackbarMessageQueue _snackbarMessageQueue;

        public FavoritesControlViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _snackbarMessageQueue = snackbarMessageQueue;

            RefreshFavoritesCommand = new RelayCommand(RefreshFavorites);
            ImportFavoritesCommand = new RelayCommand(ImportFavorites);
            ExportFavoritesCommand = new RelayCommand(ExportFavorites);
            ChangeMasterKeyCommand = new RelayCommand(ChangeMasterKey);

            OpenFavoriteCommand = new AsyncRelayCommand<Favorite>(OpenFavoriteAsync);
            CreateFavoriteCommand = new AsyncRelayCommand(CreateFavoriteAsync);
            EditFavoriteCommand = new AsyncRelayCommand<Favorite>(EditFavoriteAsync);

            RemoveFavoriteCommand = new RelayCommand<Favorite>(RemoveFavorite, fav => fav != null);
        }

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

        private bool _displayMasterKey;
        public bool DisplayMasterKey
        {
            get => _displayMasterKey;
            set => SetProperty(ref _displayMasterKey, value);
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

        private ICollectionView? _favoritesView;
        public ICollectionView? FavoritesView
        {
            get => _favoritesView;
            set => SetProperty(ref _favoritesView, value);
        }

        public RelayCommand RefreshFavoritesCommand { get; }
        public RelayCommand ImportFavoritesCommand { get; }
        public RelayCommand ExportFavoritesCommand { get; }
        public AsyncRelayCommand CreateFavoriteCommand { get; }
        public RelayCommand<Favorite> RemoveFavoriteCommand { get; }
        public AsyncRelayCommand<Favorite> EditFavoriteCommand { get; }
        public AsyncRelayCommand<Favorite> OpenFavoriteCommand { get; }
        public RelayCommand ChangeMasterKeyCommand { get; }

        public AsyncRelayCommand<object>? KeyStoreCommand { private get; set; }

        public void RefreshMasterKeyState()
        {
            DisplayMasterKey = (EncryptJsonConverter.EncryptionType == StoredSecretEncryptionType.CustomKey);
            IsDefaultMasterKey = EncryptJsonConverter.IsDefaultMasterKey();
        }

        public void RefreshFavorites()
        {
            IsLoadingFavorites = true;
            SearchTerms = string.Empty;
            Favorites = Favorites.GetSingletonInstance(true);
            ConfigureFavoritesView();
            _ = LoadingAnimationAsync();
        }

        private async Task LoadingAnimationAsync()
        {
            await Task.Delay(500);
            IsLoadingFavorites = false;
        }

        private void ConfigureFavoritesView()
        {
            if (Favorites == null)
            {
                FavoritesView = null;
                return;
            }
            FavoritesView = CollectionViewSource.GetDefaultView(Favorites.KeyStores);
            FavoritesView.SortDescriptions.Clear();
            FavoritesView.SortDescriptions.Add(new SortDescription(nameof(Favorite.Name), ListSortDirection.Ascending));
            FavoritesView.Filter = FavoritesFilter;
        }

        public void RefreshFavoritesView()
        {
            FavoritesView?.Refresh();
        }

        protected void ImportFavorites()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                CheckFileExists = true
            };

            if (ofd.ShowDialog() != true)
                return;

            try
            {
                var imported = Favorites.LoadSafeFromFile(ofd.FileName);
                var current = Favorites.GetSingletonInstance();

                if (imported == null || current == null)
                    return;

                current.ReplaceAll(imported.KeyStores);
                RefreshFavorites();
                SnackbarHelper.EnqueueMessage(_snackbarMessageQueue, "Favorites imported successfully.");
            }
            catch (Exception ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
            }
        }

        protected void ExportFavorites()
        {
            var sfd = new SaveFileDialog { Filter = "JSON Files (*.json)|*.json" };
            if (sfd.ShowDialog() != true)
                return;
            try
            {
                Favorites.GetSingletonInstance()?.SaveToFile(sfd.FileName);
                SnackbarHelper.EnqueueMessage(_snackbarMessageQueue, "Favorites exported successfully.");
            }
            catch (Exception ex)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, ex);
            }
        }

        private bool FavoritesFilter(object obj)
        {
            return string.IsNullOrWhiteSpace(_searchTerms)
                || obj is Favorite item && item.Name?.Contains(_searchTerms, StringComparison.OrdinalIgnoreCase) == true;
        }

        private async Task OpenFavoriteAsync(Favorite? fav)
        {
            if (fav == null || !fav.IsResolved)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, "This favorite requires a missing module and cannot be opened.");
                return;
            }

            DialogHost.CloseDialogCommand.Execute(null, null);
            if (KeyStoreCommand != null)
            {
                await KeyStoreCommand.ExecuteAsync(fav);
                SearchTerms = string.Empty;
            }
        }

        private async Task CreateFavoriteAsync()
        {
            var model = new KeyStoreSelectorDialogViewModel { Message = "Save a new Favorite Key Store" };

            var dialog = new KeyStoreSelectorDialog { DataContext = model };

            object? ret = await DialogHost.Show(dialog, "RootDialog");
            if (ret == null)
                return;
            var store = model.CreateKeyStore();
            if (store == null)
                return;
            Favorites?.CreateFromKeyStore(store);
        }

        private void RemoveFavorite(Favorite? fav)
        {
            if (fav == null || Favorites == null)
                return;
            DialogHost.CloseDialogCommand.Execute(null, null);
            if (!Favorites.Remove(fav))
                return;
            log.Info($"Favorite '{fav.Name}' removed.");
        }

        private async Task EditFavoriteAsync(Favorite? fav)
        {
            if (Favorites == null || fav == null || !fav.IsResolved)
            {
                SnackbarHelper.EnqueueError(_snackbarMessageQueue, "This favorite requires a missing module and cannot be opened.");
                return;
            }
            await EditKeyStoreControlViewModel.EditFavorite(Favorites, fav);
            RefreshFavorites();
        }

        private void ChangeMasterKey()
        {
            if (!string.IsNullOrWhiteSpace(NewMasterKey))
            {
                EncryptJsonConverter.ChangeEncryption(StoredSecretEncryptionType.CustomKey, NewMasterKey);
                NewMasterKey = null;
            }
            else
                EncryptJsonConverter.ResetToDefaultMasterKey();
            RefreshMasterKeyState();
            DialogHost.CloseDialogCommand.Execute(null, null);
            RefreshFavorites();
        }
    }
}
