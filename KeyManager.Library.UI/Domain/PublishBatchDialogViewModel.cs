using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Leosac.KeyManager.Library.Common;
using Leosac.KeyManager.Library.Device;
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
        private bool _uiRefreshPending;

        private readonly SynchronizationContext _uiContext;
        private readonly object _ctsLock = new();
        private CancellationTokenSource? _cts;
        private ManualResetEventSlim? _pauseEvent;
        private bool _disposed;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PublishBatchDialogViewModel));

        protected readonly IList<KeyEntriesControlViewModel> _selection;

        public PublishBatchDialogViewModel(Favorite favorite, PublishBatchOptions batchOptions, KeyStore.KeyStore sourceKeyStore, IList<KeyEntriesControlViewModel> keModels)
        {
            Favorite = favorite ?? throw new ArgumentNullException(nameof(favorite));
            BatchOptions = batchOptions ?? throw new ArgumentNullException(nameof(batchOptions));
            SourceKeyStore = sourceKeyStore ?? throw new ArgumentNullException(nameof(sourceKeyStore));
            _selection = keModels;
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
            Label = string.Format(Properties.Resources.PublishLabel, BatchOptions.Count, BatchOptions.Count > 1 ? "s" : string.Empty, Favorite.Name);
            Logs = new ObservableCollection<LogEntry>();
            BatchProgressMaximum = BatchOptions.Count;
            CurrentKeyName = string.Empty;
            StartBatchCommand = new AsyncRelayCommand(StartWorkflow, () => CanStart);
            PauseCommand = new RelayCommand(PauseBatch, () => CanPause);
            ResumeCommand = new RelayCommand(ResumeBatch, () => CanResume);
            CancelBatchCommand = new RelayCommand(CancelBatch, () => CanCancel);
            CloseCommand = new RelayCommand(Close);
            NextCommand = new RelayCommand(NextBatch, () => ShowNextButton);
            RetryCommand = new RelayCommand(() =>
            {
                _retryAccepted = true;
                _waitForRetry?.TrySetResult(true);
            });
            SkipCommand = new RelayCommand(() =>
            {
                _retryAccepted = false;
                _waitForRetry?.TrySetResult(true);
            });
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(IsRunning) or nameof(IsPaused) or nameof(HasStarted) or nameof(IsCompleted) or nameof(IsCancelled))
                {
                    UpdateCommands();
                    UpdateOptionsVisibility();
                }
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

        public RelayCommand RetryCommand { get; }
        public RelayCommand SkipCommand { get; }

        public RelayCommand NextCommand { get; }

        private TaskCompletionSource<bool>? _waitForNext;

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

        private bool _isFailed;
        public bool IsFailed
        {
            get => _isFailed;
            set => SetProperty(ref _isFailed, value);
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

        public bool ShowRetryButton => BatchOptions.RetryOnFailure && CurrentBatchFailed && _waitForRetry != null;
        public bool ShowSkipButton => BatchOptions.RetryOnFailure && CurrentBatchFailed && _waitForRetry != null;
        public bool ShowNextButton => !BatchOptions.ContinuousMode && IsWaitingForCard && !IsLastBatch;
        private bool IsLastBatch => BatchProgressValue >= BatchProgressMaximum;

        private bool _isWaitingForNext;
        public bool IsWaitingForNext
        {
            get => _isWaitingForNext;
            private set
            {
                if (SetProperty(ref _isWaitingForNext, value))
                    UpdateCommands();
            }
        }

        private TaskCompletionSource<bool>? _waitForRetry;
        private bool _retryAccepted;

        private int _batchIndex;
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

        private int _skippedBatches;
        public int SkippedBatches
        {
            get => _skippedBatches;
            set { _skippedBatches = value; OnPropertyChanged(nameof(SkippedBatches)); }
        }

        private bool _isWaitingForCard;
        public bool IsWaitingForCard
        {
            get => _isWaitingForCard;
            set { _isWaitingForCard = value; OnPropertyChanged(); UpdateOptionsVisibility(); }
        }

        private bool _currentBatchFailed;
        public bool CurrentBatchFailed
        {
            get => _currentBatchFailed;
            set { _currentBatchFailed = value; OnPropertyChanged(); UpdateOptionsVisibility(); }
        }

        private bool _isSkipOrRetry;

        public bool CanStart => !HasStarted && !IsRunning;
        public bool CanPause => !_isWaitingForCard && !_isWaitingForNext && !_isSkipOrRetry && IsRunning && !IsPaused;
        public bool CanResume => !_isWaitingForCard && !_isWaitingForNext && !_isSkipOrRetry && !IsRunning && IsPaused;
        public bool CanCancel => IsRunning || IsPaused;

        private Stopwatch? _stopwatch;

        private bool IsRetryAccepted() => _retryAccepted;

        private void PauseBatch()
        {
            _pauseEvent?.Reset();
            _stopwatch?.Stop();
            IsPaused = true;
            IsRunning = false;
        }

        private void ResumeBatch()
        {
            _pauseEvent?.Set();
            _stopwatch?.Start();
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
            _waitForNext?.TrySetResult(false);
            _waitForRetry?.TrySetResult(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            CancellationTokenSource? cts;
            ManualResetEventSlim? pause;
            lock (_ctsLock)
            {
                cts = _cts;
                _cts = null;
            }
            pause = _pauseEvent;
            _pauseEvent = null;
            try
            {
                cts?.Cancel();
            }
            catch (Exception ex)
            {
                log.Error($"Error cancelling CTS : {ex}");
            }
            try
            {
                pause?.Set();
            }
            catch (Exception ex)
            {
                log.Error($"Error releasing pause event : {ex}");
            }
            try
            {
                cts?.Dispose();
            }
            catch (Exception ex)
            {
                log.Error($"Error disposing CTS : {ex}");
            }
            try
            {
                pause?.Dispose();
            }
            catch (Exception ex)
            {
                log.Error($"Error disposing pause event : {ex}");
            }
            Logs.Clear();
        }

        private void UpdateCommands()
        {
            if (_disposed) return;
            StartBatchCommand.NotifyCanExecuteChanged();
            PauseCommand.NotifyCanExecuteChanged();
            ResumeCommand.NotifyCanExecuteChanged();
            (CancelBatchCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }

        private void UpdateOptionsVisibility()
        {
            if (_disposed) return;
            OnPropertyChanged(nameof(ShowRetryButton));
            OnPropertyChanged(nameof(ShowSkipButton));
            OnPropertyChanged(nameof(ShowNextButton));
            RetryCommand?.NotifyCanExecuteChanged();
            SkipCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
        }

        private void NotifyCommandProperties()
        {
            foreach (var prop in new[] { nameof(CanStart), nameof(CanPause), nameof(CanResume), nameof(CanCancel) })
                OnPropertyChanged(prop);
        }

        public RelayCommand CloseCommand { get; }

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
                    log.Error($"Error while cancelling workflow during close : {ex}");
                }
            }
            try
            {
                DialogHost.CloseDialogCommand.Execute(null, null);
            }
            catch (Exception ex)
            {
                log.Error($"Error while closing dialog : {ex}");
            }
        }

        private void NextBatch()
        {
            AddLog("Moving to next batch. Waiting for validation...", LogLevel.Info);
            if (_waitForNext != null && !_waitForNext.Task.IsCompleted)
            {
                _waitForNext.SetResult(true);
                _waitForNext = null;
            }
        }

        private ICardDevice CreateDeviceFromFavorite(KeyStore.KeyStore keyStore, KeyStoreFactory factory)
        {
            if (Favorite.Properties == null)
                throw new InvalidOperationException("Favorite does not have KeyStoreProperties set.");
            if (factory != null)
            {
                try
                {
                    var device = factory.CreateCardDevice(keyStore);
                    if (device != null)
                    {
                        Logs.Add(new LogEntry($"Matched {factory.Name} type", LogLevel.Info));
                        return device;
                    }
                }
                catch (NotImplementedException ex)
                {
                    log.Error($"Factory {factory.Name} is not implemented : {ex.Message}");
                }
            }
            Logs.Add(new LogEntry("Creating generic card device", LogLevel.Info));
            return new GenericCardDevice(keyStore);
        }

        private async Task<ICardDevice> InitializeDeviceAsync(KeyStore.KeyStore targetStore, KeyStoreFactory factory)
        {
            ICardDevice device = CreateDeviceFromFavorite(targetStore, factory);
            await device.InitializeReaderAsync(
                (msg, level) => Logs.Add(new LogEntry(msg, level)),
                (msg, level) => Notify(msg, level)
            );
            return device;
        }

        private async Task<List<IChangeKeyEntry>> BuildChangesForOrderingAsync(IEnumerable<KeyEntryId> keyIds, KeyStore.KeyStore targetStore)
        {
            var result = new List<IChangeKeyEntry>();
            foreach (var id in keyIds)
            {
                var entry = await targetStore.Get(id, KeyEntryClass.Symmetric);
                if (entry is IChangeKeyEntry change)
                    result.Add(change);
            }
            return result;
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
                var targetStore = Favorite.CreateKeyStore() ?? throw new KeyStoreException("Cannot create KeyStore from Favorite.");
                var factory = KeyStoreFactory.GetFactoryFromPropertyType(Favorite?.Properties?.GetType())
                    ?? throw new InvalidOperationException("No factory found for favorite.");
                ICardDevice device = await InitializeDeviceAsync(targetStore, factory);
                var keyIds = await GetSelectedKeysAsync();
                
                var changesForOrdering = await BuildChangesForOrderingAsync(keyIds, targetStore);
                keyIds = factory.OrderKeyEntries(changesForOrdering, targetStore).Select(x => x.Identifier).ToList();
                
                if (keyIds.Count == 0)
                {
                    AddLog("No keys selected for publishing. Aborting workflow.", LogLevel.Warning);
                    Notify("No keys selected for publishing. Aborting workflow.", LogLevel.Warning);
                    CompleteWorkflow();
                    return;
                }
                KeyStoreProgressMaximum = keyIds.Count;
                BatchProgressMaximum = BatchOptions.Count;
                _stopwatch = Stopwatch.StartNew();
                for (_batchIndex = 0; _batchIndex < BatchOptions.Count; _batchIndex++)
                {
                    token.ThrowIfCancellationRequested();
                    var pause = _pauseEvent ?? throw new InvalidOperationException("Pause event not initialized");
                    await WaitAsync(pause, token);
                    _retryAccepted = false;
                    BatchProgressValue = _batchIndex + 1;
                    var batchLog = new LogEntry($"Batch unit {BatchProgressValue} started", LogLevel.Info);
                    Logs.Add(batchLog);
                    _isWaitingForCard = true;
                    UpdateCommands();
                    AddLog("Waiting for card insertion...", LogLevel.Info, batchLog);
                    _stopwatch.Stop();
                    bool cardInserted = await device.WaitForCardInsertionAsync(
                        TimeSpan.FromSeconds(BatchOptions.TimeoutWaitSeconds),
                        BatchOptions.TimeoutWait,
                        token,
                        _pauseEvent,
                        batchLog.Message,
                        batchLog: (msg, level) =>
                        {
                            batchLog.Message = msg;
                            batchLog.Level = level;
                        },
                        (msg, level) => AddLog(msg, level, batchLog),
                        () => OnPropertyChanged(nameof(Logs)));
                    _stopwatch.Start();
                    _isWaitingForCard = false;
                    UpdateCommands();
                    if (!cardInserted)
                    {
                        _skippedBatches++;
                        continue;
                    }
                    AddLog("Card inserted.", LogLevel.Info, batchLog);
                    bool ready = await device.PrepareForBatchAsync(token,
                        batchIndex: _batchIndex,
                        batchLog: (msg, level) =>
                        {
                            batchLog.Message = msg;
                            batchLog.Level = level;
                        },
                        log: (msg, level) => Logs.Add(new LogEntry(msg, level)),
                        notify: (msg, level) => Notify(msg, level));
                    if (!ready)
                    {
                        _skippedBatches++;
                        continue;
                    }
                    int errors = await Task.Run(() => ProcessBatchKeysAsync(SourceKeyStore, targetStore, keyIds, batchLog, device, token), token);
                    CurrentBatchFailed = errors > 0;
                    IsFailed = errors > 0;
                    AddLog("Waiting for card removal...", LogLevel.Info, batchLog);
                    _isWaitingForCard = true;
                    UpdateCommands();
                    _stopwatch.Stop();
                    await device.WaitForRemovalAsync(token);
                    _stopwatch.Start();
                    _isWaitingForCard = false;
                    UpdateCommands();
                    AddLog("Card removed.", LogLevel.Info, batchLog);
                    await device.DisconnectAsync(token);
                    batchLog.Message += errors > 0 ? $" - Ended : {errors} fail(s)" : " - Success";
                    batchLog.Level = errors > 0 ? LogLevel.Error : LogLevel.Success;
                    await HandleRetryAsync();
                    if (!BatchOptions.ContinuousMode && _batchIndex < BatchOptions.Count - 1)
                        await WaitForNextCardAsync(_batchIndex);
                }
                CompleteWorkflow();
            }
            catch (OperationCanceledException)
            {
                IsCancelled = true;
                AddLog("Operations canceled by user.", LogLevel.Warning);
                Notify("Operations canceled by user.", LogLevel.Warning);
            }
            catch (Exception ex)
            {
                IsCancelled = true;
                AddLog($"Batch failed : {ex.Message}", LogLevel.Error);
                Notify($"Batch failed : {ex.Message}", LogLevel.Error);
            }
            finally
            {
                FinishAndPause();
                IsRunning = false;
                UpdateCommands();
            }
        }

        private void InitializeWorkflow()
        {
            _pauseEvent?.Dispose();
            _pauseEvent = new ManualResetEventSlim(true);
            IsFailed = false;
            IsPaused = false;
            IsCancelled = false;
            IsCompleted = false;
            HasStarted = true;
            IsRunning = true;
            Logs.Clear();
            _batchIndex = 0;
            BatchProgressValue = 0;
            KeyStoreProgressValue = 0;
            KeyStoreProgressMaximum = 0;
            CurrentKeyName = string.Empty;
            TotalProcessedKeys = 0;
            _skippedBatches = 0;
            SuccessfulKeys = 0;
            FailedKeys = 0;
        }

        private void CompleteWorkflow()
        {
            BatchProgressValue = BatchProgressMaximum;
            IsCompleted = true;
            AddLog("Workflow completed.", LogLevel.Success);
            AddLog($"Final summary : Total keys processed : {TotalProcessedKeys} | Successful : {SuccessfulKeys} | Failed : {FailedKeys} | Skipped batches : {SkippedBatches}", LogLevel.Success);
            HasStarted = false;
            IsPaused = false;
            IsRunning = false;
            Notify($"Workflow completed : {SuccessfulKeys} keys succeeded keys, {FailedKeys} keys failed, {_skippedBatches} batches were skipped.", (FailedKeys > 0 || _skippedBatches > 0) ? LogLevel.Warning : LogLevel.Success);
        }

        private void FinishAndPause()
        {
            CancellationTokenSource? cts;
            ManualResetEventSlim? pause;
            lock (_ctsLock)
            {
                cts = _cts;
                _cts = null;
            }
            pause = _pauseEvent;
            _pauseEvent = null;
            try
            {
                cts?.Dispose();
            }
            catch (Exception ex)
            {
                log.Error($"Error disposing CTS : {ex}");
            }
            try
            {
                pause?.Dispose();
            }
            catch (Exception ex)
            {
                log.Error($"Error disposing pause event : {ex}");
            }
        }

        private async Task<List<KeyEntryId>> GetSelectedKeysAsync()
        {
            var keyIds = (await Task.WhenAll(_selection.Select(async ke =>
            {
                if (!ke.ShowSelection)
                {
                    var keys = await SourceKeyStore.GetAll(ke.KeyEntryClass);
                    return keys;
                }
                else
                    return ke.Identifiers.Where(k => k.Selected && k.KeyEntryId != null)
                                         .Select(k => k.KeyEntryId!);
            }))).SelectMany(l => l).ToList();
            return keyIds;
        }

        private async Task<int> ProcessBatchKeysAsync(KeyStore.KeyStore source, KeyStore.KeyStore target, List<KeyEntryId> keyIds, LogEntry batchLog, ICardDevice? device, CancellationToken token)
        {
            KeyStoreProgressValue = 0;
            int successCount = 0, failureCount = 0;
            int totalKeys = keyIds.Count * BatchOptions.Count;
            try
            {
                await target.Open();
                foreach (var keyId in keyIds)
                {
                    token.ThrowIfCancellationRequested();
                    await WaitIfPausedAsync(batchLog, token);
                    var keyEntry = await source.Get(keyId, KeyEntryClass.Symmetric);
                    if (keyEntry == null)
                    {
                        AddLog($"Key {keyId} not found, skipping.", LogLevel.Warning, batchLog);
                        failureCount++;
                        FailedKeys++;
                        TotalProcessedKeys++;
                        KeyStoreProgressValue = successCount + failureCount;
                        RefreshUi();
                        continue;
                    }
                    CurrentKeyName = keyEntry.Identifier.ToString();
                    bool published = false;
                    try
                    {
                        if (device != null)
                            await device.EnsureCardInserted(token);
                        AddLog($"Publishing key {CurrentKeyName} ...", LogLevel.Info, batchLog);
                        await source.Publish(
                            store: target,
                            getFavoriteKeyStore: null,
                            askForKeyStoreSecretIfRequired: null,
                            keClass: keyEntry.KClass,
                            ids: new List<KeyEntryId> { keyId },
                            initCallback: null
                        );
                        published = true;
                        AddLog($"Key {CurrentKeyName} published successfully.", LogLevel.Success, batchLog);
                    }
                    catch (OperationCanceledException)
                    {
                        AddLog($"Publishing of key {CurrentKeyName} was canceled.", LogLevel.Warning, batchLog);
                        IsCancelled = true;
                    }
                    catch (Exception ex)
                    {
                        AddLog($"Publishing key {CurrentKeyName} failed: {ex.Message}", LogLevel.Error, batchLog);
                    }
                    if (published)
                    {
                        successCount++;
                        SuccessfulKeys++;
                    }
                    else
                    {
                        failureCount++;
                        FailedKeys++;
                    }
                    KeyStoreProgressValue = successCount + failureCount;
                    TotalProcessedKeys++;
                    var elapsedSeconds = _stopwatch?.Elapsed.TotalSeconds ?? 0;
                    CurrentSpeed = elapsedSeconds > 0 ? TotalProcessedKeys / elapsedSeconds : 0;
                    ETA = CurrentSpeed > 0 ? TimeSpan.FromSeconds((totalKeys - TotalProcessedKeys) / CurrentSpeed) : TimeSpan.Zero;
                    RefreshUi();
                }
            }
            catch (OperationCanceledException)
            {
                IsCancelled = true;
                AddLog("Processing canceled.", LogLevel.Warning, batchLog);
            }
            finally
            {
                await target.Close(true);
            }
            if (failureCount > 0)
                AddLog($"{failureCount} key(s) failed in this batch.", LogLevel.Error, batchLog);
            AddLog($"Batch unit completed ({successCount} succeeded, {failureCount} failed)", LogLevel.Info, batchLog);
            return failureCount;
        }

        private async Task WaitIfPausedAsync(LogEntry batchLog, CancellationToken token)
        {
            var pause = _pauseEvent ?? throw new InvalidOperationException("Pause event not initialized");
            if (!_pauseEvent.IsSet && IsPaused)
                AddLog("Workflow paused by user.", LogLevel.Warning, batchLog);
            await WaitAsync(pause, token);
            if (_pauseEvent.IsSet && !IsRunning)
                AddLog("Workflow resumed.", LogLevel.Warning, batchLog);
        }

        private async Task HandleRetryAsync()
        {
            if (!BatchOptions.RetryOnFailure || !CurrentBatchFailed || _cts == null || _cts.IsCancellationRequested)
                return;
            _retryAccepted = false;
            _isSkipOrRetry = true;
            UpdateCommands();
            while (CurrentBatchFailed && _cts != null && !_cts.IsCancellationRequested)
            {
                _waitForRetry = new TaskCompletionSource<bool>();
                UpdateOptionsVisibility();
                Logs.Add(new LogEntry($"Batch unit {BatchProgressValue} failed. Waiting for Retry or Skip...", LogLevel.Warning));
                await _waitForRetry.Task;
                _waitForRetry = null;
                UpdateOptionsVisibility();
                if (!IsRetryAccepted())
                {
                    _skippedBatches++;
                    Logs.Add(new LogEntry($"Batch unit {BatchProgressValue} skipped by user.", LogLevel.Error));
                    break;
                }
                else
                {
                    Logs.Add(new LogEntry($"Retrying batch unit {BatchProgressValue}...", LogLevel.Warning));
                    BatchProgressValue -= 1;
                    _batchIndex -= 1;
                    break;
                }
            }
            _isSkipOrRetry = false;
            UpdateCommands();
        }

        private async Task WaitForNextCardAsync(int batchIndex)
        {
            Logs.Add(new LogEntry($"Batch unit {batchIndex + 1} completed. Waiting for you to press 'Next' to continue with the next card.", LogLevel.Warning));
            _waitForNext = new TaskCompletionSource<bool>();
            IsWaitingForCard = true;
            IsWaitingForNext = true;
            UpdateOptionsVisibility();
            await _waitForNext.Task;
            _waitForNext = null;
            IsWaitingForCard = false;
            IsWaitingForNext = false;
            UpdateOptionsVisibility();
        }

        private static string FormatMessage(string message, LogLevel level) => level switch
        {
            LogLevel.Success => $"✔️ {message}",
            LogLevel.Warning => $"⚠️ {message}",
            LogLevel.Error => $"❌ {message}",
            _ => message
        };

        private void Notify(string message, LogLevel level)
        {
            var title = level switch
            {
                LogLevel.Success => "Success",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                _ => "Information"
            };
            Notify(title, FormatMessage(message, level));
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
                ok.Click += (_, _) => dialog.Close();
                dialog.ShowDialog();
            });
        }

        private void SafeUiAction(Action action)
        {
            if (_disposed) return;
            if (_uiContext == null)
                action();
            else
                _uiContext.Post(_ => action(), null);
        }

        private void AddLog(string message, LogLevel level, LogEntry? parent = null)
        {
            if (parent is null)
                SafeUiAction(() => Logs.Add(new LogEntry(message, level)));
            else
            {
                if (!BatchOptions.LoggingVerbosity) return;
                SafeUiAction(() => parent.Nodes.Add(new LogEntry(message, level)));
            }
        }

        private void RefreshUi()
        {
            if (_disposed) return;
            var now = DateTime.UtcNow;
            if ((now - _lastUiRefresh) >= _uiRefreshInterval)
            {
                PostUiRefresh(now);
                return;
            }
            if (_uiRefreshPending) return;
            _uiRefreshPending = true;
            var delay = _uiRefreshInterval - (now - _lastUiRefresh);
            _ = Task.Delay(delay).ContinueWith(_ =>
            {
                if (_disposed) return;
                PostUiRefresh(DateTime.UtcNow);
            }, TaskScheduler.Default);
        }

        private void PostUiRefresh(DateTime? timestamp = null)
        {
            _lastUiRefresh = timestamp ?? DateTime.UtcNow;
            _uiRefreshPending = false;
            _uiContext.Post(_ =>
            {
                if (_disposed) return;
                foreach (var prop in new[] { nameof(BatchProgressValue), nameof(KeyStoreProgressValue),
                    nameof(KeyStoreProgressMaximum), nameof(CurrentKeyName), nameof(CurrentSpeed),
                    nameof(ETA), nameof(TotalProcessedKeys) })
                    OnPropertyChanged(prop);
            }, null);
        }

        public string TimeoutWaitSecondsText
        {
            get => BatchOptions.TimeoutWaitSeconds.ToString();
            set
            {
                if (int.TryParse(value, out var seconds))
                    BatchOptions.TimeoutWaitSeconds = seconds;
                OnPropertyChanged();
            }
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