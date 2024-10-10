using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library
{
    public sealed class Favorite : ObservableObject, IEquatable<Favorite>
    {
        public Favorite()
        {
            _identifier = Guid.NewGuid().ToString();
            _name = string.Empty;
            DefaultKeyEntries = new Dictionary<KeyEntryClass, KeyEntry?>();
        }

        private string _identifier;
        public string Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
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

        public IDictionary<KeyEntryClass, KeyEntry?> DefaultKeyEntries { get; set; }

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

            return (Identifier == p.Identifier) && (Name == p.Name) && (Properties!.Equals(p.Properties));
        }

        public override int GetHashCode() => (Name, Properties).GetHashCode();

        public static bool operator ==(Favorite? lhs, Favorite? rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(Favorite? lhs, Favorite? rhs) => !(lhs == rhs);
    }
}
