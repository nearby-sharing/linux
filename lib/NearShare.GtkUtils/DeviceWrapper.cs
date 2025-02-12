using GObject;
using ShortDev.Microsoft.ConnectedDevices;

namespace NearShare.GtkUtils;

[Subclass<GObject.Object>]
public sealed partial class DeviceWrapper
{
    public CdpDevice? Device { get; }
    public DeviceWrapper(CdpDevice device) : this()
        => Device = device;
}
