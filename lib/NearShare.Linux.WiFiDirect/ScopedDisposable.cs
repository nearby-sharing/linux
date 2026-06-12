namespace NearShare.Linux.WiFiDirect;

internal sealed class ScopedDisposable<T>(T value) : IDisposable
    where T : class, IDisposable
{
    private T? _value = value;

    public T Detach()
        => Interlocked.Exchange(ref _value, null) ?? throw new InvalidOperationException("Already detached");

    public void Dispose()
        => Interlocked.Exchange(ref _value, null)?.Dispose();

    public static implicit operator ScopedDisposable<T>(T value)
        => new(value);
}