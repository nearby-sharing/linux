using System.Runtime.InteropServices;

namespace NearShare.Linux.Bluetooth.ServiceDiscovery;

[StructLayout(LayoutKind.Sequential)]
unsafe struct SdpData
{
    public SdpType type;
    public ushort attributeId;
    public SdpDataValue value;
    public SdpData* next;
    public int unitSize;
}

[StructLayout(LayoutKind.Explicit)]
unsafe struct SdpDataValue
{
    [FieldOffset(0)]
    public sbyte int8;
    [FieldOffset(0)]
    public short int16;
    [FieldOffset(0)]
    public int int32;
    [FieldOffset(0)]
    public long int64;
    [FieldOffset(0)]
    public Int128 int128;
    [FieldOffset(0)]
    public byte uint8;
    [FieldOffset(0)]
    public ushort uint16;
    [FieldOffset(0)]
    public uint uint32;
    [FieldOffset(0)]
    public ulong uint64;
    [FieldOffset(0)]
    public UInt128 uint128;
    [FieldOffset(0)]
    public Uuid uuid;
    [FieldOffset(0)]
    public byte* str;
    [FieldOffset(0)]
    public SdpData* dataseq;
}
