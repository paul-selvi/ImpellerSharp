using System;

namespace ImpellerSharp.Interop;

public sealed class ImpellerVulkanSwapchainHandle : ImpellerSafeHandle
{
    private readonly ImpellerContextHandle _context;
    private readonly bool _contextRefHeld;

    private ImpellerVulkanSwapchainHandle(nint native, ImpellerContextHandle context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        SetHandle(native);
        _context = context;

        var addedRef = false;
        context.DangerousAddRef(ref addedRef);
        _contextRefHeld = addedRef;
    }

    public static ImpellerVulkanSwapchainHandle Create(ImpellerContextHandle context, nint vulkanSurface)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (vulkanSurface == nint.Zero)
        {
            throw new ArgumentException("VkSurfaceKHR pointer must not be null.", nameof(vulkanSurface));
        }

        if (context.IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerContextHandle));
        }

        var native = ImpellerNative.ImpellerVulkanSwapchainCreateNew(context.DangerousGetHandle(), vulkanSurface);
        return FromOwned(native, context);
    }

    internal static ImpellerVulkanSwapchainHandle FromOwned(nint native, ImpellerContextHandle context)
    {
        native = EnsureSuccess(native, "Failed to create Impeller Vulkan swapchain.");
        return new ImpellerVulkanSwapchainHandle(native, context);
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerVulkanSwapchainRetain(handle);
    }

    public ImpellerSurfaceHandle? AcquireNextSurface()
    {
        ThrowIfInvalid();
        var nativeSurface = ImpellerNative.ImpellerVulkanSwapchainAcquireNextSurfaceNew(handle);
        if (nativeSurface == nint.Zero)
        {
            return null;
        }

        return ImpellerSurfaceHandle.FromOwned(nativeSurface);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerVulkanSwapchainRelease(handle);
        if (_contextRefHeld)
        {
            _context.DangerousRelease();
        }

        return true;
    }

    internal new nint DangerousGetHandle()
    {
        ThrowIfInvalid();
        return handle;
    }

    private void ThrowIfInvalid()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerVulkanSwapchainHandle));
        }
    }
}
