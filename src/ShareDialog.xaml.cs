using Gtk;
using NearShare.GtkUtils;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Transports;

namespace NearShare;

sealed class ShareDialog : IDisposable
{
    readonly ITransfer _transfer;
    readonly Adw.Dialog _dialog;
    readonly Gio.ListStore _deviceList;
    readonly ConnectedDevicesPlatform _cdp;
    public ShareDialog(ITransfer transfer, ConnectedDevicesPlatform cdp)
    {
        _transfer = transfer;
        _cdp = cdp;

        var builder = Utils.LoadUI<ShareDialog>();
        _dialog = builder.GetObject<Adw.Dialog>("dialog")!;

        _deviceList = SetupDeviceSelection(builder.GetObject<GridView>("deviceList")!);
    }

    Gio.ListStore SetupDeviceSelection(GridView gridView)
    {
        Gio.ListStore items = Gio.ListStore.New(DeviceWrapper.GetGType());
        items.Append(new DeviceWrapper(new CdpDevice("Test1", DeviceType.Android, EndpointInfo.FromTcp("127.0.0.1"))));
        items.Append(new DeviceWrapper(new CdpDevice("Test2", DeviceType.Android, EndpointInfo.FromTcp("127.0.0.1"))));
        items.Append(new DeviceWrapper(new CdpDevice("Test3", DeviceType.Android, EndpointInfo.FromTcp("127.0.0.1"))));

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
        label.SetText(device.Name);
    }

    public void Present(Window window)
    {
        _dialog.Present(window);
    }

    readonly TaskCompletionSource _promise = new();
    readonly CancellationTokenSource _discoverCancellation = new();
    public async Task ExecuteAsync(ITransfer transfer)
    {
        _cdp.DeviceDiscovered += OnDeviceDiscovered;
        _cdp.Discover(_discoverCancellation.Token);

        await _promise.Task;

        void OnDeviceDiscovered(ICdpTransport sender, CdpDevice device)
            => _deviceList.Append(new DeviceWrapper(device));
    }

    private void GridView_OnActivate(GridView sender, GridView.ActivateSignalArgs args)
    {
        _discoverCancellation.Cancel();


    }

    public void Dispose()
    {
        _discoverCancellation.Dispose();
    }
}
