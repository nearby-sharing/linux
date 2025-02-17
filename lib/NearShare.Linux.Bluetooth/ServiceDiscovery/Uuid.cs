using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NearShare.Linux.Bluetooth.ServiceDiscovery;

// https://github.com/bluez/bluez/blob/2ee08ffd4d469781dc627fa50b4a015d9ad68007/lib/sdp.h#L431-L438
[StructLayout(LayoutKind.Sequential)]
partial struct Uuid
{
    public SdpType type;
    public UuidValue value;

    public unsafe int AsProtocol()
    {
        fixed (Uuid* pThis = &this)
        {
            return UuidToProtocol(pThis);
        }
    }

    [LibraryImport("libbluetooth.so", EntryPoint = "sdp_uuid_to_proto")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int UuidToProtocol(Uuid* record);
}

[StructLayout(LayoutKind.Explicit)]
struct UuidValue
{
    [FieldOffset(0)]
    public ushort uuid16;

    [FieldOffset(0)]
    public uint uuid32;

    [FieldOffset(0)]
    public Guid guid;

    [FieldOffset(0)]
    public Uuid128 uuid128;
}

[StructLayout(LayoutKind.Sequential)]
struct Uuid128
{
    public int a;
    public short b;
    public short c;
    public byte d;
    public byte e;
    public byte f;
    public byte g;
    public byte h;
    public byte i;
    public byte j;
    public byte k;
}
