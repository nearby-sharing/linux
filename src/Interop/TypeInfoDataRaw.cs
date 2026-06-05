using System.Runtime.InteropServices;

namespace NearShare.Interop;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct TypeInfoDataRaw
{
    public ushort ClassSize;
    public delegate* unmanaged[Cdecl]<nint, void> BaseInit;
    public delegate* unmanaged[Cdecl]<nint, void> BaseFinalize;
    public delegate* unmanaged[Cdecl]<nint, nint, void> ClassInit;
    public delegate* unmanaged[Cdecl]<nint, nint, void> ClassFinalize;
    public IntPtr ClassData;
    public ushort InstanceSize;
    public ushort NPreallocs;
    public delegate* unmanaged[Cdecl]<nint, nint, void> InstanceInit;
    public IntPtr ValueTable;
}
