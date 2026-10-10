using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.KeyStore;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntrySearchDialogViewModel : ObservableObject
    {
        private readonly string? _keyStoreFavorite;
        private string? _searchTerms;
        private KeyEntryId? _selectedKeyEntryId;
        private string? _error;
        private bool _isLoaded;

        public KeyEntrySearchDialogViewModel(string? keyStoreFavorite)
        {
            _keyStoreFavorite = keyStoreFavorite;
            Identifiers = new ObservableCollection<KeyEntryId>();
        }

        public ObservableCollection<KeyEntryId> Identifiers { get; }

        public string? SearchTerms
        {
            get => _searchTerms;
            set
            {
                if (SetProperty(ref _searchTerms, value))
                {
                    if (_isLoaded)
                    {
                        _ = SearchAsync();
                    }
                }
            }
        }

        public KeyEntryId? SelectedKeyEntryId
        {
            get => _selectedKeyEntryId;
            set => SetProperty(ref _selectedKeyEntryId, value);
        }

        public string? Error
        {
            get => _error;
            private set => SetProperty(ref _error, value);
        }

        public async Task LoadAsync()
        {
            Error = null;
            if (string.IsNullOrEmpty(_keyStoreFavorite))
            {
                Error = "Select a favorite key store first.";
                return;
            }

            await SearchAsync();
            _isLoaded = true;
        }

        private async Task SearchAsync(KeyStore.KeyStore? existingKeyStore = null)
        {
            var keyStore = existingKeyStore;
            var closeKeyStore = false;
            try
            {
                if (keyStore == null)
                {
                    var favorite = FavoritesManager.Get(_keyStoreFavorite!);
                    keyStore = favorite?.CreateKeyStore();
                    if (keyStore == null)
                    {
                        Error = "Cannot create the selected key store.";
                        return;
                    }

                    await keyStore.Open();
                    closeKeyStore = true;
                }

                var identifiers = await keyStore.Search(SearchTerms);
                Identifiers.Clear();
                foreach (var identifier in identifiers)
                {
                    Identifiers.Add(identifier);
                }
                Error = null;
            }
            catch (KeyStoreException ex)
            {
                Error = ex.Message;
            }
            catch (Exception ex)
            {
                Error = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                if (closeKeyStore && keyStore != null)
                {
                    await keyStore.Close(true);
                }
            }
        }
    }
}
