using Gtk;
using NearShare.GtkUtils;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.NearShare;
using ShortDev.Microsoft.ConnectedDevices.Transports;

namespace NearShare;

sealed class ShareDialog : IDisposable
{
    readonly ITransfer _transfer;
    readonly Adw.Dialog _dialog;
    readonly Adw.ViewStack _stack;
    readonly Gio.ListStore _deviceList;
    readonly Gtk.ProgressBar _progressBar;
    readonly ConnectedDevicesPlatform _cdp;

    public ShareDialog(ITransfer transfer, ConnectedDevicesPlatform cdp)
    {
        _transfer = transfer;
        _cdp = cdp;

        var builder = Utils.LoadUI<ShareDialog>();
        _dialog = builder.GetObject<Adw.Dialog>("dialog")!;

        _stack = builder.GetObject<Adw.ViewStack>("stack")!;
        _deviceList = SetupDeviceSelection(builder.GetObject<GridView>("deviceList")!);

        _progressBar = builder.GetObject<Gtk.ProgressBar>("progressBar")!;
    }

    Gio.ListStore SetupDeviceSelection(GridView gridView)
    {
        Gio.ListStore items = Gio.ListStore.New(DeviceWrapper.GetGType());

        var itemFactory = BuilderListItemFactory.NewFromBytes(null, Utils.LoadTemplate<ShareDialog>("DeviceItem"));

        gridView.Model = SingleSelection.New(items);
        gridView.Factory = itemFactory;
        gridView.OnActivate += GridView_OnActivate;

        return items;
    }

    public void Present(Window window)
    {
        _dialog.Present(window);
    }

    readonly TaskCompletionSource _promise = new();
    readonly CancellationTokenSource _discoverCancellation = new();

    readonly HashSet<CdpDevice> _foundDevices = [];

    public async Task ExecuteAsync()
    {
        _foundDevices.Clear();

        _cdp.DeviceDiscovered += OnDeviceDiscovered;
        _cdp.Discover(_discoverCancellation.Token);

        await _promise.Task;

        void OnDeviceDiscovered(ICdpTransport sender, CdpDevice device)
        {
            if (_foundDevices.Add(device))
                _deviceList.Append(new DeviceWrapper(device));
        }
    }

    private async void GridView_OnActivate(GridView s, GridView.ActivateSignalArgs args)
    {
        if (_deviceList.GetObject(args.Position) is not DeviceWrapper item)
            return;

        _discoverCancellation.Cancel();

        _stack.VisibleChild = _stack.VisibleChild?.GetNextSibling();

        bool isWaitingForAcceptance = true;
        Progress<NearShareProgress> progress = new();
        progress.ProgressChanged += OnProgress;

        NearShareSender sender = new(_cdp);
        try
        {
            await _transfer.Execute(sender, item.Device!, progress, cancellation: default);
            _promise.TrySetResult();
            
            _stack.VisibleChild = _stack.VisibleChild?.GetNextSibling();
        }
        catch (Exception ex)
        {
            _promise.TrySetException(ex);
        }

        void OnProgress(object? sender, NearShareProgress progress)
        {
            if (isWaitingForAcceptance)
            {
                _stack.VisibleChild = _stack.VisibleChild?.GetNextSibling();
                isWaitingForAcceptance = false;
            }

            _progressBar.Fraction = (double)progress.TransferedBytes / progress.TotalBytes;
        }
    }

    public void Dispose()
    {
        _discoverCancellation.Dispose();
    }
}