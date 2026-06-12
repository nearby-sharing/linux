using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using NearShare.Linux.WiFiDirect.DBus;
using ShortDev.Microsoft.ConnectedDevices;
using Tmds.DBus.Protocol;

namespace NearShare.Linux.WiFiDirect;

public sealed class WiFiDirectHelper
{
    internal wpa_supplicant1 WpaSupplicant { get; }
    internal P2PDevice P2PDevice { get; }
    internal PhysicalAddress MacAddress { get; }

    private WiFiDirectHelper(wpa_supplicant1 supplicant, P2PDevice p2pDevice, PhysicalAddress macAddress)
    {
        WpaSupplicant = supplicant;
        P2PDevice = p2pDevice;
        MacAddress = macAddress;
    }

    public async Task<IPAddress> Connect(PhysicalAddress macAddress, string ssid, ReadOnlyMemory<byte> psk,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(macAddress.ToStringFormatted());
        Console.WriteLine(Convert.ToHexString(Encoding.ASCII.GetBytes(ssid)));
        Console.WriteLine(Convert.ToHexString(psk.ToArray()));

        TaskCompletionSource<IPAddress> promise = new();
        using var groupObserver = await P2PDevice.WatchGroupStartedAsync((properties) =>
        {
            if (!properties.TryGetValue("IpAddrGo", out var ipGoVariant))
                return;

            _ = promise.TrySetResult(new IPAddress(ipGoVariant.GetArray<byte>()));
        });

        using var failureObserver = await P2PDevice.WatchGroupFormationFailureAsync(async reason =>
        {
            _ = promise.TrySetException(new InvalidOperationException(reason));
        });

        var groupPath = await P2PDevice.AddPersistentGroupAsync(new()
        {
            { "go_p2p_dev_addr", macAddress.ToStringFormatted() },
            { "bssid", macAddress.ToStringFormatted() },
            { "ssid", ssid },
            { "psk", VariantValue.Array(psk.ToArray()) },
            { "mode", 0 }, // GO = 3, Client = 0
            { "key_mgmt", "WPA-PSK" },
            { "proto", "RSN" },
            { "pairwise", "GCMP CCMP" },
            { "group", "GCMP CCMP" },
        });

        Console.WriteLine(groupPath.ToString());

        try
        {
            await P2PDevice.GroupAddAsync(new()
            {
                // { "peer", peer },
                { "persistent", true },
                { "persistent_group_object", groupPath },
            });
        }
        catch
        {
            await P2PDevice.RemovePersistentGroupAsync(groupPath);
            throw;
        }

        return await promise.Task;
    }

    public static async Task<WiFiDirectHelper> CreateAsync()
    {
        DBusConnection connection = new(DBusAddress.System!);
        await connection.ConnectAsync();

        wpa_supplicant1 supplicant = new(connection, "fi.w1.wpa_supplicant1", "/fi/w1/wpa_supplicant1");
        var caps = await supplicant.GetCapabilitiesAsync();
        if (!caps.Contains("p2p"))
            throw new InvalidOperationException("p2p is not supported");

        var allInterfaces = await supplicant.GetInterfacesAsync();
        var defaultInterfacePath = allInterfaces.FirstOrDefault();
        if (defaultInterfacePath == null)
            throw new InvalidOperationException("No interfaces found in wpa_supplicant");

        Interface inter = new(connection, "fi.w1.wpa_supplicant1", defaultInterfacePath);
        var macAddress = await inter.GetMACAddressAsync();
        Console.WriteLine(new PhysicalAddress(macAddress).ToStringFormatted());

        P2PDevice p2pDevice = new(connection, "fi.w1.wpa_supplicant1", defaultInterfacePath);

        return new(supplicant, p2pDevice, PhysicalAddress.Parse("b8:9a:2a:25:79:b2"));
    }
}