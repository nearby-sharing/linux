using Adw;
using Gdk;
using Gtk;
using System.Diagnostics;

namespace NearShare;

sealed class MainWindow
{
    readonly Adw.Application _app;
    readonly Adw.Window _window;
    public MainWindow(Adw.Application app)
    {
        _app = app;
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
                if (file is null)
                    return;

                Debug.Print(file.GetPath());
            };
        }

        // Share clipboard
        {
            var shareClipboardAction = builder.GetObject<ActionRow>("shareClipboardAction")!;
            shareClipboardAction.OnActivated += async (button, data) =>
            {
                var clipboardContent = await button.GetClipboard().ReadTextAsync();
                Debug.Print(clipboardContent);
            };
        }

        // Share text
        {
            var shareTextAction = builder.GetObject<ActionRow>("shareTextAction")!;
            shareTextAction.OnActivated += (button, data) =>
            {
                SendTextDialog dialog = new();
                dialog.Present(_window);
            };
        }
    }

    static bool DropTarget_OnDrop(DropTarget sender, DropTarget.DropSignalArgs args)
    {
        if (args.Value.GetObject() is not Gio.File file)
            return false;
        Debug.Print(file.GetPath());
        return true;
    }
}
