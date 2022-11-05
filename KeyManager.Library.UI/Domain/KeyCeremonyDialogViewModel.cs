using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
