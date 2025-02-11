using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct Window
{
    readonly nint _handle;

    public nint Content
    {
        get => GetContent(this);
        set => SetContent(this, value);
    }

    public void Present()
        => Activate(this);

    [LibraryImport(Libs.Adwaita, EntryPoint = "adw_window_set_content")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void SetContent(Window window, nint widget);

    [LibraryImport(Libs.Adwaita, EntryPoint = "adw_window_get_content")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial nint GetContent(Window window);

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_window_present")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Activate(Window window);
}
