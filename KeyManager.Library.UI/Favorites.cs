using Leosac.KeyManager.Library.KeyStore;
using Leosac.WpfApp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI
{
    public class Favorites : PermanentConfig<Favorites>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        private static object _objlock = new object();
        private static Favorites? _singleton;

        public static EventHandler? SingletonCreated;
        private static void OnSingletonCreated()
        {
            if (SingletonCreated != null)
            {
                SingletonCreated(_singleton, new EventArgs());
            }
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
            var favorite = new Favorite();
            favorite.Name = String.Format("{0} Key Store - ({1})", store.Name, DateTimeOffset.Now.ToUnixTimeSeconds());
            favorite.Properties = store.Properties;
            KeyStores.Add(favorite);
            if (save)
            {
                SaveToFile();
            }
            log.Info(String.Format("New Favorite `{0}` saved.", favorite.Name));
            return favorite;
        }
    }
}
