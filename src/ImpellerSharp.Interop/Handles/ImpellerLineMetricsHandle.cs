using System;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerLineMetricsHandle : ImpellerSafeHandle
{
    private ImpellerLineMetricsHandle(nint native)
    {
        SetHandle(native);
    }

    internal static ImpellerLineMetricsHandle? FromOwned(nint native)
    {
        return native == nint.Zero
            ? null
            : new ImpellerLineMetricsHandle(native);
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerLineMetricsRetain(handle);
    }

    public double GetUnscaledAscent(uint line) => Query(line, ImpellerNative.ImpellerLineMetricsGetUnscaledAscent);

    public double GetAscent(uint line) => Query(line, ImpellerNative.ImpellerLineMetricsGetAscent);

    public double GetDescent(uint line) => Query(line, ImpellerNative.ImpellerLineMetricsGetDescent);

    public double GetBaseline(uint line) => Query(line, ImpellerNative.ImpellerLineMetricsGetBaseline);

    public bool IsHardBreak(uint line)
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerLineMetricsIsHardbreak(handle, line);
    }

    public double GetWidth(uint line) => Query(line, ImpellerNative.ImpellerLineMetricsGetWidth);

    public double GetHeight(uint line) => Query(line, ImpellerNative.ImpellerLineMetricsGetHeight);

    public double GetLeft(uint line) => Query(line, ImpellerNative.ImpellerLineMetricsGetLeft);

    public ulong GetCodeUnitStartIndex(uint line)
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerLineMetricsGetCodeUnitStartIndex(handle, line);
    }

    public ulong GetCodeUnitEndIndex(uint line)
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerLineMetricsGetCodeUnitEndIndex(handle, line);
    }

    public ulong GetCodeUnitEndIndexExcludingWhitespace(uint line)
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerLineMetricsGetCodeUnitEndIndexExcludingWhitespace(handle, line);
    }

    public ulong GetCodeUnitEndIndexIncludingNewline(uint line)
    {
        ThrowIfInvalid();
        return ImpellerNative.ImpellerLineMetricsGetCodeUnitEndIndexIncludingNewline(handle, line);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerLineMetricsRelease(handle);
        return true;
    }

    internal new nint DangerousGetHandle()
    {
        ThrowIfInvalid();
        return handle;
    }

    private double Query(uint line, Func<nint, nuint, double> accessor)
    {
        ThrowIfInvalid();
        return accessor(handle, line);
    }

    private void ThrowIfInvalid()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerLineMetricsHandle));
        }
    }
}
