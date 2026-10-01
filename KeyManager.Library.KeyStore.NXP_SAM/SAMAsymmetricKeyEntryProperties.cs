namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMAsymmetricKeyEntryProperties : SAMKeyEntryProperties
    {
        private bool _allowEncipherKeyEntries;
        public bool AllowEncipherKeyEntries
        {
            get => _allowEncipherKeyEntries;
            set => SetProperty(ref _allowEncipherKeyEntries, value);
        }

        private bool _allowPrivateKeyExport;
        public bool AllowPrivateKeyExport
        {
            get => _allowPrivateKeyExport;
            set => SetProperty(ref _allowPrivateKeyExport, value);
        }

        private bool _disableEncryptData;
        public bool DisableEncryptData
        {
            get => _disableEncryptData;
            set => SetProperty(ref _disableEncryptData, value);
        }

        private bool _disableSignData;
        public bool DisableSignData
        {
            get => _disableSignData;
            set => SetProperty(ref _disableSignData, value);
        }
    }
}
