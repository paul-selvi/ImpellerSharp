using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr ImpellerProcAddressCallback(
    [MarshalAs(UnmanagedType.LPUTF8Str)] string procName,
    IntPtr userData);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr ImpellerVulkanProcAddressCallback(
    IntPtr vulkanInstance,
    [MarshalAs(UnmanagedType.LPUTF8Str)] string procName,
    IntPtr userData);
