namespace Leosac.KeyManager.Library.KeyStore
{
    public interface IKeyEntry : IChangeKeyEntry
    {
        KeyEntryProperties? Properties { get; set; }

        KeyEntryVariant? Variant { get; set; }

        KeyEntryLink? Link { get; set; }
    }
}
