using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerTextureHandle : ImpellerSafeHandle
{
    private ImpellerTextureHandle(nint handle)
    {
        SetHandle(handle);
    }

    internal static ImpellerTextureHandle FromOwned(nint native)
    {
        return new ImpellerTextureHandle(
            ImpellerSafeHandle.EnsureSuccess(native, "Failed to create Impeller texture."));
    }

    public static ImpellerTextureHandle CreateWithContents(
        ImpellerContextHandle context,
        in ImpellerTextureDescriptor descriptor,
        ReadOnlySpan<byte> contents)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (contents.IsEmpty)
        {
            throw new ArgumentException("Texture data must not be empty.", nameof(contents));
        }

        var mapping = ImpellerMappingUtilities.Create(contents, out var userData);
        var addedRef = false;
        nint native = nint.Zero;

        try
        {
            context.DangerousAddRef(ref addedRef);
            native = ImpellerNative.ImpellerTextureCreateWithContentsNew(
                context.DangerousGetHandle(),
                in descriptor,
                in mapping,
                userData);
        }
        finally
        {
            if (native == nint.Zero)
            {
                ImpellerMappingUtilities.Free(userData);
            }

            if (addedRef)
            {
                context.DangerousRelease();
            }
        }

        return FromOwned(native);
    }

    public static ImpellerTextureHandle CreateWithOpenGLHandle(
        ImpellerContextHandle context,
        in ImpellerTextureDescriptor descriptor,
        ulong textureHandle)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var addedRef = false;
        nint native = nint.Zero;

        try
        {
            context.DangerousAddRef(ref addedRef);
            native = ImpellerNative.ImpellerTextureCreateWithOpenGLTextureHandleNew(
                context.DangerousGetHandle(),
                in descriptor,
                textureHandle);
        }
        finally
        {
            if (addedRef)
            {
                context.DangerousRelease();
            }
        }

        return FromOwned(native);
    }

    public ulong GetOpenGLHandle()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerTextureGetOpenGLHandle(handle);
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerTextureRetain(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerTextureRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerTextureHandle));
        }
    }
}
