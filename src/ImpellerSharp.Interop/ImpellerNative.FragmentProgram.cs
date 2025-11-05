using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerFragmentProgramNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerFragmentProgramNew(
        in ImpellerMapping mapping,
        nint userData);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerFragmentProgramRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerFragmentProgramRetain(nint fragmentProgram);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerFragmentProgramRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerFragmentProgramRelease(nint fragmentProgram);
}
