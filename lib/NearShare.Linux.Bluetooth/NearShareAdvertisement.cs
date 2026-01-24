using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using System.Runtime.Versioning;
using Tmds.DBus.Protocol;

namespace NearShare.Linux.Bluetooth;

[SupportedOSPlatform("linux")]
internal sealed class NearShareAdvertisement : ILEAdvertisement1
{
    public const string ObjectPath = "/de/shortdev/nearshare/advertisement0";

    public override Connection Connection => throw new NotImplementedException();
    public string Path { get; } = ObjectPath;

    protected override ValueTask OnReleaseAsync(Message request)
        => ValueTask.CompletedTask;

    public static NearShareAdvertisement Create(AdvertiseOptions options)
    {
        return new()
        {
            LocalName = options.BeaconData.DeviceName,
            Type = "peripheral",
            ManufacturerData = new()
            {
                { (ushort)options.ManufacturerId, VariantValue.Array(options.BeaconData.ToArray()) }
            }
        };
    }
}