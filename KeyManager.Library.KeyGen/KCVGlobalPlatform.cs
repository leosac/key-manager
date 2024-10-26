namespace Leosac.KeyManager.Library.KeyGen
{
    public class KCVGlobalPlatform : KCV
    {
        public KCVGlobalPlatform()
        {
            // For AES, GlobalPlatform specification is using a default byte value set to 0x01 and not 0x00
            _paddedByte = 0x01;
        }

        public override string Name => "KCV GlobalPlatform";
    }
}
