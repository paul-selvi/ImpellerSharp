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

        var mapping = ImpellerMappingUtilities.Create(contents, out var userData);

        var native = ImpellerNative.ImpellerTextureCreateWithContentsNew(
            context.DangerousGetHandle(),
            in descriptor,
            in mapping,
            userData);

        if (native == nint.Zero)
        {
            ImpellerMappingUtilities.Free(userData);
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

        var native = ImpellerNative.ImpellerTextureCreateWithOpenGLTextureHandleNew(
            context.DangerousGetHandle(),
            in descriptor,
            textureHandle);

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
