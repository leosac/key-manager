namespace Leosac.KeyManager.Library.KeyStore
{
    public interface IChangeKeyEntry
    {
        KeyEntryId Identifier { get; set; }

        KeyEntryClass KClass { get; }
    }
}
