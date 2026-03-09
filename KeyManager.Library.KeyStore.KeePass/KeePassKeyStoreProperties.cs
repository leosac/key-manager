using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{
    public class KeePassKeyStoreProperties : KeyStoreProperties, IEquatable<KeePassKeyStoreProperties>
    {
        private string _dbPath = string.Empty;
        private string _keyFilePath = string.Empty;
        private string _profilePath = string.Empty;
        private bool _isSilent;
        private int _selectedCredentialMode;

        public string DBPath
        {
            get => _dbPath;
            set => SetProperty(ref _dbPath, value);
        }

        public string KeyPath
        {
            get => _keyFilePath;
            set => SetProperty(ref _keyFilePath, value);
        }

        public string ProfilePath
        {
            get => _profilePath;
            set => SetProperty(ref _profilePath, value);
        }
        public bool Silent
        {
            get => _isSilent;
            set => SetProperty(ref _isSilent, value);
        }

        public int SelectedCredentialMode
        {
            get => _selectedCredentialMode;
            set => SetProperty(ref _selectedCredentialMode, value);
        }

        public override bool Equals(object? obj) => Equals(obj as KeePassKeyStoreProperties);

        public bool Equals(KeePassKeyStoreProperties? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(DBPath, other.DBPath, StringComparison.Ordinal) &&
                   string.Equals(KeyPath, other.KeyPath, StringComparison.Ordinal) &&
                   string.Equals(ProfilePath, other.ProfilePath, StringComparison.Ordinal) &&
                   Silent == other.Silent &&
                   SelectedCredentialMode == other.SelectedCredentialMode;
        }

        public override int GetHashCode() => System.HashCode.Combine(DBPath, KeyPath, ProfilePath, Silent, SelectedCredentialMode);

        public static bool operator ==(KeePassKeyStoreProperties? left, KeePassKeyStoreProperties? right) => Equals(left, right);
        public static bool operator !=(KeePassKeyStoreProperties? left, KeePassKeyStoreProperties? right) => !Equals(left, right);
    }
}