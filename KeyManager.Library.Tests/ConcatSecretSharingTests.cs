using Leosac.KeyManager.Library.KeyGen.SecretSharing;

namespace Leosac.KeyManager.Library.Tests
{
    [TestClass]
    public class ConcatSecretSharingTests
    {
        private readonly ConcatSecretSharing _sharing;

        public ConcatSecretSharingTests()
        {
            _sharing = new ConcatSecretSharing();
        }

        [TestMethod]
        public void Test_ComputeTwoFragments()
        {
            var key = _sharing.ComputeFragments(new[]
            {
                "0011223344556677",
                "8899AABBCCDDEEFF"
            });
            Assert.IsNotNull(key);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", Convert.ToHexString(key), true);
        }

        [TestMethod]
        public void Test_ComputeThreeFragments()
        {
            var key = _sharing.ComputeFragments(new[]
            {
                "00112233445",
                "566778899AA",
                "BBCCDDEEFF"
            });
            Assert.IsNotNull(key);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", Convert.ToHexString(key), true);
        }

        [TestMethod]
        public void Test_CreateTwoFragments()
        {
            var fragments = _sharing.CreateFragments(Convert.FromHexString("00112233445566778899AABBCCDDEEFF"), 2);
            Assert.IsNotNull(fragments);
            Assert.IsTrue(fragments.Length == 2);
            Assert.AreEqual("0011223344556677", fragments[0], true);
            Assert.AreEqual("8899AABBCCDDEEFF", fragments[1], true);
        }

        [TestMethod]
        public void Test_CreateThreeFragments()
        {
            var fragments = _sharing.CreateFragments(Convert.FromHexString("00112233445566778899AABBCCDDEEFF"), 3);
            Assert.IsNotNull(fragments);
            Assert.IsTrue(fragments.Length == 3);
            Assert.AreEqual("00112233445", fragments[0], true);
            Assert.AreEqual("566778899AA", fragments[1], true);
            Assert.AreEqual("BBCCDDEEFF", fragments[2], true);
        }
    }
}
