using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager
{
    public class Favorites
    {
        public Dictionary<string, KeyStoreProperties> KeyStores { get; set; } = new Dictionary<string, KeyStoreProperties>();


    }
}
