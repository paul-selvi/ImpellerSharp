using System;
using System.Collections.Generic;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Samples.BasicShapes;

internal static class BackendContextFactory
{
    internal static bool TryCreateContext(SampleOptions options, out ImpellerContextHandle? context, out string resolvedBackend, out string? error)
    {
        string? lastError = null;
        foreach (var candidate in ResolveBackends(options.Backend))
        {
            try
            {
                ImpellerContextHandle handle;
                string actualBackend;
                switch (candidate)
                {
                    case "metal":
                        handle = CreateMetalContext();
                        actualBackend = "metal";
                        break;
                    case "vulkan":
                        handle = CreateVulkanContext(out actualBackend);
                        break;
                    default:
                        throw new NotSupportedException($"Backend '{candidate}' is not supported in headless mode.");
                }

                context = handle;
                resolvedBackend = actualBackend;
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }
        }

        context = null;
        resolvedBackend = options.Backend;
        error = lastError ?? "No suitable backend available.";
        return false;
    }

    private static IEnumerable<string> ResolveBackends(string backendPreference)
    {
        switch (backendPreference)
        {
            case "auto":
                if (OperatingSystem.IsMacOS())
                {
                    yield return "metal";
                    yield return "vulkan";
                }
                else
                {
                    yield return "vulkan";
                }
                break;
            case "metal":
                yield return "metal";
                break;
            case "vulkan":
                yield return "vulkan";
                break;
            case "opengles":
                throw new NotSupportedException("OpenGLES backend is not yet supported in this sample.");
            default:
                throw new ArgumentException($"Unsupported backend '{backendPreference}'.", nameof(backendPreference));
        }
    }

    private static ImpellerContextHandle CreateMetalContext()
    {
        if (!OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("Metal backend is only available on macOS.");
        }

        return ImpellerContextHandle.CreateMetal();
    }

    private static ImpellerContextHandle CreateVulkanContext(out string resolvedBackend)
    {
        if (!VulkanLoader.TryCreateSettings(out var settings, out var failure))
        {
            throw new InvalidOperationException(failure ?? "Unable to configure Vulkan backend.");
        }

        resolvedBackend = "vulkan";
        return ImpellerContextHandle.CreateVulkan(settings);
    }
}
