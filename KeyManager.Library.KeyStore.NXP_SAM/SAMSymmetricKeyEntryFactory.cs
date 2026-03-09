using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMSymmetricKeyEntryFactory : GenericKeyEntryFactory<SAMSymmetricKeyEntry, SAMSymmetricKeyEntryProperties>
    {
        public override string Name => "NXP SAM Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Symmetric];

        public override uint Importance => 1;
    }
}
