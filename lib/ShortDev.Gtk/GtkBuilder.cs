using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct GtkBuilder
{
    readonly nint _handle;

    public static unsafe GtkBuilder FromString([StringSyntax(StringSyntaxAttribute.Xml)] string value)
    {
        var pValue = Utf8StringMarshaller.ConvertToUnmanaged(value);
        try
        {
            return FromString(pValue, -1);
        }
        finally
        {
            Utf8StringMarshaller.Free(pValue);
        }
    }

    public T GetObject<T>(string name) where T : unmanaged
        => Unsafe.BitCast<nint, T>(GetObject(this, name));

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_builder_new_from_string")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial GtkBuilder FromString(byte* value, int length);

    [LibraryImport(Libs.Gtk, EntryPoint = "gtk_builder_get_object")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial nint GetObject(GtkBuilder builder, [MarshalAs(UnmanagedType.LPStr)] string name);
}
