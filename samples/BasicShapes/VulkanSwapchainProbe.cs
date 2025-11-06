using System;
using System.Globalization;
using ImpellerSharp.Interop;
using ImpellerSharp.Samples.BasicShapes.Scenes;

namespace ImpellerSharp.Samples.BasicShapes;

internal static class VulkanSwapchainProbe
{
    internal static void Run(ImpellerContextHandle context, nint? configuredSurface)
    {
        if (context is null || context.IsInvalid)
        {
            return;
        }

        var surfaceHandle = configuredSurface ?? ParseSurfaceFromEnvironment();
        if (surfaceHandle == nint.Zero)
        {
            SceneExecutionContext.LastVulkanSwapchainProbe = "skipped";
            return;
        }

        try
        {
            using var swapchain = context.CreateVulkanSwapchain(surfaceHandle);
            using var surface = swapchain.AcquireNextSurface();
            SceneExecutionContext.LastVulkanSwapchainProbe = surface is null ? "acquired-null" : "acquired-surface";
        }
        catch (Exception ex)
        {
            SceneExecutionContext.LastVulkanSwapchainProbe = $"failed:{ex.GetType().Name}";
        }
    }

    private static nint ParseSurfaceFromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable("IMPELLER_VK_SURFACE");
        if (string.IsNullOrWhiteSpace(value))
        {
            return nint.Zero;
        }

        value = value.Trim();

        try
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (ulong.TryParse(value.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex))
                {
                    return new nint(unchecked((long)hex));
                }
            }
            else if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dec))
            {
                return new nint(dec);
            }
        }
        catch
        {
            // Ignore invalid environment values and treat as unset.
        }

        return nint.Zero;
    }
}
