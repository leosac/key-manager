using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public abstract class KeyEntry : KMObject, IChangeKeyEntry
    {
        public KeyEntry()
        {
            _identifier = Guid.NewGuid().ToString();
        }

        private string _identifier;
        private string? _label;
        private KeyEntryProperties? _properties;
        private KeyEntryVariant? _variant;
        private KeyEntryLink? _link;

        public string Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
        }

        public string? Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public KeyEntryProperties? Properties
        {
            get => _properties;
            set => SetProperty(ref _properties, value);
        }

        public KeyEntryVariant? Variant
        {
            get => _variant;
            set => SetProperty(ref _variant, value);
        }

        public KeyEntryLink? Link
        {
            get => _link;
            set => SetProperty(ref _link, value);
        }

        public abstract IList<KeyEntryVariant> GetAllVariants();

        public void SetVariant(string name)
        {
            var variants = GetAllVariants();
            Variant = variants.Where(v => v.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }
    }
}
