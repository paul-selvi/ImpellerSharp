using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorSourceRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerColorSourceRetain(nint colorSource);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorSourceRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerColorSourceRelease(nint colorSource);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorSourceCreateLinearGradientNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerColorSourceCreateLinearGradientNew(
        ImpellerPoint* startPoint,
        ImpellerPoint* endPoint,
        uint stopCount,
        ImpellerColor* colors,
        float* stops,
        ImpellerTileMode tileMode,
        ImpellerMatrix* transform);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorSourceCreateRadialGradientNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerColorSourceCreateRadialGradientNew(
        ImpellerPoint* center,
        float radius,
        uint stopCount,
        ImpellerColor* colors,
        float* stops,
        ImpellerTileMode tileMode,
        ImpellerMatrix* transform);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorSourceCreateConicalGradientNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerColorSourceCreateConicalGradientNew(
        ImpellerPoint* startCenter,
        float startRadius,
        ImpellerPoint* endCenter,
        float endRadius,
        uint stopCount,
        ImpellerColor* colors,
        float* stops,
        ImpellerTileMode tileMode,
        ImpellerMatrix* transform);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorSourceCreateSweepGradientNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerColorSourceCreateSweepGradientNew(
        ImpellerPoint* center,
        float start,
        float end,
        uint stopCount,
        ImpellerColor* colors,
        float* stops,
        ImpellerTileMode tileMode,
        ImpellerMatrix* transform);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorSourceCreateImageNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerColorSourceCreateImageNew(
        nint texture,
        ImpellerTileMode horizontalTileMode,
        ImpellerTileMode verticalTileMode,
        ImpellerTextureSampling sampling,
        ImpellerMatrix* transform);
}
