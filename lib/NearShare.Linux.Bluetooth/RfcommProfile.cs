using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace NearShare.Linux.Bluetooth;

[SupportedOSPlatform("linux")]
internal sealed class RfcommProfile(string uuid) : OrgBluezProfile1Handler
{
    public override Connection Connection => throw new NotImplementedException();
    public string Uuid { get; } = uuid;

    readonly TaskCompletionSource<Stream> _promise = new();
    public Task<Stream> ConnectionTask => _promise.Task;

    protected override ValueTask OnNewConnectionAsync(Message request, ObjectPath arg0, SafeFileHandle fd, Dictionary<string, VariantValue> properties)
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

    protected override ValueTask OnRequestDisconnectionAsync(Message request, ObjectPath arg0)
    {
        // ToDo
        Console.WriteLine($"Request disconnect for {arg0}");
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnReleaseAsync(Message request)
        => ValueTask.CompletedTask;
}
