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
            _wrappingKey = new Key();
        }

        private Key _wrappingKey;

        public Key WrappingKey
        {
            get => _wrappingKey;
            set => SetProperty(ref _wrappingKey, value);
        }

        public KeyEntryLink? KeyEntryLink
        {
            get => Link as KeyEntryLink;
        }

        public override void RunLinkImpl(KeyStore.KeyStore ks)
        {
            if (KeyEntryLink != null)
            {
                LinkResult = ks.ResolveKeyEntryLink(KeyEntryLink.KeyIdentifier, GetDivInput(), KeyEntryLink.WrappingKeyId, KeyEntryLink.WrappingKeyVersion);
            }
        }
    }
}
