using System.Diagnostics.CodeAnalysis;

namespace Leosac.KeyManager.Library.KeyStore
{
    public class KeyEntryInfo(KeyEntryId identifier, KeyEntryClass kClass) : IChangeKeyEntry
    {
        public KeyEntryId Identifier { get; set; } = identifier;
        public KeyEntryClass KClass { get; init; } = kClass;
    }
}