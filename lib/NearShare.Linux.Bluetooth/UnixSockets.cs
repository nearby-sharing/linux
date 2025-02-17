using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NearShare.Linux.Bluetooth;

static partial class UnixSockets
{
    public static unsafe Socket ConnectRfcomm(BluetoothAddress address, byte channel)
    {
        var handle = Create(UnixAddressFamily.Bluetooth, UnixSocketType.Stream, UnixSocketProtocol.Rfcomm);

        RfcommSocketAddress rfcommAddress = new()
        {
            family = UnixAddressFamily.Bluetooth,
            address = address,
            channel = channel
        };
        var result = ConnectRfcomm(handle, rfcommAddress, sizeof(RfcommSocketAddress));
        if (result != 0)
            throw new Win32Exception(Marshal.GetLastSystemError());

        Socket socket = new(handle);
        IsConnected(socket) = true;
        return socket;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_isConnected")]
    static extern ref bool IsConnected(Socket socket);

    enum UnixAddressFamily : ushort
    {
        Bluetooth = 31
    }

    enum UnixSocketType
    {
        Stream = 1
    }

    enum UnixSocketProtocol
    {
        Rfcomm = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RfcommSocketAddress
    {
        public UnixAddressFamily family;
        public BluetoothAddress address;
        public byte channel;
    }

    [LibraryImport("libc", EntryPoint = "socket")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial SafeSocketHandle Create(UnixAddressFamily domain, UnixSocketType type, UnixSocketProtocol protocol);

    [LibraryImport("libc", EntryPoint = "connect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial int ConnectRfcomm(SafeSocketHandle socket, in RfcommSocketAddress address, int addressSize);
}
