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
            favorite.Name = String.Format("{0} Key Store - ({1})", store.Name, Guid.NewGuid());
            favorite.Properties = store.Properties;
            KeyStores.Add(favorite);
            if (save)
            {
                SaveToFile();
            }
            return favorite;
        }
    }
}
