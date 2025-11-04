using System;

namespace ImpellerSharp.Interop;

public sealed class ImpellerDisplayListHandle : ImpellerSafeHandle
{
    private ImpellerDisplayListHandle(nint native)
    {
        SetHandle(native);
    }

    internal static ImpellerDisplayListHandle FromOwned(nint native)
    {
        return new ImpellerDisplayListHandle(
            EnsureSuccess(native, "Failed to create Impeller display list."));
    }

    internal void Retain()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerDisplayListHandle));
        }

        ImpellerNative.ImpellerDisplayListRetain(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerDisplayListRelease(handle);
        return true;
    }

    internal new nint DangerousGetHandle()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerDisplayListHandle));
        }

        return handle;
    }
}
