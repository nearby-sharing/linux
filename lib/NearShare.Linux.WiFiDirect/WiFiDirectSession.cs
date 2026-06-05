using NearShare.Linux.WiFiDirect.DBus;
using Tmds.DBus.Protocol;

namespace NearShare.Linux.WiFiDirect;

public sealed class WiFiDirectSession : IAsyncDisposable
{
    private readonly WiFiDirectHelper _wifiDirectHelper;
    private readonly P2PDevice _p2pDevice;
    private readonly Interface _p2pInterface;
    private readonly ObjectPath _networkPath;

    internal WiFiDirectSession(WiFiDirectHelper helper, P2PDevice p2pDevice, Interface p2pInterface,
        ObjectPath networkPath)
    {
        _wifiDirectHelper = helper;
        _p2pDevice = p2pDevice;
        _p2pInterface = p2pInterface;
        _networkPath = networkPath;
    }

    public async ValueTask DisposeAsync()
    {
        // ToDo
    }
}