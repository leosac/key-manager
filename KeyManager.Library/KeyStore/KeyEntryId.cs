using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryId : ObservableObject, IEquatable<KeyEntryId>, ICloneable
    {
        public KeyEntryId()
        {
            _id = Guid.NewGuid().ToString();
        }

        public KeyEntryId(string? id)
        {
            _id = id;
        }

        private string? _id;
        private string? _label;
        private object? _handle;

        public string? Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string? Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        [JsonIgnore]
        public object? Handle
        {
            get => _handle;
            set => SetProperty(ref _handle, value);
        }

        public bool IsConfigured()
        {
            return (!string.IsNullOrEmpty(Id) || !string.IsNullOrEmpty(Label));
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as KeyEntryId);
        }

        public bool Equals(KeyEntryId? p)
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

            return (Id == p.Id && Label == p.Label);
        }

        public override int GetHashCode() => (Id, Label).GetHashCode();

        public static bool operator ==(KeyEntryId? lhs, KeyEntryId? rhs)
        {
            if (lhs is null)
            {
                return rhs is null;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(KeyEntryId? lhs, KeyEntryId? rhs) => !(lhs == rhs);

        public override string ToString()
        {
            return string.Format("Key Entry Id (Identifier: `{0}`, Label: `{1}`)", Id, Label);
        }

        public object Clone()
        {
            return Clone(null);
        }

        public KeyEntryId Clone(IDictionary<string, string>? attributes)
        {
            var clone = new KeyEntryId();
            clone.Id = Id;
            clone.Label = Label;
            if (attributes != null)
            {
                foreach (var key in attributes.Keys)
                {
                    if (!string.IsNullOrEmpty(clone.Id))
                    {
                        clone.Id = clone.Id.Replace("%{" + key + "}", attributes[key]);
                    }
                    if (!string.IsNullOrEmpty(clone.Label))
                    {
                        clone.Label = clone.Label.Replace("%{" + key + "}", attributes[key]);
                    }
                }
            }
            clone.Handle = Handle;
            return clone;
        }
    }
}
