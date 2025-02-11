using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShortDev.Gtk;

[StructLayout(LayoutKind.Sequential)]
public readonly partial struct Application : IObject, IDisposable
{
    nint IObject.Handle { get; }

    public Window CreateNewWindow()
        => CreateNewWindow(this);

    public void AddWindow(Window window)
        => AdWindow(this, window);

    public void Dispose()
        => this.Unref();

    public static unsafe int Start(string id, ApplicationInitializationCallback callback)
    {
        using var app = Create(id);
        app.AddEvent("activate", Marshal.GetFunctionPointerForDelegate(callback), 0, 0);
        return Run(app, 0, null);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void OnActivated(Application @this, nint userData)
    {
        var handle = GCHandle.FromIntPtr(userData);
        try
        {
            var callback = handle.Target as Action<Application>;
            callback?.Invoke(@this);
        }
        finally
        {
            handle.Free();
        }
    }

    [LibraryImport(Libs.Adwaita, EntryPoint = "adw_application_new")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial Application Create([MarshalAs(UnmanagedType.LPStr)] string id, int flags = 0);

    [LibraryImport(Libs.Gtk, EntryPoint = "g_application_run")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int Run(Application @this, int argc, byte** argv);

    [LibraryImport(Libs.Adwaita, EntryPoint = "adw_window_new")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial Window CreateNewWindow(Application application);

    [LibraryImport(Libs.Adwaita, EntryPoint = "gtk_application_add_window")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void AdWindow(Application application, Window window);
}

public delegate void ApplicationInitializationCallback(Application app);
