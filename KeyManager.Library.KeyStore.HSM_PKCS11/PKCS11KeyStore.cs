using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Text;

namespace Leosac.KeyManager.Library.KeyStore.HSM_PKCS11
{
    public class PKCS11KeyStore : KeyStore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private IPkcs11Library? _library;
        private ISlot? _slot;
        private ISession? _session;

        public override string Name => "PKCS#11";

        public override bool CanCreateKeyEntries => true;

        public override bool CanDeleteKeyEntries => true;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric };
        }

        public override Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            return CheckKeyEntryExists(identifier, keClass, out _);
        }

        public Task<bool> CheckKeyEntryExists(KeyEntryId identifier, out IObjectHandle? handle)
        {
            return CheckKeyEntryExists(identifier, null, out handle);
        }

        public Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass? keClass, out IObjectHandle? handle)
        {
            if (_session == null)
            {
                throw new KeyStoreException("No valid session.");
            }

            if (identifier.Handle != null && identifier.Handle is IObjectHandle h)
            {
                handle = h;
                return Task.FromResult(true);
            }

            var attributes = new List<IObjectAttribute>();
            if (!string.IsNullOrEmpty(identifier.Id) && !GetPKCS11Properties().EnforceLabelUse)
            {
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Convert.FromHexString(identifier.Id)));
            }
            if (!string.IsNullOrEmpty(identifier.Label))
            {
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, UTF8Encoding.UTF8.GetBytes(identifier.Label)));
            }

            var objects = new List<IObjectHandle>();
            if (keClass != null)
            {
                if (keClass == KeyEntryClass.Symmetric)
                {
                    attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY));
                }
                else if (keClass == KeyEntryClass.Asymmetric)
                {
                    attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY));
                    objects = _session.FindAllObjects(attributes);
                    attributes[^1] = _session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY);
                }
                else if (keClass == KeyEntryClass.PrivateKey)
                {
                    attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY));
                }
                else if (keClass == KeyEntryClass.PublicKey)
                {
                    attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY));
                }
            }

            if (attributes.Count == 0)
            {
                throw new KeyStoreException("No key identifier.");
            }

            var allObjects = objects.Union(_session.FindAllObjects(attributes));
            handle = allObjects.FirstOrDefault();
            return Task.FromResult(handle != null);
        }

        public override Task Close(bool secretCleanup = true)
        {
            log.Info("Closing the key store...");
            _session?.CloseSession();
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
            return base.Close(secretCleanup);
        }

        public override async Task Create(IChangeKeyEntry change)
        {
            log.Info(string.Format("Creating key entry `{0}`...", change.Identifier));

            if (change is KeyEntry entry && entry.Variant != null && entry.Variant.KeyContainers.Count > 0)
            {
                foreach (var material in entry.Variant.KeyContainers[0].Key.Materials)
                {
                    var rawkey = material.GetValueAsBinary();
                    if (rawkey != null)
                    {
                        var attributes = GetKeyEntryAttributes(entry, true, material.Name);
                        attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, rawkey));
                        _session!.CreateObject(attributes);
                    }
                }
            }
            else if (change is KeyEntryCryptogram cryptogram)
            {
                var wrappingKey = cryptogram.WrappingKey;
                if (!(wrappingKey?.KeyId.IsConfigured()).GetValueOrDefault(false))
                {
                    wrappingKey = Options?.WrappingKey;
                }

                if (wrappingKey?.KeyId == null)
                {
                    log.Error("Wrapping Key Entry Identifier parameter is expected.");
                    throw new KeyStoreException("Wrapping Key Entry Identifier parameter is expected.");
                }

                if (!await CheckKeyEntryExists(wrappingKey.KeyId, out IObjectHandle? wrapHandle))
                {
                    log.Error(string.Format("The key entry `{0}` doesn't exist.", wrappingKey.KeyId));
                    throw new KeyStoreException("The key entry doesn't exist.");
                }
                if (!string.IsNullOrEmpty(cryptogram.Value))
                {
                    var attributes = GetKeyEntryAttributes(null, true);
                    var mechanism = CreateMostExpectedWrappingMechanism(wrapHandle!);
                    _session!.UnwrapKey(mechanism, wrapHandle, Convert.FromHexString(cryptogram.Value), attributes);
                }
            }

            log.Info(string.Format("Key entry `{0}` created.", change.Identifier));
        }

        public override Task<KeyEntryId> Generate(KeyEntryId? identifier, KeyEntryClass keClass)
        {
            if (identifier == null)
            {
                identifier = new KeyEntryId();
            }

            if (keClass != KeyEntryClass.Symmetric)
            {
                log.Error(string.Format("The key store doesn't support key entry generation without specifing the target type for class `{0}`.", keClass));
                throw new NotImplementedException();
            }

            var keyEntry = new SymmetricPKCS11KeyEntry
            {
                Identifier = identifier
            };
            keyEntry.Variant = keyEntry.GetAllVariants(keClass).FirstOrDefault(v => v.Name == "GENERIC_SECRET");
            keyEntry.PKCS11Properties!.Extractable = true;
            return Generate(keyEntry);
        }

        public override Task<KeyEntryId> Generate(KeyEntry keyEntry)
        {
            log.Info(string.Format("Generating key entry `{0}`...", keyEntry.Identifier));

            if(keyEntry.KClass != KeyEntryClass.Symmetric)
            {
                log.Error(string.Format("The key store doesn't support key entry generation for class `{0}`.", keyEntry.KClass));
                throw new NotImplementedException();
            }

            var attributes = GetKeyEntryAttributes(keyEntry, true);
            IMechanism? mechanism = null;
            var key = keyEntry.Variant?.KeyContainers.FirstOrDefault()?.Key;
            if (key != null)
            {
                if (key.Tags.Contains("AES"))
                {
                    mechanism = _session!.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);
                }
                else if (key.Tags.Contains("DES") && key.KeySize == 8)
                {
                    mechanism = _session!.Factories.MechanismFactory.Create(CKM.CKM_DES_KEY_GEN);
                }
                else if (key.Tags.Contains("DES") && key.KeySize == 16)
                {
                    mechanism = _session!.Factories.MechanismFactory.Create(CKM.CKM_DES2_KEY_GEN);
                }
                else if (key.Tags.Contains("DES") && key.KeySize == 24)
                {
                    mechanism = _session!.Factories.MechanismFactory.Create(CKM.CKM_DES3_KEY_GEN);
                }

                if (key.KeySize > 0)
                {
                    attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, key.KeySize));
                }
            }

            mechanism ??= _session!.Factories.MechanismFactory.Create(CKM.CKM_GENERIC_SECRET_KEY_GEN);

            _session!.GenerateKey(mechanism, attributes);
            log.Info(string.Format("Key entry `{0}` generated.", keyEntry.Identifier));
            return Task.FromResult(keyEntry.Identifier);
        }

        private List<IObjectAttribute> GetKeyEntryAttributes(KeyEntry? entry)
        {
            return GetKeyEntryAttributes(entry, false);
        }

        private List<IObjectAttribute> GetKeyEntryAttributes(KeyEntry? entry, bool create)
        {
            return GetKeyEntryAttributes(entry, create, null);
        }

        private List<IObjectAttribute> GetKeyEntryAttributes(KeyEntry? entry, bool create, string? materialName)
        {
            if (entry != null && entry.Variant?.KeyContainers.Count > 1)
            {
                throw new KeyStoreException("This key store do not support key entries with more than one key container.");
            }

            var attributes = new List<IObjectAttribute>();
            if (entry != null)
            {
                if (entry.Identifier.Id != null && create)
                {
                    attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Convert.FromHexString(entry.Identifier.Id)));
                }

                if (entry.Identifier.Label != null)
                {
                    attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, UTF8Encoding.UTF8.GetBytes(entry.Identifier.Label)));
                }
            }
            if (entry is PKCS11KeyEntry pkcsEntry)
            {
                if (create)
                {
                    attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, pkcsEntry.GetCKK()));
                    if (pkcsEntry.PKCS11Properties!.Extractable != null)
                    {
                        attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, pkcsEntry.PKCS11Properties.Extractable.Value));
                    }
                    if (pkcsEntry.PKCS11Properties.Sensitive != null)
                    {
                        attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, pkcsEntry.PKCS11Properties.Sensitive.Value));
                    }
                }
                if (pkcsEntry.PKCS11Properties!.Encrypt != null)
                {
                    attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, pkcsEntry.PKCS11Properties!.Encrypt.Value));
                }
                if (pkcsEntry.PKCS11Properties.Decrypt != null)
                {
                    attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, pkcsEntry.PKCS11Properties.Decrypt.Value));
                }
                if (pkcsEntry.PKCS11Properties.Derive != null)
                {
                    attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, pkcsEntry.PKCS11Properties.Derive.Value));
                }
            }
            else
            {
                if (create && entry != null)
                {
                    attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, PKCS11KeyEntry.GetCKK(entry)));
                }
                attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true));
                attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true));
            }

            if (create)
            {
                attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true));
            }

            var cclass = CKO.CKO_SECRET_KEY;
            if (entry != null && !string.IsNullOrEmpty(materialName))
            {
                if (entry.KClass == KeyEntryClass.PrivateKey || (entry.KClass == KeyEntryClass.Asymmetric && materialName == KeyMaterial.PRIVATE_KEY))
                {
                    cclass = CKO.CKO_PRIVATE_KEY;
                }
                else if (entry.KClass == KeyEntryClass.PublicKey || (entry.KClass == KeyEntryClass.Asymmetric && materialName == KeyMaterial.PUBLIC_KEY))
                {
                    cclass = CKO.CKO_PUBLIC_KEY;
                }
            }

            if (create)
            {
                attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, cclass));
            }

            return attributes;
        }

        public override async Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing)
        {
            log.Info(string.Format("Deleting key entry `{0}`...", identifier));
            var exists = await CheckKeyEntryExists(identifier, keClass, out IObjectHandle? handle);
            if (!exists && !ignoreIfMissing)
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            if (exists)
            {
                _session!.DestroyObject(handle);
                log.Info(string.Format("Key entry `{0}` deleted.", identifier));
            }
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            log.Info(string.Format("Getting key entry `{0}`...", identifier));
            if (!await CheckKeyEntryExists(identifier, keClass, out IObjectHandle? handle))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            var attributes = _session!.GetAttributeValue(handle, new List<CKA>
            {
                CKA.CKA_CLASS
            });
            var cko = (CKO)attributes[0].GetValueAsUlong();

            attributes = _session!.GetAttributeValue(handle, new List<CKA>
            {
                CKA.CKA_KEY_TYPE,
                CKA.CKA_ID,
                CKA.CKA_LABEL
            });

            PKCS11KeyEntry keyEntry;
            if (cko == CKO.CKO_SECRET_KEY)
            {
                keyEntry = new SymmetricPKCS11KeyEntry();
            }
            else if (cko == CKO.CKO_PRIVATE_KEY)
            {
                keyEntry = new AsymmetricPKCS11KeyEntry(KeyEntryClass.PrivateKey);
            }
            else if (cko == CKO.CKO_PUBLIC_KEY)
            {
                keyEntry = new AsymmetricPKCS11KeyEntry(KeyEntryClass.PublicKey);
            }
            else
            {
                throw new KeyStoreException("Unsupported CKO.");
            }

            keyEntry.GetAttributes(_session, handle);
            keyEntry.Identifier = identifier;
            foreach (var attribute in attributes)
            {
                if (attribute.Type == (ulong)CKA.CKA_KEY_TYPE)
                {
                    keyEntry.Variant = keyEntry.CreateVariantFromCKK((CKK)attribute.GetValueAsUlong());
                }
                else if (attribute.Type == (ulong)CKA.CKA_ID)
                {
                    keyEntry.Identifier.Id = Convert.ToHexString(attribute.GetValueAsByteArray());
                }
                else if (attribute.Type == (ulong)CKA.CKA_LABEL)
                {
                    keyEntry.Identifier.Label = attribute.GetValueAsString();
                }
                else
                {
                    throw new KeyStoreException("Unexpected attribute.");
                }
            }

            var materials = keyEntry.Variant!.KeyContainers[0].Key.Materials;
            if (materials.Count > 0)
            {
                if (cko == CKO.CKO_PUBLIC_KEY)
                {
                    materials[0].Name = KeyMaterial.PUBLIC_KEY;
                }
                else if (cko == CKO.CKO_PRIVATE_KEY)
                {
                    materials[0].Name = KeyMaterial.PRIVATE_KEY;
                }
            }

            log.Info(string.Format("Key entry `{0}` retrieved.", identifier));
            return keyEntry;
        }

        public override Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            log.Info(string.Format("Getting all key entries (class: `{0}`)...", keClass));
            IList<KeyEntryId> entries = new List<KeyEntryId>();

            if (_session == null)
            {
                throw new KeyStoreException("No valid session.");
            }

            var attributes = new List<IObjectAttribute>
            {
                _session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true)
            };

            var objects = new List<IObjectHandle>();
            if (keClass != null)
            {
                if (keClass == KeyEntryClass.Symmetric)
                {
                    attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY));
                }
                else if (keClass == KeyEntryClass.Asymmetric)
                {
                    attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY));
                    objects = _session.FindAllObjects(attributes);
                    attributes[1] = _session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY);
                }
                else if (keClass == KeyEntryClass.PrivateKey)
                {
                    attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY));
                }
                else if (keClass == KeyEntryClass.PublicKey)
                {
                    attributes.Add(_session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY));
                }
            }

            var allObjects = objects.Union(_session.FindAllObjects(attributes));
            foreach (var obj in allObjects)
            {
                if (obj.ObjectId != CK.CK_INVALID_HANDLE)
                {
                    var objAttributes = _session.GetAttributeValue(obj, new List<CKA>
                    {
                        CKA.CKA_ID,
                        CKA.CKA_LABEL
                    });

                    var entry = new KeyEntryId
                    {
                        Handle = obj
                    };
                    if (!objAttributes[0].CannotBeRead)
                    {
                        entry.Id = Convert.ToHexString(objAttributes[0].GetValueAsByteArray());
                    }
                    if (!objAttributes[1].CannotBeRead)
                    {
                        entry.Label = objAttributes[1].GetValueAsString();
                    }

                    entries.Add(entry);
                }
                else
                {
                    log.Warn("Object with invalid handle returned. Skipped.");
                }
            }

            log.Info(string.Format("{0} key entries returned.", entries.Count));
            return Task.FromResult(entries);
        }

        public PKCS11KeyStoreProperties GetPKCS11Properties()
        {
            var p = Properties as PKCS11KeyStoreProperties;
            return p ?? throw new KeyStoreException("Missing PKCS#11 key store properties.");
        }

        public override Task Open()
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
                    log.Error(string.Format("Cannot found expected slot (Filter Type: `{0}`, Filter: `{1}`).", prop.SlotFilterType, prop.SlotFilter));
                    throw new KeyStoreException("Cannot found expected slot.");
                }
                log.Info("Expected slot found.");
            }

            _session = _slot.OpenSession(SessionType.ReadWrite);
            _session.Login(GetPKCS11Properties().User, GetPKCS11Properties().GetUserPINBytes());

            log.Info("Key Store opened.");
            return Task.CompletedTask;
        }

        public override async Task<string?> ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput, WrappingKey? wrappingKey)
        {
            log.Info(string.Format("Resolving key entry link with Key Entry Identifier `{0}` and Wrapping Key Entry Identifier `{1}`...", keyIdentifier, wrappingKey?.KeyId));
            if (wrappingKey == null || !wrappingKey.KeyId.IsConfigured())
            {
                log.Error("Wrapping Key Entry Identifier parameter is expected.");
                throw new KeyStoreException("Wrapping Key Entry Identifier parameter is expected.");
            }
            if (!string.IsNullOrEmpty(divInput))
            {
                log.Error("Div Input parameter is not supported.");
                throw new KeyStoreException("Div Input parameter is not supported.");
            }

            if (!await CheckKeyEntryExists(keyIdentifier, keClass, out IObjectHandle? handle))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", keyIdentifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }
            if (!await CheckKeyEntryExists(wrappingKey.KeyId, out IObjectHandle? wrapHandle))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", wrappingKey.KeyId));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            IMechanism mechanism = CreateMostExpectedWrappingMechanism(wrapHandle!);
            log.Info(string.Format("Using mechanism {0}...", (CKM)mechanism.Type));
            var data = _session!.WrapKey(mechanism, wrapHandle, handle);
            log.Info("Key entry link completed.");
            return Convert.ToHexString(data);
        }

        protected IMechanism CreateMostExpectedWrappingMechanism(IObjectHandle handle)
        {
            var attributes = _session!.GetAttributeValue(handle, new List<CKA>
            {
                CKA.CKA_KEY_TYPE
            });
            var ckk = (CKK)attributes[0].GetValueAsUlong();
            var ckm = ckk switch
            {
                CKK.CKK_AES => CKM.CKM_AES_CBC,
                CKK.CKK_DES => CKM.CKM_DES_CBC,
                CKK.CKK_DES2 or CKK.CKK_DES3 => CKM.CKM_DES3_CBC,
                CKK.CKK_DSA => CKM.CKM_DSA,
                CKK.CKK_ECDSA => CKM.CKM_ECDSA,
                CKK.CKK_RSA => CKM.CKM_RSA_PKCS,
                _ => throw new KeyStoreException("Unsupported key type"),
            };
            return _session.Factories.MechanismFactory.Create(ckm);
        }

        public override async Task<string?> ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? containerSelector, string? divInput)
        {
            log.Info(string.Format("Resolving key link with Key Entry Identifier `{0}`...", keyIdentifier));
            if (!string.IsNullOrEmpty(divInput))
            {
                log.Error("Div Input parameter is not supported.");
                throw new KeyStoreException("Div Input parameter is not supported.");
            }

            if (!await CheckKeyEntryExists(keyIdentifier, keClass, out IObjectHandle? handle))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", keyIdentifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            var attributes = _session!.GetAttributeValue(handle, new List<CKA>
            {
                CKA.CKA_VALUE
            });
            log.Info("Key link completed.");
            return Convert.ToHexString(attributes[0].GetValueAsByteArray());
        }

        public override async Task Store(IList<IChangeKeyEntry> changes)
        {
            log.Info(string.Format("Storing `{0}` key entries...", changes.Count));

            foreach (var change in changes)
            {
                if (await CheckKeyEntryExists(change.Identifier, change.KClass))
                {
                    if (!(Options?.GenerateKeys).GetValueOrDefault(false))
                    {
                        await Update(change);
                    }
                    else
                    {
                        string msg = string.Format("Key Entry `{0}` already exists, skipping key generation update.", change.Identifier);
                        log.Info(msg);
                        OnUserMessageNotified(msg);
                    }
                }
                else
                {
                    if ((Options?.GenerateKeys).GetValueOrDefault(false))
                    {
                        if (change is KeyEntry ke)
                        {
                            await Generate(ke);
                        }
                        else
                        {
                            await Generate(change.Identifier, change.KClass);
                        }
                    }
                    else
                    {
                        await Create(change);
                    }
                }
            }

            log.Info("Key Entries storing completed.");
        }

        public override async Task Update(IChangeKeyEntry change, bool ignoreIfMissing)
        {
            log.Info(string.Format("Updating key entry `{0}`...", change.Identifier));

            if (!await CheckKeyEntryExists(change.Identifier, change.KClass, out IObjectHandle? handle))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", change.Identifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            if (change is KeyEntry entry)
            {
                var attributes = GetKeyEntryAttributes(entry);
                if (entry.Variant?.KeyContainers.Count == 1 && !entry.Variant.KeyContainers[0].Key.IsEmpty())
                {
                    var rawkey = entry.Variant.KeyContainers[0].Key.GetAggregatedValueAsBinary();
                    if (rawkey != null)
                    {
                        // We should already have only one key material during an update
                        attributes.Add(_session!.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, rawkey));
                    }
                }
                _session!.SetAttributeValue(handle, attributes);
            }
            else
            {
                throw new NotImplementedException();
            }

            log.Info(string.Format("Key entry `{0}` updated.", change.Identifier));
        }

        public override KeyEntry? GetDefaultKeyEntry(KeyEntryClass keClass)
        {
            var keyEntry = base.GetDefaultKeyEntry(keClass);
            if (keyEntry == null)
            {
                if (keClass == KeyEntryClass.Symmetric)
                {
                    keyEntry = new SymmetricPKCS11KeyEntry();
                }
                else if (keClass == KeyEntryClass.Asymmetric)
                {
                    keyEntry = new AsymmetricPKCS11KeyEntry();
                }
            }
            return keyEntry;
        }
    }
}
