using System;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerPathBuilderHandle : ImpellerSafeHandle
{
    private ImpellerPathBuilderHandle(nint native)
    {
        SetHandle(native);
    }

    public static ImpellerPathBuilderHandle Create()
    {
        var native = ImpellerNative.ImpellerPathBuilderNew();
        return new ImpellerPathBuilderHandle(
            EnsureSuccess(native, "Failed to create Impeller path builder."));
    }

    internal void Retain()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerPathBuilderHandle));
        }

        ImpellerNative.ImpellerPathBuilderRetain(handle);
    }

    public void MoveTo(in ImpellerPoint point)
    {
        ThrowIfInvalid();
        var location = point;
        ImpellerNative.ImpellerPathBuilderMoveTo(handle, &location);
    }

    public void LineTo(in ImpellerPoint point)
    {
        ThrowIfInvalid();
        var location = point;
        ImpellerNative.ImpellerPathBuilderLineTo(handle, &location);
    }

    public void QuadraticCurveTo(in ImpellerPoint controlPoint, in ImpellerPoint endPoint)
    {
        ThrowIfInvalid();
        var control = controlPoint;
        var end = endPoint;
        ImpellerNative.ImpellerPathBuilderQuadraticCurveTo(handle, &control, &end);
    }

    public void CubicCurveTo(in ImpellerPoint controlPoint1, in ImpellerPoint controlPoint2, in ImpellerPoint endPoint)
    {
        ThrowIfInvalid();
        var c1 = controlPoint1;
        var c2 = controlPoint2;
        var end = endPoint;
        ImpellerNative.ImpellerPathBuilderCubicCurveTo(handle, &c1, &c2, &end);
    }

    public ImpellerPathHandle TakePath(ImpellerFillType fillType = ImpellerFillType.NonZero)
    {
        ThrowIfInvalid();
        var native = ImpellerNative.ImpellerPathBuilderTakePathNew(handle, fillType);
        return ImpellerPathHandle.FromOwned(native);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerPathBuilderRelease(handle);
        return true;
    }

    private void ThrowIfInvalid()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerPathBuilderHandle));
        }
    }
}
