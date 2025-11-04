using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

public sealed class ImpellerContextHandle : ImpellerSafeHandle
{
    private readonly ImpellerProcAddressCallback? _openGlProcCallback;
    private readonly ImpellerVulkanProcAddressCallback? _vulkanProcCallback;

    private ImpellerContextHandle(
        nint handle,
        ImpellerProcAddressCallback? openGlCallback = null,
        ImpellerVulkanProcAddressCallback? vulkanCallback = null)
    {
        SetHandle(handle);
        _openGlProcCallback = openGlCallback;
        _vulkanProcCallback = vulkanCallback;
    }

    public static ImpellerContextHandle CreateMetal(uint? version = null)
    {
        var resolvedVersion = version ?? ImpellerNative.ImpellerGetVersion();
        var native = EnsureSuccess(
            ImpellerNative.ImpellerContextCreateMetalNew(resolvedVersion),
            "Failed to create Impeller Metal context.");

        var handle = new ImpellerContextHandle(native);
        ImpellerDiagnostics.ContextCreated("Metal", resolvedVersion);
        return handle;
    }

    public static ImpellerContextHandle CreateOpenGLES(
        ImpellerProcAddressCallback callback,
        IntPtr userData,
        uint? version = null)
    {
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        var resolvedVersion = version ?? ImpellerNative.ImpellerGetVersion();
        var callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);

        var native = EnsureSuccess(
            ImpellerNative.ImpellerContextCreateOpenGLESNew(resolvedVersion, callbackPtr, userData),
            "Failed to create Impeller OpenGL ES context.");

        var handle = new ImpellerContextHandle(native, openGlCallback: callback);
        ImpellerDiagnostics.ContextCreated("OpenGLES", resolvedVersion);
        return handle;
    }

    public static ImpellerContextHandle CreateVulkan(
        ImpellerContextVulkanSettings settings,
        uint? version = null)
    {
        if (settings.ProcAddressCallback is null)
        {
            throw new ArgumentException("Vulkan proc address callback must be supplied.", nameof(settings));
        }

        var resolvedVersion = version ?? ImpellerNative.ImpellerGetVersion();

        var nativeSettings = new ImpellerContextVulkanSettingsNative
        {
            UserData = settings.UserData,
            ProcAddressCallback = Marshal.GetFunctionPointerForDelegate(settings.ProcAddressCallback),
            EnableValidation = settings.EnableValidation,
        };

        var native = EnsureSuccess(
            ImpellerNative.ImpellerContextCreateVulkanNew(resolvedVersion, ref nativeSettings),
            "Failed to create Impeller Vulkan context.");

        var handle = new ImpellerContextHandle(native, vulkanCallback: settings.ProcAddressCallback);
        ImpellerDiagnostics.ContextCreated("Vulkan", resolvedVersion);
        return handle;
    }

    public bool TryGetVulkanInfo(out ImpellerContextVulkanInfo info)
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerContextHandle));
        }

        return ImpellerNative.ImpellerContextGetVulkanInfo(handle, out info);
    }

    internal void Retain()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerContextHandle));
        }

        ImpellerNative.ImpellerContextRetain(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerContextRelease(handle);
        return true;
    }

    internal new nint DangerousGetHandle()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerContextHandle));
        }

        return handle;
    }
}
