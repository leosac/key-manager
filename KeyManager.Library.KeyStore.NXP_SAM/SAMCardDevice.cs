using Leosac.KeyManager.Library.Common;
using Leosac.KeyManager.Library.Device;
using Leosac.KeyManager.Library.KeyStore;
using LibLogicalAccess;
using log4net;
using System.Diagnostics;

namespace Leosac.KeyManager.Domain
{
    public class SAMCardDevice : ICardDevice, IDisposable
    {
        public string DeviceName { get; }
        public string? DeviceUid { get; }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(SAMCardDevice));

        public bool IsSAMDevice => true;

        private ReaderUnit? _reader;

        public SAMCardDevice(KeyStore store)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));

            DeviceName = store.Name;
            DeviceUid = store.Attributes.TryGetValue("uid", out var uid) ? uid : null;
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
        }

        public async Task<bool> WaitForCardInsertionAsync(TimeSpan timeout, bool useTimeout, CancellationToken cancellationToken, ManualResetEventSlim _pauseEvent,
            string? message = null, Action<string, LogLevel>? batchLog = null, Action<string, LogLevel>? log = null, Action? logChanged = null)
        {
            if (_reader == null)
            {
                log?.Invoke("SAM reader not initialized", LogLevel.Error);
                return false;
            }
            Stopwatch? stopwatch = useTimeout ? Stopwatch.StartNew() : null;
            string originalMessage = message ?? "Waiting for card insertion...";
            while (!cancellationToken.IsCancellationRequested)
            {
                if (useTimeout && stopwatch != null)
                {
                    int secondsRemaining = Math.Max(0, (int)Math.Ceiling(timeout.TotalSeconds - stopwatch.Elapsed.TotalSeconds));
                    message = $"{originalMessage} {secondsRemaining} second{(secondsRemaining != 1 ? "s" : "")} remaining";
                    batchLog?.Invoke(message, LogLevel.Warning);
                    logChanged?.Invoke();
                }
                bool inserted = _reader.waitInsertion(200);
                if (inserted)
                {
                    message = originalMessage + " - Card inserted";
                    batchLog?.Invoke(message, LogLevel.Info);
                    return true;
                }
                if (useTimeout && stopwatch != null && stopwatch.Elapsed >= timeout)
                {
                    message = originalMessage + " - Card not inserted (timed out)";
                    batchLog?.Invoke(message, LogLevel.Error);
                    log?.Invoke(message ?? "Card not inserted (timed out)", LogLevel.Error);
                    return false;
                }
                await Task.Delay(200, cancellationToken);
            }
            message += " - Card insertion canceled.";
            string canceledMsg = message ?? "Card insertion canceled";
            batchLog?.Invoke(canceledMsg, LogLevel.Warning);
            log?.Invoke(canceledMsg, LogLevel.Warning);
            return false;
        }

        public async Task<bool> PrepareForBatchAsync(CancellationToken token, int batchIndex = 0, string? message = null, Action<string, LogLevel>? batchLog = null, Action<string, LogLevel>? log = null, Action<string, LogLevel>? notify = null)
        {
            if (_reader == null)
            {
                message += " - SAM reader not initialized";
                batchLog?.Invoke(message, LogLevel.Error);
                log?.Invoke(message, LogLevel.Error);
                notify?.Invoke("SAM reader not initialized.", LogLevel.Error);
                return false;
            }
            bool connected = await Task.Run(() => _reader.connect(), token);
            if (!connected)
            {
                message += " - Failed to connect to reader";
                batchLog?.Invoke(message, LogLevel.Error);
                log?.Invoke(message, LogLevel.Error);
                notify?.Invoke($"Batch unit {batchIndex + 1} : Failed to connect to reader.", LogLevel.Error);
                return false;
            }
            var chip = _reader.getSingleChip();
            var cmd = chip.getCommands() as LibLogicalAccess.Reader.SAMAV2ISO7816Commands;
            if (cmd == null)
            {
                message += " - SAM chip not in AV2 mode";
                batchLog?.Invoke(message, LogLevel.Error);
                log?.Invoke(message, LogLevel.Error);
                notify?.Invoke($"Batch unit {batchIndex + 1} : SAM chip not in AV2 mode.", LogLevel.Error);
                await Task.Run(() => _reader.waitRemoval(0), token);
                try
                {
                    _reader.disconnect();
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Error while disconnecting reader : {ex.Message}", LogLevel.Warning);
                    notify?.Invoke($"Error while disconnecting reader : {ex.Message}", LogLevel.Warning);
                }
                return false;
            }
            return true;
        }

        public async Task WaitForRemovalAsync(CancellationToken token)
        {
            if (_reader == null) return;

            while (!token.IsCancellationRequested)
            {
                bool removed = await Task.Run(() => _reader.waitRemoval(200), token);
                if (removed) return;
            }
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            if (_reader != null)
                await Task.Run(() => _reader.disconnect(), token);
        }

        public async Task EnsureCardInserted(CancellationToken token = default)
        {
            if (_reader == null) return;
            bool inserted;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    inserted = _reader.waitInsertion(200);
                }
                catch (LibLogicalAccessException ex)
                {
                    log.Error($"Reader exception : {ex.Message}");
                    throw;
                }
                if (inserted)
                    return;
            }
            throw new Exception("Card physically removed or cancelled");
        }

        public object? GetReader() => _reader;

        public void Dispose()
        {
            if (_reader != null)
            {
                try
                {
                    _reader.disconnect();
                }
                catch (Exception ex)
                {
                    log.Error($"Error disconnecting reader during Dispose : {ex.Message}");
                }
            }
            _reader = null;
            GC.SuppressFinalize(this);
        }
    }
}