using Leosac.KeyManager.Library.Plugin;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.SAM_SE
{
    public class SAM_SEKeyStore : KeyStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SAM_SEKeyStore()
        {
            Properties = new SAM_SEKeyStoreProperties();
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Formatting = Formatting.Indented
            };
        }

        JsonSerializerSettings _jsonSettings;

        public SAM_SEKeyStoreProperties GetFileProperties()
        {
            var p = Properties as SAM_SEKeyStoreProperties;
            if (p == null)
                throw new KeyStoreException("Missing SAM-SE key store properties.");
            return p;
        }

        public override string Name => "SAM-SE";

        public override bool CanCreateKeyEntries => true;

        public override bool CanDeleteKeyEntries => true;

        public override bool CanReorderKeyEntries => false;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric };
        }

        public override bool CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            return false;
        }

        public override void Close()
        {

        }

        public override void Open()
        {

        }

        public override void Create(IChangeKeyEntry change)
        {

        }

        public override void Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing = false)
        {

        }

        public override KeyEntry? Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            return null;
        }

        public override IList<KeyEntryId> GetAll(KeyEntryClass? keClass = null)
        {
            var keyEntries = new List<KeyEntryId>();
            return keyEntries;
        }

        public override void MoveDown(KeyEntryId identifier, KeyEntryClass keClass)
        {
            throw new NotImplementedException();
        }

        public override void MoveUp(KeyEntryId identifier, KeyEntryClass keClass)
        {
            throw new NotImplementedException();
        }

        public override void Store(IList<IChangeKeyEntry> changes)
        {
            log.Info(String.Format("Storing `{0}` key entries...", changes.Count));
            foreach (var change in changes)
            {
                Update(change, true);
            }
            log.Info("Key Entries storing completed.");
        }

        public override void Update(IChangeKeyEntry change, bool ignoreIfMissing = false)
        {
            log.Info(String.Format("Updating key entry `{0}`...", change.Identifier));
            Delete(change.Identifier, change.KClass, ignoreIfMissing);
            Create(change);
            OnKeyEntryUpdated(change);
            log.Info(String.Format("Key entry `{0}` updated.", change.Identifier));
        }
        public override string? ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput = null, KeyEntryId? wrappingKeyId = null, string? wrappingKeyContainerSelector = null)
        {
            string? result = null;
            log.Info(String.Format("Resolving key entry link with Key Entry Identifier `{0}`, Div Input `{1}`...", keyIdentifier, divInput));

            var keyEntry = Get(keyIdentifier, keClass);
            if (keyEntry != null)
            {
                log.Info("Key entry link resolved.");
                if (wrappingKeyId != null)
                {
                    var wrappingKey = GetKey(wrappingKeyId, KeyEntryClass.Symmetric, wrappingKeyContainerSelector);
                    if (wrappingKey != null)
                    {
                        // TODO: do something here to encipher the key value?
                        // The wrapping algorithm may be too close to the targeted Key Store
                        throw new NotSupportedException();
                    }
                    else
                    {
                        log.Error("Cannot resolve the wrapping key.");
                    }
                }
                else
                {
                    result = JsonConvert.SerializeObject(keyEntry, typeof(KeyEntry), _jsonSettings);
                }
            }
            else
            {
                log.Error("Cannot resolve the key entry link.");
            }

            log.Info("Key entry link completed.");
            return result;
        }

        public override string? ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? containerSelector, string? divInput = null)
        {
            string? result = null;
            log.Info(String.Format("Resolving key link with Key Entry Identifier `{0}`, Container Selector `{1}`, Div Input `{2}`...", keyIdentifier, containerSelector, divInput));

            if (!CheckKeyEntryExists(keyIdentifier, keClass))
            {
                log.Error(String.Format("The key entry `{0}` do not exists.", keyIdentifier));
                throw new KeyStoreException("The key entry do not exists.");
            }

            var key = GetKey(keyIdentifier, keClass, containerSelector);
            if (key != null)
            {
                log.Info("Key link resolved.");
                result = key.GetAggregatedValue<string>();
            }
            else
            {
                log.Error("Cannot resolve the key link.");
            }

            log.Info("Key link completed.");
            return result;
        }

    }
}