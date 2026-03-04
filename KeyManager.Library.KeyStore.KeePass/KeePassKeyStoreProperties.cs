using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{
    public class KeePassKeyStoreProperties : KeyStoreProperties, IEquatable<KeePassKeyStoreProperties>
    {
        private string _dbPath = string.Empty;
        private string _keyFilePath = string.Empty;
        private string _profilePath = string.Empty;

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

        public override bool Equals(object? obj) => Equals(obj as KeePassKeyStoreProperties);

        public bool Equals(KeePassKeyStoreProperties? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(DBPath, other.DBPath, StringComparison.Ordinal) &&
                   string.Equals(KeyPath, other.KeyPath, StringComparison.Ordinal);
        }

        public override int GetHashCode() => System.HashCode.Combine(DBPath, KeyPath);

        public static bool operator ==(KeePassKeyStoreProperties? left, KeePassKeyStoreProperties? right) => Equals(left, right);
        public static bool operator !=(KeePassKeyStoreProperties? left, KeePassKeyStoreProperties? right) => !Equals(left, right);
    }
}