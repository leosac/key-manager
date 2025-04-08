namespace Leosac.KeyManager.Library.KeyStore.CNG
{
    public class CNGKeyStoreProperties : KeyStoreProperties, IEquatable<CNGKeyStoreProperties>
    {
        public CNGKeyStoreProperties()
        {
            _storageProvider = "";
            _scope = CNGScope.CurrentUser;
        }

        private string _storageProvider;
        public string StorageProvider
        {
            get => _storageProvider;
            set => SetProperty(ref _storageProvider, value);
        }

        private CNGScope _scope;
        public CNGScope Scope
        {
            get => _scope;
            set => SetProperty(ref _scope, value);
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as CNGKeyStoreProperties);
        }

        public bool Equals(CNGKeyStoreProperties? p)
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

            return (StorageProvider == p.StorageProvider);
        }

        public override int GetHashCode() => (StorageProvider).GetHashCode();

        public static bool operator ==(CNGKeyStoreProperties? lhs, CNGKeyStoreProperties? rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(CNGKeyStoreProperties? lhs, CNGKeyStoreProperties? rhs) => !(lhs == rhs);
    }
}
