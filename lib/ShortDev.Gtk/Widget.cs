using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Widget : IWidget
{
    nint IObject.Handle { get; }
}
