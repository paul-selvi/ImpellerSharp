using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerVulkanSwapchainCreateNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerVulkanSwapchainCreateNew(nint context, nint vulkanSurface);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerVulkanSwapchainRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerVulkanSwapchainRetain(nint swapchain);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerVulkanSwapchainRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerVulkanSwapchainRelease(nint swapchain);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerVulkanSwapchainAcquireNextSurfaceNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerVulkanSwapchainAcquireNextSurfaceNew(nint swapchain);
}
