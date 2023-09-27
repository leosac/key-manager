using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyUsageCounter : ObservableValidator
    {
        public SAMKeyUsageCounter()
        {
            _identifier = 0;
            _changeKeyRefId = 0;
            _changeKeyRefVersion = 0;
            _limit = uint.MaxValue;
            _value = 0;
        }

        private byte _identifier;

        public byte Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
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

        private uint _limit;

        public uint Limit
        {
            get => _limit;
            set => SetProperty(ref _limit, value);
        }

        private uint _value;

        public uint Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
