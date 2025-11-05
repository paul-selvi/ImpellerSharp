using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Benchmarks;

internal sealed unsafe class BenchmarkContextProvider : IDisposable
{
    private ImpellerContextHandle? _context;
    private string? _failure;
    private static IntPtr _vulkanLibrary;
    private static bool _attemptedVulkanLoad;
    private static unsafe delegate* unmanaged[Cdecl]<IntPtr, byte*, IntPtr> _vkGetInstanceProcAddr;
    private static readonly ImpellerVulkanProcAddressCallback VulkanCallback = ResolveVulkanProcAddress;

    internal ImpellerContextHandle? Context => _context;

    internal bool TryEnsureContext(out string? failureReason)
    {
        if (_context is not null)
        {
            failureReason = null;
            return true;
        }

        try
        {
            _context = CreatePreferredContext();
            failureReason = null;
            return true;
        }
        catch (Exception ex)
        {
            _failure = ex.Message;
            failureReason = _failure;
            _context = null;
            return false;
        }
    }

    private static ImpellerContextHandle CreatePreferredContext()
    {
        if (OperatingSystem.IsMacOS())
        {
            try
            {
                return ImpellerContextHandle.CreateMetal();
            }
            catch
            {
                // Fallback to MoltenVK if available.
            }
        }

        try
        {
            return CreateVulkanContext();
        }
        catch (ImpellerInteropException)
        {
            // Fall through to platform-specific fallbacks.
        }
        catch (DllNotFoundException)
        {
        }

        if (OperatingSystem.IsMacOS())
        {
            throw new InvalidOperationException(
                "Unable to create Impeller Metal or MoltenVK context. Ensure Metal is available or MoltenVK is installed.");
        }

        if (OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException(
                "Unable to create Impeller Vulkan context on Windows. Install the Vulkan SDK or verify drivers.");
        }

        if (OperatingSystem.IsLinux())
        {
            throw new InvalidOperationException(
                "Unable to create Impeller Vulkan context on Linux. Ensure the Vulkan loader and drivers are available.");
        }

        throw new PlatformNotSupportedException("Unsupported platform for Impeller benchmarks.");
    }

    private static ImpellerContextHandle CreateVulkanContext()
    {
        if (!EnsureVulkanLoader(out var failure))
        {
            throw new InvalidOperationException(failure ?? "Unable to load Vulkan loader.");
        }

        var settings = new ImpellerContextVulkanSettings
        {
            ProcAddressCallback = VulkanCallback,
            UserData = IntPtr.Zero,
            EnableValidation = false,
        };

        return ImpellerContextHandle.CreateVulkan(settings);
    }

    private static bool EnsureVulkanLoader(out string? failure)
    {
        if (_vkGetInstanceProcAddr != null)
        {
            failure = null;
            return true;
        }

        if (_attemptedVulkanLoad && _vkGetInstanceProcAddr == null)
        {
            failure = "Vulkan loader was previously attempted but not found.";
            return false;
        }

        _attemptedVulkanLoad = true;

        foreach (var name in GetVulkanLibraryNames())
        {
            if (NativeLibrary.TryLoad(name, out var handle))
            {
                _vulkanLibrary = handle;
                break;
            }
        }

        if (_vulkanLibrary == IntPtr.Zero)
        {
            failure = "Unable to locate a Vulkan loader library (e.g., libvulkan).";
            return false;
        }

        if (!NativeLibrary.TryGetExport(_vulkanLibrary, "vkGetInstanceProcAddr", out var export))
        {
            failure = "vkGetInstanceProcAddr symbol missing from Vulkan loader.";
            return false;
        }

        unsafe
        {
            _vkGetInstanceProcAddr = (delegate* unmanaged[Cdecl]<IntPtr, byte*, IntPtr>)export;
        }

        failure = null;
        return true;
    }

    private static IEnumerable<string> GetVulkanLibraryNames()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return "vulkan-1.dll";
        }
        else if (OperatingSystem.IsMacOS())
        {
            yield return "libvulkan.dylib";
            yield return "/usr/local/lib/libvulkan.dylib";
        }
        else
        {
            yield return "libvulkan.so.1";
            yield return "libvulkan.so";
        }
    }

    private static unsafe IntPtr ResolveVulkanProcAddress(IntPtr instance, string procName, IntPtr userData)
    {
        if (_vkGetInstanceProcAddr == null)
        {
            return IntPtr.Zero;
        }

        if (procName == "vkGetInstanceProcAddr")
        {
            return (IntPtr)_vkGetInstanceProcAddr;
        }

        var utf8 = Encoding.UTF8.GetBytes(procName + "\0");
        fixed (byte* namePtr = utf8)
        {
            return _vkGetInstanceProcAddr(instance, namePtr);
        }
    }

    public void Dispose()
    {
        _context?.Dispose();
        _context = null;
    }
}
