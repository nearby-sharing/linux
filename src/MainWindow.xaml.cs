using Adw;
using Gdk;
using Gtk;
using Microsoft.Extensions.Logging;

namespace NearShare;

sealed class MainWindow
{
    readonly Adw.Application _app;
    readonly Adw.Window _window;
    readonly ILoggerFactory _loggerFactory;
    public MainWindow(Adw.Application app, ILoggerFactory loggerFactory)
    {
        _app = app;
        _loggerFactory = loggerFactory;
        var builder = Utils.LoadUI<MainWindow>();

        _window = (Adw.Window)builder.GetObject("window")!;

        SetupMenu(builder);
        SetupSendPage(builder);
    }

    public void Present()
    {
        _window.Present();
        _app.AddWindow(_window);
    }

    void SetupMenu(Builder builder)
    {
        // Settings Menu
        {
            var settingsAction = Gio.SimpleAction.New("settings", null);
            settingsAction.OnActivate += (s, e) =>
            {
                SettingsDialog dialog = new();
                dialog.Present(_window);
            };
            _app.AddAction(settingsAction);
        }

        // About Menu
        {
            var aboutDialog = builder.GetObject<Adw.Dialog>("aboutDialog")!;
            var aboutAction = Gio.SimpleAction.New("about", null);
            aboutAction.OnActivate += (s, e) =>
            {
                aboutDialog.Present(_window);
            };
            _app.AddAction(aboutAction);
        }
    }

    void SetupSendPage(Builder builder)
    {
        // Drag'n'drop
        {
            var sendPage = (Widget)builder.GetObject("sendPage")!;
            DropTarget dropTarget = DropTarget.New(Gio.FileHelper.GetGType(), DragAction.Copy);
            dropTarget.OnDrop += DropTarget_OnDrop;
            sendPage.AddController(dropTarget);
        }

        // Share file
        {
            var shareFileAction = builder.GetObject<ActionRow>("shareFileAction")!;
            shareFileAction.OnActivated += async (button, data) =>
            {
                var dialog = FileDialog.New();
                var file = await dialog.OpenAsync(_window);
                if (file is null || file.GetPath() is null)
                    return;

                SendFile(file);
            };
        }

        // Share clipboard
        {
            var shareClipboardAction = builder.GetObject<ActionRow>("shareClipboardAction")!;
            shareClipboardAction.OnActivated += async (button, data) =>
            {
                var clipboardContent = await button.GetClipboard().ReadTextAsync();
                SendText(clipboardContent);
            };
        }

        // Share text
        {
            var shareTextAction = builder.GetObject<ActionRow>("shareTextAction")!;
            shareTextAction.OnActivated += (button, data) =>
            {
                SendTextDialog dialog = new()
                {
                    SendText = SendText
                };
                dialog.Present(_window);
            };
        }
    }

    bool DropTarget_OnDrop(DropTarget sender, DropTarget.DropSignalArgs args)
    {
        if (args.Value.GetObject() is not Gio.File file)
            return false;

        if (file.GetPath() is null)
            return false;

        SendFile(file);
        return true;
    }

    void SendFile(Gio.File file)
        => StartTransfer(new FileTransfer(file));

    void SendText(string? text)
    {
        if (text is null)
            return;

        if (Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            StartTransfer(new UriTransfer(uri));
            return;
        }

        StartTransfer(new TextTransfer(text));
    }

    async void StartTransfer(ITransfer transfer)
    {
        using var cdp = await CdpUtils.Create(Environment.MachineName, _loggerFactory);
        using ShareDialog dialog = new(transfer, cdp);
        dialog.Present(_window);
        await dialog.ExecuteAsync();
    }
}
