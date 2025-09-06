using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GObject;
using GObject.Internal;
using Functions = GObject.Internal.Functions;
using Type = GObject.Type;

namespace NearShare.GtkUtils;

public static class SubclassRegistrarEx
{
    public static unsafe Type Register<TSubclass, TParent>()
        where TSubclass : InstanceFactory
        where TParent : GTypeProvider
    {
        return Register<TSubclass, TParent>(classInitializer: null);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe Type Register<TSubclass, TParent>(
        delegate*unmanaged[Cdecl]<nint, nint, void> classInitializer)
        where TSubclass : InstanceFactory
        where TParent : GTypeProvider
    {
        var newType = RegisterNewGType<TSubclass, TParent>(classInitializer);
        DynamicInstanceFactory.Register(newType, TSubclass.Create);

        return newType;
    }

    private static unsafe Type RegisterNewGType<TSubclass, TParent>(
        delegate*unmanaged[Cdecl]<nint, nint, void> classInitializer)
        where TParent : GTypeProvider
    {
        var parentType = TParent.GetGType();
        var parentTypeInfo = TypeQueryOwnedHandle.Create();
        Functions.TypeQuery(parentType, parentTypeInfo);

        if (parentTypeInfo.GetType() == 0)
            throw new InvalidOperationException("Could not query parent type");

        Debug.WriteLine($"Registering new type {typeof(TSubclass).FullName} with parent {typeof(TParent).FullName}");

        // Create TypeInfo
        //TODO: Callbacks for "ClassInit" and "InstanceInit" are disabled because if multiple instances
        //of the same type are created, the typeInfo object can get garbagec collected in the mean time
        //and with it the instances of "DoClassInit" and "DoInstanceInit". If the callback occurs the
        //runtime can't do the call anymore and crashes with:
        //A callback was made on a garbage collected delegate of type 'GObject-2.0!GObject.Internal.InstanceInitFunc::Invoke'
        //Fix this by caching the garbage collected instances somehow
        var handle = TypeInfoOwnedHandle.Create();
        handle.SetClassSize((ushort)parentTypeInfo.GetClassSize());
        handle.SetInstanceSize((ushort)parentTypeInfo.GetInstanceSize());
        //handle.SetClassInit();
        //handle.SetInstanceInit();
        var pTypeInfo = (TypeInfoDataRaw*)handle.DangerousGetHandle();
        pTypeInfo->ClassInit = classInitializer;

        var qualifiedName = QualifyName(typeof(TSubclass));
        var typeid = Functions.TypeRegisterStatic(parentType,
            GLib.Internal.NonNullableUtf8StringOwnedHandle.Create(qualifiedName), handle, 0);

        if (typeid == 0)
            throw new InvalidOperationException("Type Registration Failed!");

        return new(typeid);
    }

    private static string QualifyName(System.Type type)
        => type.ToString()
            .Replace(".", string.Empty)
            .Replace("+", string.Empty)
            .Replace("`", string.Empty)
            .Replace("[", "_")
            .Replace("]", string.Empty)
            .Replace(" ", string.Empty)
            .Replace(",", "_");
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct TypeInfoDataRaw
{
    public ushort ClassSize;
    public delegate* unmanaged[Cdecl]<nint, void> BaseInit;
    public delegate* unmanaged[Cdecl]<nint, void> BaseFinalize;
    public delegate* unmanaged[Cdecl]<nint, nint, void> ClassInit;
    public delegate* unmanaged[Cdecl]<nint, nint, void> ClassFinalize;
    public IntPtr ClassData;
    public ushort InstanceSize;
    public ushort NPreallocs;
    public delegate* unmanaged[Cdecl]<nint, nint, void> InstanceInit;
    public IntPtr ValueTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ObjectClassDataRaw
{
    public GObject.Internal.TypeClassData GTypeClass;
    public IntPtr ConstructProperties;
    public void* Constructor;

    public delegate* unmanaged[Cdecl]<nint, uint, nint, nint, void> SetProperty;

    public delegate* unmanaged[Cdecl]<nint, uint, nint, nint, void> GetProperty;
}