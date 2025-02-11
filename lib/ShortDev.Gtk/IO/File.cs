using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace ShortDev.Gtk.IO;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct File : IDisposable
{
    readonly nint _handle;

    public unsafe string? Path
    {
        get
        {
            var ptr = GetPath(this);
            try
            {
                return Utf8StringMarshaller.ConvertToManaged(ptr);
            }
            finally
            {
                Extensions.Free((nint)ptr);
            }
        }
    }

    [LibraryImport(Libs.Gtk, EntryPoint = "g_file_get_path")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial byte* GetPath(File file);

    public void Dispose()
    {
        Extensions.Free(_handle);
    }
}
