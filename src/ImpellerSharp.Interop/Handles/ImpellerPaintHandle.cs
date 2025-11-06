using System;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerPaintHandle : ImpellerSafeHandle
{
    private ImpellerPaintHandle(nint native)
    {
        SetHandle(native);
    }

    public static ImpellerPaintHandle Create()
    {
        var native = ImpellerNative.ImpellerPaintNew();
        return new ImpellerPaintHandle(
            EnsureSuccess(native, "Failed to create Impeller paint."));
    }

    internal void Retain()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerPaintHandle));
        }

        ImpellerNative.ImpellerPaintRetain(handle);
    }

    public void SetColor(in ImpellerColor color)
    {
        ThrowIfInvalid();
        var value = color;
        ImpellerNative.ImpellerPaintSetColor(handle, &value);
    }

    public void SetBlendMode(ImpellerBlendMode blendMode)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerPaintSetBlendMode(handle, blendMode);
    }

    public void SetDrawStyle(ImpellerDrawStyle drawStyle)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerPaintSetDrawStyle(handle, drawStyle);
    }

    public void SetStrokeCap(ImpellerStrokeCap strokeCap)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerPaintSetStrokeCap(handle, strokeCap);
    }

    public void SetStrokeJoin(ImpellerStrokeJoin strokeJoin)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerPaintSetStrokeJoin(handle, strokeJoin);
    }

    public void SetStrokeWidth(float width)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerPaintSetStrokeWidth(handle, width);
    }

    public void SetStrokeMiter(float miterLimit)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerPaintSetStrokeMiter(handle, miterLimit);
    }

    public void SetColorSource(ImpellerColorSourceHandle? colorSource)
    {
        ThrowIfInvalid();
        var native = colorSource?.DangerousGetHandle() ?? nint.Zero;
        ImpellerNative.ImpellerPaintSetColorSource(handle, native);
    }

    public void SetColorFilter(ImpellerColorFilterHandle colorFilter)
    {
        if (colorFilter is null)
        {
            throw new ArgumentNullException(nameof(colorFilter));
        }

        ThrowIfInvalid();
        ImpellerNative.ImpellerPaintSetColorFilter(handle, colorFilter.DangerousGetHandle());
    }

    public void SetImageFilter(ImpellerImageFilterHandle imageFilter)
    {
        if (imageFilter is null)
        {
            throw new ArgumentNullException(nameof(imageFilter));
        }

        ThrowIfInvalid();
        ImpellerNative.ImpellerPaintSetImageFilter(handle, imageFilter.DangerousGetHandle());
    }

    public void SetMaskFilter(ImpellerMaskFilterHandle maskFilter)
    {
        if (maskFilter is null)
        {
            throw new ArgumentNullException(nameof(maskFilter));
        }

        ThrowIfInvalid();
        ImpellerNative.ImpellerPaintSetMaskFilter(handle, maskFilter.DangerousGetHandle());
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerPaintRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerPaintHandle));
        }
    }
}
