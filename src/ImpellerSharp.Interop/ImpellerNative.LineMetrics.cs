using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerLineMetricsRetain(nint metrics);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerLineMetricsRelease(nint metrics);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetUnscaledAscent")]
    [SuppressGCTransition]
    internal static partial double ImpellerLineMetricsGetUnscaledAscent(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetAscent")]
    [SuppressGCTransition]
    internal static partial double ImpellerLineMetricsGetAscent(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetDescent")]
    [SuppressGCTransition]
    internal static partial double ImpellerLineMetricsGetDescent(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetBaseline")]
    [SuppressGCTransition]
    internal static partial double ImpellerLineMetricsGetBaseline(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsIsHardbreak")]
    [SuppressGCTransition]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool ImpellerLineMetricsIsHardbreak(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetWidth")]
    [SuppressGCTransition]
    internal static partial double ImpellerLineMetricsGetWidth(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetHeight")]
    [SuppressGCTransition]
    internal static partial double ImpellerLineMetricsGetHeight(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetLeft")]
    [SuppressGCTransition]
    internal static partial double ImpellerLineMetricsGetLeft(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetCodeUnitStartIndex")]
    [SuppressGCTransition]
    internal static partial nuint ImpellerLineMetricsGetCodeUnitStartIndex(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetCodeUnitEndIndex")]
    [SuppressGCTransition]
    internal static partial nuint ImpellerLineMetricsGetCodeUnitEndIndex(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetCodeUnitEndIndexExcludingWhitespace")]
    [SuppressGCTransition]
    internal static partial nuint ImpellerLineMetricsGetCodeUnitEndIndexExcludingWhitespace(nint metrics, nuint line);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerLineMetricsGetCodeUnitEndIndexIncludingNewline")]
    [SuppressGCTransition]
    internal static partial nuint ImpellerLineMetricsGetCodeUnitEndIndexIncludingNewline(nint metrics, nuint line);
}
