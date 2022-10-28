using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class FolderBrowserDialogViewModel : ViewModelBase
    {
        public FolderBrowserDialogViewModel()
        {
            Drives = new ObservableCollection<DriveInfo>(DriveInfo.GetDrives());
        }

        private string? _selectedFolder;

        public ObservableCollection<DriveInfo> Drives { get; }

        public string? SelectedFolder
        {
            get => _selectedFolder;
            set => SetProperty(ref _selectedFolder, value);
        }
    }
}
