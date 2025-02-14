using GLib.Internal;

namespace NearShare;

public sealed class GtkSynchronizationContext : SynchronizationContext
{
    readonly MainLoopSynchronizationContext _innerContext = new();

    public override SynchronizationContext CreateCopy()
        => new GtkSynchronizationContext();

    public override void Post(SendOrPostCallback d, object? state)
        => _innerContext.Post(d, state);

    public override void Send(SendOrPostCallback d, object? state)
    {
        // ToDo: This might deadlock!
        // https://github.com/gircore/gir.core/pull/915
        _innerContext.Post(d, state);
        using var resetEvent = new ManualResetEventSlim(false);

        _innerContext.Post(state =>
        {
            d(state);
            resetEvent.Set();
        }, state);

        resetEvent.Wait();
    }
}
