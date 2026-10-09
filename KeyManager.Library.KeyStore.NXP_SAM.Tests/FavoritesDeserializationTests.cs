using Leosac.KeyManager.Library;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.KeyStore.NXP_SAM;
using Leosac.KeyManager.Library.UI;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.Tests
{
    [TestClass]
    public class FavoritesDeserializationTests
    {
        [TestMethod]
        public void LoadSafeFromFile_DeserializesSymmetricDefaultKeyEntry()
        {
            var source = new Favorite { Name = "Memory Key Store" };
            source.DefaultKeyEntries[KeyEntryClass.Symmetric] = new SAMSymmetricKeyEntry
            {
                Variant = new KeyEntryVariant { Name = "AES128" }
            };
            var json = JsonConvert.SerializeObject(new { KeyStores = new[] { source } }, KeyEntry.CreateJsonSerializerSettings());
            var path = Path.Combine(Path.GetTempPath(), $"favorites-{Guid.NewGuid():N}.json");

            try
            {
                File.WriteAllText(path, json);

                var favorites = Favorites.LoadSafeFromFile(path);

                Assert.AreEqual(1, favorites.KeyStores.Count);
                Assert.IsInstanceOfType<SAMSymmetricKeyEntry>(favorites.KeyStores[0].DefaultKeyEntries[KeyEntryClass.Symmetric]);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}