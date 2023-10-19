namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.Tests
{
    [TestClass]
    public class SAMKeyEntryComparerTests
    {
        [TestMethod]
        public void Test_ListOrder()
        {
            var comparer = new SAMKeyEntryComparer(new SAMKeyStoreProperties
            {
                AuthenticateKeyEntryIdentifier = 0,
                AuthenticateKeyVersion = 0
            });

            var list = new List<SAMSymmetricKeyEntry>(new[]
            {
                new SAMSymmetricKeyEntry { Identifier = new KeyEntryId("0") },
                new SAMSymmetricKeyEntry { Identifier = new KeyEntryId("1") },
                new SAMSymmetricKeyEntry { Identifier = new KeyEntryId("2") }
            });
            var orderedList = list.Order(comparer);

            Assert.AreEqual("1", orderedList.ElementAt(0).Identifier.Id);
            Assert.AreEqual("2", orderedList.ElementAt(1).Identifier.Id);
            Assert.AreEqual("0", orderedList.ElementAt(2).Identifier.Id);
        }
    }
}
