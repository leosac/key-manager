using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
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

        public override string Name => "PKCS#11";

        public override bool CanCreateKeyEntries => true;

        public override bool CanDeleteKeyEntries => true;

        public override bool CheckKeyEntryExists(KeyEntryId identifier)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            log.Info("Closing the key store...");
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

        public override void Create(IChangeKeyEntry keyEntry)
        {
            throw new NotImplementedException();
        }

        public override void Delete(KeyEntryId identifier, bool ignoreIfMissing = false)
        {
            throw new NotImplementedException();
        }

        public override KeyEntry? Get(KeyEntryId identifier)
        {
            throw new NotImplementedException();
        }

        public override IList<KeyEntryId> GetAllSymmetric()
        {
            log.Info("Getting all symmetric key entries...");
            var entries = new List<KeyEntryId>();

            if (_slot == null)
                throw new KeyStoreException("No slot selected.");

            using (var session = _slot.OpenSession(SessionType.ReadOnly))
            {
                session.Login(GetPKCS11Properties().User, GetPKCS11Properties().GetUserPINBytes());

                var attributes = new List<IObjectAttribute>
                {
                    session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
                    session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true)
                };

                var objects = session.FindAllObjects(attributes);
                foreach (var obj in objects)
                {
                    if (obj.ObjectId != CK.CK_INVALID_HANDLE)
                    {
                        var objAttributes = session.GetAttributeValue(obj, new List<CKA>
                        {
                            CKA.CKA_ID,
                            CKA.CKA_LABEL,
                            CKA.CKA_OBJECT_ID
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

                session.CloseSession();
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
            if (prop.SlotId == null && prop.TokenLabel == null && prop.TokenSerial == null)
            {
                _slot = slots[0];
            }
            else
            {
                foreach (var slot in slots)
                {
                    if (prop.SlotId != null && slot.SlotId == prop.SlotId)
                    {
                        _slot = slot;
                        break;
                    }

                    var token = slot.GetTokenInfo();
                    if (token != null)
                    {
                        if (prop.TokenLabel != null && token.Label == prop.TokenLabel)
                        {
                            _slot = slot;
                            break;
                        }

                        if (prop.TokenLabel != null && token.Label == prop.TokenLabel)
                        {
                            _slot = slot;
                            break;
                        }
                    }
                }

                if (_slot == null)
                {
                    log.Error(String.Format("Cannot found expected slot (Id: `{0}`, Token Label: `{1}`, Token Serial: `{2}`).", prop.SlotId, prop.TokenLabel, prop.TokenSerial));
                    throw new KeyStoreException("Cannot found expected slot.");
                }
                else
                {
                    log.Info("Expected slot found.");
                }
            }

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

        public override void Update(IChangeKeyEntry keyEntry, bool ignoreIfMissing = false)
        {
            throw new NotImplementedException();
        }
    }
}
