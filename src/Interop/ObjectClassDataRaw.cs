using System.Runtime.InteropServices;

namespace NearShare.Interop;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ObjectClassDataRaw
{
    public GObject.Internal.TypeClassData GTypeClass;
    public IntPtr ConstructProperties;
    public void* Constructor;

    public delegate* unmanaged[Cdecl]<nint, uint, nint, nint, void> SetProperty;

    public delegate* unmanaged[Cdecl]<nint, uint, nint, nint, void> GetProperty;
}
