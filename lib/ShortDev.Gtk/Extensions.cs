using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

public static partial class Extensions
{
    [LibraryImport(Libs.Gtk, EntryPoint = "g_object_unref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Unref(nint handle);

    public static void Unref(this IObject obj)
        => Unref(obj.Handle);

    [LibraryImport(Libs.Gtk, EntryPoint = "g_free")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Free(nint ptr);

    [LibraryImport(Libs.Gtk, EntryPoint = "g_signal_connect_object")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial long AddEvent(nint handle, [MarshalAs(UnmanagedType.LPStr)] string eventName, nint callback, nint obj, int connectFlags);

    public static long AddEvent(this IObject widget, string eventName, nint callback, nint obj, int connectFlags)
        => AddEvent(widget.Handle, eventName, callback, obj, connectFlags);

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_widget_get_clipboard")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial ClipBoard GetClipBoard(nint handle);

    public static ClipBoard GetClipBoard(this IWidget widget)
        => GetClipBoard(widget.Handle);

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_widget_add_controller")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial void AddController(nint widget, nint controller);

    public static void AddController(this IWidget widget, IEventController controller)
        => AddController(widget.Handle, controller.Handle);
}
