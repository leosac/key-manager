using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryCryptogram : KMObject, IChangeKeyEntry
    {
        public KeyEntryCryptogram()
        {
            _identifier = new KeyEntryId();
        }

        private KeyEntryId _identifier;

        public KeyEntryId Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
        }

        public KeyEntryClass KClass => KeyEntryClass.Symmetric;

        private string? _value;

        public string? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private KeyEntryId? _wrappingKeyId;

        public KeyEntryId? WrappingKeyId
        {
            get => _wrappingKeyId;
            set => SetProperty(ref _wrappingKeyId, value);
        }

        private string? _wrappingContainerSelector;

        public string? WrappingContainerSelector
        {
            get => _wrappingContainerSelector;
            set => SetProperty(ref _wrappingContainerSelector, value);
        }
    }
}
