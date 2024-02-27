using Leosac.KeyManager.Library.KeyGen.SecretSharing;

namespace Leosac.KeyManager.Library.Tests
{
    [TestClass]
    public class ShamirsSecretSharingTests
    {
        private readonly ShamirsSecretSharing _sharing;

        public ShamirsSecretSharingTests()
        {
            _sharing = new ShamirsSecretSharing();
        }

        [TestMethod]
        public void Test_ComputeTwoFragments()
        {
            var key = _sharing.ComputeFragments(new[]
            {
                "01-34E95FCCCA0B73B74DEC515D2EA4B988A582D6EF55B6EFC0638B2B3CBBE171A30BD85DC2ABEB16F93991ADCCB2107F421F2C3BAEE69EFE271CE81A93C1E1EF3D9700",
                "03-9C99DBFED7788C37D891A0A0F1304F9AC48783CF0123CF422BA282B431A555EA2288194703C344EBADB3086618327DC75D84B10AB4DCFB7754B850B944A5CFB9C501"
            });
            Assert.IsNotNull(key);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", Convert.ToHexString(key), true);
        }

        [TestMethod]
        public void Test_ComputeThreeFragments()
        {
            var key = _sharing.ComputeFragments(new[]
            {
                "01-34E95FCCCA0B73B74DEC515D2EA4B988A582D6EF55B6EFC0638B2B3CBBE171A30BD85DC2ABEB16F93991ADCCB2107F421F2C3BAEE69EFE271CE81A93C1E1EF3D9700",
                "02-68C19D6551C27FF7123FF9FE8F6A84113505ADDFAB6CDF81C716577876C3E34617B0BB8457D72DF273225B996521FE843E58765CCD3DFD4F38D0352683C3DF7B2E01",
                "03-9C99DBFED7788C37D891A0A0F1304F9AC48783CF0123CF422BA282B431A555EA2288194703C344EBADB3086618327DC75D84B10AB4DCFB7754B850B944A5CFB9C501"
            });
            Assert.IsNotNull(key);
            Assert.AreEqual("00112233445566778899AABBCCDDEEFF", Convert.ToHexString(key), true);
        }

        [TestMethod]
        public void Test_CreateThreeFragments()
        {
            var fragments = _sharing.CreateFragments(Convert.FromHexString("00112233445566778899AABBCCDDEEFF"), 3);
            Assert.IsNotNull(fragments);
            Assert.IsTrue(fragments.Length == 3);
        }
    }
}
