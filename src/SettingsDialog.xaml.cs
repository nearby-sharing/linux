using Gtk;

namespace NearShare;

sealed class SettingsDialog
{
    readonly Adw.PreferencesDialog _dialog;
    public SettingsDialog()
    {
        _dialog = Utils.LoadUI<SettingsDialog>().GetObject<Adw.PreferencesDialog>("preferencesDialog")!;
    }

    public void Present(Window window)
    {
        _dialog.Present(window);
    }
}
