namespace Leosac.KeyManager.Library.Tests
{
    [TestClass]
    public class KCVTests
    {
        [TestMethod]
        public void Test_KCV_2K3DES()
        {
            var kcv = new KCV();
            var checksum = kcv.ComputeKCV(KeyTag.DES, "11223344556677889900AABBCCDDEEFF");

            Assert.AreEqual("5ED7EA", checksum);
        }

        [TestMethod]
        public void Test_KCV_3K3DES()
        {
            var kcv = new KCV();
            var checksum = kcv.ComputeKCV(KeyTag.DES, "11223344556677889900AABBCCDDEEFF8877665544332211");

            Assert.AreEqual("CB799D", checksum);
        }

        [TestMethod]
        public void Test_KCV_AES()
        {
            var kcv = new KCV();
            var checksum = kcv.ComputeKCV(KeyTag.AES, "11223344556677889900AABBCCDDEEFF");

            Assert.AreEqual("DD566B", checksum);
        }
    }
}
