using System.Net.NetworkInformation;
using NearShare.Linux.WiFiDirect.DBus;
using Tmds.DBus.Protocol;

namespace NearShare.Linux.WiFiDirect;

public sealed class WiFiDirectHelper(DBusConnection connection)
{
    public DBusConnection Connection { get; } = connection;

    internal wpa_supplicant1 WpaSupplicant { get; } =
        new(connection, "fi.w1.wpa_supplicant1", "/fi/w1/wpa_supplicant1");

    public async Task<WiFiDirectSession> Connect(string ssid, PhysicalAddress macAddress, ReadOnlyMemory<byte> psk)
    {
        var defaultInterfacePath = (await WpaSupplicant.GetInterfacesAsync()).FirstOrDefault();
        if (defaultInterfacePath == null)
            throw new InvalidOperationException("No interfaces found in wpa_supplicant");

        P2PDevice p2pDevice = new(Connection, "fi.w1.wpa_supplicant1", defaultInterfacePath);

        TaskCompletionSource<ObjectPath> promise = new();
        using var groupObserver = await p2pDevice.WatchGroupStartedAsync(async (properties) =>
        {
            if (!properties.TryGetValue("interface_object", out var p2pInterfacePathVariant))
                return;

            _ = promise.TrySetResult(p2pInterfacePathVariant.GetObjectPath());
        });

        await p2pDevice.GroupAddAsync(new()
        {
            { "role", "client" },
            { "persistent", false }, // ToDo: Windows is settings this to true
        });

        var p2pInterfacePath = await promise.Task;
        Interface p2pInterface = new(Connection, "fi.w1.wpa_supplicant1", p2pInterfacePath);
        var network = await p2pInterface.AddNetworkAsync(new()
        {
            { "ssid", ssid },
            { "key_mgmt", "WPA-PSK" },
            { "psk", VariantValue.Array(psk.ToArray()) },
            { "mode", 1 },
        });
        await p2pInterface.SelectNetworkAsync(network);
        return new(this, p2pDevice, p2pInterface, network);
    }

    public static async Task<WiFiDirectHelper> Create()
    {
        DBusConnection connection = new(DBusAddress.System!);
        await connection.ConnectAsync();
        return new(connection);
    }
}