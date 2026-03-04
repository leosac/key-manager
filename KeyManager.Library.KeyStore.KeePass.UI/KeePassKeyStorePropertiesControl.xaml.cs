using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using Leosac.KeyManager.Library.KeyStore.KeePass.UI.Domain;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace KeyManager.Library.KeyStore.KeePass.UI
{
    public partial class KeePassKeyStorePropertiesControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private PwDatabase? _DB;
        private CancellationTokenSource? _cts;
        
        private bool _isConnecting = false;

        public string TestButtonText => IsConnecting ? Properties.Resources.AvailabilityProcessing : Properties.Resources.ProfileChecker;
        public bool IsTestEnabled => !IsConnecting && !string.IsNullOrWhiteSpace(TxtFilePath.Text) && File.Exists(TxtFilePath.Text);
        public bool IsClearEnabled => !IsConnecting;
        public bool IsConnecting
        {
            get => _isConnecting;
            set
            {
                if (_isConnecting == value) return;
                _isConnecting = value;
                OnPropertyChanged(nameof(IsConnecting));
                OnPropertyChanged(nameof(TestButtonText));
                OnPropertyChanged(nameof(IsTestEnabled));
                OnPropertyChanged(nameof(IsClearEnabled));
            }
        }

        public ObservableCollection<ProfileItem> Profiles { get; } = new();
        private readonly List<ProfileItem> _allProfiles = new();

        private ProfileItem? _selectedProfile;
        public ProfileItem? SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (_selectedProfile == value) return;
                _selectedProfile = value;
                OnPropertyChanged(nameof(SelectedProfile));
                OnPropertyChanged(nameof(SelectedProfileBackground));
            }
        }

        public Brush SelectedProfileBackground =>
            _selectedProfile == null
                ? Brushes.Transparent
                : (_selectedProfile.IsNonExhaustive ? Brushes.Red : Brushes.Green);

        private CredentialMode _selectedCredentialMode = UI.CredentialMode.PasswordOnly;

        public CredentialMode SelectedCredential
        {
            get => _selectedCredentialMode;
            set
            {
                if (_selectedCredentialMode == value) return;
                _selectedCredentialMode = value;
                OnPropertyChanged(nameof(SelectedCredential));
                OnPropertyChanged(nameof(IsMasterKeyVisible));
            }
        }

        public bool IsMasterKeyVisible =>
            SelectedCredential != UI.CredentialMode.PasswordOnly;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public KeePassKeyStorePropertiesControl()
        {
            InitializeComponent();
            DataContext = this;

            Profiles.Add(DefaultProfile);
            SelectedProfile = DefaultProfile;

            BtnExhaustive.Checked += (_, _) => ApplyFilter();
            BtnExhaustive.Unchecked += (_, _) => ApplyFilter();
            RbPassword.Checked += CredentialMode;
            RbKeyFile.Checked += CredentialMode;
            RbPasswordKey.Checked += CredentialMode;
            TxtFilePath.TextChanged += (_, _) => OnPropertyChanged(nameof(IsTestEnabled));

            Unloaded += (_, _) => ClearAll();
            CredentialMode(null, null);
            ShowStatus(Properties.Resources.Availability, StatusType.Success);
            ShowProgress(false);
        }

        public static readonly string DefaultProfileName = Properties.Resources.DefaultProfileName;
        private static readonly ProfileItem DefaultProfile = new() { Name = DefaultProfileName, IsNonExhaustive = false };

        private void CredentialMode(object? sender, RoutedEventArgs? e)
        {
            bool pwdEnabled = RbPassword.IsChecked == true || RbPasswordKey.IsChecked == true;
            bool keyEnabled = RbKeyFile.IsChecked == true || RbPasswordKey.IsChecked == true;
            if (TxtPassword.IsEnabled != pwdEnabled)
                TxtPassword.IsEnabled = pwdEnabled;
            MasterKeyPanel.Visibility = keyEnabled ? Visibility.Visible : Visibility.Collapsed;
            if (!keyEnabled && TxtMasterKey.Text.Length > 0)
                TxtMasterKey.Clear();
        }

        private void BrowseFile(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = Properties.Resources.SelectKeePassDatabase,
                Filter = Properties.Resources.KeePassDbFilter,
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true) return;
            ShowStatus(string.Format(Properties.Resources.FileSelected, Path.GetFileName(dialog.FileName)), StatusType.Success);
            if (DataContext is KeePassKeyStorePropertiesControlViewModel model && model.FileProperties != null)
            {
                if (!string.IsNullOrEmpty(model.FileProperties.DBPath) || !string.IsNullOrEmpty(model.FileProperties.Secret)
                    || !string.IsNullOrEmpty(model.FileProperties.KeyPath))
                        ClearAll();
                model.FileProperties.DBPath = dialog.FileName;
            }
            TxtFilePath.Text = dialog.FileName;
        }

        private void BrowseKeyFile(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = Properties.Resources.SelectMasterKey,
                Filter = Properties.Resources.KeyFileFilter,
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true) return;
            TxtMasterKey.Text = dialog.FileName;
            if (RbPassword.IsChecked == true)
                RbPasswordKey.IsChecked = !string.IsNullOrEmpty(TxtPassword.Password);
            CredentialMode(null, null);
            ShowStatus(string.Format(Properties.Resources.KeyFileSelected, Path.GetFileName(dialog.FileName)), StatusType.Success);
            if (DataContext is KeePassKeyStorePropertiesControlViewModel model && model.FileProperties != null)
                model.FileProperties.KeyPath = dialog.FileName;
        }

        private void ClearAll(object? sender = null, RoutedEventArgs? e = null)
        {
            CancelConnection();
            if (DataContext is KeePassKeyStorePropertiesControlViewModel model && model.FileProperties != null)
            {
                model.FileProperties.DBPath = string.Empty;
                model.FileProperties.KeyPath = string.Empty;
                model.FileProperties.Secret = string.Empty;
                model.FileProperties.ProfilePath = string.Empty;
            }
            TxtFilePath.Clear();
            TxtPassword.Clear();
            TxtMasterKey.Clear();

            Profiles.Clear();
            Profiles.Add(DefaultProfile);
            SelectedProfile = Profiles.First();

            ShowProgress(false);
            BtnExhaustive.IsChecked = false;
            SelectedCredential = UI.CredentialMode.PasswordOnly;
            CredentialMode(null, null);
            ShowStatus(Properties.Resources.Availability, StatusType.Success);
        }

        private async void TestConnection(object sender, RoutedEventArgs e)
        {
            const int progressBarDuration = 1500; //Just for a minimum animation
            var stopwatch = Stopwatch.StartNew();
            IsConnecting = true;
            ShowProgress(true);
            try
            {
                await ExploreProfiles();
            }
            finally
            {
                stopwatch.Stop();
                var remaining = progressBarDuration - (int)stopwatch.ElapsedMilliseconds;
                if (remaining > 0)
                    await Task.Delay(remaining);
                IsConnecting = false;
                Dispatcher.Invoke(() =>
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Opacity = 1;
                });
            }
        }

        private async Task ExploreProfiles()
        {
            string dbPath = TxtFilePath.Text;
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                ShowStatus(Properties.Resources.DatabasePathMissing, StatusType.Warning);
                return;
            }
            bool requirePassword = RbPassword.IsChecked == true || RbPasswordKey.IsChecked == true;
            bool requireKey = RbKeyFile.IsChecked == true || RbPasswordKey.IsChecked == true;
            if (requirePassword && string.IsNullOrEmpty(TxtPassword.Password))
            {
                ShowStatus(Properties.Resources.PasswordRequired, StatusType.Warning);
                return;
            }
            if (requireKey && string.IsNullOrEmpty(TxtMasterKey.Text))
            {
                ShowStatus(Properties.Resources.MasterKeyRequired, StatusType.Warning);
                return;
            }

            CancelConnection();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                IsBusy = true;
                var masterKey = new CompositeKey();
                if (requirePassword) masterKey.AddUserKey(new KcpPassword(TxtPassword.Password));
                if (requireKey) masterKey.AddUserKey(new KcpKeyFile(TxtMasterKey.Text));

                var db = new PwDatabase();
                var io = new IOConnectionInfo { Path = dbPath };

                await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    db.Open(io, masterKey, null);
                }, token);
                _DB = db;
                ReloadProfiles();
                ShowStatus(string.Format(Properties.Resources.DatabaseValidated, Path.GetFileName(TxtFilePath.Text)), StatusType.Success);
            }
            catch (OperationCanceledException)
            {
                ShowStatus(Properties.Resources.OperationCancelled, StatusType.Warning);
            }
            catch (Exception ex)
            {
                ShowStatus(string.Format(Properties.Resources.ConnectionFailed, ex.Message), StatusType.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private void CancelConnection()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            if (_DB != null)
            {
                try
                {
                    _DB.Close();
                }
                catch (Exception ex)
                {
                    ShowStatus(string.Format(Properties.Resources.DatabaseCloseFailed, ex.Message), StatusType.Warning);
                }
                _DB.Close();
                _DB = null;
            }
        }

        private void ReloadProfiles()
        {
            if (_DB == null)
                return;
            _allProfiles.Clear();
            Explore(_DB.RootGroup);
            ApplyFilter();
        }

        private void Explore(PwGroup group)
        {
            int count = group.Entries?.Count() ?? 0;
            bool valid = group.Entries?.Any(e => e.Strings.Get("KeyData_Leosac") == null) ?? false;
            _allProfiles.Add(new ProfileItem
            {
                Name = group.Name ?? "Undefined",
                EntryCount = count,
                IsNonExhaustive = valid
            });
            foreach (var e in group.Groups)
                Explore(e);
        }

        private void ApplyFilter()
        {
            var exhaustive = BtnExhaustive.IsChecked == true;
            Profiles.Clear();
            var filtered = exhaustive
                ? _allProfiles.Where(p => !p.IsNonExhaustive && p.EntryCount > 0).ToList()
                : _allProfiles.ToList();
            if (filtered.Count == 0)
                filtered.Add(DefaultProfile);
            foreach (var profile in filtered)
                Profiles.Add(profile);
            SelectedProfile = Profiles.FirstOrDefault();
            if (DataContext is KeePassKeyStorePropertiesControlViewModel model && model.FileProperties != null)
                model.FileProperties.ProfilePath = SelectedProfile!.Name;
        }

        private void SelectedProfileChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedProfile != null && DataContext is KeePassKeyStorePropertiesControlViewModel model &&
                model.FileProperties != null)
                model.FileProperties.ProfilePath = SelectedProfile.Name;
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

        private void ShowStatus(string message, StatusType type)
        {
            Dispatcher.Invoke(() =>
            {
                TxtStatus.Text = message;
                Color highlightColor;
                Color startTextColor;
                switch (type)
                {
                    case StatusType.Success:
                        highlightColor = Colors.Green;
                        startTextColor = Colors.Black;
                        break;
                    case StatusType.Warning:
                        highlightColor = Colors.Orange;
                        startTextColor = Colors.White;
                        break;
                    case StatusType.Error:
                        highlightColor = Colors.Red;
                        startTextColor = Colors.White;
                        break;
                    default:
                        highlightColor = Colors.Gray;
                        startTextColor = Colors.Black;
                        break;
                }
                TxtStatus.Foreground = new SolidColorBrush(startTextColor);
                var bgBrush = new SolidColorBrush(Color.FromArgb(180, highlightColor.R, highlightColor.G, highlightColor.B));
                StatusBorder.Background = bgBrush;
                StatusBorder.BorderBrush = Brushes.LightGray;
                StatusBorder.BorderThickness = new Thickness(1);
                StatusBorder.CornerRadius = new CornerRadius(4);
                StatusBorder.Padding = new Thickness(6);
                StatusBorder.Visibility = Visibility.Visible;
                var bgFade = new ColorAnimation(Color.FromArgb(0, highlightColor.R, highlightColor.G, highlightColor.B),
                                                TimeSpan.FromSeconds(3));
                bgBrush.BeginAnimation(SolidColorBrush.ColorProperty, bgFade);
                if (TxtStatus.Foreground is SolidColorBrush textBrush)
                {
                    var textFade = new ColorAnimation(Color.FromArgb(255, highlightColor.R, highlightColor.G, highlightColor.B),
                                                      TimeSpan.FromSeconds(3));
                    textBrush.BeginAnimation(SolidColorBrush.ColorProperty, textFade);
                }
            });
        }

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }

    public sealed class ProfileItem
    {
        public string Name { get; init; } = "";
        public int EntryCount { get; set; }
        public bool IsNonExhaustive { get; init; }
        public string DisplayName => Name == KeePassKeyStorePropertiesControl.DefaultProfileName ? Name : $"{Name} ({EntryCount})";
    }

}