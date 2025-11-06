using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerPaintNew();

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintRetain(nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintRelease(nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetColor")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerPaintSetColor(nint paint, ImpellerColor* color);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetBlendMode")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintSetBlendMode(nint paint, ImpellerBlendMode blendMode);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetDrawStyle")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintSetDrawStyle(nint paint, ImpellerDrawStyle drawStyle);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetStrokeCap")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintSetStrokeCap(nint paint, ImpellerStrokeCap cap);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetStrokeJoin")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintSetStrokeJoin(nint paint, ImpellerStrokeJoin join);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetStrokeWidth")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintSetStrokeWidth(nint paint, float width);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetStrokeMiter")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintSetStrokeMiter(nint paint, float miter);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetColorSource")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintSetColorSource(nint paint, nint colorSource);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetColorFilter")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintSetColorFilter(nint paint, nint colorFilter);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetImageFilter")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintSetImageFilter(nint paint, nint imageFilter);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPaintSetMaskFilter")]
    [SuppressGCTransition]
    internal static partial void ImpellerPaintSetMaskFilter(nint paint, nint maskFilter);
}
