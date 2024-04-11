namespace Leosac.KeyManager.Library.KeyStore.File
{
    public class FileKeyStoreProperties : KeyStoreProperties, IEquatable<FileKeyStoreProperties>
    {
        public FileKeyStoreProperties()
        {
            _fullpath = string.Empty;
            _deepListing = true;
        }

        private string _fullpath;
        public string Fullpath
        {
            get => _fullpath;
            set => SetProperty(ref _fullpath, value);
        }

        private bool _deepListing;
        public bool DeepListing
        {
            get => _deepListing;
            set => SetProperty(ref _deepListing, value);
        }

        public override int? SecretMaxLength => 32;

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as FileKeyStoreProperties);
        }

        public bool Equals(FileKeyStoreProperties? p)
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

            return (Fullpath == p.Fullpath);
        }

        public override int GetHashCode() => (Fullpath).GetHashCode();

        public static bool operator ==(FileKeyStoreProperties? lhs, FileKeyStoreProperties? rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(FileKeyStoreProperties? lhs, FileKeyStoreProperties? rhs) => !(lhs == rhs);
    }
}
