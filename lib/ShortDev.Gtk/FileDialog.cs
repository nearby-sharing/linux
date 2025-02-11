using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct FileDialog
{
    readonly nint _handle;

    public async ValueTask<string?> OpenFileAsync(Window window, CancellationToken cancellation = default)
    {
        AsyncHelper promise = new();
        BeginOpenFile(this, window, cancellable: 0, promise.Complete, userData: 0);

        var (result, _) = await promise;
        using var file = EndOpenFile(this, result, out Error error);
        return file.Path;
    }

    public async ValueTask<string?> PickFolderAsnyc(Window window, CancellationToken cancellation = default)
    {
        AsyncHelper promise = new();
        BeginSelectFolder(this, window, cancellable: 0, promise.Complete, userData: 0);

        var (result, _) = await promise;
        using var file = EndSelectFolder(this, result, out Error error);
        return file.Path;
    }

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_file_dialog_new")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial FileDialog Create();

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_file_dialog_open")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void BeginOpenFile(FileDialog dialog, Window parent, nint cancellable, AsyncHelper.Callback callback, nint userData);

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_file_dialog_open_finish")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IO.File EndOpenFile(FileDialog dialog, AsyncResult result, out Error error);

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_file_dialog_select_folder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void BeginSelectFolder(FileDialog dialog, Window parent, nint cancellable, AsyncHelper.Callback callback, nint userData);

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_file_dialog_select_folder_finish")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IO.File EndSelectFolder(FileDialog dialog, AsyncResult result, out Error error);
}
