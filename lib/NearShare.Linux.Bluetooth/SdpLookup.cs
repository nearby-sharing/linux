using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NearShare.Linux.Bluetooth.ServiceDiscovery;

namespace NearShare.Linux.Bluetooth;

public static partial class SdpLookup
{
    const uint RFCOMM_UUID = 0x0003;
    public static unsafe int GetRfcommChannel(BluetoothAddress address, Guid serviceId)
    {
        int port_num = 0;
        foreach (SdpRecord* record in ExecuteSdpQuery(address, serviceId))
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

    static unsafe SdpList ExecuteSdpQuery(BluetoothAddress address, Guid serviceId)
    {
        using var session = Connect(BluetoothAddress.Any, address, SdpConnectFlags.RETRY_IF_BUSY);

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

    [LibraryImport(UnixConstants.LibBluetooth, EntryPoint = "sdp_connect")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial SdpSession Connect(in BluetoothAddress src, in BluetoothAddress dst, SdpConnectFlags flags);

    [LibraryImport(UnixConstants.LibBluetooth, EntryPoint = "sdp_close")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int Close(SdpSession session);

    [LibraryImport(UnixConstants.LibBluetooth, EntryPoint = "sdp_get_proto_port")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
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
            if (Close(this) != 0)
                throw new Win32Exception(Marshal.GetLastSystemError());
        }

        public static bool IsNull(SdpSession session)
            => session._handle <= 0;
    }

    [LibraryImport(UnixConstants.LibBluetooth, EntryPoint = "sdp_service_search_attr_req")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
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

    [LibraryImport(UnixConstants.LibBluetooth, EntryPoint = "sdp_get_access_protos")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int GetAccessProtocols(SdpRecord* record, out SdpList protocols);

    [LibraryImport(UnixConstants.LibBluetooth, EntryPoint = "sdp_record_free")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial void Free(SdpRecord* record);
}