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

        public override bool CheckKeyEntryExists(string identifier)
        {
            uint entry;
            if (uint.TryParse(identifier, out entry))
            {
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

        public override void Delete(string identifier, bool ignoreIfMissing = false)
        {
            log.Info(String.Format("Deleting key entry `{0}`...", identifier));
            log.Error("A SAM key entry cannot be deleted, only updated.");
            throw new KeyStoreException("A SAM key entry cannot be deleted, only updated.");
        }

        public override KeyEntry? Get(string identifier)
        {
            log.Info(String.Format("Getting key entry `{0}`...", identifier));
            if (!CheckKeyEntryExists(identifier))
            {
                log.Error(String.Format("The key entry `{0}` do not exists.", identifier));
                throw new KeyStoreException("The key entry do not exists.");
            }

            byte entry = byte.Parse(identifier);
            var cmd = Chip?.getCommands();
            if (cmd == null)
            {
                log.Error("No Command associated with the SAM chip.");
                throw new KeyStoreException("No Command associated with the SAM chip.");
            }

            SAMSymmetricKeyEntry keyEntry;
            if (cmd is LibLogicalAccess.Reader.SAMAV2ISO7816Commands av2cmd)
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
                    if (keysdata.Count < 2 || keysdata.Count != keyEntry.Variant.KeyVersions.Count)
                    {
                        log.Error(String.Format("Unexpected number of keys ({0}) on the SAM Key Entry.", keysdata.Count));
                        throw new KeyStoreException("Unexpected number of keys on the SAM Key Entry.");
                    }

                    keyEntry.Variant.KeyVersions[0].Key.Value = Convert.ToHexString(keysdata[0].ToArray());
                    keyEntry.Variant.KeyVersions[0].Version = infoav2.vera;
                    keyEntry.Variant.KeyVersions[1].Key.Value = Convert.ToHexString(keysdata[1].ToArray());
                    keyEntry.Variant.KeyVersions[1].Version = infoav2.verb;

                    if (keyEntry.Variant.KeyVersions.Count >= 3)
                    {
                        keyEntry.Variant.KeyVersions[2].Key.Value = Convert.ToHexString(keysdata[2].ToArray());
                        keyEntry.Variant.KeyVersions[2].Version = infoav2.verc;
                    }
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

        public override IList<string> GetAll()
        {
            log.Info("Getting all key entries...");
            var entries = new List<string>();
            for (uint i = 0; i < SAM_AV2_MAX_SYMMETRIC_ENTRIES; ++i)
            {
                entries.Add(i.ToString());
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
                    SwitchSAMToAV2(av1cmd, GetSAMProperties().AuthenticateKey);
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

            if (GetSAMProperties().AuthenticateKey == null)
            {
                log.Error("To be updated, a SAM AV2 key store requires at least the Authenticate Key.");
                throw new KeyStoreException("To be updated, a SAM AV2 key store requires at least the Authenticate Key.");
            }

            if (change is SAMSymmetricKeyEntry samkey)
            {
                var cmd = Chip?.getCommands();
                if (cmd is SAMAV2ISO7816Commands av2cmd)
                {
                    if (GetSAMProperties().UnlockKey != null && !string.IsNullOrEmpty(GetSAMProperties().UnlockKey.Key.Value) && !_unlocked)
                    {
                        UnlockSAM(av2cmd, GetSAMProperties().UnlockKeyEntryIdentifier, GetSAMProperties().UnlockKey);
                        _unlocked = true;
                    }

                    var key = new DESFireKey();
                    key.setKeyType(DESFireKeyType.DF_KEY_AES);
                    key.setKeyVersion(GetSAMProperties().AuthenticateKey.Version);
                    key.fromString(Regex.Replace(GetSAMProperties().AuthenticateKey.Key.Value, ".{2}", "$0 "));

                    var natkey = new AV2SAMKeyEntry();

                    var infoav2 = new KeyEntryAV2Information();
                    if (samkey.SAMProperties != null)
                    {
                        var set = new SETAV2();
                        infoav2.ExtSET |= (byte)samkey.SAMProperties.SAMKeyEntryType;

                        Array.Copy(samkey.SAMProperties.DESFireAID, infoav2.desfireAid, 3);
                        infoav2.desfirekeyno = samkey.SAMProperties.DESFireKeyNum;

                        infoav2.kuc = samkey.SAMProperties.KeyUsageCounter ?? 0xff;

                        infoav2.cekno = samkey.SAMProperties.ChangeKeyRefId;
                        infoav2.cekv = samkey.SAMProperties.ChangeKeyRefVersion;

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
                        infoav2.ExtSET |= (byte)(Convert.ToByte(samkey.SAMProperties.AllowDumpSecretKey) << 3);
                        infoav2.ExtSET |= (byte)(Convert.ToByte(samkey.SAMProperties.AllowDumpSecretKeyWithDiv) << 4);

                        natkey.setSET(set);
                    }

                    if (samkey.Variant != null)
                    {
                        var keys = new UCharCollectionCollection(samkey.Variant.KeyVersions.Count);
                        foreach (var keyversion in samkey.Variant.KeyVersions)
                        {
                            if (string.IsNullOrEmpty(keyversion.Key.Value))
                                keys.Add(new ByteVector(new byte[keyversion.Key.KeySize]));
                            else
                                keys.Add(new ByteVector(Convert.FromHexString(keyversion.Key.Value)));
                        }

                        infoav2.vera = samkey.Variant.KeyVersions[0].Version;
                        if (samkey.Variant.KeyVersions.Count >= 2)
                            infoav2.verb = samkey.Variant.KeyVersions[1].Version;
                        if (samkey.Variant.KeyVersions.Count >= 3)
                            infoav2.verc = samkey.Variant.KeyVersions[2].Version;

                        if ((samkey.Variant.KeyVersions[0].Key.Tags & KeyTag.AES) == KeyTag.AES)
                        {
                            natkey.setKeysData(keys, SAMKeyType.SAM_KEY_AES);
                        }
                        else
                        {
                            if (samkey.Variant.KeyVersions[0].Key.KeySize == 16)
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
                    natkey.setSETKeyTypeFromKeyType();

                    av2cmd.authenticateHost(key, GetSAMProperties().AuthenticateKeyEntryIdentifier);
                    natkey.setUpdateMask(0xFF);
                    av2cmd.changeKeyEntry((byte)Convert.ToDecimal(samkey.Identifier), natkey, key);
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

        public static void SwitchSAMToAV2(SAMAV1ISO7816Commands av1cmds, KeyVersion keyVersion)
        {
            log.Info("Switching the SAM to AV2 mode...");
            var key = new DESFireKey();
            if ((keyVersion.Key.Tags & KeyTag.AES) == KeyTag.AES)
            {
                key.setKeyType(DESFireKeyType.DF_KEY_AES);
                key.setKeyVersion(keyVersion.Version);
                key.fromString(Regex.Replace(keyVersion.Key.Value, ".{2}", "$0 "));
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

                if (keyVersion.Key.KeySize == 16)
                    key.setKeyType(DESFireKeyType.DF_KEY_DES);
                else
                    key.setKeyType(DESFireKeyType.DF_KEY_3K3DES);

                av1cmds.authenticateHost(key, 0);
                av1cmds.changeKeyEntry(0, keyav1entry, key);

                key.fromString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                key.setKeyVersion(0);
            }

            key.setKeyType(DESFireKeyType.DF_KEY_AES);
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
            key.fromString(Regex.Replace(keyVersion.Key.Value, ".{2}", "$0 "));
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

            if (GetSAMProperties().AuthenticateKey == null)
            {
                log.Error("To be updated, a SAM AV2 key store requires at least the Authenticate Key.");
                throw new KeyStoreException("To be updated, a SAM AV2 key store requires at least the Authenticate Key.");
            }

            var cmd = Chip?.getCommands();
            if (cmd is SAMAV2ISO7816Commands av2cmd)
            {
                if (GetSAMProperties().UnlockKey != null && !string.IsNullOrEmpty(GetSAMProperties().UnlockKey.Key.Value) && !_unlocked)
                {
                    UnlockSAM(av2cmd, GetSAMProperties().UnlockKeyEntryIdentifier, GetSAMProperties().UnlockKey);
                    _unlocked = true;
                }

                var key = new DESFireKey();
                key.setKeyType(DESFireKeyType.DF_KEY_AES);
                key.setKeyVersion(GetSAMProperties().AuthenticateKey.Version);
                key.fromString(Regex.Replace(GetSAMProperties().AuthenticateKey.Key.Value, ".{2}", "$0 "));

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

        public override string? ResolveKeyEntryLink(string keyIdentifier, string? divInput = null, string? wrappingKeyId = null, byte wrappingKeyVersion = 0)
        {
            // Will be supported with SAM AV3
            throw new NotSupportedException();
        }

        public override string? ResolveKeyLink(string keyIdentifier, byte keyVersion, string? divInput = null)
        {
            byte[] div;
            log.Info(String.Format("Resolving key link with Key Entry Identifier `{0}`, Key Version `{1}`, Div Input `{2}`...", keyIdentifier, keyVersion, divInput));
            if (!string.IsNullOrEmpty(divInput))
                div = Convert.FromHexString(divInput);
            else
                div = new byte[0];

            if (!CheckKeyEntryExists(keyIdentifier))
            {
                log.Error(String.Format("The key entry `{0}` doesn't exist.", keyIdentifier));
                throw new KeyStoreException("The key entry doesn't exist.");
            }
            byte entry = byte.Parse(keyIdentifier);

            var cmd = Chip?.getCommands();
            if (cmd is SAMAV2ISO7816Commands av2cmd)
            {
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
