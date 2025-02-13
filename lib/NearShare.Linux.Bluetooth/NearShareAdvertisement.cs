using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using System.Runtime.CompilerServices;
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
        WriteableBuffer beaconData = new(options.BeaconData.ToArray());
        return new()
        {
            LocalName = options.BeaconData.DeviceName,
            Type = "peripheral",
            ManufacturerData = new()
            {
                { (ushort)options.ManufacturerId, beaconData.AsVariant() }
            }
        };
    }

    readonly struct WriteableBuffer(ReadOnlyMemory<byte> buffer) : IDBusWritable
    {
        readonly ReadOnlyMemory<byte> _buffer = buffer;

        public void WriteTo(ref MessageWriter writer)
            => writer.WriteArray(_buffer.Span);

        public Variant AsVariant()
            => CreateVariant("ay"u8, this);

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        extern static Variant CreateVariant(ReadOnlySpan<byte> signature, IDBusWritable value);
    }
}
