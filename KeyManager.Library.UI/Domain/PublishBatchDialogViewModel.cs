using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.Plugin;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class PublishBatchDialogViewModel : ObservableObject, IDisposable
    {
        private readonly TimeSpan _uiRefreshInterval = TimeSpan.FromMilliseconds(150);
        private DateTime _lastUiRefresh = DateTime.MinValue;
        private bool _uiRefreshPending = false;

        private readonly SynchronizationContext _uiContext;
        private readonly Random _rng = new(); //TODO keep in code, for test
        private readonly object _ctsLock = new();
        private CancellationTokenSource? _cts;
        private ManualResetEventSlim? _pauseEvent;
        private bool _disposed;

        public PublishBatchDialogViewModel(Favorite favorite, PublishBatchOptions batchOptions, KeyStore.KeyStore sourceKeyStore)
        {
            Favorite = favorite ?? throw new ArgumentNullException(nameof(favorite));
            BatchOptions = batchOptions ?? throw new ArgumentNullException(nameof(batchOptions));
            SourceKeyStore = sourceKeyStore ?? throw new ArgumentNullException(nameof(sourceKeyStore));
            Label = $"Publish as batch to: {Favorite.Name}";
            Logs = new ObservableCollection<LogEntry>();
            BatchProgressMaximum = BatchOptions.Count;
            CurrentKeyName = string.Empty;
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
            StartBatchCommand = new AsyncRelayCommand(StartWorkflow, () => CanStart);
            PauseCommand = new RelayCommand(PauseBatch, () => CanPause);
            ResumeCommand = new RelayCommand(ResumeBatch, () => CanResume);
            CancelBatchCommand = new RelayCommand(CancelBatch, () => CanCancel);
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(IsRunning) or nameof(IsPaused) or nameof(HasStarted) or nameof(IsCompleted) or nameof(IsCancelled))
                    UpdateCommands();
            };
            UpdateCommands();
        }

        public string Label { get; }
        public Favorite Favorite { get; }
        public KeyStore.KeyStore SourceKeyStore { get; }
        public PublishBatchOptions BatchOptions { get; }
        public ObservableCollection<LogEntry> Logs { get; set; }

        public IAsyncRelayCommand StartBatchCommand { get; }
        public RelayCommand PauseCommand { get; }
        public RelayCommand ResumeCommand { get; }
        public ICommand CancelBatchCommand { get; }

        private bool _hasStarted;
        public bool HasStarted
        {
            get => _hasStarted;
            set
            {
                if (SetProperty(ref _hasStarted, value))
                    NotifyCommandProperties();
            }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value))
                    NotifyCommandProperties();
            }
        }

        private bool _isCancelled;
        public bool IsCancelled
        {
            get => _isCancelled;
            set
            {
                if (SetProperty(ref _isCancelled, value))
                    NotifyCommandProperties();
            }
        }

        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            private set
            {
                if (SetProperty(ref _isPaused, value))
                {
                    OnPropertyChanged(nameof(CanPause));
                    OnPropertyChanged(nameof(CanResume));
                }
            }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (SetProperty(ref _isRunning, value))
                    NotifyCommandProperties();
            }
        }

        private int _batchProgressValue;
        public int BatchProgressValue { get => _batchProgressValue; private set => SetProperty(ref _batchProgressValue, value, nameof(BatchProgressText)); }

        private int _batchProgressMaximum;
        public int BatchProgressMaximum { get => _batchProgressMaximum; private set => SetProperty(ref _batchProgressMaximum, value, nameof(BatchProgressText)); }

        public string BatchProgressText => $"{BatchProgressValue}/{BatchProgressMaximum}";

        private int _keyStoreProgressValue;
        public int KeyStoreProgressValue { get => _keyStoreProgressValue; private set => SetProperty(ref _keyStoreProgressValue, value, nameof(KeyStoreProgressText)); }

        private int _keyStoreProgressMaximum;
        public int KeyStoreProgressMaximum { get => _keyStoreProgressMaximum; private set => SetProperty(ref _keyStoreProgressMaximum, value, nameof(KeyStoreProgressText)); }

        public string KeyStoreProgressText => $"{KeyStoreProgressValue}/{KeyStoreProgressMaximum}";

        private string _currentKeyName = string.Empty;
        public string CurrentKeyName { get => _currentKeyName; private set => SetProperty(ref _currentKeyName, value); }

        private int _successfulKeys;
        public int SuccessfulKeys { get => _successfulKeys; private set => SetProperty(ref _successfulKeys, value); }

        private int _failedKeys;
        public int FailedKeys { get => _failedKeys; private set => SetProperty(ref _failedKeys, value); }

        private int _totalProcessedKeys;
        public int TotalProcessedKeys { get => _totalProcessedKeys; private set => SetProperty(ref _totalProcessedKeys, value, nameof(ThroughputText)); }

        private double _currentSpeed;
        public double CurrentSpeed { get => _currentSpeed; private set => SetProperty(ref _currentSpeed, value, nameof(ThroughputText)); }

        private TimeSpan _eta;
        public TimeSpan ETA { get => _eta; private set => SetProperty(ref _eta, value, nameof(ThroughputText)); }

        public bool? CurrentKeySuccess { get; private set; }

        public string ThroughputText => $"Processed : {TotalProcessedKeys} keys | Speed : {CurrentSpeed:F1} keys/sec | ETA : {ETA:hh\\:mm\\:ss}";

        public bool CanToggleLogging => !IsRunning;

        public bool LoggingVerbosity
        {
            get => BatchOptions.LoggingVerbosity;
            set
            {
                if (IsRunning) return;
                BatchOptions.LoggingVerbosity = value;
                OnPropertyChanged();
            }
        }

        public bool CanStart => !HasStarted && !IsRunning;
        public bool CanPause => IsRunning && !IsPaused;
        public bool CanResume => !IsRunning && IsPaused;
        public bool CanCancel => IsRunning || IsPaused;

        private void PauseBatch()
        {
            _pauseEvent?.Reset();
            IsPaused = true;
            IsRunning = false;
        }

        private void ResumeBatch()
        {
            _pauseEvent?.Set();
            IsPaused = false;
            IsRunning = true;
        }

        private void CancelBatch()
        {
            lock (_ctsLock)
            {
                _cts?.Cancel();
            }
            _pauseEvent?.Set();
            IsPaused = false;
            IsRunning = false;
            HasStarted = false;
            IsCancelled = true;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_ctsLock)
            {
                try
                {
                    _cts?.Cancel();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cancelling CTS: {ex}");
                }
                try
                {
                    _cts?.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disposing CTS: {ex}");
                }
                _cts = null;
            }
            try
            {
                if (_pauseEvent != null)
                {
                    _pauseEvent.Set();
                    _pauseEvent.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disposing pause event: {ex}");
            }
            finally
            {
                _pauseEvent = null;
            }
            try
            {
                Logs.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing logs: {ex}");
            }
        }

        private void UpdateCommands()
        {
            StartBatchCommand.NotifyCanExecuteChanged();
            PauseCommand.NotifyCanExecuteChanged();
            ResumeCommand.NotifyCanExecuteChanged();
            (CancelBatchCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }

        private void NotifyCommandProperties()
        {
            foreach (var prop in new[] { nameof(CanStart), nameof(CanPause), nameof(CanResume), nameof(CanCancel) })
                OnPropertyChanged(prop);
        }

        public ICommand CloseCommand => new RelayCommand(Close);

        private void Close()
        {
            if (IsRunning || IsPaused)
            {
                try
                {
                    CancelBatchCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error while cancelling workflow during close: {ex}");
                }
            }
            try
            {
                DialogHost.CloseDialogCommand.Execute(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while closing dialog: {ex}");
            }
        }

        private async Task StartWorkflow()
        {
            CancellationToken token;
            lock (_ctsLock)
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                token = _cts.Token;
            }
            if (token.IsCancellationRequested) return;
            try
            {
                InitializeWorkflow();
                var factory = KeyStoreFactory.GetFactoryFromPropertyType(Favorite.Properties?.GetType())!;
                var targetStore = Favorite.CreateKeyStore() ?? throw new KeyStoreException("Cannot create key store from Favorite.");
                var keyIds = (await SourceKeyStore.GetAll()).ToList();
                KeyStoreProgressMaximum = keyIds.Count;
                BatchProgressMaximum = BatchOptions.Count;
                var stopwatch = Stopwatch.StartNew();
                for (int batchIndex = 0; batchIndex < BatchOptions.Count; batchIndex++)
                {
                    token.ThrowIfCancellationRequested();
                    await WaitAsync(_pauseEvent ?? new ManualResetEventSlim(true), token);
                    BatchProgressValue = batchIndex + 1;
                    var batchLog = new LogEntry($"Batch unit {batchIndex + 1} started", LogLevel.Info);
                    Logs.Add(batchLog);
                    int errors = await ProcessBatchKeys(SourceKeyStore, targetStore, keyIds, batchLog, stopwatch, token);
                    batchLog.Message += errors > 0 ? $" - Ended : {errors} fail(s)" : " - Success";
                    batchLog.Level = errors > 0 ? LogLevel.Error : LogLevel.Success;
                    await Task.Delay(200, token); //TODO keep comment and code for testing
                }
                CompleteWorkflow();
                //Notify("Batch Completed", "All batch units completed successfully.");
            }
            catch (OperationCanceledException)
            {
                IsCancelled = true;
                AddLog("Batch canceled by user.", LogLevel.Warning);
                //Notify("Batch Canceled", "The batch operation was canceled by the user.");
            }
            catch (Exception ex)
            {
                IsCancelled = true;
                AddLog($"Batch failed: {ex.Message}", LogLevel.Error);
                //Notify("Batch Failed", $"The batch operation failed: {ex.Message}");
            }
            finally
            {
                lock (_ctsLock)
                {
                    _cts?.Dispose();
                    _cts = null;
                }
                IsRunning = false;
                UpdateCommands();
            }
        }

        private void InitializeWorkflow()
        {
            _pauseEvent?.Dispose();
            _pauseEvent = new ManualResetEventSlim(true);
            IsPaused = false;
            IsCancelled = false;
            IsCompleted = false;
            HasStarted = true;
            IsRunning = true;
            Logs.Clear();
            BatchProgressValue = 0;
            KeyStoreProgressValue = 0;
            KeyStoreProgressMaximum = 0;
            CurrentKeyName = string.Empty;
            TotalProcessedKeys = 0;
            SuccessfulKeys = 0;
            FailedKeys = 0;
        }

        private void CompleteWorkflow()
        {
            BatchProgressValue = BatchProgressMaximum;
            IsCompleted = true;
            AddLog("Workflow completed successfully.", LogLevel.Success);
            AddLog($"Final summary : Total keys processed : {TotalProcessedKeys} | Successful : {SuccessfulKeys} | Failed : {FailedKeys}", LogLevel.Success);
            HasStarted = false;
            IsPaused = false;
            IsRunning = false;
            Notify("LEOSAC - Workflow Completed", $"Successfully processed {SuccessfulKeys} keys. {FailedKeys} failed.");
        }

        private async Task<int> ProcessBatchKeys(KeyStore.KeyStore source, KeyStore.KeyStore target, List<KeyEntryId> keyIds, LogEntry batchLog, Stopwatch stopwatch, CancellationToken token)
        {
            KeyStoreProgressValue = 0;
            int successCount = 0, failureCount = 0;
            int totalKeys = keyIds.Count * BatchOptions.Count;
            foreach (var keyId in keyIds)
            {
                await Task.Delay(200, token); //TODO keep comment and code for testing
                token.ThrowIfCancellationRequested();
                await WaitAsync(_pauseEvent ?? new ManualResetEventSlim(true), token);
                var keyEntry = await source.Get(keyId, KeyEntryClass.Symmetric);
                if (keyEntry == null)
                {
                    AddLogVerbose(batchLog, $"Key {keyId} not found, skipping.", LogLevel.Warning);
                    FailedKeys++;
                    TotalProcessedKeys++;
                    KeyStoreProgressValue = successCount + failureCount;
                    RefreshUi();
                    continue;
                }
                CurrentKeyName = keyEntry.Identifier.ToString();
                bool published = await PublishSingleKey(source, target, keyEntry, keyId, batchLog, token);
                if (published)
                {
                    successCount++;
                    SuccessfulKeys++;
                    CurrentKeySuccess = true;
                }
                else
                {
                    failureCount++;
                    FailedKeys++;
                    CurrentKeySuccess = false;
                }
                KeyStoreProgressValue = successCount + failureCount;
                TotalProcessedKeys++;
                var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                CurrentSpeed = elapsedSeconds > 0 ? TotalProcessedKeys / elapsedSeconds : 0;
                ETA = CurrentSpeed > 0 ? TimeSpan.FromSeconds((totalKeys - TotalProcessedKeys) / CurrentSpeed) : TimeSpan.Zero;
                RefreshUi();
            }
            if (failureCount > 0)
                batchLog.Nodes.Add(new LogEntry($"{failureCount} key(s) failed in this batch.", LogLevel.Error));
            batchLog.Nodes.Add(new LogEntry($"Batch unit completed ({successCount} succeeded, {failureCount} failed)", LogLevel.Info));
            return failureCount;
        }

        private async Task<bool> PublishSingleKey(KeyStore.KeyStore source, KeyStore.KeyStore target, KeyEntry keyEntry, KeyEntryId keyId, LogEntry batchLog, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            CurrentKeyName = keyEntry.Identifier.ToString();
            try
            {
                bool published = await PublishKeyWithPolicy(source, target, keyEntry, keyId, batchLog, token);
                CurrentKeySuccess = published;
                return published;
            }
            catch (Exception ex)
            {
                CurrentKeySuccess = false;
                Debug.WriteLine($"Key {CurrentKeyName} failed : {ex.Message}");
                return false;
            }
        }

        private async Task<bool> PublishKeyWithPolicy(KeyStore.KeyStore source, KeyStore.KeyStore target, KeyEntry keyEntry, KeyEntryId keyId, LogEntry parentLog, CancellationToken token)
        {
            async Task<bool> Execute()
            {
                AddLogVerbose(parentLog, $"Publishing key {CurrentKeyName} ...", LogLevel.Info);
                if (_rng.NextDouble() < 0.1) //TODO keep in code, for test
                    throw new Exception($"Simulated failure for key {CurrentKeyName}");
                try
                {
                    var publishTask = Task.Run(() => {
                        token.ThrowIfCancellationRequested();
                        source.Publish(target, null, null, keyEntry.KClass, new[] { keyId }, null);
                    }, token);

                    if (BatchOptions.TimeoutWait)
                    {
                        var timeoutTask = Task.Delay(BatchOptions.TimeoutWaitSeconds * 1000, token);
                        var completed = await Task.WhenAny(publishTask, timeoutTask);
                        if (completed == timeoutTask)
                        {
                            AddLogVerbose(parentLog,
                                $"Key {CurrentKeyName} timed out after {BatchOptions.TimeoutWaitSeconds}s",
                                LogLevel.Warning);
                            return false;
                        }
                    }
                    await publishTask;
                    AddLogVerbose(parentLog, $"Key {CurrentKeyName} published successfully.", LogLevel.Success);
                    return true;
                }
                catch (OperationCanceledException)
                {
                    AddLogVerbose(parentLog, $"Publishing of key {CurrentKeyName} was canceled.", LogLevel.Warning);
                    return false;
                }
                catch (Exception ex)
                {
                    var keyLog = new LogEntry($"Error publishing key {CurrentKeyName} : {ex.Message}", LogLevel.Error);
                    parentLog.Nodes.Add(keyLog);
                    return false;
                }
            }
            try
            {
                var result = await Execute();
                if (!result && BatchOptions.RetryOnFailure)
                {
                    var retryLog = new LogEntry($"Retrying key {CurrentKeyName} ...", LogLevel.Info);
                    parentLog.Nodes.Add(retryLog);
                    var retryResult = await Execute();
                    if (retryResult)
                        AddLogVerbose(retryLog, $"Key {CurrentKeyName} published successfully on retry.", LogLevel.Success);
                    return retryResult;
                }
                return result;
            }
            catch (Exception retryEx)
            {
                parentLog.Nodes.Add(new LogEntry($"Retry failed unexpectedly: {retryEx.Message}", LogLevel.Error));
                return false;
            }
        }

        private void Notify(string title, string message)
        {
            if (!BatchOptions.Notifications)
                return;
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.HasShutdownStarted)
                return;
            dispatcher.Invoke(() =>
            {
                var ok = new Button
                {
                    Content = "OK",
                    Width = 80,
                    Margin = new Thickness(10),
                    IsDefault = true,
                    IsCancel = true
                };
                var stack = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(20)
                };
                stack.Children.Add(new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 10),
                    FontSize = 14,
                    Foreground = Brushes.Black
                });
                stack.Children.Add(ok);

                var dialog = new Window
                {
                    Title = title,
                    Content = stack,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Application.Current!.MainWindow,
                    ResizeMode = ResizeMode.NoResize,
                    Background = Brushes.White,
                    WindowStyle = WindowStyle.ToolWindow
                };
                ok.Click += (_, __) => dialog.Close();
                dialog.ShowDialog();
            });
        }

        private void AddLog(string message, LogLevel level) => _uiContext.Post(_ => Logs.Add(new LogEntry(message, level)), null);

        private void AddLogVerbose(LogEntry parent, string message, LogLevel level)
        {
            if (BatchOptions.LoggingVerbosity)
                parent.Nodes.Add(new LogEntry(message, level));
        }

        private void RefreshUi()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastUiRefresh) >= _uiRefreshInterval)
                PostUiRefresh(now);
            else if (!_uiRefreshPending)
            {
                _uiRefreshPending = true;
                Task.Delay(_uiRefreshInterval - (now - _lastUiRefresh))
                    .ContinueWith(_ => PostUiRefresh(DateTime.UtcNow), TaskScheduler.Default);
            }
        }

        private void PostUiRefresh(DateTime? timestamp = null)
        {
            _lastUiRefresh = timestamp ?? DateTime.UtcNow;
            _uiRefreshPending = false;
            _uiContext.Post(_ =>
            {
                foreach (var prop in new[] { nameof(BatchProgressValue), nameof(KeyStoreProgressValue),
                    nameof(KeyStoreProgressMaximum), nameof(CurrentKeyName), nameof(CurrentSpeed),
                    nameof(ETA), nameof(TotalProcessedKeys) })
                    OnPropertyChanged(prop);
            }, null);
        }

        private static Task WaitAsync(ManualResetEventSlim mre, CancellationToken token)
        {
            if (mre.IsSet)
                return Task.CompletedTask;
            var tcs = new TaskCompletionSource<object?>();
            var registration = token.Register(() => tcs.TrySetCanceled(token));
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    mre.Wait(token);
                    tcs.TrySetResult(null);
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled(token);
                }
                finally
                {
                    registration.Dispose();
                }
            });
            return tcs.Task;
        }

    }
}