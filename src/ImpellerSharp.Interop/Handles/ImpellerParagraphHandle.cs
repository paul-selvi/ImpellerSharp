using System;

namespace ImpellerSharp.Interop;

public sealed class ImpellerParagraphHandle : ImpellerSafeHandle
{
    private ImpellerParagraphHandle(nint native)
    {
        SetHandle(native);
    }

    internal static ImpellerParagraphHandle FromOwned(nint native)
    {
        return new ImpellerParagraphHandle(
            EnsureSuccess(native, "Failed to create Impeller paragraph."));
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphRetain(handle);
    }

    public float GetMaxWidth()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerParagraphGetMaxWidth(handle);
    }

    public float GetHeight()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerParagraphGetHeight(handle);
    }

    public float GetLongestLineWidth()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerParagraphGetLongestLineWidth(handle);
    }

    public float GetMinIntrinsicWidth()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerParagraphGetMinIntrinsicWidth(handle);
    }

    public float GetMaxIntrinsicWidth()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerParagraphGetMaxIntrinsicWidth(handle);
    }

    public float GetIdeographicBaseline()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerParagraphGetIdeographicBaseline(handle);
    }

    public float GetAlphabeticBaseline()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerParagraphGetAlphabeticBaseline(handle);
    }

    public uint GetLineCount()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerParagraphGetLineCount(handle);
    }

    public ImpellerRange GetWordBoundary(ulong codeUnitIndex)
    {
        ThrowIfInvalid();
        ImpellerRange range = default;
        unsafe
        {
            ImpellerNative.ImpellerParagraphGetWordBoundary(handle, (nuint)codeUnitIndex, &range);
        }

        return range;
    }

    public ImpellerLineMetricsHandle? GetLineMetrics()
    {
        ThrowIfInvalid();
        var native = ImpellerNative.ImpellerParagraphGetLineMetrics(handle);
        return ImpellerLineMetricsHandle.FromOwned(native);
    }

    public ImpellerGlyphInfoHandle? CreateGlyphInfoAtCodeUnitIndex(ulong codeUnitIndex)
    {
        ThrowIfInvalid();
        var native = ImpellerNative.ImpellerParagraphCreateGlyphInfoAtCodeUnitIndexNew(handle, (nuint)codeUnitIndex);
        return ImpellerGlyphInfoHandle.FromOwned(native);
    }

    public ImpellerGlyphInfoHandle? CreateGlyphInfoAtParagraphCoordinates(double x, double y)
    {
        ThrowIfInvalid();
        var native = ImpellerNative.ImpellerParagraphCreateGlyphInfoAtParagraphCoordinatesNew(handle, x, y);
        return ImpellerGlyphInfoHandle.FromOwned(native);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerParagraphRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerParagraphHandle));
        }
    }
}
