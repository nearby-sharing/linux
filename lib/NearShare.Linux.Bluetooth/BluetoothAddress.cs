using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace NearShare.Linux.Bluetooth;

[InlineArray(6)]
public struct BluetoothAddress
{
    byte _element0;

    public static BluetoothAddress Any { get; } = default;
    public static BluetoothAddress All { get; } = FromSpan([0xff, 0xff, 0xff, 0xff, 0xff, 0xff]);
    public static BluetoothAddress Local { get; } = FromSpan([0, 0, 0, 0xff, 0xff, 0xff]);

    public static BluetoothAddress FromSpan(ReadOnlySpan<byte> value)
    {
        BluetoothAddress result = default;
        value.CopyTo(result);
        return result;
    }

    public static BluetoothAddress FromPhysicalAddress(PhysicalAddress value)
    {
        Span<byte> addressBytes = value.GetAddressBytes(); // Big endian
        addressBytes.Reverse();
        return FromSpan(addressBytes);
    }

    public override string ToString()
    {
        return Convert.ToHexString(this);
    }
}
