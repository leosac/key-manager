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
            Directories = new ObservableCollection<DirectoryInfo>();

            GoToParentCommand = new KeyManagerCommand(
                parameter =>
                {
                    if (SelectedDirectory?.Parent != null)
                    {
                        SelectedDirectory = SelectedDirectory.Parent;
                    }
                }
            );

            CreateFolderCommand = new KeyManagerCommand(
                parameter =>
                {
                    if (SelectedDirectory != null && parameter is string newFolder)
                    {
                        var newFolderFullpath = Path.Combine(SelectedDirectory.FullName, newFolder);
                        SelectedDirectory = Directory.CreateDirectory(newFolderFullpath);
                    }
                }
            );
        }

        private bool noDirUpdate = false;
        private DirectoryInfo? _selectedDirectory;
        private string? _ioError;
        private bool _hasError = false;
        private string? _newFolderName;

        private DriveInfo? _selectedDrive;

        public ObservableCollection<DriveInfo> Drives { get; }

        public DriveInfo? SelectedDrive
        {
            get => _selectedDrive;
            set
            {
                SetProperty(ref _selectedDrive, value);
                SelectedDirectory = value?.RootDirectory;
            }
        }

        public ObservableCollection<DirectoryInfo> Directories { get; }

        public DirectoryInfo? SelectedDirectory
        {
            get => _selectedDirectory;
            set
            {
                if (!noDirUpdate)
                {
                    SetProperty(ref _selectedDirectory, value);
                    UpdateDirectories(value);
                }
            }
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public string? SelectedFolder
        {
            get => _selectedDirectory?.FullName;
        }

        public string? IOError
        {
            get => _ioError;
            set
            {
                SetProperty(ref _ioError, value);
                HasError = !string.IsNullOrEmpty(value);
            }
        }

        public string? NewFolderName
        {
            get => _newFolderName;
            set => SetProperty(ref _newFolderName, value);
        }

        public void UpdateDirectories()
        {
            UpdateDirectories(SelectedDirectory);
        }

        private void UpdateDirectories(DirectoryInfo? parent)
        {
            noDirUpdate = true;
            Directories.Clear();
            if (parent != null)
            {
                try
                {
                    foreach (var dir in parent.GetDirectories())
                    {
                        Directories.Add(dir);
                    }
                    IOError = null;
                }
                catch (UnauthorizedAccessException ex)
                {
                    IOError = ex.Message;
                }
                catch (DirectoryNotFoundException ex)
                {
                    IOError = ex.Message;
                }
            }
            noDirUpdate = false;
        }

        public KeyManagerCommand GoToParentCommand { get; }

        public KeyManagerCommand CreateFolderCommand { get; }
    }
}
