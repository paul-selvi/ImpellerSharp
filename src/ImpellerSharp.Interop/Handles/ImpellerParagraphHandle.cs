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
