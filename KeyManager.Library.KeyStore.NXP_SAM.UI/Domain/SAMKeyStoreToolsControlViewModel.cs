using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using LibLogicalAccess.Reader;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using LibLogicalAccess;
using System.Configuration.Provider;
using LibLogicalAccess.Card;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class SAMKeyStoreToolsControlViewModel : KeyStoreAdditionalControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SAMKeyStoreToolsControlViewModel()
        {
            _samAuthKey = new KeyVersion { Name = "Key" };
            _samAuthKeyId = 0;
            _samAuthKeyType = LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES128;

            _samUnlockKey = new KeyVersion { Name = "Key", Key = new Key(null, 16, "") };
            _samUnlockKeyId = 1;
            _samUnlockAction = LibLogicalAccess.Card.SAMLockUnlock.Unlock;

            KeyTypes = new ObservableCollection<LibLogicalAccess.Card.SAMKeyType>(Enum.GetValues<LibLogicalAccess.Card.SAMKeyType>());
            UnlockActions = new ObservableCollection<LibLogicalAccess.Card.SAMLockUnlock>(new[]
            {
                LibLogicalAccess.Card.SAMLockUnlock.Unlock,
                LibLogicalAccess.Card.SAMLockUnlock.LockWithoutSpecifyingKey,
                LibLogicalAccess.Card.SAMLockUnlock.SwitchAV2Mode
            });

            _samDESFireKeyId = 2;
            _rfidReaderProvider = "PCSC";

            SAMAuthCommand = new RelayCommand(SAMAuthenticate);
            SAMSwitchAV2Command = new AsyncRelayCommand(SAMSwitchAV2);
            SAMLockUnlockCommand = new RelayCommand(SAMLockUnlock);
            SAMGetVersionCommand = new RelayCommand(SAMGetVersion);
            SAMActivateMifareSAMCommand = new RelayCommand(SAMActivateMifareSAM);
            DESFireAuthenticateCommand = new RelayCommand(DESFireAuthenticate);
        }

        private KeyVersion _samAuthKey;
        public KeyVersion SAMAuthKey
        {
            get => _samAuthKey;
            set => SetProperty(ref _samAuthKey, value);
        }

        private byte _samAuthKeyId;
        public byte SAMAuthKeyId
        {
            get => _samAuthKeyId;
            set => SetProperty(ref _samAuthKeyId, value);
        }

        private LibLogicalAccess.Card.SAMKeyType _samAuthKeyType;
        public LibLogicalAccess.Card.SAMKeyType SAMAuthKeyType
        {
            get => _samAuthKeyType;
            set => SetProperty(ref _samAuthKeyType, value);
        }

        private KeyVersion _samUnlockKey;
        public KeyVersion SAMUnlockKey
        {
            get => _samUnlockKey;
            set => SetProperty(ref _samUnlockKey, value);
        }

        private byte _samUnlockKeyId;
        public byte SAMUnlockKeyId
        {
            get => _samUnlockKeyId;
            set => SetProperty(ref _samUnlockKeyId, value);
        }

        private LibLogicalAccess.Card.SAMLockUnlock _samUnlockAction;
        public LibLogicalAccess.Card.SAMLockUnlock SAMUnlockAction
        {
            get => _samUnlockAction;
            set => SetProperty(ref _samUnlockAction, value);
        }

        private byte _samDESFireKeyId;
        public byte SAMDESFireKeyId
        {
            get => _samDESFireKeyId;
            set => SetProperty(ref _samDESFireKeyId, value);
        }

        private byte _samDESFireKeyVersion;
        public byte SAMDESFireKeyVersion
        {
            get => _samDESFireKeyVersion;
            set => SetProperty(ref _samDESFireKeyVersion, value);
        }

        private string _rfidReaderProvider;
        public string RFIDReaderProvider
        {
            get => _rfidReaderProvider;
            set => SetProperty(ref _rfidReaderProvider, value);
        }

        private string _rfidReaderUnit;
        public string RFIDReaderUnit
        {
            get => _rfidReaderUnit;
            set => SetProperty(ref _rfidReaderUnit, value);
        }

        private byte[] _desfireAID = new byte[3];
        public byte[] DESFireAID
        {
            get => _desfireAID;
            set => SetProperty(ref _desfireAID, value);
        }

        private byte _desfireKeyNum;
        public byte DESFireKeyNum
        {
            get => _desfireKeyNum;
            set => SetProperty(ref _desfireKeyNum, value);
        }

        private bool _desfireUseDiversification;
        public bool DESFireUseDiversification
        {
            get => _desfireUseDiversification;
            set => SetProperty(ref _desfireUseDiversification, value);
        }

        private byte[] _desfireDivInput = Array.Empty<byte>();
        public byte[] DESFireDivInput
        {
            get => _desfireDivInput;
            set => SetProperty(ref _desfireDivInput, value);
        }

        private bool _desfireReadFile;
        public bool DESFireReadFile
        {
            get => _desfireReadFile;
            set => SetProperty(ref _desfireReadFile, value);
        }

        private byte _desfireFileNo;
        public byte DESFireFileNo
        {
            get => _desfireFileNo;
            set => SetProperty(ref _desfireFileNo, value);
        }

        public ObservableCollection<LibLogicalAccess.Card.SAMKeyType> KeyTypes { get; set; }

        public ObservableCollection<LibLogicalAccess.Card.SAMLockUnlock> UnlockActions { get; set; }

        public RelayCommand SAMAuthCommand { get; }

        public AsyncRelayCommand SAMSwitchAV2Command { get; }

        public RelayCommand SAMLockUnlockCommand { get; }

        public RelayCommand SAMGetVersionCommand { get; }

        public RelayCommand SAMActivateMifareSAMCommand { get; }

        public RelayCommand DESFireAuthenticateCommand { get; }

        private void SAMGetVersion()
        {
            try
            {
                var chip = (KeyStore as SAMKeyStore)?.Chip;
                var cmd = chip?.getCommands();
                var ctype = chip?.getCardType();
                var rctype = string.Empty;

                LibLogicalAccess.Card.SAMVersion? version = null;
                if (cmd is SAMAV1ISO7816Commands samav1cmd)
                {
                    version = samav1cmd.getVersion();
                    rctype = samav1cmd.getSAMTypeFromSAM();
                }
                else if (cmd is SAMAV2ISO7816Commands samav2cmd)
                {
                    version = samav2cmd.getVersion();
                    rctype = samav2cmd.getSAMTypeFromSAM();
                }

                var msg = string.Format("Card Type: {0}", ctype);
                if (!string.IsNullOrEmpty(rctype))
                {
                    msg += Environment.NewLine + string.Format("Real Card Type: {0}", rctype);
                }
                if (version != null)
                {
                    msg += Environment.NewLine + string.Format("UID: {0}", Convert.ToHexString(version.manufacture.uniqueserialnumber));
                    msg += Environment.NewLine + string.Format("Software Version: {0}.{1}", version.software.majorversion, version.software.minorversion);
                    msg += Environment.NewLine + string.Format("Hardware Version: {0}.{1}", version.hardware.majorversion, version.hardware.minorversion);
                }

                SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, msg);
            }
            catch (Exception ex)
            {
                log.Error("SAM communication failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }

        private void SAMAuthenticate()
        {
            try
            {
                var key = new LibLogicalAccess.Card.DESFireKey();

                if (SAMAuthKeyType == LibLogicalAccess.Card.SAMKeyType.SAM_KEY_DES)
                {
                    key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_DES);
                }
                else if (SAMAuthKeyType == LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES128)
                {
                    key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES);
                }
                else
                {
                    key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_3K3DES);
                }

                key.fromString(SAMAuthKey.Key.GetAggregatedValueAsString(KeyValueStringFormat.HexStringWithSpace));
                key.setKeyVersion(SAMAuthKey.Version);
                var cmd = (KeyStore as SAMKeyStore)?.Chip?.getCommands();
                if (cmd is SAMAV1ISO7816Commands samav1cmd)
                {
                    samav1cmd.authenticateHost(key, SAMAuthKeyId);
                }
                else if (cmd is SAMAV2ISO7816Commands samav2cmd)
                {
                    samav2cmd.authenticateHost(key, SAMAuthKeyId);
                }

                SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "SAM Authentication succeeded!");
            }
            catch (Exception ex)
            {
                log.Error("SAM Authentication failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }

        private async Task SAMSwitchAV2()
        {
            try
            {
                var ks = (KeyStore as SAMKeyStore);
                var cmd = ks?.Chip?.getCommands();
                if (cmd is SAMAV1ISO7816Commands samav1cmd)
                {
                    if (ks != null)
                    {
                        await ks.SwitchSAMToAV2(samav1cmd);
                    }
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "SAM Switch to AV2 succeeded!");
                }
                else if (cmd is SAMAV2ISO7816Commands samav2cmd)
                {
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "SAM Switch to AV2 skipped, already in AV2 mode.");
                }
            }
            catch (Exception ex)
            {
                log.Error("SAM Switch to AV2 failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }

        private void SAMActivateMifareSAM()
        {
            try
            {
                var ks = (KeyStore as SAMKeyStore);
                var cmd = ks?.Chip?.getCommands();
                if (cmd is SAMAV1ISO7816Commands samav1cmd)
                {
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "Activate Mifare SAM skipped, the SAM is in AV1 mode.");
                }   
                else if (cmd is SAMAV2ISO7816Commands samav2cmd)
                {
                    ks?.ActivateMifareSAM(samav2cmd);
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "Activate Mifare SAM completed.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Activate Mifare SAM failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }

        private void SAMLockUnlock()
        {
            try
            {
                var ks = (KeyStore as SAMKeyStore);
                var cmd = ks?.Chip?.getCommands();

                var key = new LibLogicalAccess.Card.DESFireKey();
                key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES);
                key.fromString(SAMUnlockKey.Key.GetAggregatedValueAsString(KeyValueStringFormat.HexStringWithSpace));
                key.setKeyVersion(SAMUnlockKey.Version);

                if (cmd is SAMAV1ISO7816Commands samav1cmd)
                {
                    samav1cmd.lockUnlock(key, SAMUnlockAction, SAMUnlockKeyId, 0, 0);
                }
                else if (cmd is SAMAV2ISO7816Commands samav2cmd)
                {
                    samav2cmd.lockUnlock(key, SAMUnlockAction, SAMUnlockKeyId, 0, 0);
                }
                
                SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "SAM Lock/Unlock completed.");
            }
            catch (Exception ex)
            {
                log.Error("Lock/Unlock SAM failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }

        private void DESFireAuthenticate()
        {
            try
            {
                if (!string.IsNullOrEmpty(RFIDReaderProvider))
                {
                    var ks = KeyStore as SAMKeyStore;
                    var cmd = ks?.Chip?.getCommands();
                    if (cmd is SAMAV2ISO7816Commands samav2cmd)
                    {
                        var lla = LibLogicalAccess.LibraryManager.getInstance();
                        var rp = lla.getReaderProvider(RFIDReaderProvider);
                        if (rp == null)
                        {
                            log.Error(string.Format("Cannot initialize the RFID Reader Provider `{0}`.", RFIDReaderProvider));
                            throw new KeyStoreException("Cannot initialize the RFID Reader Provider.");
                        }

                        ReaderUnit? ru = null;
                        if (string.IsNullOrEmpty(RFIDReaderUnit))
                        {
                            ru = rp.createReaderUnit();
                        }
                        else
                        {
                            var readers = rp.getReaderList();
                            foreach (var reader in readers)
                            {
                                if (reader.getName() == RFIDReaderUnit)
                                {
                                    ru = reader;
                                    break;
                                }
                            }
                        }
                        if (ru == null)
                        {
                            log.Error("Cannot initialize the RFID Reader Unit.");
                            throw new KeyStoreException("Cannot initialize the RFID Reader Unit.");
                        }

                        if (ru is not ISO7816ReaderUnit isoru)
                        {
                            throw new KeyStoreException("The RFID Reader Unit needs to implement ISO7816 interface.");
                        }

                        var ruconfig = isoru.getConfiguration() as ISO7816ReaderUnitConfiguration;
                        if (ruconfig != null)
                        {
                            ruconfig.setSAMType("SAM_AV2");
                            ruconfig.setSAMReaderName(ks.GetSAMProperties().ReaderUnit);
                            ruconfig.setSAMUnlockKey(SAMKeyStore.CreateDESFireKey(ks.GetSAMProperties().AuthenticateKeyType, ks.GetSAMProperties().AuthenticateKeyVersion, ks.GetSAMProperties().Secret), ks.GetSAMProperties().AuthenticateKeyEntryIdentifier);
                            ruconfig.setUseSAMAuthenticateHost(ks.GetSAMProperties().AuthenticationMode == SAMAuthenticationMode.AuthenticateHost);
                            isoru.setConfiguration(ruconfig);
                        }

                        if (!ru.connectToReader())
                        {
                            log.Error("Cannot connect to the RFID Reader Unit.");
                            throw new KeyStoreException("Cannot connect to the RFID Reader Unit.");
                        }

                        try
                        {
                            ru.setCardType("DESFireEV1");

                            if (!ru.waitInsertion(5000))
                            {
                                throw new KeyStoreException("No RFID card inserted.");
                            }
                            if (!ru.connect())
                            {
                                throw new KeyStoreException("Cannot connect to the RFID card.");
                            }

                            var chip = ru.getSingleChip();
                            if (chip == null)
                            {
                                throw new KeyStoreException("Cannot retrieve the chip for inserted RFID card.");
                            }
                            if (chip.getCommands() is not DESFireEV1ISO7816Commands ev1cmd)
                            {
                                throw new KeyStoreException("Unexpected commands type for the inserted RFID card.");
                            }
                            uint aid = (uint)(DESFireAID[0] << 16 | DESFireAID[1] << 8 | DESFireAID[2]);
                            ev1cmd.selectApplication(aid);

                            var key = new DESFireKey();
                            key.setKeyType(DESFireKeyType.DF_KEY_AES);
                            var kss = new SAMKeyStorage();
                            kss.setKeySlot(SAMDESFireKeyId);
                            key.setKeyVersion(SAMDESFireKeyVersion);
                            key.setKeyStorage(kss);

                            if (DESFireUseDiversification)
                            {
                                var div = new NXPAV2KeyDiversification();
                                if (DESFireDivInput != null && DESFireDivInput.Length > 0)
                                {
                                    div.setDivInput(new ByteVector(DESFireDivInput));
                                }
                                key.setKeyDiversification(div);
                            }

                            ev1cmd.authenticate(DESFireKeyNum, key);
                            SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "DESFire authentication with SAM succeeded.");

                            if (DESFireReadFile)
                            {
                                var data = ev1cmd.readData(DESFireFileNo, 0, 0, EncryptionMode.CM_ENCRYPT);
                                if (data != null)
                                {
                                    var datastr = Convert.ToHexString(data.ToArray());
                                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, string.Format("Data read: {0}", datastr));
                                }
                            }

                            ru.disconnect();
                            ru.waitRemoval(1);
                        }
                        finally
                        {
                            isoru.setSAMChip(null);
                            isoru.setSAMReaderUnit(null);

                            ru.disconnectFromReader();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error("DESFire authentication failed.", ex);
                SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }
    }
}
