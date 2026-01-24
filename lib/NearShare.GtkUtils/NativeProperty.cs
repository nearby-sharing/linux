using GLib.Internal;
using GObject;
using GObject.Internal;

namespace NearShare.GtkUtils;

public static class NativeProperty
{
    public static nint String(string name, ParamFlags flags,
        string? defaultValue = null)
    {
        return GObject.Internal.Functions.ParamSpecString(
            NonNullableUtf8StringOwnedHandle.Create(name),
            NullableUtf8StringOwnedHandle.Create(name),
            NullableUtf8StringOwnedHandle.Create(name),
            NullableUtf8StringOwnedHandle.Create(defaultValue),
            flags
        );
    }

    public static void InstallProperties(ObjectClassHandle classHandle, params ReadOnlySpan<nint> properties)
    {
        // ToDo: Prevent additional allocation
        GObject.Internal.ObjectClass.InstallProperties(classHandle, (uint)properties.Length,
            properties.ToArray());
    }
}