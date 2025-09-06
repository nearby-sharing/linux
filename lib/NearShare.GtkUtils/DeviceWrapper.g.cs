using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GObject;
using GObject.Internal;
using Value = GObject.Value;

namespace NearShare.GtkUtils;

public partial class DeviceWrapper(global::GObject.Internal.ObjectHandle handle) : global::GObject.Object(handle), GObject.GTypeProvider, GObject.InstanceFactory
{
    private static readonly unsafe GObject.Type GType = SubclassRegistrarEx.Register<DeviceWrapper, global::GObject.Object>(&ClassInitializeShim);
    public static new GObject.Type GetGType() => GType;

    static object GObject.InstanceFactory.Create(System.IntPtr handle, bool ownsHandle)
    {
        return new DeviceWrapper(new global::GObject.Internal.ObjectHandle(handle, ownsHandle));
    }
     
    public DeviceWrapper(params GObject.ConstructArgument[] constructArguments) : this(global::GObject.Internal.ObjectHandle.For<DeviceWrapper>(constructArguments)) { }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void ClassInitializeShim(nint classHandle, nint classData)
    {
        var pClass = (ObjectClassDataRaw*)classHandle;
        pClass->GetProperty = &GetPropertyValue;
        pClass->SetProperty = &SetPropertyValue;
            
        NativeProperty.InstallProperties(
            new ObjectClassUnownedHandle(classHandle),
            0,
            NativeProperty.String("DeviceName", ParamFlags.Readwrite),
            NativeProperty.String("DeviceTypeIconName", ParamFlags.Readwrite),
            NativeProperty.String("TransportTypeIconName", ParamFlags.Readwrite)
        );
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void GetPropertyValue(nint @this, uint propertyId, nint value, nint paramSpec)
    {
        GObject.Value value2 = new(new ValueOwnedHandle(value));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]

    static void SetPropertyValue(nint @this, uint propertyId, nint value, nint paramSpec)
    {
        
    }
}