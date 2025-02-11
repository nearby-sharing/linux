using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct ActionRow : IWidget
{
    nint IObject.Handle { get; }

    public event ActionRowActivatedHandler Activated
    {
        add => this.AddEvent("activated", Marshal.GetFunctionPointerForDelegate(value), IntPtr.Zero, 0);
        remove => throw new NotImplementedException();
    }
}

public delegate void ActionRowActivatedHandler(ActionRow button, nint userData);
