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
                LinkResult = await ks.ResolveKeyEntryLink(KeyEntryLink.KeyIdentifier, Class, DivInputResult, KeyEntryLink.WrappingKey, null);
            }
        }
    }
}
