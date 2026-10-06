namespace Leosac.KeyManager.Library.Tests
{
    [TestClass]
    public class KeyTests
    {
        [TestMethod]
        public void OneMaterial_GetAggregatedValue_HexString()
        {
            var key = new Key(null, 128, "00112233445566778899AABBCCDDEEFF");
            var v = key.GetAggregatedValueAsString();
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", v, true);
        }

        [TestMethod]
        public void OneMaterial_SetAggregatedValue_HexString()
        {
            var key = new Key();
            key.SetAggregatedValueAsString("00112233445566778899AABBCCDDEEFF");
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", key.Materials[0].Value, true);
        }

        [TestMethod]
        public void OneMaterial_GetAggregatedValue_Binary()
        {
            var key = new Key(null, 128, "00112233445566778899AABBCCDDEEFF");
            var v = key.GetAggregatedValueAsBinary();
            Assert.IsNotNull(v);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", Convert.ToHexString(v), true);
        }

        [TestMethod]
        public void OneMaterial_GetAggregatedValue_HexStringWithSpace()
        {
            var key = new Key(null, 128, "00112233445566778899AABBCCDDEEFF");
            var v = key.GetAggregatedValueAsString(KeyValueStringFormat.HexStringWithSpace);
            Assert.AreEqual("00 11 22 33 44 55 66 77 88 99 AA BB CC DD EE FF", v, true);
        }

        [TestMethod]
        public void OneMaterial_SetAggregatedValue_HexStringWithSpace()
        {
            var key = new Key();
            key.SetAggregatedValueAsString("00 11 22 33 44 55 66 77 88 99 AA BB CC DD EE FF", KeyValueStringFormat.HexStringWithSpace);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", key.Materials[0].Value, true);
        }

        [TestMethod]
        public void OneMaterial_GetAggregatedValue_HexStringFromHexStringWithSpace()
        {
            var key = new Key(null, 128, "00 11 22 33");
            key.Materials[0].ValueFormat = KeyValueStringFormat.HexStringWithSpace;
            var v = key.GetAggregatedValueAsString(KeyValueStringFormat.HexString);
            Assert.AreEqual("00112233", v, true);
        }

        [TestMethod]
        public void ConvertValueFormat_ConvertsFromHexStringWithSpace()
        {
            var v = KeyMaterial.ConvertValueFormat("00 11 22 33", KeyValueStringFormat.HexString, KeyValueStringFormat.HexStringWithSpace);
            Assert.AreEqual("00112233", v, true);
        }

        [TestMethod]
        public void ConvertValueFormat_ConvertsHexStringToDerAndBack()
        {
            var der = KeyMaterial.ConvertValueFormat("00112233", KeyValueStringFormat.Der, KeyValueStringFormat.HexString);
            var hex = KeyMaterial.ConvertValueFormat(der, KeyValueStringFormat.HexString, KeyValueStringFormat.Der);

            Assert.AreEqual("ABEiMw==", der);
            Assert.AreEqual("00112233", hex, true);
        }

        [TestMethod]
        public void ConvertValueFormat_ConvertsHexStringToPemAndBack()
        {
            var pem = KeyMaterial.ConvertValueFormat("00112233", KeyValueStringFormat.Pem, KeyValueStringFormat.HexString);
            var hex = KeyMaterial.ConvertValueFormat(pem, KeyValueStringFormat.HexString, KeyValueStringFormat.Pem);

            Assert.AreEqual("-----BEGIN KEY-----\nABEiMw==\n-----END KEY-----", pem);
            Assert.AreEqual("00112233", hex, true);
        }

        [TestMethod]
        public void KeyMaterial_UsesKeyNameInPemLabel()
        {
            var privateKey = new KeyMaterial("00112233", KeyMaterial.PRIVATE_KEY);
            var publicKey = new KeyMaterial("00112233", KeyMaterial.PUBLIC_KEY);

            Assert.AreEqual("-----BEGIN PRIVATE KEY-----\nABEiMw==\n-----END PRIVATE KEY-----", privateKey.GetValueAsString(KeyValueStringFormat.Pem));
            Assert.AreEqual("-----BEGIN PUBLIC KEY-----\nABEiMw==\n-----END PUBLIC KEY-----", publicKey.GetValueAsString(KeyValueStringFormat.Pem));
        }

        [TestMethod]
        public void TwoMaterials_SetAggregatedValue_PemAssignsBlocksByKeyType()
        {
            var key = new Key(null, 0,
                new KeyMaterial("", KeyMaterial.PRIVATE_KEY),
                new KeyMaterial("", KeyMaterial.PUBLIC_KEY));
            var publicKey = "-----BEGIN PUBLIC KEY-----\r\nBAUG\r\n-----END PUBLIC KEY-----";
            var privateKey = "-----BEGIN PRIVATE KEY-----\nAAEC\nAw==\n-----END PRIVATE KEY-----";

            key.SetAggregatedValueAsString($"{publicKey}\r\n{privateKey}", KeyValueStringFormat.Pem);

            Assert.AreEqual("00010203", key.Materials[0].Value, true);
            Assert.AreEqual("040506", key.Materials[1].Value, true);
        }

        [TestMethod]
        public void TwoMaterials_GetAggregatedValue_HexString()
        {
            var key = new Key(null, 64, new KeyMaterial("0011223344556677"), new KeyMaterial("8899AABBCCDDEEFF"));
            var v = key.GetAggregatedValueAsString();
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", v, true);
        }

        [TestMethod]
        public void TwoMaterials_SetAggregatedValue_HexString()
        {
            var key = new Key(null, 64, 2);
            key.SetAggregatedValueAsString("00112233445566778899AABBCCDDEEFF");
            Assert.AreEqual("0011223344556677", key.Materials[0].Value, true);
            Assert.AreEqual("8899AABBCCDDEEFF", key.Materials[1].Value, true);
        }

        [TestMethod]
        public void TwoMaterials_GetAggregatedValue_Binary()
        {
            var key = new Key(null, 64, new KeyMaterial("0011223344556677"), new KeyMaterial("8899AABBCCDDEEFF"));
            var v = key.GetAggregatedValueAsBinary();
            Assert.IsNotNull(v);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", Convert.ToHexString(v), true);
        }

        [TestMethod]
        public void ThreeMaterialsWithOneOverrideSize_GetAggregatedValue_Binary()
        {
            var key = new Key(null, 48, new KeyMaterial("001122334455"), new KeyMaterial("FFFFFFFF", null, 32), new KeyMaterial("8899AABBCCDD"));
            var v = key.GetAggregatedValueAsBinary();
            Assert.IsNotNull(v);
            Assert.AreEqual("001122334455FFFFFFFF8899AABBCCDD", Convert.ToHexString(v), true);
        }
    }
}
