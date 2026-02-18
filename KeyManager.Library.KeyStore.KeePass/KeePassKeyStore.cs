using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Security;
using System.Collections.Generic;
using KeePassLib.Serialization;

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{
    public class KeePassKeyStore : KeyStore
    {
        public override string Name => "KeePass";
        private const string GroupName = "Credential";
        private PwDatabase _database;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public override bool CanCreateKeyEntries => true;
        public override bool CanDeleteKeyEntries => true;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric };
        }

        public override Task Open()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            throw new NotImplementedException();
        }

        public override Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            throw new NotImplementedException();
        }

        public override Task Create(IChangeKeyEntry keyEntry)
        {
            throw new NotImplementedException();
        }

        public override Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            throw new NotImplementedException();
        }

        public override Task Update(IChangeKeyEntry keyEntry, bool ignoreIfMissing)
        {
            throw new NotImplementedException();
        }

        public override Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing)
        {
            throw new NotImplementedException();
        }
    }
}