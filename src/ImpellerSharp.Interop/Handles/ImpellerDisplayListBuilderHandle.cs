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

    public void SaveLayer(in ImpellerRect bounds, ImpellerPaintHandle? paint = null, ImpellerImageFilterHandle? backdrop = null)
    {
        ThrowIfInvalid();

        var rect = bounds;
        var paintHandle = paint?.DangerousGetHandle() ?? nint.Zero;
        var backdropHandle = backdrop?.DangerousGetHandle() ?? nint.Zero;

        ImpellerNative.ImpellerDisplayListBuilderSaveLayer(handle, &rect, paintHandle, backdropHandle);
    }

    public void Translate(float x, float y)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderTranslate(handle, x, y);
    }

    public void Scale(float x, float y)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderScale(handle, x, y);
    }

    public void Rotate(float degrees)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderRotate(handle, degrees);
    }

    public void Transform(in ImpellerMatrix transform)
    {
        ThrowIfInvalid();
        var matrix = transform;
        ImpellerNative.ImpellerDisplayListBuilderTransform(handle, &matrix);
    }

    public void SetTransform(in ImpellerMatrix transform)
    {
        ThrowIfInvalid();
        var matrix = transform;
        ImpellerNative.ImpellerDisplayListBuilderSetTransform(handle, &matrix);
    }

    public ImpellerMatrix GetTransform()
    {
        ThrowIfInvalid();
        ImpellerMatrix matrix = default;
        ImpellerNative.ImpellerDisplayListBuilderGetTransform(handle, &matrix);
        return matrix;
    }

    public void ResetTransform()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderResetTransform(handle);
    }

    public uint GetSaveCount()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerDisplayListBuilderGetSaveCount(handle);
    }

    public void RestoreToCount(uint count)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderRestoreToCount(handle, count);
    }

    public void ClipRect(in ImpellerRect rect, ImpellerClipOperation operation)
    {
        ThrowIfInvalid();
        var value = rect;
        ImpellerNative.ImpellerDisplayListBuilderClipRect(handle, &value, operation);
    }

    public void ClipOval(in ImpellerRect bounds, ImpellerClipOperation operation)
    {
        ThrowIfInvalid();
        var rect = bounds;
        ImpellerNative.ImpellerDisplayListBuilderClipOval(handle, &rect, operation);
    }

    public void ClipRoundedRect(in ImpellerRect rect, in ImpellerRoundingRadii radii, ImpellerClipOperation operation)
    {
        ThrowIfInvalid();
        var r = rect;
        var corner = radii;
        ImpellerNative.ImpellerDisplayListBuilderClipRoundedRect(handle, &r, &corner, operation);
    }

    public void ClipPath(ImpellerPathHandle path, ImpellerClipOperation operation)
    {
        if (path is null || path.IsInvalid)
        {
            throw new ArgumentNullException(nameof(path));
        }

        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderClipPath(handle, path.DangerousGetHandle(), operation);
    }

    public void DrawLine(in ImpellerPoint from, in ImpellerPoint to, ImpellerPaintHandle paint)
    {
        if (paint is null || paint.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paint));
        }

        ThrowIfInvalid();
        var start = from;
        var end = to;
        ImpellerNative.ImpellerDisplayListBuilderDrawLine(handle, &start, &end, paint.DangerousGetHandle());
    }

    public void DrawDashedLine(in ImpellerPoint from, in ImpellerPoint to, float onLength, float offLength, ImpellerPaintHandle paint)
    {
        if (paint is null || paint.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paint));
        }

        ThrowIfInvalid();
        var start = from;
        var end = to;
        ImpellerNative.ImpellerDisplayListBuilderDrawDashedLine(
            handle,
            &start,
            &end,
            onLength,
            offLength,
            paint.DangerousGetHandle());
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

    public void DrawOval(in ImpellerRect bounds, ImpellerPaintHandle paint)
    {
        if (paint is null || paint.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paint));
        }

        ThrowIfInvalid();
        var rect = bounds;
        ImpellerNative.ImpellerDisplayListBuilderDrawOval(handle, &rect, paint.DangerousGetHandle());
    }

    public void DrawRoundedRect(in ImpellerRect rect, in ImpellerRoundingRadii radii, ImpellerPaintHandle paint)
    {
        if (paint is null || paint.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paint));
        }

        ThrowIfInvalid();
        var value = rect;
        var rounded = radii;
        ImpellerNative.ImpellerDisplayListBuilderDrawRoundedRect(handle, &value, &rounded, paint.DangerousGetHandle());
    }

    public void DrawRoundedRectDifference(
        in ImpellerRect outerRect,
        in ImpellerRoundingRadii outerRadii,
        in ImpellerRect innerRect,
        in ImpellerRoundingRadii innerRadii,
        ImpellerPaintHandle paint)
    {
        if (paint is null || paint.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paint));
        }

        ThrowIfInvalid();
        var outer = outerRect;
        var outerRound = outerRadii;
        var inner = innerRect;
        var innerRound = innerRadii;
        ImpellerNative.ImpellerDisplayListBuilderDrawRoundedRectDifference(
            handle,
            &outer,
            &outerRound,
            &inner,
            &innerRound,
            paint.DangerousGetHandle());
    }

    public void DrawDisplayList(ImpellerDisplayListHandle displayList, float opacity = 1.0f)
    {
        if (displayList is null || displayList.IsInvalid)
        {
            throw new ArgumentNullException(nameof(displayList));
        }

        ThrowIfInvalid();
        ImpellerNative.ImpellerDisplayListBuilderDrawDisplayList(handle, displayList.DangerousGetHandle(), opacity);
    }

    public void DrawShadow(
        ImpellerPathHandle path,
        in ImpellerColor color,
        float elevation,
        bool occluderIsTransparent,
        float devicePixelRatio)
    {
        if (path is null || path.IsInvalid)
        {
            throw new ArgumentNullException(nameof(path));
        }

        ThrowIfInvalid();
        var value = color;
        ImpellerNative.ImpellerDisplayListBuilderDrawShadow(
            handle,
            path.DangerousGetHandle(),
            &value,
            elevation,
            occluderIsTransparent,
            devicePixelRatio);
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
