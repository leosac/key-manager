using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyLinkDialogViewModel : LinkDialogViewModel
    {
        public KeyLinkDialogViewModel() : base()
        {

        }

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
