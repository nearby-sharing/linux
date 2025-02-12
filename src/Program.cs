using Adw;
using Gdk;
using Gio;
using Gtk;
using Microsoft.Extensions.Logging;
using NearShare;
using System.Diagnostics;
using AdwApplication = Adw.Application;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddConsole();

    builder.SetMinimumLevel(LogLevel.Information);
});

var app = AdwApplication.New("de.shortdev.nearshare", Gio.ApplicationFlags.FlagsNone);
app.OnActivate += static (app, args) =>
{
    var builder = Builder.NewFromString(System.IO.File.ReadAllText("MainWindow.xml"), -1);

    var window = (Adw.Window)builder.GetObject("window")!;
    window.Present();
    ((AdwApplication)app).AddWindow(window);

    var sendPage = (Widget)builder.GetObject("sendPage")!;
    DropTarget dropTarget = DropTarget.New(Gio.FileHelper.GetGType(), DragAction.Copy);
    dropTarget.OnDrop += DropTarget_OnDrop;
    sendPage.AddController(dropTarget);

    var aboutDialog = builder.GetObject<Adw.Dialog>("aboutDialog")!;

    var shareFileAction = builder.GetObject<ActionRow>("shareFileAction")!;
    shareFileAction.OnActivated += async (button, data) =>
    {
        var dialog = FileDialog.New();
        var file = await dialog.OpenAsync(window);
        if (file is null)
            return;

        Debug.Print(file.GetPath());
    };

    var shareClipboardAction = builder.GetObject<ActionRow>("shareClipboardAction")!;
    shareClipboardAction.OnActivated += async (button, data) =>
    {
        var clipboardContent = await button.GetClipboard().ReadTextAsync();
        Debug.Print(clipboardContent);
    };

    var shareTextAction = builder.GetObject<ActionRow>("shareTextAction")!;
    shareTextAction.OnActivated += (button, data) =>
    {
    };

    var showAboutAction = builder.GetObject<ActionRow>("showAboutAction")!;
    showAboutAction.OnActivated += (button, data) =>
    {
        aboutDialog.Present(button);
    };
};

app.Run(args.Length, args);

static bool DropTarget_OnDrop(DropTarget sender, DropTarget.DropSignalArgs args)
{
    FileHelper file = new(args.Value.GetObject()!.Handle);
    return true;
}
