using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerPathBuilderNew();

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerPathBuilderRetain(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerPathBuilderRelease(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderMoveTo")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerPathBuilderMoveTo(nint builder, ImpellerPoint* location);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderLineTo")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerPathBuilderLineTo(nint builder, ImpellerPoint* location);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderQuadraticCurveTo")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerPathBuilderQuadraticCurveTo(
        nint builder,
        ImpellerPoint* controlPoint,
        ImpellerPoint* endPoint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderCubicCurveTo")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerPathBuilderCubicCurveTo(
        nint builder,
        ImpellerPoint* controlPoint1,
        ImpellerPoint* controlPoint2,
        ImpellerPoint* endPoint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderTakePathNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerPathBuilderTakePathNew(nint builder, ImpellerFillType fillType);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderAddRect")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerPathBuilderAddRect(nint builder, ImpellerRect* rect);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderAddArc")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerPathBuilderAddArc(
        nint builder,
        ImpellerRect* ovalBounds,
        float startAngleDegrees,
        float endAngleDegrees);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderAddOval")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerPathBuilderAddOval(nint builder, ImpellerRect* ovalBounds);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderAddRoundedRect")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerPathBuilderAddRoundedRect(
        nint builder,
        ImpellerRect* rect,
        ImpellerRoundingRadii* roundingRadii);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderClose")]
    [SuppressGCTransition]
    internal static partial void ImpellerPathBuilderClose(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathBuilderCopyPathNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerPathBuilderCopyPathNew(nint builder, ImpellerFillType fillType);
}
