using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using NearShare.Linux.Bluetooth.DBus;
using Tmds.DBus.Protocol;

namespace NearShare.Linux.Bluetooth;

[SupportedOSPlatform("linux")]
internal sealed class RfcommProfile(DBusConnection connection, string uuid, string path) :
    DBusHandler(connection, path, handlesChildPaths: false),
    IProfile1Handler
{
    public string Uuid { get; } = uuid;

    readonly TaskCompletionSource<Stream> _promise = new();
    public Task<Stream> ConnectionTask => _promise.Task;

    public ValueTask NewConnectionAsync(ObjectPath a0, SafeHandle fd, Dictionary<string, VariantValue> properties)
    {
        try
        {
            BluetoothStream stream = new(fd);
            _promise.TrySetResult(stream);
        }
        catch (Exception ex)
        {
            _promise.SetException(ex);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask RequestDisconnectionAsync(ObjectPath arg0)
    {
        // ToDo
        Console.WriteLine($"Request disconnect for {arg0}");
        return ValueTask.CompletedTask;
    }

    public ValueTask ReleaseAsync()
        => ValueTask.CompletedTask;
}