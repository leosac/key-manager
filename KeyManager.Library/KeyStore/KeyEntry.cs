﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace Leosac.KeyManager.Library.KeyStore
{
    public abstract class KeyEntry : ObservableValidator, IKeyEntry
    {
        public KeyEntry()
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

        public abstract IList<KeyEntryVariant> GetAllVariants(KeyEntryClass? classFilter = null);

        public void SetVariant(string name)
        {
            var variants = GetAllVariants();
            Variant = variants.Where(v => v.Name.ToLower() == name.ToLower()).FirstOrDefault();
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
    }
}
