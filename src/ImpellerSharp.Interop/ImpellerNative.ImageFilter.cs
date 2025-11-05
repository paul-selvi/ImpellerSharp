using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerImageFilterRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerImageFilterRetain(nint imageFilter);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerImageFilterRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerImageFilterRelease(nint imageFilter);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerImageFilterCreateBlurNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerImageFilterCreateBlurNew(
        float sigmaX,
        float sigmaY,
        ImpellerTileMode tileMode);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerImageFilterCreateDilateNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerImageFilterCreateDilateNew(float radiusX, float radiusY);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerImageFilterCreateErodeNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerImageFilterCreateErodeNew(float radiusX, float radiusY);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerImageFilterCreateMatrixNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerImageFilterCreateMatrixNew(
        ImpellerMatrix* matrix,
        ImpellerTextureSampling sampling);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerImageFilterCreateComposeNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerImageFilterCreateComposeNew(nint outerFilter, nint innerFilter);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerImageFilterCreateFragmentProgramNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerImageFilterCreateFragmentProgramNew(
        nint context,
        nint fragmentProgram,
        nint* samplers,
        nuint samplersCount,
        byte* data,
        nuint dataLength);
}
