using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

public abstract class ImpellerSafeHandle : SafeHandle
{
    protected ImpellerSafeHandle()
        : base(IntPtr.Zero, ownsHandle: true)
    {
    }

    protected ImpellerSafeHandle(bool ownsHandle)
        : base(IntPtr.Zero, ownsHandle)
    {
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected static nint EnsureSuccess(nint native, string message)
    {
        if (native == nint.Zero)
        {
            throw new ImpellerInteropException(message);
        }

        return native;
    }
}
