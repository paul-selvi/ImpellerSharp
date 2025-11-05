using System;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerGlyphInfoHandle : ImpellerSafeHandle
{
    private ImpellerGlyphInfoHandle(nint native)
    {
        SetHandle(native);
    }

    internal static ImpellerGlyphInfoHandle? FromOwned(nint native)
    {
        return native == nint.Zero
            ? null
            : new ImpellerGlyphInfoHandle(native);
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerGlyphInfoRetain(handle);
    }

    public (ulong Start, ulong End) GetGraphemeClusterCodeUnitRange()
    {
        ThrowIfInvalid();
        var begin = ImpellerNative.ImpellerGlyphInfoGetGraphemeClusterCodeUnitRangeBegin(handle);
        var end = ImpellerNative.ImpellerGlyphInfoGetGraphemeClusterCodeUnitRangeEnd(handle);
        return (begin, end);
    }

    public ImpellerRect GetGraphemeClusterBounds()
    {
        ThrowIfInvalid();
        ImpellerRect rect;
        ImpellerNative.ImpellerGlyphInfoGetGraphemeClusterBounds(handle, &rect);
        return rect;
    }

    public bool IsEllipsis()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerGlyphInfoIsEllipsis(handle);
    }

    public ImpellerTextDirection GetTextDirection()
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerGlyphInfoGetTextDirection(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerGlyphInfoRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerGlyphInfoHandle));
        }
    }
}
