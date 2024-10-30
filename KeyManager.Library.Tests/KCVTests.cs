namespace Leosac.KeyManager.Library.Tests
{
    [TestClass]
    public class KCVTests
    {
        [TestMethod]
        public void Test_KCV_GlobalPlatform_2K3DES()
        {
            var kcv = new KeyGen.KCVGlobalPlatform();
            var checksum = kcv.ComputeKCV("DES", "11223344556677889900AABBCCDDEEFF");

            Assert.AreEqual("5ED7EA", checksum);
        }

        [TestMethod]
        public void Test_KCV_GlobalPlatform_3K3DES()
        {
            var kcv = new KeyGen.KCVGlobalPlatform();
            var checksum = kcv.ComputeKCV("DES", "11223344556677889900AABBCCDDEEFF8877665544332211");

            Assert.AreEqual("CB799D", checksum);
        }

        [TestMethod]
        public void Test_KCV_GlobalPlatform_AES()
        {
            var kcv = new KeyGen.KCVGlobalPlatform();
            var checksum = kcv.ComputeKCV("AES", "11223344556677889900AABBCCDDEEFF");

            // Would be DD566B if not using 0x01 as padding value for IV
            Assert.AreEqual("CDE1DE", checksum);
        }
    }
}
