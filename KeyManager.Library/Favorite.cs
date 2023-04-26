using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library
{
    public class Favorite : KMObject, IEquatable<Favorite>
    {
        public Favorite()
        {
            _name = string.Empty;
        }

        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private KeyStoreProperties? _properties;

        public KeyStoreProperties? Properties
        {
            get => _properties;
            set => SetProperty(ref _properties, value);
        }

        private DateTime _creationDate = DateTime.Now;

        public DateTime CreationDate
        {
            get => _creationDate;
            set => SetProperty(ref _creationDate, value);
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as Favorite);
        }

        public bool Equals(Favorite? p)
        {
            if (p is null)
                return false;

            if (Object.ReferenceEquals(this, p))
                return true;

            if (this.GetType() != p.GetType())
                return false;

            return (Name == p.Name) && (Properties!.Equals(p.Properties));
        }

        public override int GetHashCode() => (Name, Properties).GetHashCode();

        public static bool operator ==(Favorite? lhs, Favorite? rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                    return true;

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(Favorite? lhs, Favorite? rhs) => !(lhs == rhs);
    }
}
