using System.Diagnostics.CodeAnalysis;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryInfo : IChangeKeyEntry
    {

        [SetsRequiredMembers]
        public KeyEntryInfo(KeyEntryId identifier, KeyEntryClass kClass)
        {
            Identifier = identifier;
            KClass = kClass;
        }

        public required KeyEntryId Identifier { get; set; }

        public required KeyEntryClass KClass { get; init; }
    }
}