using Microsoft.Extensions.Logging;
using NearShare.Linux.Bluetooth;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Encryption;
using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using ShortDev.Microsoft.ConnectedDevices.Transports.Network;
using System.Net;

namespace NearShare;

static class CdpUtils
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
        cdp.AddTransport<NetworkTransport>(new(networkHandler));

        LinuxBluetoothHandler bluetoothHandler = await LinuxBluetoothHandler.CreateAsync();
        cdp.AddTransport<BluetoothTransport>(new(bluetoothHandler));

        return cdp;
    }

    sealed class NetworkHandler : INetworkHandler
    {
        public IPAddress GetLocalIp()
            => INetworkHandler.GetLocalIpDefault();
    }
}
