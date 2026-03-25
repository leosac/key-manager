using LibLogicalAccess;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public interface ICardDevice
    {
        string DeviceName { get; }
        string? DeviceUid { get; }

        Task InitializeReaderAsync(Action<string, LogLevel> log, Action<string, LogLevel>? notify);
        Task<bool> WaitForCardInsertionAsync(TimeSpan timeout, bool useTimeout, CancellationToken cancellationToken, ManualResetEventSlim pauseEvent,
            LogEntry? batchLog = null, Action<string, LogLevel>? log = null, Action? logChanged = null);
        Task<bool> PrepareForBatchAsync(CancellationToken token, LogEntry? batchLog = null, int batchIndex = 0, Action<LogEntry>? log = null, Action<string, LogLevel>? notify = null);
        Task DisconnectAsync(CancellationToken token);
        Task EnsureCardInserted();
        ReaderUnit? GetReader();

        bool IsSAMDevice { get; }
    }
}
