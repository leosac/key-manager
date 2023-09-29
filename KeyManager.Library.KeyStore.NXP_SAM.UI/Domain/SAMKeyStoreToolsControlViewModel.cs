using Leosac.KeyManager.Library.UI.Domain;
using Leosac.WpfApp;
using LibLogicalAccess.Reader;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class SAMKeyStoreToolsControlViewModel : KeyStoreAdditionalControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SAMKeyStoreToolsControlViewModel()
        {
            _samAuthKey = new KeyVersion() { Name = "Key" };
            _samAuthKeyId = 0;
            _samAuthKeyType = LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES;

            _samUnlockKey = new KeyVersion() { Name = "Key", Key = new Key(null, 16, "") };
            _samUnlockKeyId = 1;
            _samUnlockAction = LibLogicalAccess.Card.SAMLockUnlock.Unlock;

            KeyTypes = new ObservableCollection<LibLogicalAccess.Card.SAMKeyType>(Enum.GetValues<LibLogicalAccess.Card.SAMKeyType>());
            UnlockActions = new ObservableCollection<LibLogicalAccess.Card.SAMLockUnlock>(new[]
            {
                LibLogicalAccess.Card.SAMLockUnlock.Unlock,
                LibLogicalAccess.Card.SAMLockUnlock.LockWithoutSpecifyingKey,
                LibLogicalAccess.Card.SAMLockUnlock.SwitchAV2Mode
            });

            SAMAuthCommand = new RelayCommand(
                () =>
                {
                    SAMAuthenticate();
                });

            SAMSwitchAV2Command = new RelayCommand(
                () =>
                {
                    SAMSwitchAV2();
                });

            SAMLockUnlockCommand = new RelayCommand(
                () =>
                {
                    SAMLockUnlock();
                });

            SAMGetVersionCommand = new RelayCommand(
                () =>
                {
                    SAMGetVersion();
                });

            SAMActivateMifareSAMCommand = new RelayCommand(
                () =>
                {
                    SAMActivateMifareSAM();
                });
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

        public ObservableCollection<LibLogicalAccess.Card.SAMKeyType> KeyTypes { get; set; }

        public ObservableCollection<LibLogicalAccess.Card.SAMLockUnlock> UnlockActions { get; set; }

        public RelayCommand SAMAuthCommand { get; }

        public RelayCommand SAMSwitchAV2Command { get; }

        public RelayCommand SAMLockUnlockCommand { get; }

        public RelayCommand SAMGetVersionCommand { get; }

        public RelayCommand SAMActivateMifareSAMCommand { get; }

        private void SAMGetVersion()
        {
            try
            {
                var chip = (KeyStore as SAMKeyStore)?.Chip;
                var cmd = chip?.getCommands();
                var ctype = chip?.getCardType();

                LibLogicalAccess.Card.SAMVersion? version = null;
                if (cmd is SAMAV1ISO7816Commands samav1cmd)
                    version = samav1cmd.getVersion();
                else if (cmd is SAMAV2ISO7816Commands samav2cmd)
                    version = samav2cmd.getVersion();

                var msg = string.Format("Card Type: {0}", ctype);
                if (version != null)
                {
                    msg += Environment.NewLine + string.Format("Software Version: {0}.{1}", version.software.majorversion, version.software.minorversion);
                    msg += Environment.NewLine + string.Format("Hardware Version: {0}.{1}", version.hardware.majorversion, version.hardware.minorversion);
                }

                if (SnackbarMessageQueue != null)
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, msg);
            }
            catch (Exception ex)
            {
                log.Error("SAM communication failed.", ex);
                if (SnackbarMessageQueue != null)
                    SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }

        private void SAMAuthenticate()
        {
            try
            {
                var key = new LibLogicalAccess.Card.DESFireKey();

                if (SAMAuthKeyType == LibLogicalAccess.Card.SAMKeyType.SAM_KEY_DES)
                    key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_DES);
                else if (SAMAuthKeyType == LibLogicalAccess.Card.SAMKeyType.SAM_KEY_AES)
                    key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_AES);
                else
                    key.setKeyType(LibLogicalAccess.Card.DESFireKeyType.DF_KEY_3K3DES);

                key.fromString(SAMAuthKey.Key.GetAggregatedValue<string>(KeyValueFormat.HexStringWithSpace));
                key.setKeyVersion(SAMAuthKey.Version);
                var cmd = (KeyStore as SAMKeyStore)?.Chip?.getCommands();
                if (cmd is SAMAV1ISO7816Commands samav1cmd)
                    samav1cmd.authenticateHost(key, SAMAuthKeyId);
                else if (cmd is SAMAV2ISO7816Commands samav2cmd)
                    samav2cmd.authenticateHost(key, SAMAuthKeyId);

                if (SnackbarMessageQueue != null)
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "SAM Authentication succeeded!");
            }
            catch (Exception ex)
            {
                log.Error("SAM Authentication failed.", ex);
                if (SnackbarMessageQueue != null)
                    SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }

        private void SAMSwitchAV2()
        {
            try
            {
                var ks = (KeyStore as SAMKeyStore);
                var cmd = ks?.Chip?.getCommands();
                if (cmd is SAMAV1ISO7816Commands samav1cmd)
                {
                    ks?.SwitchSAMToAV2(samav1cmd);
                    if (SnackbarMessageQueue != null)
                        SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "SAM Switch to AV2 succeeded!");
                }
                else if (cmd is SAMAV2ISO7816Commands samav2cmd)
                {
                    if (SnackbarMessageQueue != null)
                        SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "SAM Switch to AV2 skipped, already in AV2 mode.");
                }
            }
            catch (Exception ex)
            {
                log.Error("SAM Switch to AV2 failed.", ex);
                if (SnackbarMessageQueue != null)
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
                    if (SnackbarMessageQueue != null)
                        SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "Activate Mifare SAM skipped, the SAM is in AV1 mode.");
                }   
                else if (cmd is SAMAV2ISO7816Commands samav2cmd)
                {
                    ks?.ActivateMifareSAM(samav2cmd);
                    if (SnackbarMessageQueue != null)
                        SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "Activate Mifare SAM completed.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Activate Mifare SAM failed.", ex);
                if (SnackbarMessageQueue != null)
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
                key.fromString(SAMUnlockKey.Key.GetAggregatedValue<string>(KeyValueFormat.HexStringWithSpace));
                key.setKeyVersion(SAMUnlockKey.Version);

                if (cmd is SAMAV1ISO7816Commands samav1cmd)
                {
                    samav1cmd.lockUnlock(key, SAMUnlockAction, SAMUnlockKeyId, 0, 0);
                }
                else if (cmd is SAMAV2ISO7816Commands samav2cmd)
                {
                    samav2cmd.lockUnlock(key, SAMUnlockAction, SAMUnlockKeyId, 0, 0);
                }

                if (SnackbarMessageQueue != null)
                    SnackbarHelper.EnqueueMessage(SnackbarMessageQueue, "SAM Lock/Unlock completed.");
            }
            catch (Exception ex)
            {
                log.Error("Lock/Unlock SAM failed.", ex);
                if (SnackbarMessageQueue != null)
                    SnackbarHelper.EnqueueError(SnackbarMessageQueue, ex);
            }
        }
    }
}
