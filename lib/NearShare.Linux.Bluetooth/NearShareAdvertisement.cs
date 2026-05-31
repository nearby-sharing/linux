using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using System.Runtime.Versioning;
using NearShare.Linux.Bluetooth.DBus;
using Tmds.DBus.Protocol;

namespace NearShare.Linux.Bluetooth;

[SupportedOSPlatform("linux")]
internal sealed class NearShareAdvertisement(DBusConnection connection) :
    DBusHandler(connection, ObjectPath, handlesChildPaths: false),
    ILEAdvertisement1, ILEAdvertisement1Properties
{
    public const string ObjectPath = "/de/shortdev/nearshare/advertisement0";

    public static NearShareAdvertisement Create(DBusConnection connection, AdvertiseOptions options)
    {
        return new(connection)
        {
            LocalName = options.BeaconData.DeviceName,
            Type = "peripheral",
            ManufacturerData = new()
            {
                { (ushort)options.ManufacturerId, VariantValue.Array(options.BeaconData.ToArray()) }
            }
        };
    }

    ValueTask ILEAdvertisement1.HandleGetPropertyAsync(ILEAdvertisement1.GetPropertyContext context)
        => context.Handle(this);

    ValueTask ILEAdvertisement1.HandleGetAllPropertiesAsync(ILEAdvertisement1.GetAllPropertiesContext context)
        => context.Handle(this);

    ValueTask ILEAdvertisement1.ReleaseAsync()
        => ValueTask.CompletedTask;

    public required string Type { get; init; }
    public string[] ServiceUUIDs { get; init; } = [];
    public Dictionary<ushort, VariantValue> ManufacturerData { get; init; } = [];
    public string[] SolicitUUIDs { get; init; } = [];
    public Dictionary<string, VariantValue> ServiceData { get; init; } = [];
    public string[] Includes { get; init; } = [];
    public required string LocalName { get; init; }
    public ushort Appearance { get; init; }
    public ushort Duration { get; init; }
    public ushort Timeout { get; init; }
}