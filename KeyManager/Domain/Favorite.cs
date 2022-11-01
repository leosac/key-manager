using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Domain
{
    public class Favorite
    {
        public string? Name { get; set; }

        public KeyStoreProperties? Properties { get; set; }

        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }
}
