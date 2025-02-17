﻿using InTheHand.Net;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Transports;
using ShortDev.Microsoft.ConnectedDevices.Transports.Bluetooth;
using Spectre.Console;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using Tmds.DBus.Protocol;

namespace NearShare.Linux.Bluetooth;

[SupportedOSPlatform("linux")]
public sealed class LinuxBluetoothHandler : IBluetoothHandler
{
    readonly BlueZManager _manager;
    readonly IAdapter1 _adapter;
    private LinuxBluetoothHandler(BlueZManager manager, IAdapter1 adapter, PhysicalAddress macAddress)
    {
        _manager = manager;
        _adapter = adapter;
        MacAddress = macAddress;
    }

    public PhysicalAddress MacAddress { get; }

    public static async ValueTask<LinuxBluetoothHandler> CreateAsync()
    {
        var manager = await BlueZManager.CreateAsync();
        var adapters = await manager.GetAdaptersAsync();
        var adapter = adapters.FirstOrDefault() ?? throw new InvalidOperationException("Could not get adapter");

        var addressStr = await adapter.GetAddressPropertyAsync();
        var macAddress = PhysicalAddress.Parse(addressStr);

        return new(manager, adapter, macAddress);
    }

    public async Task AdvertiseBLeBeaconAsync(AdvertiseOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            await _adapter.SetPoweredPropertyAsync(true);
            await _adapter.SetDiscoverablePropertyAsync(true);

            var advertisement = NearShareAdvertisement.Create(options);
            await _manager.AdvertiseAsync(advertisement.Path, advertisement, cancellationToken);

            await _adapter.SetDiscoverablePropertyAsync(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task ScanBLeAsync(ScanOptions scanOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            await ScanInternal(scanOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    async Task ScanInternal(ScanOptions scanOptions, CancellationToken cancellationToken = default)
    {
        await _adapter.SetPoweredPropertyAsync(true);
        await _adapter.SetDiscoveryFilterAsync(new()
        {
            { "Transport", "le" },
            { "DuplicateData", false }
        });

        using var watcher = await _manager.ObjectManager.WatchInterfacesAddedAsync((ex, changes) =>
        {
            if (!changes.InterfacesAndProperties.TryGetValue("org.bluez.Device1", out var props))
                return;

            if (!props.TryGetValue("ManufacturerData", out var manufacturerData))
                return;

            ParseDevice(manufacturerData.GetDictionary<ushort, VariantValue>());
        });

        await _adapter.StartDiscoveryAsync();
        await cancellationToken.AwaitCancellation();
        await _adapter.StopDiscoveryAsync();

        void ParseDevice(Dictionary<ushort, VariantValue> manufacturerData)
        {
            try
            {
                if (manufacturerData.TryGetValue(Constants.BLeBeaconManufacturerId, out var beaconData) != true)
                    return;

                if (!BLeBeacon.TryParse(beaconData.GetArray<byte>(), out var beacon))
                    return;

                scanOptions.OnDeviceDiscovered?.Invoke(beacon);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }
    }

    public async Task<CdpSocket> ConnectRfcommAsync(EndpointInfo endpoint, RfcommOptions options, CancellationToken cancellationToken = default)
    {
        Guid serviceId = Guid.Parse(options.ServiceId!);
        BluetoothAddress address = BluetoothAddress.Parse(endpoint.Address);
        var port = SdpLookup.GetRfcommChannel(PhysicalAddress.Parse(endpoint.Address), serviceId);
        Console.WriteLine($"Resolved rfcomm port for {serviceId}@{endpoint.Address} to {port}");

        InTheHand.Net.Sockets.BluetoothClient client = new();
        client.Connect(new BluetoothEndPoint(address, serviceId, port));
        return new CdpSocket()
        {
            Endpoint = endpoint,
            InputStream = client.GetStream(),
            OutputStream = client.GetStream(),
            Close = () =>
            {
                client.Close();
                client.Dispose();
            }
        };
    }

    public Task ListenRfcommAsync(RfcommOptions options, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
