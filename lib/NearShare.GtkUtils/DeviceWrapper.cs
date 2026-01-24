using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Transports;

namespace NearShare.GtkUtils;

public sealed partial class DeviceWrapper
{
    public CdpDevice? Device { get; }

    public DeviceWrapper(CdpDevice device) : this()
    {
        Device = device;
    }

    public string DeviceName => Device?.Name ?? string.Empty;
    public string DeviceTypeIconName => Device?.Type.IsMobile() == true ? "phone" : "computer";

    public string TransportTypeIconName => Device?.Endpoint.TransportType switch
    {
        CdpTransportType.Tcp or CdpTransportType.Udp => "network-wireless",
        _ => "bluetooth-active"
    };
}