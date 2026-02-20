using KeePassLib.Resources;
using Leosac.KeyManager.Library.KeyStore.KeePass.UI.Domain;
using Leosac.KeyManager.Library.UI;
using Leosac.KeyManager.Library.UI.Domain;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace KeyManager.Library.KeyStore.KeePass.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class KeePassKeyStorePropertiesControl : UserControl, INotifyPropertyChanged
    {
        private string? _defaultPath = "";
        private string? _fileInfo = "";
        private string _statusMessage = "";
        private string? _keyFilePath = "";
        private bool _isConnecting = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string TestButtonText => IsConnecting ? "🔄 Testing..." : "🔗 Test Connection";
        public bool IsTestEnabled => !IsConnecting && !string.IsNullOrWhiteSpace(DefaultPath) && File.Exists(DefaultPath);
        public bool IsClearEnabled => !IsConnecting;

        public KeePassKeyStorePropertiesControl()
        {
            InitializeComponent();
            DataContext = this;
            TxtStatus.Foreground = Brushes.Gray;
            UpdateBindings();
            ShowProgress(false);
        }

        public string DefaultPath
        {
            get => _defaultPath ?? "";
            set
            {
                _defaultPath = value;
                OnPropertyChanged(nameof(DefaultPath));
                UpdateStatus();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value ?? "Ready to connect • No file selected";
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public string StatusFileInfo
        {
            get => _fileInfo ?? "";
            set
            {
                _fileInfo = value;
                OnPropertyChanged(nameof(StatusFileInfo));
            }
        }

        public string KeyFilePath
        {
            get => _keyFilePath ?? "";
            set
            {
                _keyFilePath = value;
                OnPropertyChanged(nameof(KeyFilePath));
                UpdateStatus();
            }
        }

        public bool IsConnecting
        {
            get => _isConnecting;
            set
            {
                _isConnecting = value;
                OnPropertyChanged(nameof(IsConnecting));
                OnPropertyChanged(nameof(TestButtonText));
                OnPropertyChanged(nameof(IsTestEnabled));
                OnPropertyChanged(nameof(IsClearEnabled));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateStatus()
        {
            if (!string.IsNullOrEmpty(DefaultPath) && File.Exists(DefaultPath))
            {
                StatusMessage = "Ready to test connection • Valid file selected";
            }
            else if (!string.IsNullOrEmpty(DefaultPath))
            {
                StatusMessage = "⚠️ File path specified but no file found";
            }
            else
            {
                StatusMessage = "Ready to connect • No file selected";
            }
            if (!string.IsNullOrEmpty(KeyFilePath))
            {
                StatusMessage += $" • Key file : {(KeyFilePath.Length > 20 ? Path.GetFileName(KeyFilePath) : KeyFilePath)}";
            }
        }
        private void UpdateBindings()
        {
            OnPropertyChanged(nameof(TestButtonText));
            OnPropertyChanged(nameof(IsTestEnabled));
            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(StatusFileInfo));
            OnPropertyChanged(nameof(IsClearEnabled));
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {

            var dialog = new OpenFileDialog
            {
                Title = "KeePass Database (.kdbx)",
                Filter = "KeePass Database (*.kdbx)|*.kdbx|All Files (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = true
            };
            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                TxtFilePath.Text = filePath;
                _defaultPath = filePath;
                OnPropertyChanged(nameof(DefaultPath));
                UpdateStatus();
                UpdateBindings();
                TxtFilePath.Focus();
                TxtFilePath.CaretIndex = filePath.Length;
                var model = DataContext as KeePassKeyStorePropertiesControlViewModel;
                if (model?.FileProperties != null)
                {
                    model.FileProperties.DBpath = filePath;
                }
            }
        }

        private void BtnKeyFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Key File",
                Filter = "Key Files (*.keyx)|*.keyx|All Files (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = true
            };
            if (dialog.ShowDialog() == true)
            {
                KeyFilePath = dialog.FileName;
                UpdateStatus();
            }
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            IsConnecting = true;
            ShowProgress(true);
            try
            {
                await TestConnectionAsync();
            }
            catch (Exception ex)
            {
                ShowStatusError($"Connection failed : {ex.Message}");
            }
            finally
            {
                IsConnecting = false;
                ShowProgress(false);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearAllFields();
        }

        private void ClearAllFields()
        {
            DefaultPath = "";
            Password.Password = "";
            KeyFilePath = "";
            StatusFileInfo = "";
            StatusMessage = "Ready to connect • No file selected";
            TxtStatus.Text = StatusMessage;
            TxtStatus.Foreground = Brushes.Gray;
            ShowProgress(false);
            Keyboard.ClearFocus();
            TxtFilePath.Focus();
            UpdateBindings();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(DefaultPath))
            {
                ShowStatusError("Select a KeePass database file.");
                TxtFilePath.Focus();
                return false;
            }

            if (!File.Exists(DefaultPath))
            {
                ShowStatusError("Selected KeePass file does not exist.");
                TxtFilePath.Focus();
                return false;
            }
            if (Password.Password?.Length < 4 && string.IsNullOrEmpty(KeyFilePath))
            {
                ShowStatusWarning("Master password or key file is missing or incorrect.");
                Password.Focus();
                return false;
            }
            return true;
        }

        private void ShowProgress(bool show)
        {
            if (show)
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;
                ProgressBar.Opacity = 0;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2));
                ProgressBar.BeginAnimation(OpacityProperty, fadeIn);
            }
            else
            {
                ProgressBar.IsIndeterminate = false;
                ProgressBar.BeginAnimation(OpacityProperty, null);
                ProgressBar.Opacity = 1;
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.2));
                fadeOut.Completed += (s, e) =>
                {
                    ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);
                };
                ProgressBar.BeginAnimation(OpacityProperty, fadeOut);
            }
        }

        private async Task TestConnectionAsync()
        {
            try
            {
                if (!File.Exists(DefaultPath))
                    throw new FileNotFoundException("KeePass database .kdbx file not found.");
                var fileInfo = new FileInfo(DefaultPath);
                if (fileInfo.Length < 1024)
                    throw new InvalidDataException("File too small - Can't be a KeePass database.");
                StatusFileInfo = $"File: {fileInfo.Name}\nSize: {(fileInfo.Length / 1024f):F1} KB\nModified: {fileInfo.LastWriteTime:MMM dd, yyyy}";
                ShowStatusSuccess("✅ KeePass database validated successfully!");
            }
            catch (UnauthorizedAccessException)
            {
                throw new InvalidOperationException("Access denied to KeePass file. Check file permissions.");
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Cannot read file: {ex.Message}");
            }
        }
        private void ShowStatusSuccess(string message)
        {
            TxtStatus.Text = message;
            TxtStatus.Foreground = Brushes.Green;
            StatusMessage = message;
            OnPropertyChanged(nameof(StatusMessage));
        }

        private void ShowStatusError(string message)
        {
            TxtStatus.Text = $"❌ {message}";
            TxtStatus.Foreground = Brushes.Red;
            StatusMessage = $"❌ {message}";
            OnPropertyChanged(nameof(StatusMessage));
        }

        private void ShowStatusWarning(string message)
        {
            TxtStatus.Text = $"⚠️ {message}";
            TxtStatus.Foreground = Brushes.Orange;
            StatusMessage = $"⚠️ {message}";
            OnPropertyChanged(nameof(StatusMessage));
        }
    }
}