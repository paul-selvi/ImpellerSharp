using System;

namespace ImpellerSharp.Interop;

public sealed class ImpellerPathHandle : ImpellerSafeHandle
{
    private ImpellerPathHandle(nint native)
    {
        SetHandle(native);
    }

    internal static ImpellerPathHandle FromOwned(nint native)
    {
        return new ImpellerPathHandle(
            EnsureSuccess(native, "Failed to create Impeller path."));
    }

    internal void Retain()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerPathHandle));
        }

        ImpellerNative.ImpellerPathRetain(handle);
    }

    public ImpellerRect GetBounds()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerPathHandle));
        }

        unsafe
        {
            ImpellerRect bounds = default;
            ImpellerNative.ImpellerPathGetBounds(handle, &bounds);
            return bounds;
        }
    }

    internal new nint DangerousGetHandle()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerPathHandle));
        }

        return handle;
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerPathRelease(handle);
        return true;
    }
}
