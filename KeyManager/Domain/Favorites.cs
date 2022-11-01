using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Domain
{
    public class Favorites
    {
        public List<Favorite> KeyStores { get; set; } = new List<Favorite>();
    }
}
