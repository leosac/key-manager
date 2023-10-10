namespace Leosac.KeyManager.Library.KeyStore.LCP
{
    public class LCPKeyStoreProperties : KeyStoreProperties, IEquatable<LCPKeyStoreProperties>
    {
        public LCPKeyStoreProperties()
        {
            _serverAddress = "http://localhost:5000";
            _username = "demo@leosac.com";
        }

        private string _serverAddress;
        public string ServerAddress
        {
            get => _serverAddress;
            set => SetProperty(ref _serverAddress, value);
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as LCPKeyStoreProperties);
        }

        public bool Equals(LCPKeyStoreProperties? p)
        {
            if (p is null)
                return false;

            if (Object.ReferenceEquals(this, p))
                return true;

            if (this.GetType() != p.GetType())
                return false;

            return (ServerAddress == p.ServerAddress && Username == p.Username);
        }

        public override int GetHashCode() => (ServerAddress, Username).GetHashCode();

        public static bool operator ==(LCPKeyStoreProperties? lhs, LCPKeyStoreProperties? rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                    return true;

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(LCPKeyStoreProperties? lhs, LCPKeyStoreProperties? rhs) => !(lhs == rhs);
    }
}
