using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorFilterRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerColorFilterRetain(nint colorFilter);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorFilterRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerColorFilterRelease(nint colorFilter);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorFilterCreateBlendNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerColorFilterCreateBlendNew(
        ImpellerColor* color,
        ImpellerBlendMode blendMode);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerColorFilterCreateColorMatrixNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerColorFilterCreateColorMatrixNew(
        ImpellerColorMatrix* colorMatrix);
}
