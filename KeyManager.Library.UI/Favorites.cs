using Leosac.WpfApp;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.UI
{
    public class Favorites : PermanentConfig<Favorites>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        private static readonly object _objlock = new();
        private static Favorites? _singleton;

        public static EventHandler? SingletonCreated { get; set; }
        private static void OnSingletonCreated()
        {
            SingletonCreated?.Invoke(_singleton, new EventArgs());
        }

        public static Favorites GetSingletonInstance(bool forceRecreate = false)
        {
            lock (_objlock)
            {
                if (_singleton == null || forceRecreate)
                {
                    _singleton = LoadFromFile();
                    OnSingletonCreated();
                }

                return _singleton!;
            }
        }

        public ObservableCollection<Favorite> KeyStores { get; set; } = new ObservableCollection<Favorite>();

        public Favorite CreateFromKeyStore(KeyStore.KeyStore store, bool save = true)
        {
            var favorite = new Favorite
            {
                Name = string.Format("{0} Key Store - ({1})", store.Name, DateTimeOffset.Now.ToUnixTimeSeconds()),
                Properties = store.Properties
            };
            KeyStores.Add(favorite);
            if (save)
            {
                SaveToFile();
            }
            log.Info(string.Format("New Favorite `{0}` saved.", favorite.Name));
            return favorite;
        }
    }
}
