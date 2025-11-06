using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerPathRetain(nint path);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerPathRelease(nint path);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerPathGetBounds")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerPathGetBounds(nint path, ImpellerRect* outBounds);
}
