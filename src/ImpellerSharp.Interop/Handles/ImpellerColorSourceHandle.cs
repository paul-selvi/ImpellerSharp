using System;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerColorSourceHandle : ImpellerSafeHandle
{
    private ImpellerColorSourceHandle(nint native)
    {
        SetHandle(native);
    }

    internal static ImpellerColorSourceHandle FromOwned(nint native)
    {
        return new ImpellerColorSourceHandle(
            EnsureSuccess(native, "Failed to create Impeller color source."));
    }

    public static ImpellerColorSourceHandle CreateLinearGradient(
        in ImpellerPoint start,
        in ImpellerPoint end,
        ReadOnlySpan<ImpellerColor> colors,
        ReadOnlySpan<float> stops,
        ImpellerTileMode tileMode,
        ImpellerMatrix? transform = null)
    {
        ValidateStops(colors, stops);

        var matrix = transform ?? default;

        fixed (ImpellerPoint* startPtr = &start)
        fixed (ImpellerPoint* endPtr = &end)
        fixed (ImpellerColor* colorPtr = colors)
        fixed (float* stopPtr = stops)
        {
            var matrixPtr = transform.HasValue ? &matrix : null;
            var native = ImpellerNative.ImpellerColorSourceCreateLinearGradientNew(
                startPtr,
                endPtr,
                (uint)colors.Length,
                colorPtr,
                stopPtr,
                tileMode,
                matrixPtr);

            return FromOwned(native);
        }
    }

    public static ImpellerColorSourceHandle CreateRadialGradient(
        in ImpellerPoint center,
        float radius,
        ReadOnlySpan<ImpellerColor> colors,
        ReadOnlySpan<float> stops,
        ImpellerTileMode tileMode,
        ImpellerMatrix? transform = null)
    {
        ValidateStops(colors, stops);
        var matrix = transform ?? default;

        fixed (ImpellerPoint* centerPtr = &center)
        fixed (ImpellerColor* colorPtr = colors)
        fixed (float* stopPtr = stops)
        {
            var matrixPtr = transform.HasValue ? &matrix : null;
            var native = ImpellerNative.ImpellerColorSourceCreateRadialGradientNew(
                centerPtr,
                radius,
                (uint)colors.Length,
                colorPtr,
                stopPtr,
                tileMode,
                matrixPtr);

            return FromOwned(native);
        }
    }

    public static ImpellerColorSourceHandle CreateConicalGradient(
        in ImpellerPoint startCenter,
        float startRadius,
        in ImpellerPoint endCenter,
        float endRadius,
        ReadOnlySpan<ImpellerColor> colors,
        ReadOnlySpan<float> stops,
        ImpellerTileMode tileMode,
        ImpellerMatrix? transform = null)
    {
        ValidateStops(colors, stops);
        var matrix = transform ?? default;

        fixed (ImpellerPoint* startPtr = &startCenter)
        fixed (ImpellerPoint* endPtr = &endCenter)
        fixed (ImpellerColor* colorPtr = colors)
        fixed (float* stopPtr = stops)
        {
            var matrixPtr = transform.HasValue ? &matrix : null;
            var native = ImpellerNative.ImpellerColorSourceCreateConicalGradientNew(
                startPtr,
                startRadius,
                endPtr,
                endRadius,
                (uint)colors.Length,
                colorPtr,
                stopPtr,
                tileMode,
                matrixPtr);

            return FromOwned(native);
        }
    }

    public static ImpellerColorSourceHandle CreateSweepGradient(
        in ImpellerPoint center,
        float startAngle,
        float endAngle,
        ReadOnlySpan<ImpellerColor> colors,
        ReadOnlySpan<float> stops,
        ImpellerTileMode tileMode,
        ImpellerMatrix? transform = null)
    {
        ValidateStops(colors, stops);
        var matrix = transform ?? default;

        fixed (ImpellerPoint* centerPtr = &center)
        fixed (ImpellerColor* colorPtr = colors)
        fixed (float* stopPtr = stops)
        {
            var matrixPtr = transform.HasValue ? &matrix : null;
            var native = ImpellerNative.ImpellerColorSourceCreateSweepGradientNew(
                centerPtr,
                startAngle,
                endAngle,
                (uint)colors.Length,
                colorPtr,
                stopPtr,
                tileMode,
                matrixPtr);

            return FromOwned(native);
        }
    }

    public static ImpellerColorSourceHandle CreateImage(
        ImpellerTextureHandle texture,
        ImpellerTileMode horizontalTileMode,
        ImpellerTileMode verticalTileMode,
        ImpellerTextureSampling sampling,
        ImpellerMatrix? transform = null)
    {
        if (texture is null)
        {
            throw new ArgumentNullException(nameof(texture));
        }

        var matrix = transform ?? default;

        var matrixPtr = transform.HasValue ? &matrix : null;
        var native = ImpellerNative.ImpellerColorSourceCreateImageNew(
            texture.DangerousGetHandle(),
            horizontalTileMode,
            verticalTileMode,
            sampling,
            matrixPtr);

        return FromOwned(native);
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerColorSourceRetain(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerColorSourceRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerColorSourceHandle));
        }
    }

    private static void ValidateStops(ReadOnlySpan<ImpellerColor> colors, ReadOnlySpan<float> stops)
    {
        if (colors.Length == 0)
        {
            throw new ArgumentException("Gradient requires at least one color stop.", nameof(colors));
        }

        if (colors.Length != stops.Length)
        {
            throw new ArgumentException("Colors and stops spans must have the same length.");
        }
    }
}
