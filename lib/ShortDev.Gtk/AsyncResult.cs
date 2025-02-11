using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly struct AsyncResult
{
    readonly nint _handle;
}
