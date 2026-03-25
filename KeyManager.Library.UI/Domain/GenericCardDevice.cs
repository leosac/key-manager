using Leosac.KeyManager.Library.KeyStore;
using Leosac.KeyManager.Library.UI.Domain;
using LibLogicalAccess;

public class GenericCardDevice : ICardDevice, IDisposable
{
    private readonly KeyStore _store;
    public string DeviceName => _store.Name;
    public string? DeviceUid => _store.Attributes.ContainsKey("uid") ? _store.Attributes["uid"] : null;

    public bool IsSAMDevice => false;

    public GenericCardDevice(KeyStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task InitializeReaderAsync(Action<string, LogLevel> log, Action<string, LogLevel>? notify)
    {
        log?.Invoke("Using reader : None (generic device)", LogLevel.Info);
        await Task.CompletedTask;
    }

    public Task<bool> WaitForCardInsertionAsync(TimeSpan timeout, bool useTimeout, CancellationToken cancellationToken, ManualResetEventSlim pauseEvent,
            LogEntry? batchLog = null, Action<string, LogLevel>? log = null, Action? logChanged = null)
    {
        log?.Invoke("Generic device, no card insertion required.", LogLevel.Info);
        return Task.FromResult(true);
    }

    public Task<bool> PrepareForBatchAsync(CancellationToken token, LogEntry? batchLog = null, int batchIndex = 0, Action<LogEntry>? log = null, Action<string, LogLevel>? notify = null)
    {
        return Task.FromResult(true);
    }

    public Task DisconnectAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task EnsureCardInserted()
    {
        return Task.CompletedTask;
    }

    public ReaderUnit? GetReader()
    {
        return null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

}