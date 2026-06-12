using NearShare.Linux.WiFiDirect.DBus;
using Tmds.DBus.Protocol;

namespace NearShare.Linux.WiFiDirect;

public sealed class WiFiDirectSession : IAsyncDisposable
{
    private readonly WiFiDirectHelper _wifiDirectHelper;
    private readonly Interface _p2pInterface;
    private readonly ObjectPath _networkPath;

    internal WiFiDirectSession(WiFiDirectHelper helper, Interface p2pInterface, ObjectPath networkPath)
    {
        _wifiDirectHelper = helper;
        _p2pInterface = p2pInterface;
        _networkPath = networkPath;
    }
    
    internal WiFiDirectSession(WiFiDirectHelper helper, ObjectPath groupPath)
    {
        _wifiDirectHelper = helper;
        _p2pInterface = null!;
        _networkPath = groupPath;
    }

    public async ValueTask DisposeAsync()
    {
        // ToDo
    }
}