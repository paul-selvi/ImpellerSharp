using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct ImpellerContextVulkanSettings
{
    public IntPtr UserData;
    public ImpellerVulkanProcAddressCallback ProcAddressCallback;

    [MarshalAs(UnmanagedType.I1)]
    public bool EnableValidation;
}
