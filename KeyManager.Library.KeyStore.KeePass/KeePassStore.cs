using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Security;

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{
    public class KeePassStore : KeyStore
    {
        public override string Name => "KeePass";









        public override bool CanCreateKeyEntries => throw new NotImplementedException();

        public override bool CanDeleteKeyEntries => throw new NotImplementedException();

        public override IEnumerable<KeyEntryClass> SupportedClasses => throw new NotImplementedException();

        public override Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            throw new NotImplementedException();
        }

        public override Task Create(IChangeKeyEntry keyEntry)
        {
            throw new NotImplementedException();
        }

        public override Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing)
        {
            throw new NotImplementedException();
        }

        public override Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            throw new NotImplementedException();
        }

        public override Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            throw new NotImplementedException();
        }

        public override Task Open()
        {
            throw new NotImplementedException();
        }

        public override Task Update(IChangeKeyEntry keyEntry, bool ignoreIfMissing)
        {
            throw new NotImplementedException();
        }
    }
}