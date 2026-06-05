using GObject;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Transports;

namespace NearShare.ViewModels;

[Subclass<GObject.Object>]
public sealed partial class DeviceWrapper
{
    public CdpDevice? Device { get; private set; }

    public static DeviceWrapper Create(CdpDevice device)
    {
        var result = NewWithProperties([]);
        result.Device = device;
        return result;
    }

    public string DeviceName => Device?.Name ?? string.Empty;
    public string DeviceTypeIconName => Device?.Type.IsMobile() == true ? "phone" : "computer";

    public string TransportTypeIconName => Device?.Endpoint.TransportType switch
    {
        CdpTransportType.Tcp or CdpTransportType.Udp => "network-wireless",
        _ => "bluetooth-active"
    };
}