using Leosac.KeyManager.Library.Plugin;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyStoreFactory : GenericKeyStoreFactory<SAMKeyStore, SAMKeyStoreProperties>
    {
        public override string Name => "NXP SAM AV2/AV3";

        public override Device.ICardDevice? CreateCardDevice(KeyStore keyStore)
        {
            if (keyStore is SAMKeyStore samStore)
                return new Domain.SAMCardDevice(samStore);
            return null;
        }

        public override IEnumerable<IChangeKeyEntry> OrderKeyEntries(IList<IChangeKeyEntry> entries, KeyStore keyStore)
        {
            if (keyStore is not SAMKeyStore sam)
                return entries;

            return entries.Order(new SAMKeyEntryComparer(sam.GetSAMProperties()));
        }

    }
}
