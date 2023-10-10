namespace Leosac.KeyManager.Library.KeyStore.LCP
{
    public class LCPKeyEntryProperties : KeyEntryProperties
    {
        public LCPKeyEntryProperties()
        {

        }

        public string? Scope { get; set; }

        public string? ScopeDiversifier { get; set; }
    }
}
