using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Leosac.KeyManager.Library.KeyStore.NXP_SAM
{
    public class SAMKeyStoreProperties : KeyStoreProperties, IEquatable<SAMKeyStoreProperties>
    {
        public SAMKeyStoreProperties() : base()
        {
            _readerProvider = "PCSC";
            _readerUnit = String.Empty;
            _autoSwitchToAV2 = true;
            _authenticateKeyEntryIdentifier = 0;
            _authenticateKey = new KeyVersion("Authenticate Key", 0, new Key(KeyTag.AES, 16, "00000000000000000000000000000000"));
            _unlockKeyEntryIdentifier = 0;
            _unlockKey = new KeyVersion("Unlock Key", 0, new Key(KeyTag.AES, 16));
        }

        private string _readerProvider;

        public string ReaderProvider
        {
            get => _readerProvider;
            set => SetProperty(ref _readerProvider, value);
        }

        private string _readerUnit;

        public string ReaderUnit
        {
            get => _readerUnit;
            set => SetProperty(ref _readerUnit, value);
        }

        private bool _autoSwitchToAV2;

        public bool AutoSwitchToAV2
        {
            get => _autoSwitchToAV2;
            set => SetProperty(ref _autoSwitchToAV2, value);
        }

        private byte _authenticateKeyEntryIdentifier;

        public byte AuthenticateKeyEntryIdentifier
        {
            get => _authenticateKeyEntryIdentifier;
            set => SetProperty(ref _authenticateKeyEntryIdentifier, value);
        }

        private KeyVersion _authenticateKey;

        public KeyVersion AuthenticateKey
        {
            get => _authenticateKey;
            set => SetProperty(ref _authenticateKey, value);
        }

        private byte _unlockKeyEntryIdentifier;

        public byte UnlockKeyEntryIdentifier
        {
            get => _unlockKeyEntryIdentifier;
            set => SetProperty(ref _unlockKeyEntryIdentifier, value);
        }

        private KeyVersion _unlockKey;

        public KeyVersion UnlockKey
        {
            get => _unlockKey;
            set => SetProperty(ref _unlockKey, value);
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as SAMKeyStoreProperties);
        }

        public bool Equals(SAMKeyStoreProperties? p)
        {
            if (p is null)
                return false;

            if (Object.ReferenceEquals(this, p))
                return true;

            if (this.GetType() != p.GetType())
                return false;

            return (ReaderProvider == p.ReaderProvider) && (ReaderUnit == p.ReaderUnit) && (AutoSwitchToAV2 == p.AutoSwitchToAV2) &&
                (AuthenticateKeyEntryIdentifier == p.AuthenticateKeyEntryIdentifier) && (AuthenticateKey == p.AuthenticateKey) &&
                (UnlockKeyEntryIdentifier == p.UnlockKeyEntryIdentifier) && (UnlockKey == p.UnlockKey);
        }

        public override int GetHashCode() => (ReaderProvider, ReaderUnit, AutoSwitchToAV2, AuthenticateKeyEntryIdentifier, AuthenticateKey, UnlockKeyEntryIdentifier, UnlockKey).GetHashCode();

        public static bool operator ==(SAMKeyStoreProperties lhs, SAMKeyStoreProperties rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                    return true;

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(SAMKeyStoreProperties lhs, SAMKeyStoreProperties rhs) => !(lhs == rhs);
    }
}
