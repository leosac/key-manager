using Leosac.KeyManager.Library.Plugin;
using Leosac.SharedServices;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Leosac.KeyManager.Library.UI
{
    public class Favorites : PermanentConfig<Favorites>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        private static readonly object _instanceLock = new();
        private static Favorites? _singleton;
        private static KMSettings? _settings;

        private readonly Dictionary<string, Favorite> _index = new(StringComparer.OrdinalIgnoreCase);

        private const string RootName = nameof(KeyStores);

        public ObservableCollection<Favorite> KeyStores { get; private set; } = new();

        public static event EventHandler? SingletonCreated;

        public Favorites()
        {
            IsUserConfiguration = true;
            KeyStores.CollectionChanged += KeyStores_CollectionChanged;
        }

        private void KeyStores_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Favorite favorite in e.OldItems)
                {
                    favorite.PropertyChanged -= Favorite_PropertyChanged;
                    RemoveFromIndex(favorite);
                }
            }
            if (e.NewItems != null)
            {
                foreach (Favorite favorite in e.NewItems)
                {
                    favorite.PropertyChanged -= Favorite_PropertyChanged;
                    favorite.PropertyChanged += Favorite_PropertyChanged;
                    Index(favorite);
                }
            }
        }

        public static Favorites? GetSingletonInstance(bool forceRecreate = false)
        {
            lock (_instanceLock)
            {
                if (_singleton == null || forceRecreate)
                {
                    try
                    {
                        _settings ??= KMSettings.LoadFromFile(false);
                        var path = GetFavoritesPath();
                        _singleton = LoadSafeFromFile(path);
                        OnSingletonCreated();
                    }
                    catch (Exception ex)
                    {
                        log.Error("Cannot load Favorites from file.", ex);
                    }
                }
                return _singleton;
            }
        }

        public override bool SaveToFile()
        {
            try
            {
                return SaveToFile(GetFavoritesPath());
            }
            catch (Exception ex)
            {
                log.Error("Unexpected error while saving Favorites.", ex);
                return false;
            }
        }

        private static string GetFavoritesPath()
        {
            return !string.IsNullOrWhiteSpace(_settings?.FavoritesPath)
                ? _settings!.FavoritesPath
                : GetConfigFilePath(GetDefaultFileName(), true);
        }

        public Favorite? Get(string favoriteIdOrName)
        {
            if (string.IsNullOrWhiteSpace(favoriteIdOrName))
                return null;
            return _index.TryGetValue(favoriteIdOrName, out var fav) ? fav : null;
        }

        public Favorite CreateFromKeyStore(KeyStore.KeyStore store, bool save = true)
        {
            ArgumentNullException.ThrowIfNull(store);
            var favorite = new Favorite
            {
                Name = string.Format("{0} Key Store - ({1})", store.Name, DateTimeOffset.Now.ToUnixTimeSeconds()),
                Properties = store.Properties
            };
            KeyStores.Add(favorite);
            if (save)
                SaveToFile();
            log.Info(string.Format("New Favorite `{0}` saved.", favorite.Name));
            store.Attributes[KeyStore.KeyStore.ATTRIBUTE_NAME] = favorite.Name;
            store.Attributes[KeyStore.KeyStore.ATTRIBUTE_HEXNAME] = Convert.ToHexString(Encoding.UTF8.GetBytes(favorite.Name));
            return favorite;
        }

        public bool Remove(Favorite favorite)
        {
            ArgumentNullException.ThrowIfNull(favorite);
            if (!KeyStores.Remove(favorite))
                return false;
            SaveToFile();
            return true;
        }

        private void Favorite_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Favorite fav)
                return;

            if (e.PropertyName is nameof(Favorite.Name) or nameof(Favorite.Identifier))
            {
                RemoveFromIndex(fav);
                Index(fav);
            }
        }

        public void RebuildIndex()
        {
            _index.Clear();
            foreach (var fav in KeyStores)
            {
                fav.PropertyChanged -= Favorite_PropertyChanged;
                fav.PropertyChanged += Favorite_PropertyChanged;
                Index(fav);
            }
        }

        private void IndexKey(Favorite fav, string? key)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _index[key] = fav;
        }

        private void Index(Favorite fav)
        {
            IndexKey(fav, fav.Identifier);
            IndexKey(fav, fav.Name);
        }

        private void RemoveFromIndex(Favorite fav)
        {
            if (!string.IsNullOrWhiteSpace(fav.Identifier))
                _index.Remove(fav.Identifier);

            if (!string.IsNullOrWhiteSpace(fav.Name))
                _index.Remove(fav.Name);
        }

        public static Favorites LoadSafeFromFile(string path)
        {
            var favorites = new Favorites();

            if (!File.Exists(path))
            {
                log.Warn($"Favorites file does not exist: '{path}'.");
                return favorites;
            }

            try
            {
                log.Info($"Safely loading favorites from file '{path}'...");
                var json = File.ReadAllText(path);
                var root = JObject.Parse(json);

                if (root[RootName] is not JArray array)
                {
                    log.Warn("Favorites file does not contain a valid 'KeyStores' array.");
                    return favorites;
                }

                foreach (var token in array)
                {
                    if (LoadFavorite(token, out var favorite))
                        favorites.KeyStores.Add(favorite!);
                }

                log.Info($"Favorites safely loaded. Count={favorites.KeyStores.Count}");
            }
            catch (Exception ex)
            {
                log.Error($"Cannot safely load favorites file '{path}'.", ex);
            }

            return favorites;
        }

        private static bool LoadFavorite(JToken token, out Favorite? favorite)
        {
            favorite = null;

            if (token is not JObject obj)
            {
                log.Warn("Skipping malformed favorite entry.");
                return false;
            }

            const string PROPERTIES = nameof(Favorite.Properties);

            try
            {
                var propsToken = obj[PROPERTIES];
                obj.Remove(PROPERTIES);

                if (!TryDeserializeFavorite(obj, out favorite))
                    return false;

                if (favorite == null)
                {
                    log.Warn("Failed to deserialize favorite entry.");
                    return false;
                }

                if (propsToken != null)
                {
                    if (propsToken is JObject propsObj)
                        RestoreProperties(favorite, propsObj);
                    else
                        log.Warn($"Invalid properties object for favorite '{favorite.Name}'. Expected JObject but got {propsToken.Type}.");
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Warn("Skipping invalid favorite entry.", ex);
                return false;
            }
        }

        private static bool TryDeserializeFavorite(JObject obj, out Favorite? favorite)
        {
            try
            {
                favorite = obj.ToObject<Favorite>();
                return favorite != null;
            }
            catch (Exception ex)
            {
                log.Warn("Favorite deserialization failed, skipping entry.", ex);
                favorite = null;
                return false;
            }
        }

        private static void RestoreProperties(Favorite favorite, JObject propsObj)
        {
            var (factory, typeName) = ExtractFactoryFromProperties(favorite.Name, propsObj);

            if (factory != null)
            {
                try
                {
                    favorite.Properties = factory.CreateKeyStoreProperties(propsObj.ToString());
                }
                catch (Exception ex)
                {
                    log.Warn($"Failed to restore properties for favorite '{favorite.Name}'.", ex);
                    favorite.UnresolvedModule = propsObj.DeepClone() as JObject;
                }
            }
            else
            {
                favorite.UnresolvedModule = propsObj.DeepClone() as JObject;
            }
            var fallbackName = typeName != null ? $"(Missing module) {typeName.Split('.').LastOrDefault()}" : "(Missing module) Unknown";
            favorite.KeyStoreTypeName = factory?.Name ?? fallbackName;
        }

        private static (KeyStoreFactory? factory, string? typeName) ExtractFactoryFromProperties(string name, JObject propsObj)
        {
            var type = propsObj["$type"]?.ToString();

            if (string.IsNullOrWhiteSpace(type))
            {
                log.Warn($"Favorite '{name}' does not contain a valid property type.");
                return (null, null);
            }

            var typeName = type.Split(',')[0].Trim();
            var factory = KeyStoreFactory.GetFactoryFromPropertyType(typeName);
            if (factory == null)
                log.Warn($"Missing plugin for favorite '{name}' ({typeName}).");

            return (factory, typeName);
        }

        private static void OnSingletonCreated()
        {
            var instance = _singleton;
            if (instance != null)
                SingletonCreated?.Invoke(instance, EventArgs.Empty);
        }

        public void ReplaceAll(IEnumerable<Favorite> favorites)
        {
            ArgumentNullException.ThrowIfNull(favorites);

            KeyStores.Clear();
            _index.Clear();

            foreach (var fav in favorites)
            {
                if (fav != null)
                    KeyStores.Add(fav);
            }
            SaveToFile();
        }

    }
}