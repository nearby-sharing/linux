using Microsoft.Extensions.Logging;
using NearShare.Linux.Bluetooth;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Encryption;
using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using ShortDev.Microsoft.ConnectedDevices.Transports.Network;
using System.Net;
using NearShare.Linux.WiFiDirect;
using ShortDev.Microsoft.ConnectedDevices.Transports.WiFiDirect;

namespace NearShare;

internal static class CdpUtils
{
    public static async Task<ConnectedDevicesPlatform> Create(string deviceName, ILoggerFactory loggerFactory)
    {
        LocalDeviceInfo deviceInfo = new()
        {
            Name = deviceName,
            OemManufacturerName = Environment.UserName,
            OemModelName = Environment.UserDomainName,
            Type = DeviceType.Linux,
            DeviceCertificate = ConnectedDevicesPlatform.CreateDeviceCertificate(CdpEncryptionParams.Default)
        };

        ConnectedDevicesPlatform cdp = new(deviceInfo, loggerFactory);

        NetworkHandler networkHandler = new();
        NetworkTransport networkTransport = new(networkHandler);
        cdp.AddTransport<NetworkTransport>(networkTransport);

        var bluetoothHandler = await LinuxBluetoothHandler.CreateAsync();
        cdp.AddTransport<BluetoothTransport>(new(bluetoothHandler));

        var wifiDirectHandler = await LinuxWiFiDirectHandler.CreateAsync(deviceName);
        cdp.AddTransport<WiFiDirectTransport>(new(wifiDirectHandler, networkTransport));

        return cdp;
    }

    sealed class NetworkHandler : INetworkHandler
    {
        public IPAddress GetLocalIp()
            => INetworkHandler.GetLocalIpDefault();
    }
}