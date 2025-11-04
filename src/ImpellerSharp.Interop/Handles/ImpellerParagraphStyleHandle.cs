using System;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerParagraphStyleHandle : ImpellerSafeHandle
{
    private ImpellerParagraphStyleHandle(nint native)
    {
        SetHandle(native);
    }

    public static ImpellerParagraphStyleHandle Create()
    {
        var native = ImpellerNative.ImpellerParagraphStyleNew();
        return new ImpellerParagraphStyleHandle(
            EnsureSuccess(native, "Failed to create Impeller paragraph style."));
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphStyleRetain(handle);
    }

    public void SetForeground(ImpellerPaintHandle paint)
    {
        ThrowIfInvalid();
        if (paint is null || paint.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paint));
        }

        ImpellerNative.ImpellerParagraphStyleSetForeground(handle, paint.DangerousGetHandle());
    }

    public void SetBackground(ImpellerPaintHandle paint)
    {
        ThrowIfInvalid();
        if (paint is null || paint.IsInvalid)
        {
            throw new ArgumentNullException(nameof(paint));
        }

        ImpellerNative.ImpellerParagraphStyleSetBackground(handle, paint.DangerousGetHandle());
    }

    public void SetFontWeight(ImpellerFontWeight weight)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphStyleSetFontWeight(handle, weight);
    }

    public void SetFontStyle(ImpellerFontStyle style)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphStyleSetFontStyle(handle, style);
    }

    public void SetFontFamily(string? family)
    {
        ThrowIfInvalid();
        using var utf8 = new Utf8String(family);
        ImpellerNative.ImpellerParagraphStyleSetFontFamily(handle, (byte*)utf8.Pointer);
    }

    public void SetFontSize(float size)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphStyleSetFontSize(handle, size);
    }

    public void SetHeight(float height)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphStyleSetHeight(handle, height);
    }

    public void SetTextAlignment(ImpellerTextAlignment alignment)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphStyleSetTextAlignment(handle, alignment);
    }

    public void SetTextDirection(ImpellerTextDirection direction)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphStyleSetTextDirection(handle, direction);
    }

    public void SetTextDecoration(ImpellerTextDecoration decoration)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphStyleSetTextDecoration(handle, &decoration);
    }

    public void SetMaxLines(uint maxLines)
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphStyleSetMaxLines(handle, maxLines);
    }

    public void SetLocale(string? locale)
    {
        ThrowIfInvalid();
        using var utf8 = new Utf8String(locale);
        ImpellerNative.ImpellerParagraphStyleSetLocale(handle, (byte*)utf8.Pointer);
    }

    public void SetEllipsis(string? ellipsis)
    {
        ThrowIfInvalid();
        using var utf8 = new Utf8String(ellipsis);
        ImpellerNative.ImpellerParagraphStyleSetEllipsis(handle, (byte*)utf8.Pointer);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerParagraphStyleRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerParagraphStyleHandle));
        }
    }
}
