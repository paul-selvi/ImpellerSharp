using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerSurfaceRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerSurfaceRetain(nint surface);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerSurfaceRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerSurfaceRelease(nint surface);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerSurfaceDrawDisplayList")]
    [SuppressGCTransition]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool ImpellerSurfaceDrawDisplayList(nint surface, nint displayList);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerSurfacePresent")]
    [SuppressGCTransition]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool ImpellerSurfacePresent(nint surface);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerSurfaceCreateWrappedFBONew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerSurfaceCreateWrappedFBONew(
        nint context,
        ulong framebuffer,
        ImpellerPixelFormat format,
        in ImpellerISize size);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerSurfaceCreateWrappedMetalDrawableNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerSurfaceCreateWrappedMetalDrawableNew(
        nint context,
        nint metalDrawable);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerSurfaceCreateWrappedMetalTextureNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerSurfaceCreateWrappedMetalTextureNew(
        nint context,
        nint metalTexture);
}
