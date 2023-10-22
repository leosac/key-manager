using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyCeremonyDialogViewModel : ObservableValidator
    {
        public KeyCeremonyDialogViewModel()
        {
            Fragments = new ObservableCollection<string>();
        }

        private bool _isReunification;
        public bool IsReunification
        {
            get => _isReunification;
            set => SetProperty(ref _isReunification, value);
        }

        public ObservableCollection<string> Fragments { get; set; }
    }
}
