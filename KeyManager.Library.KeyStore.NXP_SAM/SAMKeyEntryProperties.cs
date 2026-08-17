namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public abstract class SAMKeyEntryProperties : KeyEntryProperties
    {
        private bool _disableKeyEntry;
        public bool DisableKeyEntry
        {
            get => _disableKeyEntry;
            set => SetProperty(ref _disableKeyEntry, value);
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
