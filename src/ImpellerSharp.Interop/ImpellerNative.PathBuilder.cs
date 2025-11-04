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
}
