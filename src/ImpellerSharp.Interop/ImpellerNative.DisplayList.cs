using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListRetain(nint displayList);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListRelease(nint displayList);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerDisplayListBuilderNew(ImpellerRect* cullRect);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderRetain(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderRelease(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderCreateDisplayListNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerDisplayListBuilderCreateDisplayListNew(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderSave")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderSave(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderRestore")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderRestore(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderSaveLayer")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderSaveLayer(
        nint builder,
        ImpellerRect* bounds,
        nint paint,
        nint backdrop);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderSetTransform")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderSetTransform(nint builder, ImpellerMatrix* transform);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderGetTransform")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderGetTransform(nint builder, ImpellerMatrix* outTransform);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderResetTransform")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderResetTransform(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderGetSaveCount")]
    [SuppressGCTransition]
    internal static partial uint ImpellerDisplayListBuilderGetSaveCount(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderRestoreToCount")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderRestoreToCount(nint builder, uint count);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderTranslate")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderTranslate(nint builder, float x, float y);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderScale")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderScale(nint builder, float xScale, float yScale);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderRotate")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderRotate(nint builder, float degrees);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderTransform")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderTransform(nint builder, ImpellerMatrix* transform);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderClipRect")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderClipRect(nint builder, ImpellerRect* rect, ImpellerClipOperation op);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderClipOval")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderClipOval(nint builder, ImpellerRect* rect, ImpellerClipOperation op);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderClipRoundedRect")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderClipRoundedRect(
        nint builder,
        ImpellerRect* rect,
        ImpellerRoundingRadii* radii,
        ImpellerClipOperation op);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderClipPath")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderClipPath(nint builder, nint path, ImpellerClipOperation op);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawRect")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderDrawRect(nint builder, ImpellerRect* rect, nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawPaint")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderDrawPaint(nint builder, nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawPath")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderDrawPath(nint builder, nint path, nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawTexture")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderDrawTexture(
        nint builder,
        nint texture,
        ImpellerPoint* point,
        ImpellerTextureSampling sampling,
        nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawTextureRect")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderDrawTextureRect(
        nint builder,
        nint texture,
        ImpellerRect* srcRect,
        ImpellerRect* dstRect,
        ImpellerTextureSampling sampling,
        nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawParagraph")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderDrawParagraph(
        nint builder,
        nint paragraph,
        ImpellerPoint* point);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawLine")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderDrawLine(
        nint builder,
        ImpellerPoint* from,
        ImpellerPoint* to,
        nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawDashedLine")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderDrawDashedLine(
        nint builder,
        ImpellerPoint* from,
        ImpellerPoint* to,
        float onLength,
        float offLength,
        nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawOval")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderDrawOval(
        nint builder,
        ImpellerRect* bounds,
        nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawRoundedRect")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderDrawRoundedRect(
        nint builder,
        ImpellerRect* rect,
        ImpellerRoundingRadii* radii,
        nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawRoundedRectDifference")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderDrawRoundedRectDifference(
        nint builder,
        ImpellerRect* outerRect,
        ImpellerRoundingRadii* outerRadii,
        ImpellerRect* innerRect,
        ImpellerRoundingRadii* innerRadii,
        nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawDisplayList")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderDrawDisplayList(
        nint builder,
        nint displayList,
        float opacity);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderDrawShadow")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerDisplayListBuilderDrawShadow(
        nint builder,
        nint path,
        ImpellerColor* color,
        float elevation,
        [MarshalAs(UnmanagedType.I1)] bool occluderIsTransparent,
        float devicePixelRatio);
}
