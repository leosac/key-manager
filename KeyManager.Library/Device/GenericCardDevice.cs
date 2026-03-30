using Leosac.KeyManager.Library.Common;
using Leosac.KeyManager.Library.Device;
using Leosac.KeyManager.Library.KeyStore;

public class GenericCardDevice : ICardDevice, IDisposable
{
    public string DeviceName { get; }
    public string? DeviceUid { get; }

    public bool IsSAMDevice => false;

    public GenericCardDevice(KeyStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        DeviceName = store.Name;
        DeviceUid = store.Attributes.TryGetValue("uid", out var uid) ? uid : null;
    }

    public async Task InitializeReaderAsync(Action<string, LogLevel> log, Action<string, LogLevel>? notify)
    {
        log?.Invoke("Using reader : None (generic device)", LogLevel.Info);
        await Task.CompletedTask;
    }

    public Task<bool> WaitForCardInsertionAsync(TimeSpan timeout, bool useTimeout, CancellationToken cancellationToken, ManualResetEventSlim pauseEvent,
            string? message = null, Action<string, LogLevel>? batchLogUpdate = null, Action<string, LogLevel>? log = null, Action? logChanged = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        log?.Invoke("Generic device, no device insertion required.", LogLevel.Info);
        return Task.FromResult(true);
    }

    public Task<bool> PrepareForBatchAsync(CancellationToken token, int batchIndex = 0, string? message = null, Action<string, LogLevel>? batchLog = null, Action<string, LogLevel>? log = null, Action<string, LogLevel>? notify = null)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }

    public Task WaitForRemovalAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task EnsureCardInserted(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public object? GetReader()
    {
        return null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

}