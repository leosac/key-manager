using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.KeyStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public string? KeyStoreTypeName { get; set; }

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

        [JsonIgnore]
        public KeyStoreProperties? Properties
        {
            get => _properties;
            set => SetProperty(ref _properties, value);
        }

        [JsonIgnore]
        public bool IsResolved => Properties != null;

        [JsonIgnore]
        public JObject? UnresolvedModule { get; set; }

        [JsonProperty(nameof(Properties))]
        public object? SerializedProperties
        {
            get
            {
                if (UnresolvedModule != null) //If plugin missing
                    return UnresolvedModule;
                return Properties;
            }
            set
            {
                if (value is JObject jObject)
                    UnresolvedModule = jObject;
                else if (value is KeyStoreProperties props)
                    Properties = props;
            }
        }

        public IDictionary<KeyEntryClass, KeyEntry?> DefaultKeyEntries { get; set; }

        private DateTime _creationDate = DateTime.Now;
        public DateTime CreationDate
        {
            get => _creationDate;
            set => SetProperty(ref _creationDate, value);
        }

        private string[] _tags = [];
        public string[] Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        public override bool Equals(object? obj) => Equals(obj as Favorite);

        public bool Equals(Favorite? p)
        {
            if (p is null)
            {
                return false;
            }

            if (ReferenceEquals(this, p))
            {
                return true;
            }

            if (GetType() != p.GetType())
            {
                return false;
            }

            return (Identifier == p.Identifier) && (Name == p.Name) && (Equals(Properties, p.Properties));
        }

        public override int GetHashCode() =>
            HashCode.Combine(Identifier, Name, Properties);

        public static bool operator ==(Favorite? lhs, Favorite? rhs) =>
            lhs is null ? rhs is null : lhs.Equals(rhs);

        public static bool operator !=(Favorite? lhs, Favorite? rhs) => !(lhs == rhs);
    }
}