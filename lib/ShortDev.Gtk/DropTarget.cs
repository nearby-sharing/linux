using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct DropTarget : IEventController
{
    nint IObject.Handle { get; }

    public event DropTargetDropHandler Drop
    {
        add => this.AddEvent("drop", Marshal.GetFunctionPointerForDelegate(value), 0, 0);
        remove => throw new NotImplementedException();
    }

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_drop_target_new")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial DropTarget Create(GType type, DragAction actions);
}

[return: MarshalAs(UnmanagedType.Bool)]
public delegate bool DropTargetDropHandler(DropTarget target, nint value, double x, double y, nint userData);

[Flags]
public enum DragAction
{
    Copy = 1,
    Move = 2,
    Link = 4,
    Ask = 8
}
