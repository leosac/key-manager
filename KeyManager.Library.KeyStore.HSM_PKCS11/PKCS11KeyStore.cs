using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class PKCS11KeyStore : KeyStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public PKCS11KeyStore()
        {

        }

        private IPkcs11Library? _library;
        private ISlot? _slot;
        private ISession? _session;

        public override string Name => "PKCS#11";

        public override bool CanCreateKeyEntries => true;

        public override bool CanDeleteKeyEntries => true;

        public override bool CheckKeyEntryExists(KeyEntryId identifier)
        {
            IObjectHandle? handle;
            return CheckKeyEntryExists(identifier, out handle);
        }

        public bool CheckKeyEntryExists(KeyEntryId identifier, out IObjectHandle? handle)
        {
            if (_session == null)
                throw new KeyStoreException("No valid session.");

            var attributes = new List<IObjectAttribute>();
            if (!string.IsNullOrEmpty(identifier.Id))
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Convert.FromHexString(identifier.Id)));
            if (!string.IsNullOrEmpty(identifier.Label))
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, UTF8Encoding.UTF8.GetBytes(identifier.Label)));

            if (attributes.Count == 0)
                throw new KeyStoreException("No key identifier.");

            var objects = _session.FindAllObjects(attributes);
            if (objects.Count > 0)
            {
                handle = objects[0];
                return true;
            }

            handle = null;
            return false;
        }

        public override void Close()
        {
            log.Info("Closing the key store...");
            if (_session != null)
            {
                _session.CloseSession();
            }
            if (_slot != null)
            {
                _slot.CloseAllSessions();
                _slot = null;
            }
            if (_library != null)
            {
                _library.Dispose();
                _library = null;
            }
            log.Info("Key Store closed.");
        }

        public override void Create(IChangeKeyEntry change)
        {
            log.Info(String.Format("Creating key entry `{0}`...", change.Identifier));
            if (CheckKeyEntryExists(change.Identifier))
            {
                log.Error(String.Format("A key entry with the same identifier `{0}` already exists.", change.Identifier));
                throw new KeyStoreException("A key entry with the same identifier already exists.");
            }

            if (change is KeyEntry entry)
            {
                var attributes = GetKeyEntryAttributes(entry);
                _session!.CreateObject(attributes);
            }
            else
                throw new NotImplementedException();

            log.Info(String.Format("Key entry `{0}` created.", change.Identifier));
        }

        private List<IObjectAttribute> GetKeyEntryAttributes(KeyEntry entry)
        {
            var attributes = new List<IObjectAttribute>();
            if (entry.Identifier.Id != null)
                attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Convert.FromHexString(entry.Identifier.Id)));
            if (entry.Identifier.Label != null)
                attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, UTF8Encoding.UTF8.GetBytes(entry.Identifier.Label)));
            attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY));
            if (entry is PKCS11KeyEntry pkcsEntry)
            {
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, pkcsEntry.GetCKK()));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, pkcsEntry.PKCS11Properties!.Encrypt));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, pkcsEntry.PKCS11Properties.Decrypt));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, pkcsEntry.PKCS11Properties.Derive));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, pkcsEntry.PKCS11Properties.Extractable));
            }
            else
            {
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true));
            }
            if (entry.Variant?.KeyVersions.Count > 0 && !string.IsNullOrEmpty(entry.Variant.KeyVersions[0].Key.Value))
            {
                var key = Convert.FromHexString(entry.Variant.KeyVersions[0].Key.Value);
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (ulong)key.Length));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, key));
            }
            return attributes;
        }

        public override void Delete(KeyEntryId identifier, bool ignoreIfMissing = false)
        {
            log.Info(String.Format("Deleting key entry `{0}`...", identifier));
            IObjectHandle? handle;
            var exists = CheckKeyEntryExists(identifier, out handle);
            if (!exists && !ignoreIfMissing)
            {
                log.Error(String.Format("The key entry `{0}` doesn't exist.", identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            if (exists)
            {
                _session!.DestroyObject(handle);
                log.Info(String.Format("Key entry `{0}` deleted.", identifier));
            }
        }

        public override KeyEntry? Get(KeyEntryId identifier)
        {
            log.Info(String.Format("Getting key entry `{0}`...", identifier));
            IObjectHandle? handle;
            if (!CheckKeyEntryExists(identifier, out handle))
            {
                log.Error(String.Format("The key entry `{0}` doesn't exist.", identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            var attributes = _session!.GetAttributeValue(handle, new List<CKA>
            {
                CKA.CKA_ID,
                CKA.CKA_LABEL,
                CKA.CKA_KEY_TYPE,
                CKA.CKA_ENCRYPT,
                CKA.CKA_DECRYPT,
                CKA.CKA_DERIVE,
                CKA.CKA_EXTRACTABLE,
            });

            var keyEntry = new PKCS11KeyEntry();
            foreach (var attribute in attributes)
            {
                if (attribute.Type == (ulong)CKA.CKA_KEY_TYPE)
                    keyEntry.Variant = PKCS11KeyEntry.CreateVariantFromCKK((CKK)attribute.GetValueAsUlong());
                else if (attribute.Type == (ulong)CKA.CKA_ID)
                    keyEntry.Identifier.Id = Convert.ToHexString(attribute.GetValueAsByteArray());
                else if (attribute.Type == (ulong)CKA.CKA_LABEL)
                    keyEntry.Identifier.Label = attribute.GetValueAsString();
                else if (attribute.Type == (ulong)CKA.CKA_ENCRYPT)
                    keyEntry.PKCS11Properties!.Encrypt = attribute.GetValueAsBool();
                else if (attribute.Type == (ulong)CKA.CKA_DECRYPT)
                    keyEntry.PKCS11Properties!.Decrypt = attribute.GetValueAsBool();
                else if (attribute.Type == (ulong)CKA.CKA_DERIVE)
                    keyEntry.PKCS11Properties!.Derive = attribute.GetValueAsBool();
                else if (attribute.Type == (ulong)CKA.CKA_EXTRACTABLE)
                    keyEntry.PKCS11Properties!.Extractable = attribute.GetValueAsBool();
                else
                    throw new KeyStoreException("Unexpected attribute.");
            }

            log.Info(String.Format("Key entry `{0}` retrieved.", identifier));
            return keyEntry;
        }

        public override IList<KeyEntryId> GetAllSymmetric()
        {
            log.Info("Getting all symmetric key entries...");
            var entries = new List<KeyEntryId>();

            if (_session == null)
                throw new KeyStoreException("No valid session.");

            var attributes = new List<IObjectAttribute>
            {
                _session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
                _session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true)
            };

            var objects = _session.FindAllObjects(attributes);
            foreach (var obj in objects)
            {
                if (obj.ObjectId != CK.CK_INVALID_HANDLE)
                {
                    var objAttributes = _session.GetAttributeValue(obj, new List<CKA>
                    {
                        CKA.CKA_ID,
                        CKA.CKA_LABEL
                    });

                    var entry = new KeyEntryId();
                    if (!objAttributes[0].CannotBeRead)
                        entry.Id = Convert.ToHexString(objAttributes[0].GetValueAsByteArray());
                    if (!objAttributes[1].CannotBeRead)
                        entry.Label = objAttributes[1].GetValueAsString();
                }
                else
                {
                    log.Warn("Object with invalid handle returned. Skipped.");
                }
            }

            log.Info(String.Format("{0} key entries returned.", entries.Count));
            return entries;
        }

        public PKCS11KeyStoreProperties GetPKCS11Properties()
        {
            var p = Properties as PKCS11KeyStoreProperties;
            if (p == null)
                throw new KeyStoreException("Missing PKCS#11 key store properties.");
            return p;
        }

        public override void Open()
        {
            log.Info("Opening the key store...");
            var factories = new Pkcs11InteropFactories();
            _library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories, GetPKCS11Properties().LibraryPath, AppType.MultiThreaded);

            var slots = _library.GetSlotList(SlotsType.WithTokenPresent);
            if (slots.Count == 0)
            {
                log.Error("No slots with token available.");
                throw new KeyStoreException("No slots with token available.");
            }

            var prop = GetPKCS11Properties();
            if (string.IsNullOrEmpty(prop.SlotFilter))
            {
                _slot = slots[0];
            }
            else
            {
                foreach (var slot in slots)
                {
                    if (prop.SlotFilterType == SlotFilterType.SlotId && prop.SlotFilter == slot.SlotId.ToString())
                    {
                        _slot = slot;
                        break;
                    }

                    var token = slot.GetTokenInfo();
                    if (token != null)
                    {
                        if (prop.SlotFilterType == SlotFilterType.TokenLabel && token.Label == prop.SlotFilter)
                        {
                            _slot = slot;
                            break;
                        }

                        if (prop.SlotFilterType == SlotFilterType.TokenSerial && token.SerialNumber == prop.SlotFilter)
                        {
                            _slot = slot;
                            break;
                        }
                    }
                }

                if (_slot == null)
                {
                    log.Error(String.Format("Cannot found expected slot (Filter Type: `{0}`, Filter: `{1}`).", prop.SlotFilterType, prop.SlotFilter));
                    throw new KeyStoreException("Cannot found expected slot.");
                }
                log.Info("Expected slot found.");
            }

            _session = _slot.OpenSession(SessionType.ReadWrite);
            _session.Login(GetPKCS11Properties().User, GetPKCS11Properties().GetUserPINBytes());

            log.Info("Key Store opened.");
        }

        public override string? ResolveKeyEntryLink(KeyEntryId keyIdentifier, string? divInput = null, KeyEntryId? wrappingKeyId = null, byte wrappingKeyVersion = 0)
        {
            throw new NotImplementedException();
        }

        public override string? ResolveKeyLink(KeyEntryId keyIdentifier, byte keyVersion, string? divInput = null)
        {
            throw new NotImplementedException();
        }

        public override void Store(IList<IChangeKeyEntry> changes)
        {
            throw new NotImplementedException();
        }

        public override void Update(IChangeKeyEntry change, bool ignoreIfMissing = false)
        {
            log.Info(String.Format("Updating key entry `{0}`...", change.Identifier));

            IObjectHandle? handle;
            if (!CheckKeyEntryExists(change.Identifier, out handle))
            {
                log.Error(String.Format("The key entry `{0}` doesn't exist.", change.Identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            if (change is KeyEntry entry)
            {
                var attributes = GetKeyEntryAttributes(entry);
                _session!.SetAttributeValue(handle, attributes);
            }
            else
                throw new NotImplementedException();

            log.Info(String.Format("Key entry `{0}` updated.", change.Identifier));
        }
    }
}
