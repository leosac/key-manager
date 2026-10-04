using Leosac.KeyManager.Library.Crypto;
using LibLogicalAccess;
using LibLogicalAccess.Card;
using System.Security.Cryptography;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyStore : KeyStore
    {
        public static uint SAM_AV2_MAX_SYMMETRIC_ENTRIES => 128;
        public static byte SAM_AV2_MAX_USAGE_COUNTERS => 16;
        public static uint SAM_AV2_MAX_ASYMMETRIC_RSA_ENTRIES => 3;
        public static uint SAM_AV3_MAX_ASYMMETRIC_ECC_ENTRIES => 8;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private const string ATTRIBUTE_UID = "uid";
        private const string AV1_DEPRECATED_MSG = "Inserted SAM is not in AV2 mode, AV1 support has been deprecated, please check to option to auto switch to AV2 or manually perform a Switch.";
        private bool _unlocked;

        public LibLogicalAccess.ReaderProvider? ReaderProvider { get; private set; }
        public LibLogicalAccess.ReaderUnit? ReaderUnit { get; private set; }
        public LibLogicalAccess.Chip? Chip { get; private set; }

        public SAMKeyStoreProperties GetSAMProperties()
        {
            var p = Properties as SAMKeyStoreProperties;
            return p ?? throw new KeyStoreException("Missing SAM key store properties.");
        }

        public override string Name => "NXP SAM AV2/AV3";

        public override bool CanCreateKeyEntries => false;

        public override bool CanDeleteKeyEntries => false;

        public override bool CanDefineKeyEntryLabel => false;

        public override bool IsNumericKeyId => true;

        public override bool SupportsBatching => true;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => [KeyEntryClass.Symmetric, KeyEntryClass.Asymmetric];
        }

        public override Task Open()
        {
            log.Info("Opening the key store...");
            var lla = LibLogicalAccess.LibraryManager.getInstance();
            var providerName = GetSAMProperties().ReaderProvider;
            if (string.IsNullOrEmpty(providerName))
            {
                providerName = "PCSC";
            }
            ReaderProvider = lla.getReaderProvider(providerName);
            if (ReaderProvider == null)
            {
                log.Error(string.Format("Cannot initialize the Reader Provider `{0}`.", providerName));
                throw new KeyStoreException("Cannot initialize the Reader Provider.");
            }

            var ruName = GetSAMProperties().ReaderUnit;
            if (string.IsNullOrEmpty(ruName))
            {
                ReaderUnit = ReaderProvider.createReaderUnit();
            }
            else
            {
                var readers = ReaderProvider.getReaderList();
                foreach (var reader in readers)
                {
                    if (reader.getName() == ruName)
                    {
                        ReaderUnit = reader;
                        break;
                    }
                }
            }
            if (ReaderUnit == null)
            {
                log.Error("Cannot initialize the Reader Unit.");
                throw new KeyStoreException("Cannot initialize the Reader Unit."); 
            }

            if (!ReaderUnit.connectToReader())
            {
                log.Error("Cannot connect to the Reader Unit.");
                throw new KeyStoreException("Cannot connect to the Reader Unit.");
            }

            var cardType = GetSAMProperties().ForceCardType;
            if (!string.IsNullOrEmpty(cardType))
            {
                ReaderUnit.setCardType(cardType);
            }

            if (ReaderUnit.waitInsertion(1000))
            {
                if (ReaderUnit.connect())
                {
                    var chip = ReaderUnit.getSingleChip();
                    var genericType = chip.getGenericCardType();
                    if (string.Compare(genericType, "SAM") >= 0)
                    {
                        Chip = chip;

                        var cmd = chip.getCommands();
                        LibLogicalAccess.Card.SAMVersion? version = null;
                        if (cmd is LibLogicalAccess.Reader.SAMAV1ISO7816Commands av1cmd)
                        {
                            version = av1cmd.getVersion();
                        }
                        else if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
                        {
                            version = av2cmd.getVersion();
                        }

                        if (version != null)
                        {
                            Attributes[ATTRIBUTE_UID] = Convert.ToHexString(version.manufacture.uniqueserialnumber);
                            log.Info(string.Format("SAM Version {0}.{1}, UID: {2}", version.software.majorversion, version.software.minorversion, Attributes[ATTRIBUTE_UID]));
                        }
                    }
                    else
                    {
                        Close();
                        log.Error(string.Format("The card detected `{0}` is not a SAM.", genericType));
                        throw new KeyStoreException("The card detected is not a SAM.");
                    }
                }
                else
                {
                    Close();
                    log.Error("Cannot connect to the SAM card.");
                    throw new KeyStoreException("Cannot connect to the SAM card.");
                }
            }
            else
            {
                Close();
                log.Error("No SAM has been detected on the reader.");
                throw new KeyStoreException("No SAM has been detected on the reader.");
            }
            log.Info("Key Store opened.");
            return Task.CompletedTask;
        }

        public override Task Close(bool secretCleanup = true)
        {
            log.Info("Closing the key store...");
            if (ReaderUnit != null)
            {
                if (Chip != null)
                {
                    ReaderUnit.disconnect();
                    ReaderUnit.waitRemoval(1);
                    Chip = null;
                }
                ReaderUnit.disconnectFromReader();
                ReaderUnit.Dispose();
                ReaderUnit = null;
            }

            if (ReaderProvider != null)
            {
                ReaderProvider.release();
                ReaderProvider.Dispose();
                ReaderProvider = null;
            }

            if (Attributes.ContainsKey(ATTRIBUTE_UID))
            {
                Attributes.Remove(ATTRIBUTE_UID);
            }

            _unlocked = false;
            log.Info("Key Store closed.");
            return base.Close(secretCleanup);
        }

        public override Task<bool> CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            if (!string.IsNullOrEmpty(identifier.Label))
            {
                log.Warn("KeyEntry label specified but such key resolution is not supported by the key store type.");
            }
            if (identifier.Id == null)
            {
                return Task.FromResult(false);
            }

            if (keClass == KeyEntryClass.Symmetric)
            {
                return Task.FromResult(identifier.NumericId < SAM_AV2_MAX_SYMMETRIC_ENTRIES);
            }
            else if (keClass == KeyEntryClass.Asymmetric)
            {
                if (identifier.IdPrefix == "RSA")
                {
                    return Task.FromResult(identifier.NumericId < SAM_AV2_MAX_ASYMMETRIC_RSA_ENTRIES);
                }
                else if (identifier.IdPrefix == "ECC")
                {
                    return Task.FromResult(identifier.NumericId < SAM_AV3_MAX_ASYMMETRIC_ECC_ENTRIES);
                }
            }

            return Task.FromResult(false);
        }

        public static bool CheckKeyUsageCounterExists(byte identifier)
        {
            return (identifier < SAM_AV2_MAX_USAGE_COUNTERS);
        }

        public override Task Create(IChangeKeyEntry change)
        {
            log.Info(string.Format("Creating key entry `{0}`...", change.Identifier));
            log.Error("A SAM key entry cannot be created, only updated.");
            throw new KeyStoreException("A SAM key entry cannot be created, only updated.");
        }

        public override Task Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing)
        {
            log.Info(string.Format("Deleting key entry `{0}`...", identifier));
            log.Error("A SAM key entry cannot be deleted, only updated.");
            throw new KeyStoreException("A SAM key entry cannot be deleted, only updated.");
        }

        public override Task<byte[]?> GenerateBytes(byte size)
        {
            var cmd = Chip?.getCommands();
            if (cmd == null)
            {
                log.Error("No Command associated with the SAM chip.");
                throw new KeyStoreException("No Command associated with the SAM chip.");
            }

            if (cmd is not LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
            {
                log.Error("Unexpected Command associated with the SAM chip.");
                throw new KeyStoreException("Unexpected Command associated with the SAM chip.");
            }

            if (!_unlocked)
            {
                UnlockSAM(av2cmd, GetSAMProperties().AuthenticationMode, GetSAMProperties().AuthenticateKeyEntryIdentifier, GetAuthenticationKey());
                _unlocked = true;
            }

            return Task.FromResult<byte[]?>(av2cmd.getRandom(size).ToArray());
        }

        protected override Task GenerateCore(KeyEntry keyEntry, bool update)
        {
            if (keyEntry.KClass == KeyEntryClass.Asymmetric && keyEntry.Variant?.Name == "RSA")
            {
                var cmd = Chip?.getCommands();
                if (cmd == null)
                {
                    log.Error("No Command associated with the SAM chip.");
                    throw new KeyStoreException("No Command associated with the SAM chip.");
                }

                if (cmd is not LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
                {
                    log.Error("Unexpected Command associated with the SAM chip.");
                    throw new KeyStoreException("Unexpected Command associated with the SAM chip.");
                }

                byte keyNoCEK = 0;
                byte keyVCEK = 0;
                byte refNoKUC = 0xff;
                PKISet pkiSet = new();
                if (keyEntry is SAMAsymmetricKeyEntry samKeyEntry)
                {
                    if (samKeyEntry.SAMProperties != null)
                    {
                        refNoKUC = samKeyEntry.SAMProperties.KeyUsageCounter ?? 0xff;
                        keyNoCEK = samKeyEntry.SAMProperties.ChangeKeyRefId;
                        keyVCEK = samKeyEntry.SAMProperties.ChangeKeyRefVersion;
                        pkiSet.setAllowPrivateExport(samKeyEntry.SAMProperties.AllowPrivateKeyExport);
                        pkiSet.setEncipherKeyEntries(samKeyEntry.SAMProperties.AllowEncipherKeyEntries);
                        pkiSet.setDisabled(samKeyEntry.SAMProperties.DisableKeyEntry);
                        pkiSet.setEncryptionDisabled(samKeyEntry.SAMProperties.DisableEncryptData);
                        pkiSet.setSignatureDisabled(samKeyEntry.SAMProperties.DisableSignData);
                        pkiSet.setForceHostChange(samKeyEntry.SAMProperties.ForceHostInternalChange);
                        pkiSet.setForceHostUsage(samKeyEntry.SAMProperties.ForceHostInternalUsage);
                        pkiSet.setCRT(true);
                        pkiSet.setPrivateKey(true);
                    }
                }

                av2cmd.authenticateHost(GetAuthenticationKey(), GetSAMProperties().AuthenticateKeyEntryIdentifier);
                av2cmd.PKI_GenerateKeyPair((byte)keyEntry.Identifier.NumericId.GetValueOrDefault(0), pkiSet, keyNoCEK, keyVCEK, refNoKUC, new AEKVAEK());
                return Task.CompletedTask;
            }
            else
            {
                return base.GenerateCore(keyEntry, update);
            }
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            log.Info(string.Format("Getting key entry `{0}`...", identifier));
            if (!await CheckKeyEntryExists(identifier, keClass))
            {
                log.Error(string.Format("The key entry `{0}` do not exists.", identifier));
                throw new KeyStoreException("The key entry do not exists.");
            }

            var cmd = Chip?.getCommands();
            if (cmd == null)
            {
                log.Error("No Command associated with the SAM chip.");
                throw new KeyStoreException("No Command associated with the SAM chip.");
            }

            KeyEntry keyEntry;
            if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
            {
                if (keClass == KeyEntryClass.Symmetric)
                {
                    SAMSymmetricKeyEntry? samKeyEntry = null;
                    var av2entry = av2cmd.getKeyEntry((byte)identifier.NumericId!);
                    var set = av2entry.getSETStruct();
                    samKeyEntry = CreateKeyEntryFromKeyType(av2entry.getKeyType());
                    samKeyEntry.Identifier = identifier;
                    var infoav2 = av2entry.getKeyEntryInformation();
                    ParseKeyEntryProperties(infoav2, set, samKeyEntry.SAMProperties);
                    if (samKeyEntry.Variant != null)
                    {
                        var keysdata = av2entry.getKeysData();
                        var keyVersions = samKeyEntry.Variant.KeyContainers.OfType<KeyVersion>().ToArray();
                        if (keysdata.Count < 1 || keysdata.Count != keyVersions.Length)
                        {
                            log.Error(string.Format("Unexpected number of keys ({0}) on the SAM Key Entry.", keysdata.Count));
                            throw new KeyStoreException("Unexpected number of keys on the SAM Key Entry.");
                        }

                        keyVersions[0].Key.SetAggregatedValueAsString(string.Empty);
                        keyVersions[0].Version = infoav2.vera;
                        keyVersions[0].TrackChanges();
                        if (samKeyEntry.Variant.KeyContainers.Count >= 2)
                        {
                            keyVersions[1].Key.SetAggregatedValueAsString(string.Empty);
                            keyVersions[1].Version = infoav2.verb;
                            keyVersions[1].TrackChanges();
                        }

                        if (samKeyEntry.Variant.KeyContainers.Count >= 3)
                        {
                            keyVersions[2].Key.SetAggregatedValueAsString(string.Empty);
                            keyVersions[2].Version = infoav2.verc;
                            keyVersions[2].TrackChanges();
                        }
                    }
                    keyEntry = samKeyEntry;
                }
                else
                {
                    // Temporary workaround to retrieve the public key of an asymmetric key entry (RSA or ECC) from the SAM
                    // Shouldn't be required on further LLA update
                    av2cmd.authenticateHost(GetAuthenticationKey(), GetSAMProperties().AuthenticateKeyEntryIdentifier);

                    var samKeyEntry = new SAMAsymmetricKeyEntry();
                    if (identifier.IdPrefix == "RSA")
                    {
                        samKeyEntry.Identifier.Id = string.Format("RSA - {0}", identifier.NumericId);
                        samKeyEntry.SetVariant("RSA");
                        try
                        {
                            var pubkey = av2cmd.PKI_ExportPublicKey((byte)identifier.NumericId!);
                            if (pubkey != null)
                            {
                                samKeyEntry.SAMProperties!.KeyUsageCounter = (pubkey.refNoKUC != 0xff) ? pubkey.refNoKUC : null;
                                samKeyEntry.SAMProperties!.ChangeKeyRefId = pubkey.keyNoCEK;
                                samKeyEntry.SAMProperties.ChangeKeyRefVersion = pubkey.keyVCEK;

                                samKeyEntry.SAMProperties.AllowPrivateKeyExport = pubkey.config.privateKeyExportAllowed();
                                samKeyEntry.SAMProperties.DisableEncryptData = pubkey.config.encryptionDisabled();
                                samKeyEntry.SAMProperties.DisableSignData = pubkey.config.signatureDisabled();
                                samKeyEntry.SAMProperties.ForceHostInternalChange = pubkey.config.hostChangeForced();
                                samKeyEntry.SAMProperties.ForceHostInternalUsage = pubkey.config.hostUsageForced();
                                samKeyEntry.SAMProperties.AllowEncipherKeyEntries = pubkey.config.encipherKeyEntriesEnabled();
                                samKeyEntry.SAMProperties.DisableKeyEntry = pubkey.config.disabled();

                                if (pubkey.eLen > 0 && pubkey.nLen > 0)
                                {
                                    var rsa = System.Security.Cryptography.RSA.Create();
                                    rsa.ImportParameters(new RSAParameters
                                    {
                                        Modulus = pubkey.n.ToArray(),
                                        Exponent = pubkey.e.ToArray()
                                    });
                                    var pem = rsa.ExportSubjectPublicKeyInfoPem();
                                    var pubKeyMaterial = samKeyEntry.Variant!.KeyContainers[0].Key.Materials.FirstOrDefault(k => k.Name == KeyMaterial.PUBLIC_KEY);
                                    if (pubKeyMaterial != null)
                                    {
                                        pubKeyMaterial.ValueFormat = KeyValueStringFormat.Pem;
                                        pubKeyMaterial.SetValueAsString(pem, KeyValueStringFormat.Pem);
                                    }
                                }
                            }
                        }
                        catch(LibLogicalAccessException ex)
                        {
                            if (ex.Message.EndsWith("Conditions of use not satisfied, invalid key type, invalid CID, or key limit reached"))
                            {
                                var msg = "Cannot retrieve RSA key public information. Falling back to default value.";
                                log.Warn(msg, ex);
                                OnUserMessageNotified(msg);

                                samKeyEntry.SAMProperties!.DisableKeyEntry = true;
                            }
                            else
                                throw;
                        }
                    }

                    keyEntry = samKeyEntry;
                }
            }
            else if (cmd is LibLogicalAccess.Reader.SAMAV1ISO7816Commands)
            {
                log.Error("SAM is AV1. Please switch the SAM chip to AV2 mode first.");
                throw new KeyStoreException("SAM is AV1. Please switch the SAM chip to AV2 mode first.");
            }
            else
            {
                log.Error("Unexpected Command associated with the SAM chip.");
                throw new KeyStoreException("Unexpected Command associated with the SAM chip.");
            }

            log.Info(string.Format("Key entry `{0}` retrieved.", identifier));
            return keyEntry;
        }

        internal static void ParseKeyEntryProperties(LibLogicalAccess.Card.KeyEntryAV2Information infoav2, LibLogicalAccess.Card.SETAV2 set, SAMSymmetricKeyEntryProperties? properties)
        {
            if (properties != null)
            {
                properties.SAMKeyEntryType = (SAMKeyEntryType)(infoav2.ExtSET & 0x07);

                Array.Copy(infoav2.desfireAid, properties.DESFireAID, 3);
                properties.DESFireKeyNum = infoav2.desfirekeyno;

                properties.KeyUsageCounter = (infoav2.kuc != 0xff) ? infoav2.kuc : null;

                properties.ChangeKeyRefId = infoav2.cekno;
                properties.ChangeKeyRefVersion = infoav2.cekv;

                properties.EnableDumpSessionKey = Convert.ToBoolean(set.dumpsessionkey);
                properties.CryptoBasedOnSecretKey = Convert.ToBoolean(set.allowcrypto);
                properties.DisableDecryptData = Convert.ToBoolean(set.disabledecryption);
                properties.DisableEncryptData = Convert.ToBoolean(set.disableencryption);
                properties.DisableGenerateMACFromPICC = Convert.ToBoolean(set.disablegeneratemac);
                properties.DisableKeyEntry = Convert.ToBoolean(set.disablekeyentry);
                properties.DisableVerifyMACFromPICC = Convert.ToBoolean(set.disableverifymac);
                properties.DisableChangeKeyPICC = Convert.ToBoolean(set.disablewritekeytopicc);
                properties.LockUnlock = Convert.ToBoolean(set.lockkey);
                properties.KeepIV = Convert.ToBoolean(set.keepIV);
                properties.AuthenticateHost = Convert.ToBoolean(set.authkey);
                properties.AllowDumpSecretKey = Convert.ToBoolean(infoav2.ExtSET & 0x08);
                properties.AllowDumpSecretKeyWithDiv = Convert.ToBoolean(infoav2.ExtSET & 0x10);
                properties.ReservedForPerso = Convert.ToBoolean(infoav2.ExtSET & 0x20);
            }
        }

        private static SAMSymmetricKeyEntry CreateKeyEntryFromKeyType(LibLogicalAccess.Card.SAMKeyType keyType)
        {
            var keyEntry = new SAMSymmetricKeyEntry();
            keyEntry.SetVariant(GetVariantName(keyType));
            return keyEntry;
        }

        public static string GetVariantName(LibLogicalAccess.Card.SAMKeyType keyType)
        {
            return keyType switch
            {
                LibLogicalAccess.Card.SAMKeyType.SAM_KEY_DES => "DES",
                LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES128 => "AES128",
                LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES192 => "AES192",
                LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES256 => "AES256",
                LibLogicalAccess.Card.SAMKeyType.SAM_KEY_MIFARE => "MIFARE",
                _ => "TK3DES",
            };
        }

        public override Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            log.Info(string.Format("Getting all key entries (class: `{0}`)...", keClass));
            IList<KeyEntryId> entries = [];
            if (keClass == null || keClass == KeyEntryClass.Symmetric)
            {
                for (uint i = 0; i < SAM_AV2_MAX_SYMMETRIC_ENTRIES; ++i)
                {
                    entries.Add(new KeyEntryId { Id = i.ToString() });
                }
            }
            if (keClass == null || keClass == KeyEntryClass.Asymmetric)
            {
                for (uint i = 0; i < SAM_AV2_MAX_ASYMMETRIC_RSA_ENTRIES; ++i)
                {
                    entries.Add(new KeyEntryId { Id = $"RSA - {i.ToString()}" });
                }
                /*for (uint i = 0; i < SAM_AV3_MAX_ASYMMETRIC_ECC_ENTRIES; ++i)
                {
                    entries.Add(new KeyEntryId { Id = $"ECC - {i.ToString()}" });
                }*/
            }
            log.Info(string.Format("{0} key entries returned.", entries.Count));
            return Task.FromResult(entries);
        }

        public override async Task Store(IList<IChangeKeyEntry> changes)
        {
            log.Info(string.Format("Storing `{0}` key entries...", changes.Count));

            var cmd = Chip?.getCommands();
            bool activated = false;
            if (cmd is LibLogicalAccess.Reader.SAMAV1ISO7816Commands av1cmd)
            {
                if (GetSAMProperties().AutoSwitchToAV2)
                {
                    await SwitchSAMToAV2(av1cmd);
                    cmd = Chip?.getCommands();
                    if (cmd is LibLogicalAccess.Reader.SAMAV1ISO7816Commands)
                    {
                        log.Error("The SAM didn't switched properly to AV2 mode.");
                        throw new KeyStoreException("The SAM didn't switched properly to AV2 mode.");
                    }
                    activated = true;
                }
                else
                {
                    log.Error("Inserted SAM is AV1 mode and Auto Switch to AV2 wasn't enabled.");
                    throw new KeyStoreException("Inserted SAM is AV1 mode and Auto Switch to AV2 wasn't enabled.");
                }
            }
            else if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
            {
                try
                {
                    if (GetSAMProperties().AutoSwitchToAV2)
                    {
                        var version = av2cmd.getVersion();
                        if (version != null && version.manufacture.modecompatibility == 0x03) // Unactivated MIFARE SAM AV3
                        {
                            ActivateMifareSAM(av2cmd);
                            activated = true;
                        }
                    }
                }
                catch(LibLogicalAccessException ex)
                {
                    log.Error("SAM automatic activation failed.", ex);
                }
            }

            // We sort the changes to update change key reference last
            var ochanges = changes.Order(new SAMKeyEntryComparer(GetSAMProperties()));
            await base.Store(ochanges.ToList());
            if (activated)
            {
                // Workaround for AV3 to allow unlock by any Unlock key and not only the SAM Master Key (to have backward compatibility behavior with SAM AV2)...
                if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
                {
                    var mke = changes.FirstOrDefault(c => c.Identifier.Id == "0");
                    if (mke != null && mke is SAMSymmetricKeyEntry smke && smke.SAMProperties != null && smke.SAMProperties.LockUnlock)
                    {
                        log.Info("Explicitly Locking the SAM to ensure auto-lock will not requires the SAM Master Key to unlock but any Unlock Key...");
                        var mkey = new LibLogicalAccess.Card.DESFireKey();
                        if (smke.Variant != null && smke.Variant.KeyContainers.Count > 0)
                        {
                            var container = smke.Variant.KeyContainers[0];
                            mkey.setKeyType(container.Key.Tags.Contains("AES") ? LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES : LibLogicalAccess.Card.DESFireKeyType.DF_KEY_DES);
                            mkey.setKeyVersion((container as KeyVersion)?.Version ?? 0 );
                            if (!container.Key.IsEmpty())
                            { 
                                mkey.fromString(container.Key.GetAggregatedValueAsString(KeyValueStringFormat.HexStringWithSpace));
                            }
                        }
                        av2cmd.lockUnlock(mkey, LibLogicalAccess.Card.SAMLockUnlock.LockWithoutSpecifyingKey, 0, 0, 0);
                    }
                }
            }

            log.Info("Key Entries storing completed.");
        }

        public override Task Update(IChangeKeyEntry change, bool ignoreIfMissing)
        {
            log.Info(string.Format("Updating key entry `{0}`...", change.Identifier));
            var key = GetAuthenticationKey();
            if (change is SAMSymmetricKeyEntry samkey)
            {
                var cmd = Chip?.getCommands();
                if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
                {
                    var natkey = new LibLogicalAccess.Card.AV2SAMKeyEntry();
                    var infoav2 = new LibLogicalAccess.Card.KeyEntryAV2Information();

                    if (samkey.SAMProperties != null)
                    {
                        infoav2.ExtSET |= (byte)samkey.SAMProperties.SAMKeyEntryType;

                        if (samkey.SAMProperties.DESFireAID != null && samkey.SAMProperties.DESFireAID.Length == 3)
                        {
                            infoav2.desfireAid = samkey.SAMProperties.DESFireAID;
                        }
                        infoav2.desfirekeyno = samkey.SAMProperties.DESFireKeyNum;

                        infoav2.kuc = samkey.SAMProperties.KeyUsageCounter ?? 0xff;

                        infoav2.cekno = samkey.SAMProperties.ChangeKeyRefId;
                        infoav2.cekv = samkey.SAMProperties.ChangeKeyRefVersion;
                        infoav2.ExtSET |= (byte)(Convert.ToByte(samkey.SAMProperties.AllowDumpSecretKey) << 3);
                        infoav2.ExtSET |= (byte)(Convert.ToByte(samkey.SAMProperties.AllowDumpSecretKeyWithDiv) << 4);
                        infoav2.ExtSET |= (byte)(Convert.ToByte(samkey.SAMProperties.ReservedForPerso) << 5);
                    }

                    var updateSettings = new LibLogicalAccess.Card.KeyEntryUpdateSettings
                    {
                        df_aid_keyno = 1,
                        key_no_v_cek = 1,
                        refkeykuc = 1,
                        keyversionsentseparatly = 1,
                        updateset = 1
                    };

                    if (samkey.Variant != null)
                    {
                        var containers = samkey.Variant.KeyContainers;
                        var keys = new LibLogicalAccess.UCharCollectionCollection(containers.Count)
                        {
                            new LibLogicalAccess.ByteVector(containers[0].Key.GetAggregatedValueAsBinary(true))
                        };
                        if (containers[0].IsConfigured())
                        {
                            log.Info("Updating value for key version A.");
                            updateSettings.keyVa = 1;
                        }
                        if (containers[0] is KeyVersion keyVersionA)
                        {
                            infoav2.vera = keyVersionA.Version;
                        }
                        if (containers.Count >= 2)
                        {
                            if (containers[1].IsConfigured())
                            {
                                log.Info("Updating value for key version B.");
                                updateSettings.keyVb = 1;
                            }
                            keys.Add(new LibLogicalAccess.ByteVector(containers[1].Key.GetAggregatedValueAsBinary(true)));
                            if (containers[1] is KeyVersion keyVersionB)
                            {
                                infoav2.verb = keyVersionB.Version;
                            }

                            if (containers.Count >= 3)
                            {
                                if (containers[2].IsConfigured())
                                {
                                    log.Info("Updating value for key version C.");
                                    updateSettings.keyVc = 1;
                                }
                                keys.Add(new LibLogicalAccess.ByteVector(containers[2].Key.GetAggregatedValueAsBinary(true)));
                                if (containers[2] is KeyVersion keyVersionC)
                                {
                                    infoav2.verc = keyVersionC.Version;
                                }
                            }
                        }

                        var samkt = LibLogicalAccess.Card.SAMKeyType.SAM_KEY_DES;
                        if (containers[0].Key.Tags.Contains("AES"))
                        {
                            if (containers[0].Key.KeySize == 256)
                            {
                                samkt = LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES256;
                            }
                            else if (containers[0].Key.KeySize == 192)
                            {
                                samkt = LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES192;
                            }
                            else
                            {
                                samkt = LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES128;
                            }
                        }
                        else if (containers[0].Key.Tags.Contains("MIFARE"))
                        {
                            samkt = LibLogicalAccess.Card.SAMKeyType.SAM_KEY_MIFARE;
                        }
                        else
                        {
                            if (containers[0].Key.KeySize > 128)
                            {
                                samkt = LibLogicalAccess.Card.SAMKeyType.SAM_KEY_3K3DES;
                            }
                        }
                        natkey.setKeysData(keys, samkt);
                    }
                    natkey.setKeyEntryInformation(infoav2);

                    if (samkey.SAMProperties != null)
                    {
                        var set = new LibLogicalAccess.Card.SETAV2
                        {
                            dumpsessionkey = Convert.ToByte(samkey.SAMProperties.EnableDumpSessionKey),
                            allowcrypto = Convert.ToByte(samkey.SAMProperties.CryptoBasedOnSecretKey),
                            disabledecryption = Convert.ToByte(samkey.SAMProperties.DisableDecryptData),
                            disableencryption = Convert.ToByte(samkey.SAMProperties.DisableEncryptData),
                            disablegeneratemac = Convert.ToByte(samkey.SAMProperties.DisableGenerateMACFromPICC),
                            disablekeyentry = Convert.ToByte(samkey.SAMProperties.DisableKeyEntry),
                            disableverifymac = Convert.ToByte(samkey.SAMProperties.DisableVerifyMACFromPICC),
                            disablewritekeytopicc = Convert.ToByte(samkey.SAMProperties.DisableChangeKeyPICC),
                            lockkey = Convert.ToByte(samkey.SAMProperties.LockUnlock),
                            keepIV = Convert.ToByte(samkey.SAMProperties.KeepIV),
                            authkey = Convert.ToByte(samkey.SAMProperties.AuthenticateHost)
                        };
                        natkey.setSET(set);
                    }
                    natkey.setSETKeyTypeFromKeyType();
                    // We don't take care of AuthenticationMode here as key entry update always requires Host Authentication
                    av2cmd.authenticateHost(key, GetSAMProperties().AuthenticateKeyEntryIdentifier);
                    natkey.setUpdateSettings(updateSettings); // Or call setUpdateMask
                    av2cmd.changeKeyEntry((byte)Convert.ToDecimal(samkey.Identifier.Id), natkey, key);
                }
                else
                {
                    log.Error(AV1_DEPRECATED_MSG);
                    throw new KeyStoreException(AV1_DEPRECATED_MSG);
                }
            }
            else if (change is SAMAsymmetricKeyEntry asamkey)
            {
                if (asamkey.Variant == null)
                {
                    log.Error("Asymmetric key entry variant is not specified.");
                    throw new KeyStoreException("Asymmetric key entry variant is not specified.");
                }

                var cmd = Chip?.getCommands();
                if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
                {
                    if (asamkey.Variant.Name == "RSA")
                    {
                        byte kuc = 0xff, cekno = 0x00, cekv = 0x00;
                        PKISet pkiSet = new PKISet();
                        if (asamkey.SAMProperties != null)
                        {
                            kuc = asamkey.SAMProperties.KeyUsageCounter ?? 0xff;
                            cekno = asamkey.SAMProperties.ChangeKeyRefId;
                            cekv = asamkey.SAMProperties.ChangeKeyRefVersion;

                            pkiSet.setAllowPrivateExport(asamkey.SAMProperties.AllowPrivateKeyExport);
                            pkiSet.setEncipherKeyEntries(asamkey.SAMProperties.AllowEncipherKeyEntries);
                            pkiSet.setDisabled(asamkey.SAMProperties.DisableKeyEntry);
                            pkiSet.setEncryptionDisabled(asamkey.SAMProperties.DisableEncryptData);
                            pkiSet.setSignatureDisabled(asamkey.SAMProperties.DisableSignData);
                            pkiSet.setForceHostChange(asamkey.SAMProperties.ForceHostInternalChange);
                            pkiSet.setForceHostUsage(asamkey.SAMProperties.ForceHostInternalUsage);
                        }

                        bool updateSettingsOnly = true;
                        var n = new ByteVector();
                        var e = new ByteVector();
                        var p = new ByteVector();
                        var q = new ByteVector();
                        var dP = new ByteVector();
                        var dQ = new ByteVector();
                        var ipq = new ByteVector();
                        var containers = asamkey.Variant.KeyContainers;
                        if (containers.Count > 0)
                        {
                            if (containers[0].IsConfigured())
                            {
                                log.Info("Updating value for key");
                                updateSettingsOnly = false;

                                var hasPrivateKey = containers[0].Key.Materials.Any(k => k.Name == KeyMaterial.PRIVATE_KEY && !string.IsNullOrEmpty(k.Value));
                                if (hasPrivateKey)
                                {
                                    pkiSet.setPrivateKey(hasPrivateKey);
                                    pkiSet.setCRT(true);
                                }
                                var pem = containers[0].Key.GetAggregatedValueAsString(KeyValueStringFormat.Pem);
                                var rsa = System.Security.Cryptography.RSA.Create();
                                rsa.ImportFromPem(pem);
                                var rsaParams = rsa.ExportParameters(hasPrivateKey);
                                if (rsaParams.Modulus != null)
                                    n = [.. rsaParams.Modulus];
                                if (rsaParams.Exponent != null)
                                {
                                    e = [.. rsaParams.Exponent];
                                    if (e.Count == 3 && e[0] == 0x01 && e[1] == 0x00 && e[2] == 0x01)
                                    {
                                        // we add the leading 0x00 byte to avoid the exponent being interpreted as a negative number
                                        e.Insert(0, 0x00);
                                    }
                                }
                                if (rsaParams.P != null)
                                    p = [.. rsaParams.P];
                                if (rsaParams.Q != null)
                                    q = [.. rsaParams.Q];
                                if (rsaParams.DP != null)
                                    dP = [.. rsaParams.DP];
                                if (rsaParams.DQ != null)
                                    dQ = [.. rsaParams.DQ];
                                if (rsaParams.InverseQ != null)
                                    ipq = [.. rsaParams.InverseQ];
                            }
                        }

                        // We don't take care of AuthenticationMode here as key entry update always requires Host Authentication
                        av2cmd.authenticateHost(key, GetSAMProperties().AuthenticateKeyEntryIdentifier);
                        av2cmd.PKI_ImportKey((byte)asamkey.Identifier.NumericId.GetValueOrDefault(0), pkiSet, cekno, cekv, kuc, n, e, p, q, dP, dQ, ipq);
                    }
                    else
                    {
                        log.Error(string.Format("Unsupported asymmetric key entry variant `{0}`.", asamkey.Variant.Name));
                        throw new KeyStoreException(string.Format("Unsupported asymmetric key entry variant `{0}`.", asamkey.Variant.Name));
                    }
                }
                else
                {
                    log.Error(AV1_DEPRECATED_MSG);
                    throw new KeyStoreException(AV1_DEPRECATED_MSG);
                }
            }
            else if (change is KeyEntryCryptogram cryptogram)
            {
                var cmd = Chip?.getCommands();
                if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
                {
                    av2cmd.authenticateHost(key, GetSAMProperties().AuthenticateKeyEntryIdentifier);
                    //av2cmd.activateOfflineKey();
                    //av2cmd.changeKeyEntryOffline();
                    throw new NotImplementedException();
                }
            }
            else
            {
                log.Error("Unsupported Key Entry type for this Key Store.");
                throw new KeyStoreException("Unsupported Key Entry type for this Key Store.");
            }

            OnKeyEntryUpdated(change);
            log.Info(string.Format("Key entry `{0}` updated.", change.Identifier));
            return Task.CompletedTask;
        }

        public async Task SwitchSAMToAV2(LibLogicalAccess.Reader.SAMAV1ISO7816Commands av1cmd)
        {
            SwitchSAMToAV2(av1cmd, GetSAMProperties().AuthenticateKeyEntryIdentifier, GetSAMProperties().AuthenticateKeyType, GetSAMProperties().AuthenticateKeyVersion, Properties?.Secret);
            await Close();
            await Open();
        }

        public static void SwitchSAMToAV2(LibLogicalAccess.Reader.SAMAV1ISO7816Commands av1cmd, byte keyno, LibLogicalAccess.Card.DESFireKeyType keyType, byte keyVersion, string? keyValue)
        {
            log.Info("Switching the SAM to AV2 mode...");
            var keyav1entry = av1cmd.getKeyEntry(keyno);
            if (string.IsNullOrEmpty(keyValue))
            {
                keyValue = "00000000000000000000000000000000";
                keyType = keyav1entry.getKeyType() switch
                {
                    LibLogicalAccess.Card.SAMKeyType.SAM_KEY_3K3DES => LibLogicalAccess.Card.DESFireKeyType.DF_KEY_3K3DES,
                    LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES128 => LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES,
                    _ => LibLogicalAccess.Card.DESFireKeyType.DF_KEY_DES,
                };
            }
            var key = CreateDESFireKey(keyType, keyVersion, keyValue);
            if (keyType != LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES)
            {
                var kb = Convert.FromHexString(keyValue);
                var keys = new LibLogicalAccess.UCharCollectionCollection(3)
                {
                    new LibLogicalAccess.ByteVector(kb),
                    new LibLogicalAccess.ByteVector(kb),
                    new LibLogicalAccess.ByteVector(kb)
                };

                keyav1entry.setKeysData(keys, LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES128);
                var keyInfo = keyav1entry.getKeyEntryInformation();
                keyInfo.vera = keyVersion;
                keyInfo.verb = keyVersion;
                keyInfo.verc = keyVersion;
                keyInfo.cekno = keyno;
                keyInfo.cekv = keyVersion;
                keyav1entry.setKeyEntryInformation(keyInfo);
                keyav1entry.setUpdateMask(0xff);

                av1cmd.authenticateHost(key, keyno);
                av1cmd.changeKeyEntry(keyno, keyav1entry, key);

                key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES);
            }

            av1cmd.authenticateHost(key, keyno);
            av1cmd.lockUnlock(key, LibLogicalAccess.Card.SAMLockUnlock.SwitchAV2Mode, keyno, 0, 0);
            log.Info("SAM switched to AV2 mode.");
        }

        public static LibLogicalAccess.Card.DESFireKey CreateDESFireKey(LibLogicalAccess.Card.DESFireKeyType keyType, byte keyVersion, string? keyValue)
        {
            var key = new LibLogicalAccess.Card.DESFireKey();
            key.setKeyVersion(keyVersion);
            key.setKeyType(keyType);
            if (!string.IsNullOrEmpty(keyValue))
            {
                key.fromString(KeyMaterial.ConvertValueFormat(keyValue, KeyValueStringFormat.HexStringWithSpace));
            }
            return key;
        }

        public LibLogicalAccess.Card.DESFireKey GetAuthenticationKey()
        {
            var key = new LibLogicalAccess.Card.DESFireKey();
            key.setKeyType(GetSAMProperties().AuthenticateKeyType);
            key.setKeyVersion(GetSAMProperties().AuthenticateKeyVersion);
            if (!string.IsNullOrEmpty(Properties?.Secret))
            {
                var kv = Properties.Secret;
                if (GetSAMProperties().AuthenticationDivInput.Count > 0)
                {
                    var divContext = new DivInput.DivInputContext
                    {
                        KeyStore = this
                    };
                    var input = ComputeDivInput(divContext, GetSAMProperties().AuthenticationDivInput);
                    if (!string.IsNullOrEmpty(input))
                    {
                        kv = AN10922KeyDiversification.Diversify(kv, input);
                    }
                }
                if (kv.Length == 64)
                {
                    key.setLength(32);
                }
                else if (kv.Length == 48)
                {
                    key.setLength(24);
                }
                key.fromString(KeyMaterial.ConvertValueFormat(kv, KeyValueStringFormat.HexStringWithSpace));
            }
            else
            {
                key.fromString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            }
            return key;
        }

        public void ActivateMifareSAM(LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
        {
            ActivateMifareSAM(av2cmd, GetSAMProperties().AuthenticateKeyEntryIdentifier, GetAuthenticationKey());
            Close();
            Open();
        }

        public static void ActivateMifareSAM(LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd, byte keyno, LibLogicalAccess.Card.DESFireKeyType keyType, byte keyVersion, string? keyValue)
        {
            var key = CreateDESFireKey(keyType, keyVersion, keyValue);
            ActivateMifareSAM(av2cmd, keyno, key);
        }

        public static void ActivateMifareSAM(LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd, byte keyno, LibLogicalAccess.Card.DESFireKey key)
        {
            av2cmd.lockUnlock(key, LibLogicalAccess.Card.SAMLockUnlock.SwitchAV2Mode /* AV3 = Active Mifare SAM */, keyno, 0, 0);
            log.Info("Mifare SAM features activation completed.");
        }

        public static void UnlockSAM(LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd, SAMAuthenticationMode mode, byte keyEntry, byte keyVersion, string? keyValue)
        {
            var key = new LibLogicalAccess.Card.DESFireKey();
            key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES);
            key.setKeyVersion(keyVersion);
            key.fromString(keyValue ?? "");
            UnlockSAM(av2cmd, mode, keyEntry, key);
        }

        public void UnlockSAM(LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
        {
            UnlockSAM(av2cmd, GetSAMProperties().AuthenticationMode, GetSAMProperties().AuthenticateKeyEntryIdentifier, GetAuthenticationKey());
        }

        public static void UnlockSAM(LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd, SAMAuthenticationMode mode, byte keyEntry, LibLogicalAccess.Card.DESFireKey key)
        {
            log.Info("Unlocking SAM...");
            if (mode == SAMAuthenticationMode.AuthenticateHost)
            {
                av2cmd.authenticateHost(key, keyEntry);
            }
            else if (mode == SAMAuthenticationMode.Unlock)
            {
                av2cmd.lockUnlock(key, LibLogicalAccess.Card.SAMLockUnlock.Unlock, keyEntry, 0, 0);
            }
            log.Info("SAM unlocked.");
        }

        public SAMKeyUsageCounter GetCounter(byte identifier)
        {
            log.Info(String.Format("Getting key usage counter `{0}`...", identifier));
            if (!CheckKeyUsageCounterExists(identifier))
            {
                log.Error(String.Format("The key usage counter `{0}` do not exists.", identifier));
                throw new KeyStoreException("The key usage counter do not exists.");
            }

            var cmd = Chip?.getCommands();
            if (cmd == null)
            {
                log.Error("No Command associated with the SAM chip.");
                throw new KeyStoreException("No Command associated with the SAM chip.");
            }

            var counter = new SAMKeyUsageCounter
            {
                Identifier = identifier
            };
            if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
            {
                try
                {
                    var kucEntry = av2cmd.getKUCEntry(identifier);
                    var entry = kucEntry.getKucEntryStruct();
                    counter.ChangeKeyRefId = entry.keynockuc;
                    counter.ChangeKeyRefVersion = entry.keyvckuc;
                    counter.Limit = BitConverter.ToUInt32(entry.limit, 0);
                    counter.Value = BitConverter.ToUInt32(entry.curval, 0);
                }
                catch (LibLogicalAccessException ex)
                {
                    log.Error(string.Format("Failed to get key usage counter `{0}`.", identifier), ex);
                    throw new KeyStoreException("Failed to get key usage counter.", ex);
                }
            }
            else if (cmd is LibLogicalAccess.Reader.SAMAV1ISO7816Commands)
            {
                log.Error("SAM is AV1. Please switch the SAM chip to AV2 mode first.");
                throw new KeyStoreException("SAM is AV1. Please switch the SAM chip to AV2 mode first.");
            }
            else
            {
                log.Error("Unexpected Command associated with the SAM chip.");
                throw new KeyStoreException("Unexpected Command associated with the SAM chip.");
            }

            log.Info(string.Format("Key usage counter `{0}` retrieved.", identifier));
            return counter;
        }
        public void UpdateCounter(SAMKeyUsageCounter counter)
        {
            log.Info(string.Format("Updating key usage counter `{0}`...", counter.Identifier));

            var cmd = Chip?.getCommands();
            if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
            {
                var key = GetAuthenticationKey();
                var kucEntry = new LibLogicalAccess.Card.SAMKucEntry();
                var entry = kucEntry.getKucEntryStruct();

                entry.keynockuc = counter.ChangeKeyRefId;
                entry.keyvckuc = counter.ChangeKeyRefVersion;
                entry.limit = BitConverter.GetBytes(counter.Limit);
                entry.curval = BitConverter.GetBytes(counter.Value);
                kucEntry.setKucEntryStruct(entry);

                av2cmd.authenticateHost(key, GetSAMProperties().AuthenticateKeyEntryIdentifier);
                kucEntry.setUpdateMask(0xE0);
                av2cmd.changeKUCEntry(counter.Identifier, kucEntry, key);
            }
            else
            {
                log.Error("Inserted SAM is not in AV2 mode, AV1 support has been deprecated, please check to option to auto switch to AV2 or manually perform a Switch.");
                throw new KeyStoreException("Inserted SAM is not in AV2 mode, AV1 support has been deprecated, please check to option to auto switch to AV2 or manually perform a Switch.");
            }

            log.Info(string.Format("Key usage counter `{0}` updated.", counter.Identifier));
        }

        public override Task<string?> ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput, WrappingKey? wrappingKey, KeyEntryId? targetKeyIdentifier)
        {
            log.Info(string.Format("Resolving key entry link with Key Entry Identifier `{0}` and Wrapping Key Entry Identifier `{1}`...", keyIdentifier, wrappingKey?.KeyId));
            if (wrappingKey == null || !wrappingKey.KeyId.IsConfigured())
            {
                log.Error("Wrapping Key Entry Identifier parameter is expected.");
                throw new KeyStoreException("Wrapping Key Entry Identifier parameter is expected.");
            }

            if (keClass != KeyEntryClass.Symmetric)
            {
                log.Error("Key Entry Class parameter must be Symmetric.");
                throw new KeyStoreException("Key Entry Class parameter must be Symmetric.");
            }

            var cmd = Chip?.getCommands();
            if (cmd is LibLogicalAccess.Reader.SAMAV3ISO7816Commands av3cmd)
            {
                if (!string.IsNullOrEmpty(GetSAMProperties().Secret) && !_unlocked)
                {
                    UnlockSAM(av3cmd);
                    _unlocked = true;
                }

                byte entry = (byte)keyIdentifier.NumericId!;
                byte targetEntry = entry;
                if (targetKeyIdentifier != null)
                {
                    targetEntry = (byte)targetKeyIdentifier.NumericId!;
                }

                byte[] div;
                if (!string.IsNullOrEmpty(divInput))
                {
                    div = Convert.FromHexString(divInput);
                }
                else
                {
                    div = [];
                }

                var keyCipheredVector = av3cmd.encipherKeyEntry(entry, targetEntry, wrappingKey.ChangeCounter ?? 0, 0x00, [], [.. div]);
                log.Info("Key link completed.");
                return Task.FromResult<string?>(Convert.ToHexString([.. keyCipheredVector]));
            }
            else
            {
                log.Error("Inserted SAM is not AV3.");
                throw new KeyStoreException("Inserted SAM is not in AV3.");
            }
        }

        public override async Task<string?> ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? containerSelector, string? divInput)
        {
            log.Info(string.Format("Resolving key link with Key Entry Identifier `{0}`, Key Version `{1}`, Div Input `{2}`...", keyIdentifier, containerSelector, divInput));

            if (keClass != KeyEntryClass.Symmetric)
            {
                log.Error("Key Entry Class parameter must be Symmetric.");
                throw new KeyStoreException("Key Entry Class parameter must be Symmetric.");
            }

            byte[] div;
            if (!string.IsNullOrEmpty(divInput))
            {
                div = Convert.FromHexString(divInput);
            }
            else
            {
                div = [];
            }

            if (!await CheckKeyEntryExists(keyIdentifier, keClass))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", keyIdentifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            byte entry = (byte)keyIdentifier.NumericId!;
            var cmd = Chip?.getCommands();
            if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
            {
                if (!string.IsNullOrEmpty(GetSAMProperties().Secret) && !_unlocked)
                {
                    UnlockSAM(av2cmd);
                    _unlocked = true;
                }

                if (!byte.TryParse(containerSelector, out byte keyVersion))
                {
                    log.Warn("Cannot parse the container selector as a key version, falling back to version 0.");
                }
                
                var keyVector = av2cmd.dumpSecretKey(entry, keyVersion, [.. div]);
                log.Info("Key link completed.");
                return Convert.ToHexString([.. keyVector]);
            }
            else
            {
                log.Error("Inserted SAM is not in AV2 mode.");
                throw new KeyStoreException("Inserted SAM is not in AV2 mode.");
            }
        }
        public override KeyEntry? GetDefaultKeyEntry(KeyEntryClass keClass)
        {
            var keyEntry = base.GetDefaultKeyEntry(keClass);
            if (keyEntry == null)
            {
                if (keClass == KeyEntryClass.Symmetric)
                {
                    keyEntry = new SAMSymmetricKeyEntry();
                }
                else if (keClass == KeyEntryClass.Asymmetric)
                {
                    keyEntry = new SAMAsymmetricKeyEntry();
                }
            }
            return keyEntry;
        }
    }
}
