using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerTextureCreateWithContentsNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerTextureCreateWithContentsNew(
        nint context,
        in ImpellerTextureDescriptor descriptor,
        in ImpellerMapping mapping,
        nint contentsOnReleaseUserData);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerTextureCreateWithOpenGLTextureHandleNew")]
    [SuppressGCTransition]
    internal static unsafe partial nint ImpellerTextureCreateWithOpenGLTextureHandleNew(
        nint context,
        in ImpellerTextureDescriptor descriptor,
        ulong handle);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerTextureRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerTextureRetain(nint texture);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerTextureRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerTextureRelease(nint texture);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerTextureGetOpenGLHandle")]
    [SuppressGCTransition]
    internal static partial ulong ImpellerTextureGetOpenGLHandle(nint texture);
}
