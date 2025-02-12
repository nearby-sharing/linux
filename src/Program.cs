using Adw;
using Microsoft.Extensions.Logging;
using NearShare;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddConsole();

    builder.SetMinimumLevel(LogLevel.Information);
});

var app = Adw.Application.New("de.shortdev.nearshare", Gio.ApplicationFlags.FlagsNone);
app.OnActivate += (app, args) =>
{
    StyleManager.GetDefault().ColorScheme = ColorScheme.PreferDark;

    MainWindow window = new((Adw.Application)app, loggerFactory);
    window.Present();
};

return app.RunWithSynchronizationContext(args);
