using System;

namespace ImpellerSharp.Interop;

public sealed class ImpellerMaskFilterHandle : ImpellerSafeHandle
{
    private ImpellerMaskFilterHandle(nint native)
    {
        SetHandle(native);
    }

    public static ImpellerMaskFilterHandle CreateBlur(ImpellerBlurStyle style, float sigma)
    {
        var native = ImpellerNative.ImpellerMaskFilterCreateBlurNew(style, sigma);
        return new ImpellerMaskFilterHandle(
            EnsureSuccess(native, "Failed to create Impeller mask filter (blur)."));
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerMaskFilterRetain(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerMaskFilterRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerMaskFilterHandle));
        }
    }
}
