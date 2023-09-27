using Leosac.KeyManager.Library.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyEntryLinkDialogViewModel : LinkDialogViewModel
    {
        public KeyEntryLinkDialogViewModel() : base()
        {
            
        }

        public KeyEntryLink? KeyEntryLink
        {
            get => Link as KeyEntryLink;
        }

        public override async Task RunLinkImpl(KeyStore.KeyStore ks)
        {
            if (KeyEntryLink != null)
            {
                LinkResult = await ks.ResolveKeyEntryLink(KeyEntryLink.KeyIdentifier, Class, DivInputResult, KeyEntryLink.WrappingKeyId, KeyEntryLink.WrappingKeySelector);
            }
        }
    }
}
