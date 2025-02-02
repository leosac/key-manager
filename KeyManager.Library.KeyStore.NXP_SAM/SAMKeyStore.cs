﻿using LibLogicalAccess;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyStore : KeyStore
    {
        public static uint SAM_AV2_MAX_SYMMETRIC_ENTRIES => 128;
        public static byte SAM_AV2_MAX_USAGE_COUNTERS => 16;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private const string ATTRIBUTE_UID = "uid";

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

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric };
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
                            Attributes.Add(ATTRIBUTE_UID, Convert.ToHexString(version.manufacture.uniqueserialnumber));
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

            if (uint.TryParse(identifier.Id, out uint entry))
            {
                if (keClass == KeyEntryClass.Symmetric)
                {
                    return Task.FromResult(entry < SAM_AV2_MAX_SYMMETRIC_ENTRIES);
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
                UnlockSAM(av2cmd, GetSAMProperties().AuthenticateKeyEntryIdentifier, GetSAMProperties().AuthenticateKeyVersion, KeyMaterial.GetValueAsString(Properties?.Secret, KeyValueStringFormat.HexStringWithSpace));
                _unlocked = true;
            }

            return Task.FromResult<byte[]?>(av2cmd.getRandom(size).ToArray());
        }

        public override async Task<KeyEntry?> Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            log.Info(string.Format("Getting key entry `{0}`...", identifier));
            if (!await CheckKeyEntryExists(identifier, keClass))
            {
                log.Error(string.Format("The key entry `{0}` do not exists.", identifier));
                throw new KeyStoreException("The key entry do not exists.");
            }

            byte entry = byte.Parse(identifier.Id!);
            var cmd = Chip?.getCommands();
            if (cmd == null)
            {
                log.Error("No Command associated with the SAM chip.");
                throw new KeyStoreException("No Command associated with the SAM chip.");
            }

            SAMSymmetricKeyEntry keyEntry;
            if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
            {
                if (keClass == KeyEntryClass.Symmetric)
                {
                    var av2entry = av2cmd.getKeyEntry(entry);
                    var set = av2entry.getSETStruct();
                    keyEntry = CreateKeyEntryFromKeyType(av2entry.getKeyType());
                    keyEntry.Identifier = identifier;
                    var infoav2 = av2entry.getKeyEntryInformation();
                    ParseKeyEntryProperties(infoav2, set, keyEntry.SAMProperties);
                    if (keyEntry.Variant != null)
                    {
                        var keysdata = av2entry.getKeysData();
                        var keyVersions = keyEntry.Variant.KeyContainers.OfType<KeyVersion>().ToArray();
                        if (keysdata.Count < 1 || keysdata.Count != keyVersions.Length)
                        {
                            log.Error(string.Format("Unexpected number of keys ({0}) on the SAM Key Entry.", keysdata.Count));
                            throw new KeyStoreException("Unexpected number of keys on the SAM Key Entry.");
                        }

                        keyVersions[0].Key.SetAggregatedValueAsString(string.Empty);
                        keyVersions[0].Version = infoav2.vera;
                        keyVersions[0].TrackChanges();
                        if (keyEntry.Variant.KeyContainers.Count >= 2)
                        {
                            keyVersions[1].Key.SetAggregatedValueAsString(string.Empty);
                            keyVersions[1].Version = infoav2.verb;
                            keyVersions[1].TrackChanges();
                        }

                        if (keyEntry.Variant.KeyContainers.Count >= 3)
                        {
                            keyVersions[2].Key.SetAggregatedValueAsString(string.Empty);
                            keyVersions[2].Version = infoav2.verc;
                            keyVersions[2].TrackChanges();
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
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
            log.Info(string.Format("{0} key entries returned.", entries.Count));
            return Task.FromResult(entries);
        }

        public override async Task Store(IList<IChangeKeyEntry> changes)
        {
            log.Info(string.Format("Storing `{0}` key entries...", changes.Count));

            var cmd = Chip?.getCommands();
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

            log.Info("Key Entries storing completed.");
        }

        public override Task Update(IChangeKeyEntry change, bool ignoreIfMissing)
        {
            log.Info(string.Format("Updating key entry `{0}`...", change.Identifier));

            var key = new LibLogicalAccess.Card.DESFireKey();
            key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES);
            key.setKeyVersion(GetSAMProperties().AuthenticateKeyVersion);
            if (!string.IsNullOrEmpty(Properties?.Secret))
            {
                key.fromString(KeyMaterial.GetValueAsString(Properties.Secret, KeyValueStringFormat.HexStringWithSpace));
            }
            else
            {
                key.fromString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            }

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
                            if (containers[0].Key.KeySize == 32)
                            {
                                samkt = LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES256;
                            }
                            else if (containers[0].Key.KeySize == 24)
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
                            if (containers[0].Key.KeySize > 16)
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

                    av2cmd.authenticateHost(key, GetSAMProperties().AuthenticateKeyEntryIdentifier);
                    natkey.setUpdateSettings(updateSettings); // Or call setUpdateMask
                    av2cmd.changeKeyEntry((byte)Convert.ToDecimal(samkey.Identifier.Id), natkey, key);
                }
                else
                {
                    log.Error("Inserted SAM is not in AV2 mode, AV1 support has been deprecated, please check to option to auto switch to AV2 or manually perform a Switch.");
                    throw new KeyStoreException("Inserted SAM is not in AV2 mode, AV1 support has been deprecated, please check to option to auto switch to AV2 or manually perform a Switch.");
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
                key.fromString(KeyMaterial.GetValueAsString(keyValue, KeyValueStringFormat.HexStringWithSpace));
            }
            return key;
        }

        public void ActivateMifareSAM(LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
        {
            ActivateMifareSAM(av2cmd, GetSAMProperties().AuthenticateKeyEntryIdentifier, GetSAMProperties().AuthenticateKeyType, GetSAMProperties().AuthenticateKeyVersion, Properties?.Secret);
            Close();
            Open();
        }

        public static void ActivateMifareSAM(LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd, byte keyno, LibLogicalAccess.Card.DESFireKeyType keyType, byte keyVersion, string? keyValue)
        {
            var key = CreateDESFireKey(keyType, keyVersion, keyValue);
            av2cmd.lockUnlock(key, LibLogicalAccess.Card.SAMLockUnlock.SwitchAV2Mode /* AV3 = Active Mifare SAM */, keyno, 0, 0);
            log.Info("Mifare SAM features activation completed.");
        }

        public static void UnlockSAM(LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd, byte keyEntry, byte keyVersion, string? keyValue)
        {
            log.Info("Unlocking SAM...");
            var key = new LibLogicalAccess.Card.DESFireKey();
            key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES);
            key.setKeyVersion(keyVersion);
            key.fromString(keyValue ?? "");
            av2cmd.lockUnlock(key, LibLogicalAccess.Card.SAMLockUnlock.Unlock, keyEntry, 0, 0);
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
                var kucEntry = av2cmd.getKUCEntry(identifier);
                var entry = kucEntry.getKucEntryStruct();
                counter.ChangeKeyRefId = entry.keynockuc;
                counter.ChangeKeyRefVersion = entry.keyvckuc;
                counter.Limit = BitConverter.ToUInt32(entry.limit, 0);
                counter.Value = BitConverter.ToUInt32(entry.curval, 0);
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
                var key = new LibLogicalAccess.Card.DESFireKey();
                key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES);
                key.setKeyVersion(GetSAMProperties().AuthenticateKeyVersion);
                if (!string.IsNullOrEmpty(Properties?.Secret))
                {
                    key.fromString(KeyMaterial.GetValueAsString(Properties.Secret, KeyValueStringFormat.HexStringWithSpace));
                }
                else
                {
                    key.fromString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                }

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

            var cmd = Chip?.getCommands();
            if (cmd is LibLogicalAccess.Reader.SAMAV3ISO7816Commands av3cmd)
            {
                if (!string.IsNullOrEmpty(GetSAMProperties().Secret) && !_unlocked)
                {
                    UnlockSAM(av3cmd, GetSAMProperties().AuthenticateKeyEntryIdentifier, GetSAMProperties().AuthenticateKeyVersion, KeyMaterial.GetValueAsString(Properties?.Secret, KeyValueStringFormat.HexStringWithSpace));
                    _unlocked = true;
                }

                byte entry = byte.Parse(keyIdentifier.Id!);
                byte targetEntry = entry;
                if (targetKeyIdentifier != null)
                {
                    targetEntry = byte.Parse(targetKeyIdentifier.Id!);
                }

                byte[] div;
                if (!string.IsNullOrEmpty(divInput))
                {
                    div = Convert.FromHexString(divInput);
                }
                else
                {
                    div = Array.Empty<byte>();
                }

                var keyCipheredVector = av3cmd.encipherKeyEntry(entry, targetEntry, wrappingKey.ChangeCounter ?? 0, 0x00, [], new ByteVector(div));
                log.Info("Key link completed.");
                return Task.FromResult<string?>(Convert.ToHexString(keyCipheredVector.ToArray()));
            }
            else
            {
                log.Error("Inserted SAM is not AV3.");
                throw new KeyStoreException("Inserted SAM is not in AV3.");
            }
        }

        public override async Task<string?> ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? containerSelector, string? divInput)
        {
            byte[] div;
            log.Info(string.Format("Resolving key link with Key Entry Identifier `{0}`, Key Version `{1}`, Div Input `{2}`...", keyIdentifier, containerSelector, divInput));
            if (!string.IsNullOrEmpty(divInput))
            {
                div = Convert.FromHexString(divInput);
            }
            else
            {
                div = Array.Empty<byte>();
            }

            if (!await CheckKeyEntryExists(keyIdentifier, keClass))
            {
                log.Error(string.Format("The key entry `{0}` doesn't exist.", keyIdentifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }

            byte entry = byte.Parse(keyIdentifier.Id!);

            var cmd = Chip?.getCommands();
            if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
            {
                if (!string.IsNullOrEmpty(GetSAMProperties().Secret) && !_unlocked)
                {
                    UnlockSAM(av2cmd, GetSAMProperties().AuthenticateKeyEntryIdentifier, GetSAMProperties().AuthenticateKeyVersion, KeyMaterial.GetValueAsString(Properties?.Secret, KeyValueStringFormat.HexStringWithSpace));
                    _unlocked = true;
                }

                if (!byte.TryParse(containerSelector, out byte keyVersion))
                {
                    log.Warn("Cannot parse the container selector as a key version, falling back to version 0.");
                }
                var keyVector = av2cmd.dumpSecretKey(entry, keyVersion, new LibLogicalAccess.ByteVector(div));
                log.Info("Key link completed.");
                return Convert.ToHexString(keyVector.ToArray());
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
            }
            return keyEntry;
        }
    }
}
