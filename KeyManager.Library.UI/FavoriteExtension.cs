using Leosac.KeyManager.Library.Plugin;

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

                return ks;
            }

            return null;
        }
    }
}
