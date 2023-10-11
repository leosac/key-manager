using Leosac.CredentialProvisioning.Core.Models;

namespace Leosac.KeyManager.Library.KeyStore.LCP
{
    public class LCPKeyEntryProperties : KeyEntryProperties
    {
        public LCPKeyEntryProperties()
        {

        }

        public CredentialKeyScope Scope { get; set; }

        public string? ScopeDiversifier { get; set; }
    }
}
