using System.Collections.ObjectModel;
using System.Security;

namespace Leosac.KeyManager.Library.KeyStore.KeePass
{

    public class KeePassKeyStoreProperties : KeyStoreProperties, IEquatable<KeePassKeyStoreProperties>
    {
        private string _dbpath;
        private string _keyFilePath;

        public KeePassKeyStoreProperties()
        {
            _dbpath = string.Empty;
            _keyFilePath = string.Empty;
        }

        public string DBpath
        {
            get => _dbpath;
            set => SetProperty(ref _dbpath, value);
        }

        public string KeyPath
        {
            get => _keyFilePath;
            set => SetProperty(ref _keyFilePath, value);
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as KeePassKeyStoreProperties);
        }

        public bool Equals(KeePassKeyStoreProperties? p)
        {
            if (p is null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            if (this.GetType() != p.GetType())
            {
                return false;
            }

            return (DBpath == p.DBpath) && (KeyPath == p.KeyPath);
        }

        public override int GetHashCode() => (DBpath, KeyPath).GetHashCode();

        public static bool operator ==(KeePassKeyStoreProperties? lhs, KeePassKeyStoreProperties? rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(KeePassKeyStoreProperties? lhs, KeePassKeyStoreProperties? rhs) => !(lhs == rhs);
    }
}
