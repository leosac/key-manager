namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyLinkDialogViewModel : LinkDialogViewModel
    {
        public KeyLink? KeyLink
        {
            get => Link as KeyLink;
        }

        public override async Task RunLinkImpl(KeyStore.KeyStore ks)
        {
            if (KeyLink != null)
            {
                LinkResult = await ks.ResolveKeyLink(KeyLink.KeyIdentifier, Class, KeyLink.ContainerSelector, DivInputResult);
            }
        }
    }
}
