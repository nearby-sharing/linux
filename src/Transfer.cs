using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.NearShare;

namespace NearShare;

interface ITransfer
{
    Task Execute(NearShareSender sender, CdpDevice device, Progress<NearShareProgress> progress, CancellationToken cancellation);
}

sealed record FileTransfer(Gio.File File) : ITransfer
{
    public async Task Execute(NearShareSender sender, CdpDevice device, Progress<NearShareProgress> progress, CancellationToken cancellation)
    {
        using FileStream stream = System.IO.File.OpenRead(File.GetPath() ?? throw new InvalidOperationException("Cannot get path"));

        var fileProvider = CdpFileProvider.FromStream(File.GetParseName(), stream);
        await sender.SendFileAsync(device, fileProvider, progress, cancellation);
    }
}

sealed record TextTransfer(string Text) : ITransfer
{
    public async Task Execute(NearShareSender sender, CdpDevice device, Progress<NearShareProgress> progress, CancellationToken cancellation)
    {
        var fileProvider = CdpFileProvider.FromContent("NearShare.txt", Text);
        await sender.SendFileAsync(device, fileProvider, progress, cancellation);
    }
}

sealed record UriTransfer(Uri Uri) : ITransfer
{
    public async Task Execute(NearShareSender sender, CdpDevice device, Progress<NearShareProgress> progress, CancellationToken cancellation)
    {
        await sender.SendUriAsync(device, Uri, cancellation);
    }
}
