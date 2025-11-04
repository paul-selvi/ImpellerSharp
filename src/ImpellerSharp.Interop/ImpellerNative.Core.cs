using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct ImpellerContextVulkanSettingsNative
{
    public IntPtr UserData;
    public IntPtr ProcAddressCallback;

    [MarshalAs(UnmanagedType.I1)]
    public bool EnableValidation;
}

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerGetVersion")]
    internal static partial uint ImpellerGetVersion();

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerContextCreateMetalNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerContextCreateMetalNew(uint version);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerContextCreateOpenGLESNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerContextCreateOpenGLESNew(
        uint version,
        IntPtr callback,
        IntPtr userData);

    [DllImport(ImpellerLibrary.Name, EntryPoint = "ImpellerContextCreateVulkanNew")]
    internal static extern nint ImpellerContextCreateVulkanNew(
        uint version,
        ref ImpellerContextVulkanSettingsNative settings);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerContextGetVulkanInfo")]
    [SuppressGCTransition]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool ImpellerContextGetVulkanInfo(
        nint context,
        out ImpellerContextVulkanInfo info);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerContextRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerContextRetain(nint context);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerContextRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerContextRelease(nint context);
}
