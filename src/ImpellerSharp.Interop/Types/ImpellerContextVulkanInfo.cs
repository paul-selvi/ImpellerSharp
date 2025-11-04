using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct ImpellerContextVulkanInfo
{
    public IntPtr Instance;
    public IntPtr PhysicalDevice;
    public IntPtr LogicalDevice;
    public uint GraphicsQueueFamilyIndex;
    public uint GraphicsQueueIndex;
}
