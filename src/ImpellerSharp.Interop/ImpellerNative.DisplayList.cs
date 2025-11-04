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

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderTranslate")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderTranslate(nint builder, float x, float y);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerDisplayListBuilderRotate")]
    [SuppressGCTransition]
    internal static partial void ImpellerDisplayListBuilderRotate(nint builder, float degrees);

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
}
