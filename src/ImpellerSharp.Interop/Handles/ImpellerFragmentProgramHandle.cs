using System;

namespace ImpellerSharp.Interop;

public sealed class ImpellerFragmentProgramHandle : ImpellerSafeHandle
{
    private ImpellerFragmentProgramHandle(nint native, bool ownsHandle)
        : base(ownsHandle)
    {
        SetHandle(native);
    }

    public static ImpellerFragmentProgramHandle Create(ReadOnlySpan<byte> compiledShader)
    {
        if (compiledShader.IsEmpty)
        {
            throw new ArgumentException("Fragment program data must not be empty.", nameof(compiledShader));
        }

        var mapping = ImpellerMappingUtilities.Create(compiledShader, out var userData);
        var native = ImpellerNative.ImpellerFragmentProgramNew(in mapping, userData);
        if (native == nint.Zero)
        {
            ImpellerMappingUtilities.Free(userData);
            throw new ImpellerInteropException("Failed to create Impeller fragment program.");
        }

        return new ImpellerFragmentProgramHandle(native, ownsHandle: true);
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerFragmentProgramRetain(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerFragmentProgramRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerFragmentProgramHandle));
        }
    }
}
