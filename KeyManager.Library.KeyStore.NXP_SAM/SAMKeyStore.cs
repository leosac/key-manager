namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyStore : KeyStore
    {
        public static uint SAM_AV2_MAX_SYMMETRIC_ENTRIES => 128;
        public static byte SAM_AV2_MAX_USAGE_COUNTERS => 16;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private bool _unlocked;

        public LibLogicalAccess.ReaderProvider? ReaderProvider { get; private set; }
        public LibLogicalAccess.ReaderUnit? ReaderUnit { get; private set; }
        public LibLogicalAccess.Chip? Chip { get; private set; }

        public SAMKeyStoreProperties GetSAMProperties()
        {
            var p = Properties as SAMKeyStoreProperties;
            return p ?? throw new KeyStoreException("Missing SAM key store properties.");
        }

        public override string Name => "NXP SAM AV2";

        public override bool CanCreateKeyEntries => false;

        public override bool CanDeleteKeyEntries => false;

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
                        if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av1cmd)
                        {
                            version = av1cmd.getVersion();
                        }
                        else if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
                        {
                            version = av2cmd.getVersion();
                        }

                        if (version != null)
                        {
                            log.Info(string.Format("SAM Version {0}.{1}, UID: {2}", version.software.majorversion, version.software.minorversion, Convert.ToHexString(version.manufacture.uniqueserialnumber)));
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

        public override Task Close()
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

            _unlocked = false;
            log.Info("Key Store closed.");
            return Task.CompletedTask;
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
                        if (keysdata.Count < 2 || keysdata.Count != keyVersions.Length)
                        {
                            log.Error(string.Format("Unexpected number of keys ({0}) on the SAM Key Entry.", keysdata.Count));
                            throw new KeyStoreException("Unexpected number of keys on the SAM Key Entry.");
                        }

                        keyVersions[0].Key.SetAggregatedValueString(Convert.ToHexString(keysdata[0].ToArray()));
                        keyVersions[0].Version = infoav2.vera;
                        keyVersions[1].Key.SetAggregatedValueString(Convert.ToHexString(keysdata[1].ToArray()));
                        keyVersions[1].Version = infoav2.verb;

                        if (keyEntry.Variant.KeyContainers.Count >= 3)
                        {
                            keyVersions[2].Key.SetAggregatedValueString(Convert.ToHexString(keysdata[2].ToArray()));
                            keyVersions[2].Version = infoav2.verc;
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
            }
        }

        private static SAMSymmetricKeyEntry CreateKeyEntryFromKeyType(LibLogicalAccess.Card.SAMKeyType keyType)
        {
            var keyEntry = new SAMSymmetricKeyEntry();
            if (keyType == LibLogicalAccess.Card.SAMKeyType.SAM_KEY_DES)
            {
                keyEntry.SetVariant("DES");
            }
            else if (keyType == LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES)
            {
                keyEntry.SetVariant("AES128");
            }
            else
            {
                keyEntry.SetVariant("TK3DES");
            }
            return keyEntry;
        }

        public override Task<IList<KeyEntryId>> GetAll(KeyEntryClass? keClass)
        {
            log.Info(string.Format("Getting all key entries (class: `{0}`)...", keClass));
            IList<KeyEntryId> entries = new List<KeyEntryId>();
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

            // We sort the changes to update change key reference last
            var ochanges = changes.Order(new SAMKeyEntryComparer(GetSAMProperties()));
            foreach (var change in ochanges)
            {
                await Update(change);
            }

            log.Info("Key Entries storing completed.");
        }

        public override Task Update(IChangeKeyEntry change, bool ignoreIfMissing)
        {
            log.Info(string.Format("Updating key entry `{0}`...", change.Identifier));

            if (change is SAMSymmetricKeyEntry samkey)
            {
                var cmd = Chip?.getCommands();
                if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
                {
                    var key = new LibLogicalAccess.Card.DESFireKey();
                    key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES);
                    key.setKeyVersion(GetSAMProperties().AuthenticateKeyVersion);
                    if (!string.IsNullOrEmpty(Properties?.Secret))
                    {
                        key.fromString(KeyMaterial.GetValueString(Properties.Secret, KeyValueStringFormat.HexStringWithSpace));
                    }
                    else
                    {
                        key.fromString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                    }

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
                    }

                    var updateSettings = new LibLogicalAccess.Card.KeyEntryUpdateSettings();
                    updateSettings.df_aid_keyno = 1;
                    updateSettings.key_no_v_cek = 1;
                    updateSettings.refkeykuc = 1;
                    updateSettings.keyversionsentseparatly = 1;
                    updateSettings.updateset = 1;

                    if (samkey.Variant != null)
                    {
                        updateSettings.keyVa = 1;
                        updateSettings.keyVb = 1;
                        updateSettings.keyVc = 1;

                        var keyVersions = samkey.Variant.KeyContainers.OfType<KeyVersion>().ToArray();
                        var keys = new LibLogicalAccess.UCharCollectionCollection(keyVersions.Length);
                        foreach (var keyversion in samkey.Variant.KeyContainers)
                        {
                            if (string.IsNullOrEmpty(keyversion.Key.GetAggregatedValueString()))
                            {
                                keys.Add(new LibLogicalAccess.ByteVector(new byte[keyversion.Key.KeySize]));
                            }
                            else
                            {
                                keys.Add(new LibLogicalAccess.ByteVector(keyversion.Key.GetAggregatedValueBinary()));
                            }
                        }

                        infoav2.vera = keyVersions[0].Version;
                        if (keyVersions.Length >= 2)
                        {
                            infoav2.verb = keyVersions[1].Version;
                        }

                        if (keyVersions.Length >= 3)
                        {
                            infoav2.verc = keyVersions[2].Version;
                        }

                        if (keyVersions[0].Key.Tags.Contains("AES"))
                        {
                            natkey.setKeysData(keys, LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES);
                        }
                        else
                        {
                            if (keyVersions[0].Key.KeySize == 16)
                            {
                                natkey.setKeysData(keys, LibLogicalAccess.Card.SAMKeyType.SAM_KEY_DES);
                            }
                            else
                            {
                                natkey.setKeysData(keys, LibLogicalAccess.Card.SAMKeyType.SAM_KEY_3K3DES);
                                updateSettings.keyVc = 0;
                            }
                        }
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
                    LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES => LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES,
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

                keyav1entry.setKeysData(keys, LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES);
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
                key.fromString(KeyMaterial.GetValueString(keyValue, KeyValueStringFormat.HexStringWithSpace));
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
                    key.fromString(KeyMaterial.GetValueString(Properties.Secret, KeyValueStringFormat.HexStringWithSpace));
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

        public override Task<string?> ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput, KeyEntryId? wrappingKeyId, string? wrappingContainerSelector)
        {
            // Will be supported with SAM AV3
            throw new NotSupportedException();
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
                    UnlockSAM(av2cmd, GetSAMProperties().AuthenticateKeyEntryIdentifier, GetSAMProperties().AuthenticateKeyVersion, KeyMaterial.GetValueString(Properties?.Secret, KeyValueStringFormat.HexStringWithSpace));
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
