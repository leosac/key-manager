using Leosac.KeyManager.Library.KeyGen.SecretSharing;

namespace Leosac.KeyManager.Library.Tests
{
    [TestClass]
    public class XorSecretSharingTests
    {
        private readonly XorSecretSharing _sharing;

        public XorSecretSharingTests()
        {
            _sharing = new XorSecretSharing();
        }

        [TestMethod]
        public void Test_ComputeTwoFragments()
        {
            var key = _sharing.ComputeFragments(new[]
            {
                "C37CAE51E95AF034DE05EEFA1AF48CF8",
                "C36D8C62AD0F9643569C4441D6296207"
            });
            Assert.IsNotNull(key);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", Convert.ToHexString(key), true);
        }

        [TestMethod]
        public void Test_ComputeThreeFragments()
        {
            var key = _sharing.ComputeFragments(new[]
            {
                "77BE07C2B01AE24B7D20E1988898107A",
                "F84B15A46502C6B4589CD51579A4B23F",
                "8FE43055914D4288AD259E363DE14CBA"
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
            Assert.AreEqual(fragments[0].Length, fragments[1].Length);
            Assert.AreNotEqual(fragments[0], fragments[1], true);
        }

        [TestMethod]
        public void Test_CreateThreeFragments()
        {
            var fragments = _sharing.CreateFragments(Convert.FromHexString("00112233445566778899AABBCCDDEEFF"), 3);
            Assert.IsNotNull(fragments);
            Assert.IsTrue(fragments.Length == 3);
            Assert.AreEqual(fragments[0].Length, fragments[1].Length);
            Assert.AreEqual(fragments[1].Length, fragments[2].Length);
        }
    }
}
