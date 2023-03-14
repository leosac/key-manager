using Leosac.KeyManager.Library.Plugin.Domain;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyCeremonyDialogViewModel : ViewModelBase
    {
        public KeyCeremonyDialogViewModel()
        {
            Fragments = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Fragments { get; set; }
    }
}
