using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct ClipBoard
{
    readonly nint _handle;

    public async Task<string?> ReadTextAsync()
    {
        AsyncHelper promise = new();
        BeginReadText(this, cancellable: 0, promise.Complete, userData: 0);
        var (result, _) = await promise;
        unsafe
        {
            var ptr = EndReadText(this, result, out var error);
            try
            {
                return Utf8StringMarshaller.ConvertToManaged(ptr);
            }
            finally
            {
                Extensions.Free((nint)ptr);
            }
        }
    }

    [LibraryImport(Libs.Gtk, EntryPoint = "gdk_clipboard_read_text_async")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void BeginReadText(ClipBoard clipboard, nint cancellable, AsyncHelper.Callback readyCallback, nint userData);

    [LibraryImport(Libs.Gtk, EntryPoint = "gdk_clipboard_read_text_finish")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte* EndReadText(ClipBoard clipboard, AsyncResult result, out Error error);
}
