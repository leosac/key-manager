namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMSymmetricKeyEntryProperties : KeyEntryProperties
    {
        private SAMKeyEntryType _samKeyEntryType = SAMKeyEntryType.Host;
        public SAMKeyEntryType SAMKeyEntryType
        {
            get => _samKeyEntryType;
            set => SetProperty(ref _samKeyEntryType, value);
        }

        private bool _enableDumpSessionKey;
        public bool EnableDumpSessionKey
        {
            get => _enableDumpSessionKey;
            set => SetProperty(ref _enableDumpSessionKey, value);
        }

        private bool _cryptoBasedOnSecretKey;
        public bool CryptoBasedOnSecretKey
        {
            get => _cryptoBasedOnSecretKey;
            set => SetProperty(ref _cryptoBasedOnSecretKey, value);
        }

        private bool _keepIV;
        public bool KeepIV
        {
            get => _keepIV;
            set => SetProperty(ref _keepIV, value);
        }

        private bool _lockUnlock;
        public bool LockUnlock
        {
            get => _lockUnlock;
            set => SetProperty(ref _lockUnlock, value);
        }

        private bool _authenticateHost = true;
        public bool AuthenticateHost
        {
            get => _authenticateHost;
            set => SetProperty(ref _authenticateHost, value);
        }

        private bool _disableChangeKeyPICC;
        public bool DisableChangeKeyPICC
        {
            get => _disableChangeKeyPICC;
            set => SetProperty(ref _disableChangeKeyPICC, value);
        }

        private bool _disableDecryptData;
        public bool DisableDecryptData
        {
            get => _disableDecryptData;
            set => SetProperty(ref _disableDecryptData, value);
        }

        private bool _disableEncryptData;
        public bool DisableEncryptData
        {
            get => _disableEncryptData;
            set => SetProperty(ref _disableEncryptData, value);
        }

        private bool _disableVerifyMACFromPICC;
        public bool DisableVerifyMACFromPICC
        {
            get => _disableVerifyMACFromPICC;
            set => SetProperty(ref _disableVerifyMACFromPICC, value);
        }

        private bool _disableGenerateMACFromPICC;
        public bool DisableGenerateMACFromPICC
        {
            get => _disableGenerateMACFromPICC;
            set => SetProperty(ref _disableGenerateMACFromPICC, value);
        }

        private bool _disableKeyEntry;
        public bool DisableKeyEntry
        {
            get => _disableKeyEntry;
            set => SetProperty(ref _disableKeyEntry, value);
        }

        private bool _allowDumpSecretKey;
        public bool AllowDumpSecretKey
        {
            get => _allowDumpSecretKey;
            set => SetProperty(ref _allowDumpSecretKey, value);
        }

        private bool _allowDumpSecretKeyWithDiv;
        public bool AllowDumpSecretKeyWithDiv
        {
            get => _allowDumpSecretKeyWithDiv;
            set => SetProperty(ref _allowDumpSecretKeyWithDiv, value);
        }

        private bool _reservedForPerso;
        public bool ReservedForPerso
        {
            get => _reservedForPerso;
            set => SetProperty(ref _reservedForPerso, value);
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

        private byte? keyUsageCounter = null;
        public byte? KeyUsageCounter
        {
            get => keyUsageCounter;
            set => SetProperty(ref keyUsageCounter, value);
        }

        private byte _changeKeyRefId;
        public byte ChangeKeyRefId
        {
            get => _changeKeyRefId;
            set => SetProperty(ref _changeKeyRefId, value);
        }

        private byte _changeKeyRefVersion;
        public byte ChangeKeyRefVersion
        {
            get => _changeKeyRefVersion;
            set => SetProperty(ref _changeKeyRefVersion, value);
        }
    }
}
