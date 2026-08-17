using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMAsymmetricRSAKeyEntryFactory : GenericKeyEntryFactory<SAMAsymmetricRSAKeyEntry, SAMAsymmetricRSAKeyEntryProperties>
    {
        public override string Name => "NXP SAM Asymmetric RSA Key Entry";

        public override IEnumerable<KeyEntryClass> KClasses => [KeyEntryClass.Asymmetric];

        public override uint Importance => 1;
    }
}
