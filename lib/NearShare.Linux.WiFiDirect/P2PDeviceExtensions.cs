using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using NearShare.Linux.WiFiDirect.DBus;
using Tmds.DBus.Protocol;

namespace NearShare.Linux.WiFiDirect;

static class P2PDeviceExtensions
{
    extension(P2PDevice device)
    {
        public async Task<ObjectPath> FindDevice(PhysicalAddress address, int timeoutSeconds = 7)
        {
            foreach (var peer in await device.GetPeersAsync())
            {
                if (DoesMatch(peer, address))
                    return peer;
            }

            TaskCompletionSource<ObjectPath> promise = new();
            using var deviceWatcher = await device.WatchDeviceFoundAsync(path =>
            {
                if (DoesMatch(path, address))
                    promise.TrySetResult(path);
            });
            using var findStoppedWatcher = await device.WatchFindStoppedAsync(() => { _ = promise.TrySetCanceled(); });

            await device.FindAsync(new()
            {
                { "Timeout", timeoutSeconds },
            });

            try
            {
                return await promise.Task;
            }
            finally
            {
                await device.StopFindAsync();
            }

            static bool DoesMatch(ObjectPath path, PhysicalAddress address)
                => path.TryParsePeer(out var peerAddress) && peerAddress.Equals(address);
        }

        public async Task Provision(ObjectPath peer)
        {
            TaskCompletionSource promise = new();
            
            using var watcher1  = await device.WatchProvisionDiscoveryFailureAsync((e) =>
            {
                var (actualPeer, status) = e;
                if (actualPeer != peer)
                    return;

                if (status == 0)
                    _ = promise.TrySetResult();
                else
                    _ = promise.TrySetException(
                        new InvalidOperationException($"Provision failed with status={status}"));
            });

            using var watcher2 = await device.WatchProvisionDiscoveryPBCResponseAsync((actualPeer) =>
            {
                if (actualPeer == peer)
                    _=promise.TrySetResult();
            });

            await device.ProvisionDiscoveryRequestAsync(peer, "pbc");
            await promise.Task;
        }
    }

    extension(ObjectPath path)
    {
        // parse_peer_object_path
        public bool TryParsePeer([MaybeNullWhen(false)] out PhysicalAddress address)
        {
            address = null;

            var peerPath = path.ToString();
            if (string.IsNullOrEmpty(peerPath))
                return false;

            var start = peerPath.LastIndexOf('/');
            return PhysicalAddress.TryParse(peerPath.AsSpan()[(start + 1)..], out address);
        }
    }
}