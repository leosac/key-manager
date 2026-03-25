using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using LibLogicalAccess;
using System.Diagnostics;

namespace Leosac.KeyManager.Domain
{
    public class SAMCardDevice : ICardDevice, IDisposable
    {
        public readonly KeyStore _sam;
        public string DeviceName => _sam.Name;
        public string? DeviceUid => _sam.Attributes.ContainsKey("uid") ? _sam.Attributes["uid"] : null;

        public bool IsSAMDevice => true;


        private ReaderUnit? _reader;

        public SAMCardDevice(KeyStore samKeyStore)
        {
            _sam = samKeyStore ?? throw new ArgumentNullException(nameof(samKeyStore));
        }

        public async Task InitializeReaderAsync(Action<string, LogLevel> log, Action<string, LogLevel>? notify)
        {
            var provider = LibraryManager.getInstance().getReaderProvider("PCSC");
            var readers = provider.getReaderList();
            if (readers.Count == 0)
            {
                log?.Invoke("No card readers detected for SAM device.", LogLevel.Error);
                notify?.Invoke("No card readers detected.", LogLevel.Error);
                throw new InvalidOperationException("No card readers detected.");
            }
            _reader = readers[0];
            log?.Invoke($"Using reader : {_reader.getName()}", LogLevel.Info);
            await Task.CompletedTask;
        }

        public async Task<bool> WaitForCardInsertionAsync(TimeSpan timeout, bool useTimeout, CancellationToken cancellationToken, ManualResetEventSlim _pauseEvent,
            LogEntry? batchLog = null, Action<string, LogLevel>? log = null, Action? logChanged = null)
        {
            if (_reader == null)
            {
                log?.Invoke("SAM reader not initialized", LogLevel.Error);
                return false;
            }
            Stopwatch? stopwatch = useTimeout ? Stopwatch.StartNew() : null;
            int timeoutSeconds = useTimeout ? (int)timeout.TotalSeconds : 0;
            string originalMessage = batchLog?.Message ?? "Waiting for card insertion...";
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_pauseEvent != null && !_pauseEvent.IsSet)
                {
                    await Task.Delay(200, cancellationToken);
                    continue;
                }
                if (useTimeout && stopwatch != null)
                {
                    int secondsRemaining = Math.Max(0, timeoutSeconds - (int)stopwatch.Elapsed.TotalSeconds);
                    if (batchLog != null)
                    {
                        batchLog.Message = $"{originalMessage} {secondsRemaining} second{(secondsRemaining != 1 ? "s" : "")} remaining";
                        batchLog.Level = LogLevel.Warning;
                        logChanged?.Invoke();
                    }
                }
                bool inserted = await Task.Run(() => _reader.waitInsertion(200), cancellationToken);
                if (inserted)
                {
                    if (batchLog != null)
                    {
                        batchLog.Message = originalMessage + " - Card inserted";
                        batchLog.Level = LogLevel.Info;
                    }
                    return true;
                }
                if (useTimeout && stopwatch != null && stopwatch.Elapsed >= timeout)
                {
                    if (batchLog != null)
                    {
                        batchLog.Message = originalMessage + " - Card not inserted (timed out)";
                        batchLog.Level = LogLevel.Error;
                    }
                    log?.Invoke(batchLog?.Message ?? "Card not inserted (timed out)", LogLevel.Error);
                    return false;
                }
                await Task.Delay(200, cancellationToken);
            }
            if (batchLog != null)
            {
                batchLog.Message += " - Card insertion canceled.";
                batchLog.Level = LogLevel.Warning;
            }
            log?.Invoke(batchLog?.Message ?? "Card insertion canceled", LogLevel.Warning);
            return false;
        }

        public async Task<bool> PrepareForBatchAsync(CancellationToken token, LogEntry? batchLog = null, int batchIndex = 0, Action<LogEntry>? log = null, Action<string, LogLevel>? notify = null)
        {
            if (_reader == null)
            {
                if (batchLog != null)
                {
                    batchLog.Message += " - SAM reader not initialized";
                    batchLog.Level = LogLevel.Error;
                    log?.Invoke(batchLog);
                }
                notify?.Invoke("SAM reader not initialized.", LogLevel.Error);
                return false;
            }
            bool connected = await Task.Run(() => _reader.connect(), token);
            if (!connected)
            {
                if (batchLog != null)
                {
                    batchLog.Message += " - Failed to connect to reader";
                    batchLog.Level = LogLevel.Error;
                    log?.Invoke(batchLog);
                }
                notify?.Invoke($"Batch unit {batchIndex + 1} : Failed to connect to reader.", LogLevel.Error);
                return false;
            }
            var chip = _reader.getSingleChip();
            var cmd = chip.getCommands() as LibLogicalAccess.Reader.SAMAV2ISO7816Commands;
            if (cmd == null)
            {
                if (batchLog != null)
                {
                    batchLog.Message += " - SAM chip not in AV2 mode";
                    batchLog.Level = LogLevel.Error;
                    log?.Invoke(batchLog);
                }
                notify?.Invoke($"Batch unit {batchIndex + 1} : SAM chip not in AV2 mode.", LogLevel.Error);
                await Task.Run(() => _reader.waitRemoval(0), token);
                await Task.Run(() => _reader.disconnect(), token);
                return false;
            }
            return true;
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            if (_reader != null)
                await Task.Run(() => _reader.disconnect(), token);
        }

        public async Task EnsureCardInserted()
        {
            if (_reader == null) return;
            _reader.waitInsertion(0);
        }

        public ReaderUnit? GetReader()
        {
            return _reader;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}