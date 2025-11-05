using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerMaskFilterRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerMaskFilterRetain(nint maskFilter);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerMaskFilterRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerMaskFilterRelease(nint maskFilter);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerMaskFilterCreateBlurNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerMaskFilterCreateBlurNew(ImpellerBlurStyle style, float sigma);
}
