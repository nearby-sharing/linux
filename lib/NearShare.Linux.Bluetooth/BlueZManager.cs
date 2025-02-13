using ShortDev.Microsoft.ConnectedDevices;
using Spectre.Console;
using System.Runtime.Versioning;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace NearShare.Linux.Bluetooth;

[SupportedOSPlatform("linux")]
internal sealed class BlueZManager(Connection connection)
{
    public Connection Connection { get; } = connection;
    public ILEAdvertisingManager1 AdvertisingManager { get; } = new(connection, "org.bluez", "/org/bluez/hci0");
    public IObjectManager ObjectManager { get; } = new(connection, "org.bluez", "/");
    public IProfileManager1 ProfileManager { get; } = new(connection, "org.bluez", "/org/bluez");

    public async Task<IReadOnlyList<IAdapter1>> GetAdaptersAsync()
    {
        var objects = await ObjectManager.GetManagedObjectsAsync();
        return objects.Where(x => x.Value.ContainsKey("org.bluez.Adapter1"))
            .Select(x => new IAdapter1(Connection, "org.bluez", x.Key))
            .ToList();
    }

    public async IAsyncEnumerable<IDevice1> GetDevicesAsync()
    {
        var objects = await ObjectManager.GetManagedObjectsAsync();
        foreach (var (path, interfaces) in objects)
        {
            if (!interfaces.ContainsKey("org.bluez.Device1"))
                continue;

            yield return new(Connection, "org.bluez", path);
        }
    }

    public async Task<IDevice1> GetDeviceAsync(string address)
    {
        var objects = await ObjectManager.GetManagedObjectsAsync();
        foreach (var (path, interfaces) in objects)
        {
            if (!interfaces.ContainsKey("org.bluez.Device1"))
                continue;

            IDevice1 device = new(Connection, "org.bluez", path);
            if (await device.GetAddressPropertyAsync() == address)
                return device;
        }

        throw new FileNotFoundException($"Could not find device with address {address}");
    }

    public async ValueTask AdvertiseAsync(ObjectPath path, ILEAdvertisement1 advertisement, CancellationToken cancellationToken)
    {
        PathHandler handler = new(path);
        handler.Add(advertisement);

        Connection.AddMethodHandler(handler);
        try
        {
            await AdvertisingManager.RegisterAdvertisementAsync(path, []);

            await cancellationToken.AwaitCancellation();

            await AdvertisingManager.UnregisterAdvertisementAsync(path);
        }
        finally
        {
            Connection.RemoveMethodHandler(path);
        }
    }

    public async ValueTask<Stream> CreateRfcommSocketAsync(IDevice1 device, string serviceId)
    {
        RfcommProfile profile = new(serviceId);

        PathHandler handler = new("/de/shortdev/nearshare/profile0");
        handler.Add(profile);

        Connection.AddMethodHandler(handler);
        try
        {
            await ProfileManager.RegisterProfileAsync(handler.Path, serviceId, []);
            try
            {
                await device.ConnectProfileAsync(serviceId);
                return await profile.ConnectionTask;
            }
            finally
            {
                await ProfileManager.UnregisterProfileAsync(handler.Path);
            }
        }
        finally
        {
            Connection.RemoveMethodHandler(handler.Path);
        }
    }

    public static async ValueTask<BlueZManager> CreateAsync()
    {
        Connection connection = new(Address.System!);
        await connection.ConnectAsync();
        return new(connection);
    }
}
