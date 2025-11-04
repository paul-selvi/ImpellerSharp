using System;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerDisplayListBuilderHandle : ImpellerSafeHandle
{
    private ImpellerDisplayListBuilderHandle(nint native)
    {
        SetHandle(native);
    }

    public static ImpellerDisplayListBuilderHandle Create(ImpellerRect? cullRect = null)
    {
        nint builderPtr;
        if (cullRect.HasValue)
        {
            var rect = cullRect.Value;
            builderPtr = ImpellerNative.ImpellerDisplayListBuilderNew(&rect);
        }
        else
        {
            builderPtr = ImpellerNative.ImpellerDisplayListBuilderNew((ImpellerRect*)null);
        }

        return new ImpellerDisplayListBuilderHandle(
            EnsureSuccess(builderPtr, "Failed to create Impeller display list builder."));
    }

    internal void Retain()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerDisplayListBuilderHandle));
        }

        ImpellerNative.ImpellerDisplayListBuilderRetain(handle);
    }

    public void Save()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderSave(handle);
    }

    public void Restore()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderRestore(handle);
    }

    public void Translate(float x, float y)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderTranslate(handle, x, y);
    }

    public void Rotate(float degrees)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderRotate(handle, degrees);
    }

    public void DrawRect(in ImpellerRect rect, ImpellerPaintHandle paint)
    {
        if (paint is null || paint.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paint));
        }

        ThrowIfInvalid();
        var nativeRect = rect;
        ImpellerNative.ImpellerDisplayListBuilderDrawRect(handle, &nativeRect, paint.DangerousGetHandle());
    }

    public void DrawPaint(ImpellerPaintHandle paint)
    {
        if (paint is null || paint.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paint));
        }

        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderDrawPaint(handle, paint.DangerousGetHandle());
    }

    public void DrawPath(ImpellerPathHandle path, ImpellerPaintHandle paint)
    {
        if (path is null || path.IsInvalid)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (paint is null || paint.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paint));
        }

        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderDrawPath(
            handle,
            path.DangerousGetHandle(),
            paint.DangerousGetHandle());
    }

    public void DrawTexture(
        ImpellerTextureHandle texture,
        in ImpellerPoint point,
        ImpellerTextureSampling sampling,
        ImpellerPaintHandle? paint = null)
    {
        if (texture is null || texture.IsInvalid)
        {
            throw new ArgumentNullException(nameof(texture));
        }

        ThrowIfInvalid();
        var location = point;
        var paintHandle = paint?.DangerousGetHandle() ?? nint.Zero;
        ImpellerNative.ImpellerDisplayListBuilderDrawTexture(
            handle,
            texture.DangerousGetHandle(),
            &location,
            sampling,
            paintHandle);
    }

    public void DrawTextureRect(
        ImpellerTextureHandle texture,
        in ImpellerRect source,
        in ImpellerRect destination,
        ImpellerTextureSampling sampling,
        ImpellerPaintHandle? paint = null)
    {
        if (texture is null || texture.IsInvalid)
        {
            throw new ArgumentNullException(nameof(texture));
        }

        ThrowIfInvalid();
        var srcRect = source;
        var dstRect = destination;
        var paintHandle = paint?.DangerousGetHandle() ?? nint.Zero;
        ImpellerNative.ImpellerDisplayListBuilderDrawTextureRect(
            handle,
            texture.DangerousGetHandle(),
            &srcRect,
            &dstRect,
            sampling,
            paintHandle);
    }

    public void DrawParagraph(
        ImpellerParagraphHandle paragraph,
        in ImpellerPoint origin)
    {
        if (paragraph is null || paragraph.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paragraph));
        }

        ThrowIfInvalid();
        var location = origin;
        ImpellerNative.ImpellerDisplayListBuilderDrawParagraph(
            handle,
            paragraph.DangerousGetHandle(),
            &location);
    }

    public ImpellerDisplayListHandle Build()
    {
        ThrowIfInvalid();
        var dl = ImpellerNative.ImpellerDisplayListBuilderCreateDisplayListNew(handle);
        return ImpellerDisplayListHandle.FromOwned(dl);
    }

    internal new nint DangerousGetHandle()
    {
        ThrowIfInvalid();
        return handle;
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerDisplayListBuilderRelease(handle);
        return true;
    }

    private void ThrowIfInvalid()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerDisplayListBuilderHandle));
        }
    }
}
