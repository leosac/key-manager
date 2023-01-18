using LibLogicalAccess.Reader;
using LibLogicalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibLogicalAccess.Card;
using System.Text.RegularExpressions;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyStore : KeyStore
    {
        public const uint SAM_AV2_MAX_SYMMETRIC_ENTRIES = 128;
        public const byte SAM_AV2_MAX_USAGE_COUNTERS = 16;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SAMKeyStore()
        {
            
        }

        private bool _unlocked = false;

        public LibLogicalAccess.ReaderProvider? ReaderProvider { get; private set; }
        public LibLogicalAccess.ReaderUnit? ReaderUnit { get; private set; }
        public LibLogicalAccess.Chip? Chip { get; private set; }

        public SAMKeyStoreProperties GetSAMProperties()
        {
            var p = Properties as SAMKeyStoreProperties;
            if (p == null)
                throw new KeyStoreException("Missing SAM key store properties.");
            return p;
        }

        public override string Name => "NXP SAM AV2";

        public override bool CanCreateKeyEntries => false;

        public override bool CanDeleteKeyEntries => false;

        public override IEnumerable<KeyEntryClass> SupportedClasses
        {
            get => new KeyEntryClass[] { KeyEntryClass.Symmetric };
        }

        public override void Open()
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
                log.Error(String.Format("Cannot initialize the Reader Provider `{0}`.", providerName));
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

            if (ReaderUnit.waitInsertion(1000))
            {
                if (ReaderUnit.connect())
                {
                    var chip = ReaderUnit.getSingleChip();
                    var genericType = chip.getGenericCardType();
                    if (String.Compare(genericType, "SAM") >= 0)
                    {
                        Chip = chip;
                    }
                    else
                    {
                        Close();
                        log.Error(String.Format("The card detected `{0}` is not a SAM.", genericType));
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
        }

        public override void Close()
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
        }

        public override bool CheckKeyEntryExists(KeyEntryId identifier, KeyEntryClass keClass)
        {
            if (identifier.Id == null)
                return false;

            uint entry;
            if (uint.TryParse(identifier.Id, out entry))
            {
                if (keClass == KeyEntryClass.Symmetric)
                    return (entry < SAM_AV2_MAX_SYMMETRIC_ENTRIES);
            }

            return false;
        }

        public bool CheckKeyUsageCounterExists(byte identifier)
        {
            return (identifier < SAM_AV2_MAX_USAGE_COUNTERS);
        }

        public override void Create(IChangeKeyEntry change)
        {
            log.Info(String.Format("Creating key entry `{0}`...", change.Identifier));
            log.Error("A SAM key entry cannot be created, only updated.");
            throw new KeyStoreException("A SAM key entry cannot be created, only updated.");
        }

        public override void Delete(KeyEntryId identifier, KeyEntryClass keClass, bool ignoreIfMissing = false)
        {
            log.Info(String.Format("Deleting key entry `{0}`...", identifier));
            log.Error("A SAM key entry cannot be deleted, only updated.");
            throw new KeyStoreException("A SAM key entry cannot be deleted, only updated.");
        }

        public override KeyEntry? Get(KeyEntryId identifier, KeyEntryClass keClass)
        {
            log.Info(String.Format("Getting key entry `{0}`...", identifier));
            if (!CheckKeyEntryExists(identifier, keClass))
            {
                log.Error(String.Format("The key entry `{0}` do not exists.", identifier));
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
                    if (keyEntry.SAMProperties != null)
                    {
                        keyEntry.SAMProperties.SAMKeyEntryType = (SAMKeyEntryType)(infoav2.ExtSET & 0x07);

                        Array.Copy(infoav2.desfireAid, keyEntry.SAMProperties.DESFireAID, 3);
                        keyEntry.SAMProperties.DESFireKeyNum = infoav2.desfirekeyno;

                        keyEntry.SAMProperties.KeyUsageCounter = (infoav2.kuc != 0xff) ? infoav2.kuc : null;

                        keyEntry.SAMProperties.ChangeKeyRefId = infoav2.cekno;
                        keyEntry.SAMProperties.ChangeKeyRefVersion = infoav2.cekv;

                        keyEntry.SAMProperties.EnableDumpSessionKey = Convert.ToBoolean(set.dumpsessionkey);
                        keyEntry.SAMProperties.CryptoBasedOnSecretKey = Convert.ToBoolean(set.allowcrypto);
                        keyEntry.SAMProperties.DisableDecryptData = Convert.ToBoolean(set.disabledecryption);
                        keyEntry.SAMProperties.DisableEncryptData = Convert.ToBoolean(set.disableencryption);
                        keyEntry.SAMProperties.DisableGenerateMACFromPICC = Convert.ToBoolean(set.disablegeneratemac);
                        keyEntry.SAMProperties.DisableKeyEntry = Convert.ToBoolean(set.disablekeyentry);
                        keyEntry.SAMProperties.DisableVerifyMACFromPICC = Convert.ToBoolean(set.disableverifymac);
                        keyEntry.SAMProperties.DisableChangeKeyPICC = Convert.ToBoolean(set.disablewritekeytopicc);
                        keyEntry.SAMProperties.LockUnlock = Convert.ToBoolean(set.lockkey);
                        keyEntry.SAMProperties.KeepIV = Convert.ToBoolean(set.keepIV);
                        keyEntry.SAMProperties.AuthenticateHost = Convert.ToBoolean(set.authkey);
                        keyEntry.SAMProperties.AllowDumpSecretKey = Convert.ToBoolean(infoav2.ExtSET & 0x08);
                        keyEntry.SAMProperties.AllowDumpSecretKeyWithDiv = Convert.ToBoolean(infoav2.ExtSET & 0x10);
                    }

                    if (keyEntry.Variant != null)
                    {
                        var keysdata = av2entry.getKeysData();
                        var keyVersions = keyEntry.Variant.KeyContainers.OfType<KeyVersion>().ToArray();
                        if (keysdata.Count < 2 || keysdata.Count != keyVersions.Length)
                        {
                            log.Error(String.Format("Unexpected number of keys ({0}) on the SAM Key Entry.", keysdata.Count));
                            throw new KeyStoreException("Unexpected number of keys on the SAM Key Entry.");
                        }

                        keyVersions[0].Key.SetAggregatedValue(Convert.ToHexString(keysdata[0].ToArray()));
                        keyVersions[0].Version = infoav2.vera;
                        keyVersions[1].Key.SetAggregatedValue(Convert.ToHexString(keysdata[1].ToArray()));
                        keyVersions[1].Version = infoav2.verb;

                        if (keyEntry.Variant.KeyContainers.Count >= 3)
                        {
                            keyVersions[2].Key.SetAggregatedValue(Convert.ToHexString(keysdata[2].ToArray()));
                            keyVersions[2].Version = infoav2.verc;
                        }
                    }
                }
                else
                    throw new NotImplementedException();
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

            log.Info(String.Format("Key entry `{0}` retrieved.", identifier));
            return keyEntry;
        }

        private SAMSymmetricKeyEntry CreateKeyEntryFromKeyType(SAMKeyType keyType)
        {
            var keyEntry = new SAMSymmetricKeyEntry();
            if (keyType == SAMKeyType.SAM_KEY_DES)
                keyEntry.SetVariant("DES");
            else if (keyType == SAMKeyType.SAM_KEY_AES)
                keyEntry.SetVariant("AES128");
            else
                keyEntry.SetVariant("TK3DES");
            return keyEntry;
        }

        public override IList<KeyEntryId> GetAll(KeyEntryClass? keClass = null)
        {
            log.Info(String.Format("Getting all key entries (class: `{0}`)...", keClass));
            var entries = new List<KeyEntryId>();
            if (keClass == null || keClass == KeyEntryClass.Symmetric)
            {
                for (uint i = 0; i < SAM_AV2_MAX_SYMMETRIC_ENTRIES; ++i)
                {
                    entries.Add(new KeyEntryId { Id = i.ToString() });
                }
            }
            log.Info(String.Format("{0} key entries returned.", entries.Count));
            return entries;
        }

        public override void Store(IList<IChangeKeyEntry> changes)
        {
            log.Info(String.Format("Storing `{0}` key entries...", changes.Count));

            var cmd = Chip.getCommands();
            if (cmd is SAMAV1ISO7816Commands av1cmd)
            {
                if (GetSAMProperties().AutoSwitchToAV2)
                {
                    SwitchSAMToAV2(av1cmd, GetSAMProperties().AuthenticateKeyType, GetSAMProperties().AuthenticateKeyVersion, Properties?.Secret);
                    Close();
                    Open();
                    cmd = Chip.getCommands();
                    if (cmd is SAMAV1ISO7816Commands)
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

            foreach (var change in changes)
            {
                Update(change);
            }

            log.Info("Key Entries storing completed.");
        }

        public override void Update(IChangeKeyEntry change, bool ignoreIfMissing = false)
        {
            log.Info(String.Format("Updating key entry `{0}`...", change.Identifier));

            if (change is SAMSymmetricKeyEntry samkey)
            {
                var cmd = Chip?.getCommands();
                if (cmd is SAMAV2ISO7816Commands av2cmd)
                {
                    if (GetSAMProperties().UnlockKey != null && !string.IsNullOrEmpty(GetSAMProperties().UnlockKey.Key.GetAggregatedValue<string>()) && !_unlocked)
                    {
                        UnlockSAM(av2cmd, GetSAMProperties().UnlockKeyEntryIdentifier, GetSAMProperties().UnlockKey);
                        _unlocked = true;
                    }

                    var key = new DESFireKey();
                    key.setKeyType(DESFireKeyType.DF_KEY_AES);
                    key.setKeyVersion(GetSAMProperties().AuthenticateKeyVersion);
                    if (!string.IsNullOrEmpty(Properties?.Secret))
                    {
                        key.fromString(KeyMaterial.GetFormattedValue<string>(Properties.Secret, KeyValueFormat.HexStringWithSpace));
                    }
                    else
                    {
                        key.fromString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                    }

                    var natkey = new AV2SAMKeyEntry();

                    var infoav2 = new KeyEntryAV2Information();

                    if (samkey.SAMProperties != null)
                    {
                        infoav2.ExtSET |= (byte)samkey.SAMProperties.SAMKeyEntryType;

                        Array.Copy(samkey.SAMProperties.DESFireAID, infoav2.desfireAid, 3);
                        infoav2.desfirekeyno = samkey.SAMProperties.DESFireKeyNum;

                        infoav2.kuc = samkey.SAMProperties.KeyUsageCounter ?? 0xff;

                        infoav2.cekno = samkey.SAMProperties.ChangeKeyRefId;
                        infoav2.cekv = samkey.SAMProperties.ChangeKeyRefVersion;

                        infoav2.ExtSET |= (byte)(Convert.ToByte(samkey.SAMProperties.AllowDumpSecretKey) << 3);
                        infoav2.ExtSET |= (byte)(Convert.ToByte(samkey.SAMProperties.AllowDumpSecretKeyWithDiv) << 4);
                    }
                    if (samkey.Variant != null)
                    {
                        var keyVersions = samkey.Variant.KeyContainers.OfType<KeyVersion>().ToArray();
                        var keys = new UCharCollectionCollection(keyVersions.Length);
                        foreach (var keyversion in samkey.Variant.KeyContainers)
                        {
                            if (string.IsNullOrEmpty(keyversion.Key.GetAggregatedValue<string>()))
                                keys.Add(new ByteVector(new byte[keyversion.Key.KeySize]));
                            else
                                keys.Add(new ByteVector(keyversion.Key.GetAggregatedValue<byte[]>(KeyValueFormat.Binary)));
                        }

                        infoav2.vera = keyVersions[0].Version;
                        if (keyVersions.Length >= 2)
                            infoav2.verb = keyVersions[1].Version;
                        if (keyVersions.Length >= 3)
                            infoav2.verc = keyVersions[2].Version;

                        if (keyVersions[0].Key.Tags.Contains("AES"))
                        {
                            natkey.setKeysData(keys, SAMKeyType.SAM_KEY_AES);
                        }
                        else
                        {
                            if (keyVersions[0].Key.KeySize == 16)
                            {
                                natkey.setKeysData(keys, SAMKeyType.SAM_KEY_DES);
                            }
                            else
                            {
                                natkey.setKeysData(keys, SAMKeyType.SAM_KEY_3K3DES);
                            }
                        }
                    }
                    natkey.setKeyEntryInformation(infoav2);

                    if (samkey.SAMProperties != null)
                    {
                        var set = new SETAV2();

                        set.dumpsessionkey = Convert.ToByte(samkey.SAMProperties.EnableDumpSessionKey);
                        set.allowcrypto = Convert.ToByte(samkey.SAMProperties.CryptoBasedOnSecretKey);
                        set.disabledecryption = Convert.ToByte(samkey.SAMProperties.DisableDecryptData);
                        set.disableencryption = Convert.ToByte(samkey.SAMProperties.DisableEncryptData);
                        set.disablegeneratemac = Convert.ToByte(samkey.SAMProperties.DisableGenerateMACFromPICC);
                        set.disablekeyentry = Convert.ToByte(samkey.SAMProperties.DisableKeyEntry);
                        set.disableverifymac = Convert.ToByte(samkey.SAMProperties.DisableVerifyMACFromPICC);
                        set.disablewritekeytopicc = Convert.ToByte(samkey.SAMProperties.DisableChangeKeyPICC);
                        set.lockkey = Convert.ToByte(samkey.SAMProperties.LockUnlock);
                        set.keepIV = Convert.ToByte(samkey.SAMProperties.KeepIV);
                        set.authkey = Convert.ToByte(samkey.SAMProperties.AuthenticateHost);

                        natkey.setSET(set);
                    }
                    natkey.setSETKeyTypeFromKeyType();

                    av2cmd.authenticateHost(key, GetSAMProperties().AuthenticateKeyEntryIdentifier);
                    natkey.setUpdateMask(0xFF);
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
            log.Info(String.Format("Key entry `{0}` updated.", change.Identifier));
        }

        public static void SwitchSAMToAV2(SAMAV1ISO7816Commands av1cmds, DESFireKeyType keyType, byte keyVersion, string keyValue)
        {
            log.Info("Switching the SAM to AV2 mode...");
            var key = new DESFireKey();
            key.setKeyType(keyType);
            if (keyType == DESFireKeyType.DF_KEY_AES)
            {
                key.setKeyVersion(keyVersion);
                key.fromString(KeyMaterial.GetFormattedValue<string>(keyValue, KeyValueFormat.HexStringWithSpace));
            }
            else
            {
                var keyav1entry = av1cmds.getKeyEntry(0);
                var keys = new UCharCollectionCollection(3)
                {
                    new ByteVector(new byte[16]),
                    new ByteVector(new byte[16]),
                    new ByteVector(new byte[16])
                };

                keyav1entry.setKeysData(keys, SAMKeyType.SAM_KEY_AES);
                var keyInfo = keyav1entry.getKeyEntryInformation();
                keyInfo.vera = 0;
                keyInfo.verb = 0;
                keyInfo.verc = 0;
                keyInfo.cekno = 0;
                keyInfo.cekv = 0;
                keyav1entry.setKeyEntryInformation(keyInfo);
                keyav1entry.setUpdateMask(0xff);

                av1cmds.authenticateHost(key, 0);
                av1cmds.changeKeyEntry(0, keyav1entry, key);

                key.fromString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                key.setKeyVersion(0);
                key.setKeyType(DESFireKeyType.DF_KEY_AES);
            }

            av1cmds.authenticateHost(key, 0);
            av1cmds.lockUnlock(key, SAMLockUnlock.SwitchAV2Mode, 0, 0, 0);
            log.Info("SAM switched to AV2 mode.");
        }

        public static void UnlockSAM(SAMAV2ISO7816Commands av2cmds, byte keyEntry, KeyVersion keyVersion)
        {
            log.Info("Unlocking SAM...");
            var key = new DESFireKey();
            key.setKeyType(DESFireKeyType.DF_KEY_AES);
            key.setKeyVersion(keyVersion.Version);
            key.fromString(keyVersion.Key.GetAggregatedValue<string>(KeyValueFormat.HexStringWithSpace));
            av2cmds.lockUnlock(key, SAMLockUnlock.Unlock, keyEntry, 0x00, 0x00);
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

            var counter = new SAMKeyUsageCounter();
            counter.Identifier = identifier;
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

            log.Info(String.Format("Key usage counter `{0}` retrieved.", identifier));
            return counter;
        }
        public void UpdateCounter(SAMKeyUsageCounter counter)
        {
            log.Info(String.Format("Updating key usage counter `{0}`...", counter.Identifier));

            var cmd = Chip?.getCommands();
            if (cmd is SAMAV2ISO7816Commands av2cmd)
            {
                if (GetSAMProperties().UnlockKey != null && !string.IsNullOrEmpty(GetSAMProperties().UnlockKey.Key.GetAggregatedValue<string>()) && !_unlocked)
                {
                    UnlockSAM(av2cmd, GetSAMProperties().UnlockKeyEntryIdentifier, GetSAMProperties().UnlockKey);
                    _unlocked = true;
                }

                var key = new DESFireKey();
                key.setKeyType(DESFireKeyType.DF_KEY_AES);
                key.setKeyVersion(GetSAMProperties().AuthenticateKeyVersion);
                if (!string.IsNullOrEmpty(Properties?.Secret))
                {
                    key.fromString(KeyMaterial.GetFormattedValue<string>(Properties.Secret, KeyValueFormat.HexStringWithSpace));
                }
                else
                {
                    key.fromString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                }

                var kucEntry = new SAMKucEntry();
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

            log.Info(String.Format("Key usage counter `{0}` updated.", counter.Identifier));
        }

        public override string? ResolveKeyEntryLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? divInput = null, KeyEntryId? wrappingKeyId = null, string? wrappingContainerSelector = null)
        {
            // Will be supported with SAM AV3
            throw new NotSupportedException();
        }

        public override string? ResolveKeyLink(KeyEntryId keyIdentifier, KeyEntryClass keClass, string? containerSelector = null, string? divInput = null)
        {
            byte[] div;
            log.Info(String.Format("Resolving key link with Key Entry Identifier `{0}`, Key Version `{1}`, Div Input `{2}`...", keyIdentifier, containerSelector, divInput));
            if (!string.IsNullOrEmpty(divInput))
                div = Convert.FromHexString(divInput);
            else
                div = new byte[0];

            if (!CheckKeyEntryExists(keyIdentifier, keClass))
            {
                log.Error(String.Format("The key entry `{0}` doesn't exist.", keyIdentifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }
            byte entry = byte.Parse(keyIdentifier.Id!);

            var cmd = Chip?.getCommands();
            if (cmd is SAMAV2ISO7816Commands av2cmd)
            {
                byte keyVersion = 0;
                if (!byte.TryParse(containerSelector, out keyVersion))
                    log.Warn("Cannot parse the container selector as a key version, falling back to version 0.");
                var keyVector = av2cmd.dumpSecretKey(entry, keyVersion, new ByteVector(div));
                log.Info("Key link completed.");
                return Convert.ToHexString(keyVector.ToArray());
            }
            else
            {
                log.Error("Inserted SAM is not in AV2 mode.");
                throw new KeyStoreException("Inserted SAM is not in AV2 mode.");
            }
        }
    }
}
