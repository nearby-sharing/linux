using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Error
{
    readonly int _value;
}
