namespace Leosac.KeyManager.Library.Tests
{
    [TestClass]
    public class KeyGenerationTests
    {
        [TestMethod]
        [DataRow(8)]
        [DataRow(16)]
        [DataRow(32)]
        public void Test_Random(int keySize)
        {
            var key1 = KeyGeneration.Random(keySize);
            Assert.AreEqual(keySize * 2, key1.Length);

            var key2 = KeyGeneration.Random(keySize);
            Assert.AreNotEqual(key1, key2);
        }

        [TestMethod]
        [DataRow(8)]
        [DataRow(16)]
        [DataRow(32)]
        public void Test_FromPassword(int keySize)
        {
            var key = KeyGeneration.FromPassword("test", "Security Freedom", keySize);
            var rkey = "E088566240571EAD486818BE1199F53EB407411014BA1E36101C242FC34DEBAF"[..(keySize * 2)];
            Assert.AreEqual(rkey, key, true);
        }
    }
}
