// https://github.com/blueman-project/blueman/blob/64f9cfb5201232b58d109387285d59c27ee434f8/module/libblueman.c#L355
// https://github.com/bluez/bluez/blob/master/lib/sdp_lib.h
// https://people.csail.mit.edu/rudolph/Teaching/Articles/BTBook.pdf

using System.Buffers.Binary;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NearShare.Linux.Bluetooth.ServiceDiscovery;

namespace NearShare.Linux.Bluetooth;

public static partial class SdpLookup
{
    const uint RFCOMM_UUID = 0x0003;
    public static unsafe int GetRfcommChannel(PhysicalAddress macAddress, Guid serviceId)
    {
        byte[] addressBytes = macAddress.GetAddressBytes(); // Big endian
        addressBytes.AsSpan().Reverse();

        int port_num = 0;
        foreach (SdpRecord* record in ExecuteSdpQuery(addressBytes, serviceId))
        {
            try
            {
                // get a list of the protocol sequences
                if (GetAccessProtocols(record, out var protocolList) != 0)
                    continue;

                using (protocolList)
                {
                    port_num = GetProtocolPort(protocolList, RFCOMM_UUID);
                    Console.WriteLine($"Port {port_num}");
                }
            }
            finally
            {
                Free(record);
            }
        }
        return port_num;
    }

    static unsafe SdpList ExecuteSdpQuery(ReadOnlySpan<byte> address, Guid serviceId)
    {
        BluetoothAddress target = BluetoothAddress.FromSpan(address);
        using var session = Connect(BluetoothAddress.Any, target, SdpConnectFlags.RETRY_IF_BUSY);

        if (SdpSession.IsNull(session))
            throw new InvalidOperationException($"Could not connect to {address.ToString()} via sdp");

        Uuid serviceUuid = new()
        {
            type = SdpType.UUID128,
            value = new()
            {
                guid = serviceId
            }
        };

        // Convert to big endian
        serviceUuid.value.uuid128.a = BinaryPrimitives.ReverseEndianness(serviceUuid.value.uuid128.a);
        serviceUuid.value.uuid128.b = BinaryPrimitives.ReverseEndianness(serviceUuid.value.uuid128.b);
        serviceUuid.value.uuid128.c = BinaryPrimitives.ReverseEndianness(serviceUuid.value.uuid128.c);

        using var searchList = SdpList.Create(&serviceUuid);

        // Request all attributes
        uint range = 0x0000_ffff;
        using var attributeList = SdpList.Create(&range);

        if (0 != ServiceSearchAttributeRequest(session, searchList, AttributeRequestType.Range, attributeList, out var results))
        {
            results.Dispose();
            throw new InvalidOperationException($"Failed to search for {serviceId} in {Convert.ToHexString(address)} sdp attributes");
        }

        return results;
    }

    [LibraryImport("libbluetooth.so", EntryPoint = "sdp_connect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial SdpSession Connect(in BluetoothAddress src, in BluetoothAddress dst, SdpConnectFlags flags);

    [LibraryImport("libbluetooth.so", EntryPoint = "sdp_close")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int Close(SdpSession session);

    [LibraryImport("libbluetooth.so", EntryPoint = "sdp_get_proto_port")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int GetProtocolPort(SdpList list, uint protocol);

    enum SdpConnectFlags : uint
    {
        RETRY_IF_BUSY = 0x01,
        WAIT_ON_CLOSE = 0x02,
        NON_BLOCKING = 0x04,
        LARGE_MTU = 0x08,
    }

    [StructLayout(LayoutKind.Sequential)]
    readonly struct SdpSession : IDisposable
    {
        readonly nint _handle;

        public void Dispose()
        {
            Close(this);
        }

        public static bool IsNull(SdpSession session)
            => session._handle <= 0;
    }

    [InlineArray(6)]
    struct BluetoothAddress
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
    }

    [LibraryImport("libbluetooth.so", EntryPoint = "sdp_service_search_attr_req")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int ServiceSearchAttributeRequest(SdpSession session, SdpList search, AttributeRequestType type, SdpList attributes, out SdpList services);

    enum AttributeRequestType
    {
        Individual = 1,
        Range
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SdpRecord
    {
        public uint handle;
        public SdpList pattern;
        public SdpList attributes;
        public Uuid serviceClass;
    }

    [LibraryImport("libbluetooth.so", EntryPoint = "sdp_get_access_protos")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int GetAccessProtocols(SdpRecord* record, out SdpList protocols);

    [LibraryImport("libbluetooth.so", EntryPoint = "sdp_record_free")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial void Free(SdpRecord* record);
}