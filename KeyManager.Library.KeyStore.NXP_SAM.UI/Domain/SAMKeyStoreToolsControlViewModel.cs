using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using LibLogicalAccess.Card;
using LibLogicalAccess.Reader;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM.UI.Domain
{
    public class SAMKeyStoreToolsControlViewModel : KeyStoreAdditionalControlViewModel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public SAMKeyStoreToolsControlViewModel()
        {
            _samAuthKey = new KeyVersion() { Name = "Key" };
            _samAuthKeyId = 0;
            _samAuthKeyType = SAMKeyType.SAM_KEY_AES;

            KeyTypes = new ObservableCollection<SAMKeyType>(Enum.GetValues<SAMKeyType>());

            SAMAuthCommand = new KeyManagerCommand(
                parameter =>
                {
                    SAMAuthenticate();
                });

            SAMSwitchAV2Command = new KeyManagerCommand(
                parameter =>
                {
                    SAMSwitchAV2();
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

        private SAMKeyType _samAuthKeyType;

        public SAMKeyType SAMAuthKeyType
        {
            get => _samAuthKeyType;
            set => SetProperty(ref _samAuthKeyType, value);
        }

        public ObservableCollection<SAMKeyType> KeyTypes { get; set; }

        public KeyManagerCommand SAMAuthCommand { get; }

        public KeyManagerCommand SAMSwitchAV2Command { get; }

        private void SAMAuthenticate()
        {
            try
            {
                var key = new DESFireKey();

                if (SAMAuthKeyType == SAMKeyType.SAM_KEY_DES)
                    key.setKeyType(DESFireKeyType.DF_KEY_DES);
                else if (SAMAuthKeyType == SAMKeyType.SAM_KEY_AES)
                    key.setKeyType(DESFireKeyType.DF_KEY_AES);
                else
                    key.setKeyType(DESFireKeyType.DF_KEY_3K3DES);

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
    }
}
