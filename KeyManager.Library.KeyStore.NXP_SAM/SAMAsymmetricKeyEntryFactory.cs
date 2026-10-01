using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMAsymmetricKeyEntryFactory : GenericKeyEntryFactory<SAMAsymmetricKeyEntry, SAMAsymmetricKeyEntryProperties>
    {
        public override string Name => "NXP SAM Asymmetric Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Asymmetric];

        public override uint Importance => 1;
    }
}
