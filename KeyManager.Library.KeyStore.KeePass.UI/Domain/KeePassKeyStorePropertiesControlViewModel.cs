using CommunityToolkit.Mvvm.Input;
using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using Leosac.KeyManager.Library.Plugin.UI.Domain;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace Leosac.KeyManager.Library.KeyStore.KeePass.UI.Domain
{
    public class KeePassKeyStorePropertiesControlViewModel : KeyStorePropertiesControlViewModel
    {
        public KeePassKeyStorePropertiesControlViewModel()
        {
            _properties = new KeePassKeyStoreProperties();
            Profiles.Add(DefaultProfile);
            SelectedProfile = Profiles.FirstOrDefault();
            ShowStatus(UI.Properties.Resources.Availability, StatusType.Success);
            ShowProgress = false;
        }


        private PwDatabase? _DB;
        private CancellationTokenSource? _cts;

        private bool _isConnecting;
        public bool IsConnecting
        {
            get => _isConnecting;
            set => SetProperty(ref _isConnecting, value);
        }

        public ObservableCollection<ProfileItem> Profiles { get; } = new();
        private readonly List<ProfileItem> _allProfiles = new();

        private ProfileItem? _selectedProfile;
        public ProfileItem? SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                SetProperty(ref _selectedProfile, value);
                OnPropertyChanged(nameof(SelectedProfileBackground));
                if (value != null && FileProperties != null)
                    FileProperties.ProfilePath = value.Name;
            }
        }

        public Brush SelectedProfileBackground =>
            _selectedProfile == null
                ? Brushes.Transparent
                : (_selectedProfile.IsNonExhaustive ? Brushes.Red : Brushes.Green);

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _isExhaustive;
        public bool IsExhaustive
        {
            get => _isExhaustive;
            set => SetProperty(ref _isExhaustive, value);
        }

        private bool _showProgress;
        public bool ShowProgress
        {
            get => _showProgress;
            set => SetProperty(ref _showProgress, value);
        }

        private string _status = UI.Properties.Resources.Availability;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public KeePassKeyStoreProperties? FileProperties
        {
            get { return Properties as KeePassKeyStoreProperties; }
        }

        public RelayCommand BrowseFileCommand => new(BrowseFile);

        public RelayCommand BrowseKeyFileCommand => new(BrowseKeyFile);

        public AsyncRelayCommand TestConnectionCommand => new(TestConnection);

        public RelayCommand ClearAllCommand => new(ClearAll);

        public AsyncRelayCommand CreateKeyStoreCommand => new(CreateKeyStore);

        public static readonly string DefaultProfileName = UI.Properties.Resources.DefaultProfileName;
        private static readonly ProfileItem DefaultProfile = new() { Name = DefaultProfileName, IsNonExhaustive = false };

        private void BrowseFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = UI.Properties.Resources.SelectKeePassDatabase,
                Filter = UI.Properties.Resources.KeePassDbFilter,
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true) return;
            ShowStatus(string.Format(UI.Properties.Resources.FileSelected, Path.GetFileName(dialog.FileName)), StatusType.Success);
            if (FileProperties != null)
            {
                if (!string.IsNullOrEmpty(FileProperties.DBPath) || !string.IsNullOrEmpty(FileProperties.Secret)
                    || !string.IsNullOrEmpty(FileProperties.KeyPath))
                    ClearAll();
                FileProperties.DBPath = dialog.FileName;
            }
        }

        private void BrowseKeyFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = UI.Properties.Resources.SelectMasterKey,
                Filter = UI.Properties.Resources.KeyFileFilter,
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true) return;
            if (FileProperties != null)
                FileProperties.KeyPath = dialog.FileName;
            ShowStatus(string.Format(UI.Properties.Resources.KeyFileSelected, Path.GetFileName(dialog.FileName)), StatusType.Success);
            if (FileProperties != null)
                FileProperties.KeyPath = dialog.FileName;
        }

        private void ClearAll()
        {
            CancelConnection();
            if (FileProperties != null)
            {
                FileProperties.DBPath = string.Empty;
                FileProperties.KeyPath = string.Empty;
                FileProperties.Secret = string.Empty;
                FileProperties.ProfilePath = string.Empty;
                FileProperties.SelectedCredentialMode = KeePass.CredentialMode.PasswordOnly;
            }

            Profiles.Clear();
            Profiles.Add(DefaultProfile);
            SelectedProfile = Profiles.First();

            ShowProgress = false;
            IsExhaustive = false;
            ShowStatus(UI.Properties.Resources.Availability, StatusType.Success);
        }

        private async Task TestConnection()
        {
            const int progressBarDuration = 1500; //Just for a minimum animation
            var stopwatch = Stopwatch.StartNew();
            IsConnecting = true;
            ShowProgress = true;
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
                ShowProgress = false;
            }
        }

        private async Task ExploreProfiles()
        {
            if (FileProperties != null)
            {
                if (string.IsNullOrWhiteSpace(FileProperties.DBPath))
                {
                    ShowStatus(UI.Properties.Resources.DatabasePathMissing, StatusType.Warning);
                    return;
                }
                bool requirePassword = FileProperties.SelectedCredentialMode == KeePass.CredentialMode.PasswordOnly || FileProperties.SelectedCredentialMode == KeePass.CredentialMode.PasswordAndKey;
                bool requireKey = FileProperties.SelectedCredentialMode == KeePass.CredentialMode.KeyFileOnly || FileProperties.SelectedCredentialMode == KeePass.CredentialMode.PasswordAndKey;
                if (requirePassword && string.IsNullOrEmpty(FileProperties.Secret))
                {
                    ShowStatus(UI.Properties.Resources.PasswordRequired, StatusType.Warning);
                    return;
                }
                if (requireKey && string.IsNullOrEmpty(FileProperties.KeyPath))
                {
                    ShowStatus(UI.Properties.Resources.MasterKeyRequired, StatusType.Warning);
                    return;
                }

                CancelConnection();
                _cts = new CancellationTokenSource();
                var token = _cts.Token;

                try
                {
                    IsBusy = true;
                    var masterKey = new CompositeKey();
                    if (requirePassword) masterKey.AddUserKey(new KcpPassword(FileProperties.Secret));
                    if (requireKey) masterKey.AddUserKey(new KcpKeyFile(FileProperties.KeyPath));

                    var db = new PwDatabase();
                    var io = new IOConnectionInfo { Path = FileProperties.DBPath };

                    await Task.Run(() =>
                    {
                        token.ThrowIfCancellationRequested();
                        db.Open(io, masterKey, null);
                    }, token);
                    _DB = db;
                    ReloadProfiles();
                    ShowStatus(string.Format(UI.Properties.Resources.DatabaseValidated, Path.GetFileName(FileProperties.DBPath)), StatusType.Success);
                }
                catch (OperationCanceledException)
                {
                    ShowStatus(UI.Properties.Resources.OperationCancelled, StatusType.Warning);
                }
                catch (Exception ex)
                {
                    ShowStatus(string.Format(UI.Properties.Resources.ConnectionFailed, ex.Message), StatusType.Error);
                }
                finally
                {
                    IsBusy = false;
                }
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
                    ShowStatus(string.Format(UI.Properties.Resources.DatabaseCloseFailed, ex.Message), StatusType.Warning);
                }
                finally
                {
                    _DB = null;
                }
            }
        }

        private void ReloadProfiles()
        {
            if (_DB == null)
                return;
            _allProfiles.Clear();
            Explore(_DB.RootGroup, isRoot: true);
            ApplyFilter();
        }

        private void Explore(PwGroup group, bool isRoot = false)
        {
            int count = group.Entries?.Count() ?? 0;
            bool valid = group.Entries?.Any(e => e.Strings.Get("KeyData_Leosac") == null) ?? false;
            _allProfiles.Add(new ProfileItem
            {
                Name = group.Name ?? "Undefined",
                EntryCount = count,
                IsNonExhaustive = valid,
                IsRoot = isRoot
            });
            foreach (var e in group.Groups)
                Explore(e);
        }

        private void ApplyFilter()
        {
            Profiles.Clear();
            var filtered = IsExhaustive
                ? _allProfiles.Where(p => !p.IsNonExhaustive && p.EntryCount > 0).ToList()
                : _allProfiles.ToList();
            if (filtered.Count == 0)
                filtered.Add(DefaultProfile);
            foreach (var profile in filtered)
                Profiles.Add(profile);
            SelectedProfile = Profiles.FirstOrDefault();
            if (FileProperties != null)
                FileProperties.ProfilePath = SelectedProfile!.Name;
        }

        private void ShowStatus(string message, StatusType type)
        {
            Status = message;
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
            /*TxtStatus.Foreground = new SolidColorBrush(startTextColor);
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
            }*/
        }

        private async Task CreateKeyStore()
        {
            if (FileProperties != null)
            {
                try
                {
                    ShowStatus(UI.Properties.Resources.Creating, StatusType.Info);

                    string filePath = FileProperties.DBPath?.Trim() ?? "";
                    string password = FileProperties.Secret ?? "";
                    string masterKeyPath = FileProperties.KeyPath?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        ShowStatus(UI.Properties.Resources.CreateFileNotFound, StatusType.Error);
                        return;
                    }
                    if (File.Exists(filePath))
                    {
                        ShowStatus(UI.Properties.Resources.CreateFileExists, StatusType.Error);
                        return;
                    }
                    if (string.IsNullOrEmpty(password) && string.IsNullOrEmpty(masterKeyPath))
                    {
                        ShowStatus(UI.Properties.Resources.CreateCredentialRequired, StatusType.Error);
                        return;
                    }
                    await CreateEmptyKeePassDatabase(filePath, password, masterKeyPath);
                    ShowStatus(UI.Properties.Resources.CreateSuccessful, StatusType.Success);
                }
                catch (Exception ex)
                {
                    ShowStatus(string.Format(UI.Properties.Resources.Error, ex.Message), StatusType.Error);
                }
            }
        }

        private async Task CreateEmptyKeePassDatabase(string filePath, string password, string masterKeyPath)
        {
            if (File.Exists(filePath))
                throw new InvalidOperationException("File already exists.");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Invalid file path."));
            var compositeKey = BuildCompositeKey(password, masterKeyPath);
            var io = new IOConnectionInfo { Path = filePath };
            var db = new PwDatabase();
            try
            {
                db.New(io, compositeKey);
                db.RootGroup.Groups.Clear();
                db.RootGroup.AddGroup(new PwGroup(true, true, "LEOSAC", PwIcon.Folder), true);
                await Task.Run(() => db.Save(new NullStatusLogger()));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create KeePass database.", ex);
            }
            finally
            {
                db.Close();
            }
        }

        private static CompositeKey BuildCompositeKey(string? password, string? masterKeyPath)
        {
            var compositeKey = new CompositeKey();
            if (!string.IsNullOrEmpty(password))
            {
                var pwBytes = Encoding.UTF8.GetBytes(password);
                compositeKey.AddUserKey(new KcpPassword(pwBytes));
                WipeMemory(pwBytes);
            }
            if (!string.IsNullOrEmpty(masterKeyPath))
            {
                if (!File.Exists(masterKeyPath))
                    throw new FileNotFoundException("Master key file not found.", masterKeyPath);
                compositeKey.AddUserKey(new KcpKeyFile(masterKeyPath));
            }
            return compositeKey;
        }

        private static void WipeMemory(byte[] sensitiveBytes)
        {
            if (sensitiveBytes == null) return;
            for (int i = 0; i < sensitiveBytes.Length; i++)
                sensitiveBytes[i] = 0;
        }
    }
}