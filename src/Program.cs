using Microsoft.Extensions.Logging;
using ShortDev.Gtk;
using System.Diagnostics;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddConsole();

    builder.SetMinimumLevel(LogLevel.Information);
});

return Application.Start("de.shortdev.nearshare", app =>
{
    var builder = GtkBuilder.FromString(File.ReadAllText("MainWindow.xml"));

    var window = builder.GetObject<Window>("window");
    window.Present();
    app.AddWindow(window);

    var sendPage = builder.GetObject<Widget>("sendPage");
    DropTarget dropTarget = DropTarget.Create(ShortDev.Gtk.IO.File.GetGType(), DragAction.Copy);
    dropTarget.Drop += DropTarget_Drop;
    sendPage.AddController(dropTarget);

    var aboutDialog = builder.GetObject<Dialog>("aboutDialog");

    var shareFileAction = builder.GetObject<ActionRow>("shareFileAction");
    shareFileAction.Activated += async (button, data) =>
    {
        var dialog = FileDialog.Create();
        var path = await dialog.OpenFileAsync(window);
        Debug.Print(path);
    };

    var shareClipboardAction = builder.GetObject<ActionRow>("shareClipboardAction");
    shareClipboardAction.Activated += async (button, data) =>
    {
        var clipboardContent = await button.GetClipBoard().ReadTextAsync();

    };

    var shareTextAction = builder.GetObject<ActionRow>("shareTextAction");
    shareTextAction.Activated += (button, data) =>
    {
    };

    var showAboutAction = builder.GetObject<ActionRow>("showAboutAction");
    showAboutAction.Activated += (button, data) =>
    {
        aboutDialog.Present(button);
    };
});

bool DropTarget_Drop(DropTarget target, nint value, double x, double y, nint userData)
{
    return true;
}