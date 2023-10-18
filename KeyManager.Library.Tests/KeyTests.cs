namespace Leosac.KeyManager.Library.Tests
{
    [TestClass]
    public class KeyTests
    {
        [TestMethod]
        public void OneMaterial_GetAggregatedValue_HexString()
        {
            var key = new Key(null, "00112233445566778899AABBCCDDEEFF");
            var v = key.GetAggregatedValueString();
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", v, true);
        }

        [TestMethod]
        public void OneMaterial_SetAggregatedValue_HexString()
        {
            var key = new Key();
            key.SetAggregatedValueString("00112233445566778899AABBCCDDEEFF");
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", key.Materials[0].Value, true);
        }

        [TestMethod]
        public void OneMaterial_GetAggregatedValue_Binary()
        {
            var key = new Key(null, "00112233445566778899AABBCCDDEEFF");
            var v = key.GetAggregatedValueBinary();
            Assert.IsNotNull(v);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", Convert.ToHexString(v), true);
        }

        [TestMethod]
        public void OneMaterial_GetAggregatedValue_HexStringWithSpace()
        {
            var key = new Key(null, "00112233445566778899AABBCCDDEEFF");
            var v = key.GetAggregatedValueString(KeyValueStringFormat.HexStringWithSpace);
            Assert.AreEqual("00 11 22 33 44 55 66 77 88 99 AA BB CC DD EE FF", v, true);
        }

        [TestMethod]
        public void OneMaterial_SetAggregatedValue_HexStringWithSpace()
        {
            var key = new Key();
            key.SetAggregatedValueString("00 11 22 33 44 55 66 77 88 99 AA BB CC DD EE FF", KeyValueStringFormat.HexStringWithSpace);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", key.Materials[0].Value, true);
        }

        [TestMethod]
        public void TwoMaterials_GetAggregatedValue_HexString()
        {
            var key = new Key(null, 8, new KeyMaterial("0011223344556677"), new KeyMaterial("8899AABBCCDDEEFF"));
            var v = key.GetAggregatedValueString();
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", v, true);
        }

        [TestMethod]
        public void TwoMaterials_SetAggregatedValue_HexString()
        {
            var key = new Key(null, 8, 2);
            key.SetAggregatedValueString("00112233445566778899AABBCCDDEEFF");
            Assert.AreEqual("0011223344556677", key.Materials[0].Value, true);
            Assert.AreEqual("8899AABBCCDDEEFF", key.Materials[1].Value, true);
        }

        [TestMethod]
        public void TwoMaterials_GetAggregatedValue_Binary()
        {
            var key = new Key(null, 8, new KeyMaterial("0011223344556677"), new KeyMaterial("8899AABBCCDDEEFF"));
            var v = key.GetAggregatedValueBinary();
            Assert.IsNotNull(v);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", Convert.ToHexString(v), true);
        }
    }
}
