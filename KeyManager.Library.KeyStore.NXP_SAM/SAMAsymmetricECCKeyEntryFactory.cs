using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMAsymmetricECCKeyEntryFactory : GenericKeyEntryFactory<SAMAsymmetricECCKeyEntry, SAMAsymmetricECCKeyEntryProperties>
    {
        public override string Name => "NXP SAM Asymmetric ECC Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Asymmetric];

        public override uint Importance => 1;
    }
}
