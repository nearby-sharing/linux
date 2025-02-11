using ShortDev.Gtk;
using System.Diagnostics;

// https://docs.gtk.org/gtk4/index.html
// https://gnome.pages.gitlab.gnome.org/libadwaita/doc/1-latest/index.html

return Application.Start("de.shortdev.nearshare", app =>
{
    var builder = GtkBuilder.FromString(File.ReadAllText("MainWindow.xml"));

    var window = builder.GetObject<Window>("window");
    window.Present();
    app.AddWindow(window);

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

delegate void ClickedCallback(nint button, nint userData);
