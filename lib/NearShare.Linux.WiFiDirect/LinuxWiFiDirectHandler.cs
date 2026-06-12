using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using ShortDev.Microsoft.ConnectedDevices.Transports.WiFiDirect;

namespace NearShare.Linux.WiFiDirect;

public sealed class LinuxWiFiDirectHandler(WiFiDirectHelper helper) : IWiFiDirectHandler
{
    private readonly WiFiDirectHelper _helper = helper;

    public PhysicalAddress MacAddress => _helper.MacAddress; // PhysicalAddress.Parse("8c:b8:4a:5d:47:50");
    public bool IsEnabled => true;

    public async Task<IPAddress> ConnectAsync(string address, GroupInfo groupInfo,
        CancellationToken cancellationToken = default)
    {
        IPAddress ipAddress;
        try
        {
            ipAddress = await _helper.Connect(PhysicalAddress.Parse(address), groupInfo.Ssid, groupInfo.PreSharedKey,
                cancellationToken);
        }
        catch (Exception e)
        {
            Debug.Print(e.StackTrace);
            throw;
        }

        Console.WriteLine($"go_ip={ipAddress}");
        return ipAddress;
    }

    public Task<GroupInfo> CreateAutonomousGroup()
    {
        throw new NotImplementedException();
    }

    public void AddGroupAllowedDevice(PhysicalAddress allowedAddress)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }

    public static async Task<LinuxWiFiDirectHandler> CreateAsync()
        => new(await WiFiDirectHelper.CreateAsync());
}