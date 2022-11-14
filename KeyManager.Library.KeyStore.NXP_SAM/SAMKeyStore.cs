using LibLogicalAccess.Reader;
using LibLogicalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibLogicalAccess.Card;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyStore : KeyStore
    {
        public const uint SAM_AV2_MAX_SYMMETRIC_ENTRIES = 128;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SAMKeyStore()
        {
            
        }

        private LibLogicalAccess.ReaderProvider? _readerProvider;
        private LibLogicalAccess.ReaderUnit? _readerUnit;
        private LibLogicalAccess.Chip? _chip;

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
            _readerProvider = lla.getReaderProvider(providerName);
            if (_readerProvider == null)
            {
                log.Error(String.Format("Cannot initialize the Reader Provider `{0}`.", providerName));
                throw new KeyStoreException("Cannot initialize the Reader Provider.");
            }

            var ruName = GetSAMProperties().ReaderUnit;
            if (string.IsNullOrEmpty(ruName))
            {
                _readerUnit = _readerProvider.createReaderUnit();
            }
            else
            {
                var readers = _readerProvider.getReaderList();
                foreach (var reader in readers)
                {
                    if (reader.getName() == ruName)
                    {
                        _readerUnit = reader;
                        break;
                    }
                }
            }
            if (_readerUnit == null)
            {
                log.Error("Cannot initialize the Reader Unit.");
                throw new KeyStoreException("Cannot initialize the Reader Unit."); 
            }

            if (!_readerUnit.connectToReader())
            {
                log.Error("Cannot connect to the Reader Unit.");
                throw new KeyStoreException("Cannot connect to the Reader Unit.");
            }

            if (_readerUnit.waitInsertion(1000))
            {
                if (_readerUnit.connect())
                {
                    var chip = _readerUnit.getSingleChip();
                    var genericType = chip.getGenericCardType();
                    if (String.Compare(genericType, "SAM") >= 0)
                    {
                        _chip = chip;
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
        }

        public override void Close()
        {
            log.Info("Closing the key store...");
            if (_readerUnit != null)
            {
                if (_chip != null)
                {
                    _readerUnit.disconnect();
                    _readerUnit.waitRemoval(1);
                    _chip = null;
                }
                _readerUnit.disconnectFromReader();
                _readerUnit.Dispose();
                _readerUnit = null;
            }

            if (_readerProvider != null)
            {
                _readerProvider.release();
                _readerProvider.Dispose();
                _readerProvider = null;
            }
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

        public override void Create(KeyEntry keyEntry)
        {
            log.Info(String.Format("Creating key entry `{0}`...", keyEntry.Identifier));
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
            var cmd = _chip?.getCommands();
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
                keyEntry = CreateKeyEntryFromKeyType(set.keytype);
                
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

            return keyEntry;
        }

        private SAMSymmetricKeyEntry CreateKeyEntryFromKeyType(byte[] keyType)
        {
            if (keyType.Length != 3)
            {
                log.Error("Expected KeyType vector length to be 3 byte.");
                throw new KeyStoreException("Expected KeyType vector length to be 3 byte.");
            }

            var keyEntry = new SAMSymmetricKeyEntry();
            if (keyType[2] == 0 && keyType[1] == 0 && keyType[0] == 0)
                keyEntry.SetVariant("DES");
            else if (keyType[2] == 1 && keyType[1] == 0 && keyType[0] == 0)
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
            return entries;
        }

        public override void Store(IList<KeyEntry> keyEntries)
        {
            log.Info(String.Format("Storing `{0}` key entries...", keyEntries.Count));
            foreach (var keyEntry in keyEntries)
            {
                Update(keyEntry);
            }
        }

        public override void Update(KeyEntry keyEntry, bool ignoreIfMissing = false)
        {
            log.Info(String.Format("Updating key entry `{0}`...", keyEntry.Identifier));



            log.Info(String.Format("Key entry `{0}` updated.", keyEntry.Identifier));
        }
    }
}
