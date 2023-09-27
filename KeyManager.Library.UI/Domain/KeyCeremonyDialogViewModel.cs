using CommunityToolkit.Mvvm.ComponentModel;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Leosac.WpfApp.Domain;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyCeremonyDialogViewModel : ObservableValidator
    {
        public KeyCeremonyDialogViewModel()
        {
            Fragments = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Fragments { get; set; }
    }
}
