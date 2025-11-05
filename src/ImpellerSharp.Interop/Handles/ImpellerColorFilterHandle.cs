using System;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerColorFilterHandle : ImpellerSafeHandle
{
    private ImpellerColorFilterHandle(nint native)
    {
        SetHandle(native);
    }

    public static ImpellerColorFilterHandle CreateBlend(in ImpellerColor color, ImpellerBlendMode blendMode)
    {
        var value = color;
        var native = ImpellerNative.ImpellerColorFilterCreateBlendNew(&value, blendMode);
        return new ImpellerColorFilterHandle(
            EnsureSuccess(native, "Failed to create Impeller color filter (blend)."));
    }

    public static ImpellerColorFilterHandle CreateColorMatrix(in ImpellerColorMatrix matrix)
    {
        var value = matrix;
        var native = ImpellerNative.ImpellerColorFilterCreateColorMatrixNew(&value);
        return new ImpellerColorFilterHandle(
            EnsureSuccess(native, "Failed to create Impeller color filter (matrix)."));
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerColorFilterRetain(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerColorFilterRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerColorFilterHandle));
        }
    }
}
