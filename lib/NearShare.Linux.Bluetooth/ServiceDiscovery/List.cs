using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NearShare.Linux.Bluetooth.ServiceDiscovery;

[StructLayout(LayoutKind.Sequential)]
unsafe readonly partial struct SdpList : IDisposable
{
    public readonly SdpListElement* head;

    public SdpListEnumerator GetEnumerator() => new(this);

    public void Dispose()
    {
        Free(this, 0);
    }

    public static unsafe SdpList Create(void* value)
    {
        return ListAppend(default, value);
    }

    [LibraryImport("libbluetooth.so", EntryPoint = "sdp_list_free")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial void Free(SdpList list, nint unused);

    [LibraryImport("libbluetooth.so", EntryPoint = "sdp_list_append")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial SdpList ListAppend(SdpList list, void* value);
}

[StructLayout(LayoutKind.Sequential)]
unsafe struct SdpListElement
{
    public SdpListElement* next;
    public void* data;
}

unsafe ref struct SdpListEnumerator(SdpList list)
{
    readonly SdpList _list = list;
    SdpListElement* _currentElement = list.head;

    public nint Current => (nint)_currentElement->data;

    bool _started = false;
    public bool MoveNext()
    {
        if (_currentElement == null)
            return false;

        if (!_started)
        {
            _started = true;
            return true;
        }

        if (_currentElement->next == null)
            return false;

        _currentElement = _currentElement->next;
        return true;
    }

    public void Dispose()
        => _list.Dispose();

    public void Reset()
        => _currentElement = _list.head;
}
