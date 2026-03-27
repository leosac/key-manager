using Leosac.CredentialProvisioning.Core.Models;

namespace Leosac.KeyManager.Library.KeyStore.LCP
{
    public class LCPKeyEntryProperties : KeyEntryProperties
    {
        private CredentialKeyScope _scope;
        public CredentialKeyScope Scope
        {
            get => _scope;
            set => SetProperty(ref _scope, value);
        }

        private string? _scopeDiversifier;
        public string? ScopeDiversifier
        {
            get => _scopeDiversifier;
            set => SetProperty(ref _scopeDiversifier, value);
        }
    }
}
