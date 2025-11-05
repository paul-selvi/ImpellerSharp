using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Samples.BasicShapes;

internal static unsafe class VulkanLoader
{
    private static readonly object Gate = new();
    private static bool _attemptedLoad;
    private static IntPtr _vulkanLibrary;
    private static unsafe delegate* unmanaged[Cdecl]<IntPtr, byte*, IntPtr> _vkGetInstanceProcAddr;
    private static readonly ImpellerVulkanProcAddressCallback Callback = ResolveProcAddress;

    internal static bool TryCreateSettings(out ImpellerContextVulkanSettings settings, out string? error)
    {
        lock (Gate)
        {
            if (!EnsureLoader(out error))
            {
                settings = default;
                return false;
            }
        }

        settings = new ImpellerContextVulkanSettings
        {
            ProcAddressCallback = Callback,
            UserData = IntPtr.Zero,
            EnableValidation = false,
        };
        error = null;
        return true;
    }

    private static bool EnsureLoader(out string? error)
    {
        if (_vkGetInstanceProcAddr != null)
        {
            error = null;
            return true;
        }

        if (_attemptedLoad && _vkGetInstanceProcAddr == null)
        {
            error = "Vulkan loader not found or vkGetInstanceProcAddr missing.";
            return false;
        }

        _attemptedLoad = true;

        foreach (var name in EnumerateCandidateLibraries())
        {
            if (NativeLibrary.TryLoad(name, out var handle))
            {
                _vulkanLibrary = handle;
                break;
            }
        }

        if (_vulkanLibrary == IntPtr.Zero)
        {
            error = "Unable to locate Vulkan loader (libvulkan).";
            return false;
        }

        if (!NativeLibrary.TryGetExport(_vulkanLibrary, "vkGetInstanceProcAddr", out var export))
        {
            error = "vkGetInstanceProcAddr symbol missing from Vulkan loader.";
            return false;
        }

        unsafe
        {
            _vkGetInstanceProcAddr = (delegate* unmanaged[Cdecl]<IntPtr, byte*, IntPtr>)export;
        }

        error = null;
        return true;
    }

    private static IEnumerable<string> EnumerateCandidateLibraries()
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

    private static unsafe IntPtr ResolveProcAddress(IntPtr instance, string procName, IntPtr userData)
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
}
