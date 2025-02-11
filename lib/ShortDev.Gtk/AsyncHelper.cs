using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

internal readonly struct AsyncHelper
{
    public Callback Complete { get; }

    readonly TaskCompletionSource<(AsyncResult result, nint userData)> promise = new();
    public AsyncHelper()
    {
        Complete = Unsafe.BitCast<nint, Callback>(Marshal.GetFunctionPointerForDelegate<OnCompletedDelegate>(OnCompleted));
    }

    public TaskAwaiter<(AsyncResult result, nint userData)> GetAwaiter()
        => promise.Task.GetAwaiter();

    void OnCompleted(nint sourceObject, AsyncResult result, nint userData)
    {
        try
        {
            promise.TrySetResult((result, userData));
        }
        catch (Exception ex)
        {
            promise.TrySetException(ex);
        }
    }

    delegate void OnCompletedDelegate(nint sourceObject, AsyncResult result, nint userData);

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Callback
    {
        readonly nint _callbackPtr;
    }
}
