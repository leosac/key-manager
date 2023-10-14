using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.KeyStore;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class SelectableKeyEntryId : ObservableObject
    {
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        private KeyEntryId? _keyEntryId;
        public KeyEntryId? KeyEntryId
        {
            get => _keyEntryId;
            set => SetProperty(ref _keyEntryId, value);
        }
    }
}
