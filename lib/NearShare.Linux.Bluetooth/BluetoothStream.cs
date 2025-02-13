using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace NearShare.Linux.Bluetooth;

[SupportedOSPlatform("linux")]
internal sealed partial class BluetoothStream(SafeFileHandle handle) : Stream
{
    readonly SafeFileHandle _handle = handle;

    [LibraryImport("libc", EntryPoint = "read", SetLastError = true)]
    private static partial int Receive(SafeFileHandle handle, ref byte buffer, int length);

    public override bool CanRead { get; } = true;
    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();

    public override int Read(Span<byte> buffer)
    {
        var receivedBytes = Receive(_handle, ref MemoryMarshal.GetReference(buffer), buffer.Length);
        if (receivedBytes == -1)
            throw new Win32Exception();

        return receivedBytes;
    }

    [LibraryImport("libc", EntryPoint = "send", SetLastError = true)]
    private static partial int Send(SafeFileHandle handle, ref byte buffer, nuint length, int flags = 0);

    public override bool CanWrite { get; } = true;
    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var sentBytes = Send(_handle, ref MemoryMarshal.GetReference(buffer), (nuint)buffer.Length);
        if (sentBytes == -1)
            throw new Win32Exception();

        if (sentBytes != buffer.Length)
            throw new InvalidOperationException("Did not send all bytes");
    }

    public override void Flush() { }

    #region Seek
    public override bool CanSeek { get; } = false;
    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotImplementedException();

    public override long Length => throw new NotImplementedException();
    public override void SetLength(long value)
        => throw new NotImplementedException();

    public override long Position
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
    #endregion
}
