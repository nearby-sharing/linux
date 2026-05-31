using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GLib.Internal;
using GObject;
using GObject.Internal;
using Value = GObject.Value;

namespace NearShare.GtkUtils;

partial class DeviceWrapper
{
    static partial void CompositeTemplateClassInit(IntPtr classHandle, IntPtr classData)
        => ClassInitializeShim(classHandle, classData);
    
    private static unsafe void ClassInitializeShim(nint classHandle, nint classData)
    {
        var pClass = (ObjectClassDataRaw*)classHandle;
        pClass->GetProperty = &GetPropertyValue;
        pClass->SetProperty = &SetPropertyValue;
            
        NativeProperty.InstallProperties(
            new ObjectClassUnownedHandle(classHandle),
            0,
            NativeProperty.String(nameof(DeviceName), ParamFlags.Readable),
            NativeProperty.String(nameof(DeviceTypeIconName), ParamFlags.Readable),
            NativeProperty.String(nameof(TransportTypeIconName), ParamFlags.Readable)
        );
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void GetPropertyValue(nint @this, uint propertyId, nint value, nint paramSpec)
    {
        DeviceWrapper instance = (DeviceWrapper)InstanceWrapper.WrapHandle<DeviceWrapper>(@this, ownedRef:false);
        
        ValueUnownedHandle valueHandle = new(value);
        switch (propertyId)
        {
            case 1:
                GObject.Internal.Value.SetString(valueHandle, NullableUtf8StringOwnedHandle.Create(instance.DeviceName));
                break;
            
            case 2:
                GObject.Internal.Value.SetString(valueHandle, NullableUtf8StringOwnedHandle.Create(instance.DeviceTypeIconName));
                break;
            
            case 3:
                GObject.Internal.Value.SetString(valueHandle, NullableUtf8StringOwnedHandle.Create(instance.TransportTypeIconName));
                break;
            
            default:
                Debug.Fail($"Unknown property id {propertyId}");
                break;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]

    static void SetPropertyValue(nint @this, uint propertyId, nint value, nint paramSpec)
    {
        
    }
}