namespace Leosac.KeyManager.Library.DivInput
{
    public class DivInputContext
    {
        public string? CurrentDivInput { get; set; }

        public KeyStore.KeyStore? KeyStore { get; set; }

        public IDictionary<string, string>? AdditionalKeyStoreAttributes { get; set; }

        public KeyStore.KeyEntry? KeyEntry { get; set; }

        public KeyContainer? KeyContainer { get; set; }
    }
}
