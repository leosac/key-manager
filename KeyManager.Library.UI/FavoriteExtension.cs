using Leosac.KeyManager.Library.Plugin;
using System.Text;

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
                ks.DefaultKeyEntries = fav.DefaultKeyEntries;

                ks.Attributes[KeyStore.KeyStore.ATTRIBUTE_NAME] = fav.Name;
                ks.Attributes[KeyStore.KeyStore.ATTRIBUTE_HEXNAME] = Convert.ToHexString(Encoding.UTF8.GetBytes(fav.Name));

                return ks;
            }

            return null;
        }
    }
}
