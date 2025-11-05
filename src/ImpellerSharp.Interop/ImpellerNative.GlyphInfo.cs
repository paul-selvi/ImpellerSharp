using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerGlyphInfoRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerGlyphInfoRetain(nint glyphInfo);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerGlyphInfoRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerGlyphInfoRelease(nint glyphInfo);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerGlyphInfoGetGraphemeClusterCodeUnitRangeBegin")]
    [SuppressGCTransition]
    internal static partial nuint ImpellerGlyphInfoGetGraphemeClusterCodeUnitRangeBegin(nint glyphInfo);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerGlyphInfoGetGraphemeClusterCodeUnitRangeEnd")]
    [SuppressGCTransition]
    internal static partial nuint ImpellerGlyphInfoGetGraphemeClusterCodeUnitRangeEnd(nint glyphInfo);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerGlyphInfoGetGraphemeClusterBounds")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerGlyphInfoGetGraphemeClusterBounds(nint glyphInfo, ImpellerRect* bounds);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerGlyphInfoIsEllipsis")]
    [SuppressGCTransition]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool ImpellerGlyphInfoIsEllipsis(nint glyphInfo);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerGlyphInfoGetTextDirection")]
    [SuppressGCTransition]
    internal static partial ImpellerTextDirection ImpellerGlyphInfoGetTextDirection(nint glyphInfo);
}
