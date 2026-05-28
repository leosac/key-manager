using Leosac.KeyManager.Library.Plugin;
using System.Text;

namespace Leosac.KeyManager.Library.UI
{
    public static class FavoriteExtension
    {
        public static KeyStore.KeyStore? CreateKeyStore(this Favorite fav)
        {
            ArgumentNullException.ThrowIfNull(fav);

            if (fav.Properties == null)
                return null;

            var factory = KeyStoreFactory.GetFactoryFromPropertyType(fav.Properties.GetType());
            if (factory == null)
                return null;

            var ks = factory.CreateKeyStore() ?? throw new InvalidOperationException($"Factory failed to create KeyStore for '{fav.Name}'");
            ks.Properties = fav.Properties;
            ks.DefaultKeyEntries = fav.DefaultKeyEntries;
            var name = fav.Name ?? string.Empty;

            ks.Attributes[KeyStore.KeyStore.ATTRIBUTE_NAME] = name;
            ks.Attributes[KeyStore.KeyStore.ATTRIBUTE_HEXNAME] = Convert.ToHexString(Encoding.UTF8.GetBytes(name));

            return ks;
        }
    }

}
