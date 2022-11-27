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

        private string? _value;

        public string? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
