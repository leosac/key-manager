using Leosac.KeyManager.Library.Common;

namespace Leosac.KeyManager.Library.Device
{
    public interface ICardDevice
    {
        string DeviceName { get; }
        string? DeviceUid { get; }

        Task InitializeReaderAsync(Action<string, LogLevel> log, Action<string, LogLevel>? notify);
        Task<bool> WaitForCardInsertionAsync(TimeSpan timeout, bool useTimeout, CancellationToken cancellationToken, ManualResetEventSlim _pauseEvent,
            string? message = null, Action<string, LogLevel>? batchLog = null, Action<string, LogLevel>? log = null, Action? logChanged = null);
        Task<bool> PrepareForBatchAsync(CancellationToken token, int batchIndex = 0, string? message = null, Action<string, LogLevel>? batchLog = null, Action<string, LogLevel>? log = null, Action<string, LogLevel>? notify = null);
        Task WaitForRemovalAsync(CancellationToken token);
        Task DisconnectAsync(CancellationToken token);
        Task EnsureCardInserted(CancellationToken token = default);
        object? GetReader();

    }
}
