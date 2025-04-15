using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace Leosac.KeyManager.Library.KeyStore
{
    public abstract class KeyEntry : ObservableObject, IKeyEntry
    {
        protected KeyEntry()
        {
            _identifier = new KeyEntryId();
        }

        private KeyEntryId _identifier;
        private KeyEntryProperties? _properties;
        private KeyEntryVariant? _variant;
        private KeyEntryLink? _link;

        public KeyEntryId Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
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

        public abstract KeyEntryClass KClass { get; }

        public abstract IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter);

        public IList<KeyEntryVariant> GetAllVariants()
        {
            return GetAllVariants(null);
        }

        public void SetVariant(string name)
        {
            var variants = GetAllVariants();
            Variant = variants.Where(v => v.Name.ToLowerInvariant() == name.ToLowerInvariant()).FirstOrDefault();
        }

        protected KeyEntryClass GetKeyEntryClassFromFirstKeyVariant()
        {
            var kclass = KeyEntryClass.Symmetric;
            if (Variant != null && Variant.KeyContainers.Count > 0)
            {
                var tags = Variant.KeyContainers[0].Key.Tags;
                foreach (var k in Enum.GetValues<KeyEntryClass>())
                {
                    if (tags.Contains(k.ToString()))
                    {
                        kclass = k;
                        break;
                    }
                }
            }

            return kclass;
        }

        protected KeyEntryVariant CreateVariantFromAlgo<T>(string algo, uint keySize) where T : KeyContainer, new()
        {
            var variant = new KeyEntryVariant { Name = algo };
            var tags = new[]
            {
                    algo,
                    KClass.ToString()
            };
            variant.KeyContainers.Add(new T { Name = "Key", Key = new Key(tags, keySize) });
            return variant;
        }

        public KeyEntry? DeepCopy()
        {
            var serialized = JsonConvert.SerializeObject(this, CreateJsonSerializerSettings());
            return serialized != null ? JsonConvert.DeserializeObject(serialized, this.GetType(), CreateJsonSerializerSettings()) as KeyEntry : null;
        }

        public static JsonSerializerSettings CreateJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                Formatting = Formatting.Indented
            };
        }
    }
}
