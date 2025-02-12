using Gtk;

namespace NearShare;

static class Extensions
{
    public static T? GetObject<T>(this Builder builder, string name) where T : Widget
        => (T?)builder.GetObject(name);
}
