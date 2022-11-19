using Leosac.KeyManager.Library.KeyStore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library
{
    public class Favorites : KMPermanentConfig<Favorites>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        private static object _objlock = new object();
        private static Favorites? _singleton;

        public static Favorites GetSingletonInstance(bool forceRecreate = false)
        {
            lock (_objlock)
            {
                if (_singleton == null || forceRecreate)
                {
                    _singleton = LoadFromFile();
                }

                return _singleton!;
            }
        }

        public ObservableCollection<Favorite> KeyStores { get; set; } = new ObservableCollection<Favorite>();

        public static string DefaultFileName { get => "Favorites.json"; }

        public static Favorites? LoadFromFile()
        {
            return LoadFromFile(GetConfigFilePath(DefaultFileName));
        }

        public override string GetDefaultFileName()
        {
            return DefaultFileName;
        }

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
