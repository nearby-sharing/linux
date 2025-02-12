using Gtk;

namespace NearShare;

static class Utils
{
    public static Builder LoadUI<T>()
    {
        Type type = typeof(T);

        using var stream = type.Assembly.GetManifestResourceStream($"{type.FullName}.xaml")
            ?? throw new InvalidOperationException($"Could not load ui for {type.FullName}");

        using StreamReader reader = new(stream);
        return Builder.NewFromString(reader.ReadToEnd(), -1);
    }
}
