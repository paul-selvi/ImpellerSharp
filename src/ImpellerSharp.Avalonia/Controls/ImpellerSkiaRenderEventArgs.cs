using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Metal;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.Vulkan;
using SkiaSharp;

namespace ImpellerSharp.Avalonia.Controls;

/// <summary>
/// Event arguments containing the Skia lease used during Impeller rendering.
/// Exposes platform-specific hints so native backends can be detected.
/// </summary>
public sealed class ImpellerSkiaRenderEventArgs : IDisposable
{
    private readonly ISkiaSharpApiLease _lease;
    private ISkiaSharpPlatformGraphicsApiLease? _platformLease;
    private bool _platformLeaseRequested;

    internal ImpellerSkiaRenderEventArgs(ISkiaSharpApiLease lease)
    {
        _lease = lease;
    }

    public SKCanvas Canvas => _lease.SkCanvas;

    public GRContext? GrContext => _lease.GrContext;

    public SKSurface? Surface => _lease.SkSurface;

    public double CurrentOpacity => _lease.CurrentOpacity;

    /// <summary>
    /// Indicates which GPU backend the current lease is bound to (Metal/Vulkan/OpenGL/Software).
    /// </summary>
    public ImpellerSkiaBackend Backend
    {
        get
        {
            if (MetalDevice is not null)
            {
                return ImpellerSkiaBackend.Metal;
            }

            if (VulkanContext is not null)
            {
                return ImpellerSkiaBackend.Vulkan;
            }

            if (PlatformContext is not null)
            {
                return ImpellerSkiaBackend.OpenGlOrDirect3D;
            }

            return ImpellerSkiaBackend.Software;
        }
    }

    private ISkiaSharpPlatformGraphicsApiLease? EnsurePlatformLease()
    {
        if (!_platformLeaseRequested)
        {
            _platformLeaseRequested = true;
            _platformLease = _lease.TryLeasePlatformGraphicsApi();
        }

        return _platformLease;
    }

    public IPlatformGraphicsContext? PlatformContext => EnsurePlatformLease()?.Context;

    public IMetalDevice? MetalDevice => PlatformContext as IMetalDevice;

    public IVulkanPlatformGraphicsContext? VulkanContext => PlatformContext as IVulkanPlatformGraphicsContext;

    public bool TryGetMetalRenderTarget(out MetalRenderTargetInfo info)
    {
        info = default;

        if (MetalDevice is null)
        {
            return false;
        }

        if (GetFieldValue(_lease, "_context") is not { } drawingContext)
        {
            return false;
        }

        var session = GetFieldRecursive(drawingContext, typeof(ISkiaGpuRenderSession));
        if (session is null)
        {
            DumpFields("drawingContext", drawingContext);
            return false;
        }

        var metalSession = GetFieldRecursive(session, typeof(IMetalPlatformSurfaceRenderingSession))
            as IMetalPlatformSurfaceRenderingSession;
        if (metalSession is null)
        {
            DumpFields("session", session);
            return false;
        }

        return TryPopulateFromMetalSession(metalSession, out info);
    }

    public bool TryGetMetalDrawable(out nint drawable)
    {
        drawable = nint.Zero;
        if (!TryGetMetalRenderTarget(out var info))
        {
            return false;
        }

        drawable = info.Texture;
        return drawable != nint.Zero;
    }

    public void Dispose()
    {
        ResetPlatformLease();
        _lease.Dispose();
    }

    public void ResetPlatformLease()
    {
        if (_platformLeaseRequested)
        {
            _platformLeaseRequested = false;
            _platformLease?.Dispose();
            _platformLease = null;
        }
    }

    public readonly record struct MetalRenderTargetInfo(
        IntPtr Texture,
        PixelSize Size,
        double Scaling,
        bool IsYFlipped);

    private static object? GetFieldValue(object instance, string fieldName)
    {
        var type = instance.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                return field.GetValue(instance);
            }

            type = type.BaseType;
        }

        return null;
    }

    private static object? GetFieldRecursive(object instance, Type assignableType)
    {
        var type = instance.GetType();
        while (type != null)
        {
            var field = type.GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                var value = field.GetValue(instance);
                if (value != null)
                {
                    Debug.WriteLine($"[ImpellerSharp] {instance.GetType().Name}._session => {value.GetType().FullName}");
                    if (assignableType.IsInstanceOfType(value))
                    {
                        return value;
                    }

                    var nested = GetFieldRecursive(value, assignableType);
                    if (nested != null)
                    {
                        return nested;
                    }
                }
            }

            type = type.BaseType;
        }

        return null;
    }

    private static bool TryPopulateFromMetalSession(
        IMetalPlatformSurfaceRenderingSession metalSession,
        out MetalRenderTargetInfo info)
    {
        var sessionType = metalSession.GetType();

        var textureValue = sessionType.GetProperty("Texture")?.GetValue(metalSession);
        if (textureValue is not IntPtr texture || texture == IntPtr.Zero)
        {
            info = default;
            return false;
        }

        var sizeValue = sessionType.GetProperty("Size")?.GetValue(metalSession);
        var scalingValue = sessionType.GetProperty("Scaling")?.GetValue(metalSession);
        var flippedValue = sessionType.GetProperty("IsYFlipped")?.GetValue(metalSession);

        var size = sizeValue is PixelSize ps ? ps : default;
        var scaling = scalingValue is double d ? d : 1.0;
        var isYFlipped = flippedValue is bool b && b;

        info = new MetalRenderTargetInfo(texture, size, scaling, isYFlipped);
        return true;
    }

    private static void DumpFields(string label, object instance)
    {
        var type = instance.GetType();
        Debug.WriteLine($"[ImpellerSharp] Dumping fields for {label} ({type.FullName})");
        while (type != null)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var value = field.GetValue(instance);
                Debug.WriteLine($"[ImpellerSharp]   {type.Name}.{field.Name} ({field.FieldType.FullName}) = {value?.GetType().FullName ?? "null"}");
            }

            type = type.BaseType;
        }
    }
}

public enum ImpellerSkiaBackend
{
    Metal,
    Vulkan,
    OpenGlOrDirect3D,
    Software
}
