using Gtk;

namespace NearShare;

sealed class SendTextDialog
{
    readonly Adw.MessageDialog _sendTextDialog;
    readonly Gtk.TextView _textArea;
    public SendTextDialog()
    {
        var builder = Utils.LoadUI<SendTextDialog>();
        _textArea = builder.GetObject<Gtk.TextView>("textArea")!;
        _sendTextDialog = builder.GetObject<Adw.MessageDialog>("sendTextDialog")!;
        _sendTextDialog.OnResponse += SendTextDialog_OnResponse;
    }

    public void Present(Window window)
    {
        _sendTextDialog.SetTransientFor(window);
        _sendTextDialog.Present();
    }

    public required Action<string?> SendText;
    void SendTextDialog_OnResponse(Adw.MessageDialog sender, Adw.MessageDialog.ResponseSignalArgs args)
    {
        if (args.Response != "send")
            return;

        SendText(_textArea.Buffer?.Text);
    }
}
