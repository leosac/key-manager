using System.Security.Cryptography;
using Newtonsoft.Json;

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
            var key1 = KeyGeneration.Random((uint)keySize);
            Assert.AreEqual(keySize, key1.Length);

            var key2 = KeyGeneration.Random((uint)keySize);
            Assert.AreNotEqual(key1, key2);
        }

        [TestMethod]
        [DataRow(8)]
        [DataRow(16)]
        [DataRow(32)]
        public void Test_FromPassword(int keySize)
        {
            var key = Convert.ToHexString(KeyGeneration.FromPassword("test", "Security Freedom", keySize));
            var rkey = "E088566240571EAD486818BE1199F53EB407411014BA1E36101C242FC34DEBAF"[..(keySize * 2)];
            Assert.AreEqual(rkey, key, true);
        }

        [TestMethod]
        public void Key_GenerateAes_UsesTagAndPreservesTagsInJson()
        {
            var key = new Key(["AES", "legacy-tag"], 128);

            key.Generate();

            Assert.AreEqual(1, key.Materials.Count);
            Assert.AreEqual(16, key.Materials[0].GetValueAsBinary()!.Length);
            var json = JsonConvert.SerializeObject(key);
            StringAssert.Contains(json, "AES");
            var restored = JsonConvert.DeserializeObject<Key>(json);
            CollectionAssert.AreEqual(key.Tags.ToArray(), restored!.Tags.ToArray());
        }

        [TestMethod]
        public void Key_DeserializesLegacySymmetricByteSizeAsBits()
        {
            var key = JsonConvert.DeserializeObject<Key>("{\"KeySize\":16,\"Tags\":[\"AES\"]}");

            Assert.AreEqual(128u, key!.KeySize);
        }

        [TestMethod]
        public void KeyMaterial_DeserializesLegacyByteOverrideSizeAsBits()
        {
            var material = JsonConvert.DeserializeObject<KeyMaterial>("{\"Value\":\"00\",\"OverrideSize\":4}");

            Assert.AreEqual(32u, material!.OverrideSize);
        }

        [TestMethod]
        public void Key_GenerateRsa_CreatesImportableKeyPair()
        {
            var key = new Key(["RSA"], 1024);

            key.Generate();

            Assert.AreEqual(2, key.Materials.Count);
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(key.Materials[0].Value), out _);
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(key.Materials[1].Value), out _);
        }

        [TestMethod]
        public void Key_GenerateCustomAlgorithm_UsesPluginGenerator()
        {
            KeyGeneration.Register("plugin-algorithm", new PluginGenerator());
            try
            {
                var key = new Key(["plugin-algorithm"]);
                key.Generate();
                Assert.AreEqual("AABB", key.Materials.Single().Value);
            }
            finally
            {
                KeyGeneration.Unregister("plugin-algorithm");
            }
        }

        private sealed class PluginGenerator : IKeyGenerator
        {
            public IEnumerable<KeyMaterial> Generate(Key key)
            {
                return [new KeyMaterial("AABB")];
            }
        }
    }
}
