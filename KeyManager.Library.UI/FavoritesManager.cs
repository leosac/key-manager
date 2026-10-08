using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Leosac.KeyManager.Library.UI
{
    public static class FavoritesManager
    {
        private static readonly object _lock = new();
        private static readonly ObservableCollection<Favorite> _availableKeyStores = new();
        private static readonly ReadOnlyObservableCollection<Favorite> _readOnlyAvailableKeyStores = new(_availableKeyStores);
        private static Favorites? _user;
        private static Favorites? _shared;

        public static ReadOnlyObservableCollection<Favorite> AvailableKeyStores
        {
            get
            {
                lock (_lock)
                {
                    _ = User;
                    _ = Shared;
                    return _readOnlyAvailableKeyStores;
                }
            }
        }

        public static Favorites User
        {
            get
            {
                lock (_lock)
                {
                    if (_user == null)
                    {
                        _user = Load(Favorites.GetFavoritesPath());
                        Subscribe(_user);
                    }
                    return _user;
                }
            }
        }

        public static Favorites Shared
        {
            get
            {
                lock (_lock)
                {
                    if (_shared == null)
                    {
                        _shared = Load(Favorites.GetSharedFavoritesPath());
                        Subscribe(_shared);
                    }
                    return _shared;
                }
            }
        }

        public static Favorite? Get(string favoriteIdOrName)
        {
            return User.Get(favoriteIdOrName) ?? Shared.Get(favoriteIdOrName);
        }

        public static bool ToggleShared(Favorite favorite)
        {
            ArgumentNullException.ThrowIfNull(favorite);

            lock (_lock)
            {
                var user = User;
                var shared = Shared;
                var isShared = shared.KeyStores.Contains(favorite);
                var source = isShared ? shared : user;
                var target = isShared ? user : shared;

                if (!source.KeyStores.Remove(favorite))
                    return false;

                target.KeyStores.Add(favorite);
                source.SaveToFile();
                target.SaveToFile();
                RebuildAvailableKeyStores();
                return true;
            }
        }

        public static void Reload()
        {
            lock (_lock)
            {
                Unsubscribe(_user);
                Unsubscribe(_shared);
                _user = null;
                _shared = null;
                RebuildAvailableKeyStores();
            }
        }

        private static Favorites Load(string path)
        {
            return Favorites.LoadSafeFromFile(path);
        }

        private static void Subscribe(Favorites favorites)
        {
            favorites.KeyStores.CollectionChanged += SourceKeyStores_CollectionChanged;
            RebuildAvailableKeyStores();
        }

        private static void Unsubscribe(Favorites? favorites)
        {
            if (favorites != null)
                favorites.KeyStores.CollectionChanged -= SourceKeyStores_CollectionChanged;
        }

        private static void SourceKeyStores_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            lock (_lock)
            {
                RebuildAvailableKeyStores();
            }
        }

        private static void RebuildAvailableKeyStores()
        {
            _availableKeyStores.Clear();

            if (_user != null)
            {
                foreach (var favorite in _user.KeyStores)
                {
                    favorite.IsShared = false;
                    _availableKeyStores.Add(favorite);
                }
            }

            if (_shared != null)
            {
                foreach (var favorite in _shared.KeyStores)
                {
                    favorite.IsShared = true;
                    _availableKeyStores.Add(favorite);
                }
            }
        }
    }
}
