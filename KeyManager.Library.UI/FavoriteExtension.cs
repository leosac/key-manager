using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI
{
    public static class FavoriteExtension
    {
        public static KeyStore.KeyStore? CreateKeyStore(this Favorite fav)
        {
            var factory = KeyStoreFactory.GetFactoryFromPropertyType(fav.Properties!.GetType());
            if (factory != null)
            {
                var ks = factory.CreateKeyStore();
                ks.Properties = fav.Properties;

                return ks;
            }

            return null;
        }
    }
}
