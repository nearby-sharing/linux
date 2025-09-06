using ShortDev.Microsoft.ConnectedDevices;

namespace NearShare.GtkUtils;

public sealed partial class DeviceWrapper
{
    public CdpDevice? Device { get; }

    public DeviceWrapper(CdpDevice device) : this()
    {
        Device = device;
    }

    public string DeviceName => Device?.Name ?? string.Empty;
    public string DeviceTypeIconName => "computer";
    public string TransportTypeIconName => "bluetooth-active";
}