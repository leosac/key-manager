using System.Text.Json.Serialization;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyVersion : KeyContainer
    {
        private bool _trackChanges;
        private bool _hasChanges;

        public KeyVersion() : base("Key Version")
        {
            _version = 0;
        }

        public KeyVersion(string name, byte version) : base(name)
        {
            _version = version;
        }

        public KeyVersion(string name, byte version, Key key) : base(name, key)
        {
            _version = version;
        }

        private byte _version;

        public byte Version
        {
            get => _version;
            set
            {
                SetProperty(ref _version, value);
                if (_trackChanges)
                {
                    _hasChanges = true;
                }
            }
        }

        public void TrackChanges()
        {
            _trackChanges = true;
            _hasChanges = false;
        }

        public override bool IsConfigured()
        {
            return base.IsConfigured() || _hasChanges;
        }
    }
}
