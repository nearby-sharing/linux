using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct Button : IWidget
{
    nint IObject.Handle { get; }

    public event ClickEventHandler Clicked
    {
        add => this.AddEvent("clicked", Marshal.GetFunctionPointerForDelegate(value), IntPtr.Zero, 0);
        remove => throw new NotImplementedException();
    }
}

public delegate void ClickEventHandler(Button button, nint userData);
