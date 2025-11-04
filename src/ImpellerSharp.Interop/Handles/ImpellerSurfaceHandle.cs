using System;
using System.Diagnostics;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerSurfaceHandle : ImpellerSafeHandle
{
    private ImpellerSurfaceHandle(nint native)
    {
        SetHandle(native);
    }

    internal static ImpellerSurfaceHandle FromOwned(nint native)
    {
        return new ImpellerSurfaceHandle(EnsureSuccess(native, "Failed to create Impeller surface."));
    }

    public static ImpellerSurfaceHandle WrapMetalDrawable(ImpellerContextHandle context, nint metalDrawable)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (metalDrawable == nint.Zero)
        {
            throw new ArgumentException("CAMetalDrawable pointer must not be null.", nameof(metalDrawable));
        }

        var native = ImpellerNative.ImpellerSurfaceCreateWrappedMetalDrawableNew(
            context.DangerousGetHandle(),
            metalDrawable);

        return FromOwned(native);
    }

    public static ImpellerSurfaceHandle WrapFramebuffer(
        ImpellerContextHandle context,
        ulong framebuffer,
        ImpellerPixelFormat pixelFormat,
        ImpellerISize size)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (framebuffer == 0)
        {
            throw new ArgumentException("Framebuffer object must not be zero.", nameof(framebuffer));
        }

        var native = ImpellerNative.ImpellerSurfaceCreateWrappedFBONew(
            context.DangerousGetHandle(),
            framebuffer,
            pixelFormat,
            in size);

        return FromOwned(native);
    }

    public static ImpellerSurfaceHandle WrapMetalTexture(ImpellerContextHandle context, nint metalTexture)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (metalTexture == nint.Zero)
        {
            throw new ArgumentException("MTLTexture pointer must not be null.", nameof(metalTexture));
        }

        var native = ImpellerNative.ImpellerSurfaceCreateWrappedMetalTextureNew(
            context.DangerousGetHandle(),
            metalTexture);

        return FromOwned(native);
    }

    internal void Retain()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerSurfaceHandle));
        }

        ImpellerNative.ImpellerSurfaceRetain(handle);
    }

    public bool DrawDisplayList(ImpellerDisplayListHandle displayList)
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerSurfaceHandle));
        }

        if (displayList is null || displayList.IsInvalid)
        {
            throw new ArgumentNullException(nameof(displayList));
        }

        var dlHandle = displayList.DangerousGetHandle();
        using var activity = ImpellerDiagnostics.ActivitySource.StartActivity("ImpellerSurface.DrawDisplayList");
        var success = ImpellerCommandPointers.SurfaceDrawDisplayList(handle, dlHandle) != 0;
        ImpellerDiagnostics.SurfaceDrawDisplayList(success);
        return success;
    }

    public bool Present()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerSurfaceHandle));
        }

        return ImpellerNative.ImpellerSurfacePresent(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerSurfaceRelease(handle);
        return true;
    }

    internal new nint DangerousGetHandle()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerSurfaceHandle));
        }

        return handle;
    }
}
