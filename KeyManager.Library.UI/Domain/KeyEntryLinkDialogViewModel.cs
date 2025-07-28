using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntryLinkDialogViewModel : LinkDialogViewModel
    {
        public KeyEntryLink? KeyEntryLink
        {
            get => Link as KeyEntryLink;
        }

        public override async Task RunLinkImpl(KeyStore.KeyStore ks)
        {
            if (KeyEntryLink != null)
            {
                var divContext = new DivInput.DivInputContext
                {
                    AdditionalKeyStoreAttributes = KeyStoreAttributes
                };
                LinkResult = await ks.ResolveKeyEntryLink(KeyEntryLink.KeyIdentifier, Class, KeyStore.KeyStore.ComputeDivInput(divContext, KeyEntryLink.DivInput), KeyEntryLink.WrappingKey, null);
            }
        }
    }
}
