using System;
using System.Text;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerTypographyContextHandle : ImpellerSafeHandle
{
    private ImpellerTypographyContextHandle(nint native)
    {
        SetHandle(native);
    }

    public static ImpellerTypographyContextHandle Create()
    {
        var native = ImpellerNative.ImpellerTypographyContextNew();
        return new ImpellerTypographyContextHandle(
            EnsureSuccess(native, "Failed to create Impeller typography context."));
    }

    public bool RegisterFont(ReadOnlySpan<byte> fontData, string? familyAlias = null)
    {
        ThrowIfInvalid();

        var mapping = ImpellerMappingUtilities.Create(fontData, out var userData);

        using var alias = new Utf8String(familyAlias);
        var result = ImpellerNative.ImpellerTypographyContextRegisterFont(
            handle,
            in mapping,
            userData,
            (byte*)alias.Pointer);

        if (!result)
        {
            ImpellerMappingUtilities.Free(userData);
        }

        return result;
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerTypographyContextRetain(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerTypographyContextRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerTypographyContextHandle));
        }
    }
}
