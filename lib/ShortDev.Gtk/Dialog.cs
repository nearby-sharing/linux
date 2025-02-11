using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct Dialog
{
    readonly nint _handle;

    public void Present(IWidget parent)
        => Present(this, parent.Handle);

    [LibraryImport(Libs.Adwaita, EntryPoint = "adw_dialog_present")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Present(Dialog dialog, nint parent);
}
