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
    readonly Gtk.Stack _stack;
    readonly Gio.ListStore _deviceList;
    readonly ConnectedDevicesPlatform _cdp;
    public ShareDialog(ITransfer transfer, ConnectedDevicesPlatform cdp)
    {
        _transfer = transfer;
        _cdp = cdp;

        var builder = Utils.LoadUI<ShareDialog>();
        _dialog = builder.GetObject<Adw.Dialog>("dialog")!;

        _stack = builder.GetObject<Gtk.Stack>("stack")!;
        _deviceList = SetupDeviceSelection(builder.GetObject<GridView>("deviceList")!);
    }

    Gio.ListStore SetupDeviceSelection(GridView gridView)
    {
        Gio.ListStore items = Gio.ListStore.New(DeviceWrapper.GetGType());

        var itemFactory = SignalListItemFactory.New();
        itemFactory.OnSetup += DeviceSelect_OnSetupItem;
        itemFactory.OnBind += DeviceSelect_OnBindItem;

        gridView.Model = SingleSelection.New(items);
        gridView.Factory = itemFactory;
        gridView.OnActivate += GridView_OnActivate;

        return items;
    }

    private void DeviceSelect_OnSetupItem(SignalListItemFactory sender, SignalListItemFactory.SetupSignalArgs args)
    {
        if (args.Object is not ListItem listItem)
            return;

        var box = Box.New(Orientation.Vertical, 2);
        box.SetSizeRequest(100, 100);
        listItem.Child = box;
        var label = Label.New(null);
        box.Append(label);
    }

    private void DeviceSelect_OnBindItem(SignalListItemFactory sender, SignalListItemFactory.BindSignalArgs args)
    {
        if (args.Object is not ListItem { Child: Box box, Item: DeviceWrapper { Device: CdpDevice device } })
            return;

        var label = (Label)box.GetFirstChild()!;
        label.SetText($"{device.Name} {device.Type} {device.Endpoint.TransportType}");
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

        Progress<NearShareProgress> progress = new();

        NearShareSender sender = new(_cdp);
        try
        {
            await _transfer.Execute(sender, item.Device!, progress, cancellation: default);
            _promise.TrySetResult();
        }
        catch (Exception ex)
        {
            _promise.TrySetException(ex);
        }
    }

    public void Dispose()
    {
        _discoverCancellation.Dispose();
    }
}
