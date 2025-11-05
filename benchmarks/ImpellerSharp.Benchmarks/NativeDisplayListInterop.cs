using System;
using System.IO;
using System.Runtime.InteropServices;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Benchmarks;

internal static unsafe class NativeDisplayListInterop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeImpellerMapping
    {
        public byte* Data;
        public ulong Length;
        public delegate* unmanaged[Cdecl]<nint, void> OnRelease;
    }

    private const string ImpellerLibraryName = "impeller";
    private static readonly nint LibraryHandle = LoadLibrary();

    internal static readonly delegate* unmanaged[Cdecl]<ImpellerRect*, nint> DisplayListBuilderNew =
        (delegate* unmanaged[Cdecl]<ImpellerRect*, nint>)GetExport("ImpellerDisplayListBuilderNew");

    internal static readonly delegate* unmanaged[Cdecl]<nint, void> DisplayListBuilderRelease =
        (delegate* unmanaged[Cdecl]<nint, void>)GetExport("ImpellerDisplayListBuilderRelease");

    internal static readonly delegate* unmanaged[Cdecl]<nint, ImpellerRect*, nint, void> DisplayListBuilderDrawRect =
        (delegate* unmanaged[Cdecl]<nint, ImpellerRect*, nint, void>)GetExport("ImpellerDisplayListBuilderDrawRect");

    internal static readonly delegate* unmanaged[Cdecl]<nint, nint> DisplayListBuilderCreateDisplayListNew =
        (delegate* unmanaged[Cdecl]<nint, nint>)GetExport("ImpellerDisplayListBuilderCreateDisplayListNew");

    internal static readonly delegate* unmanaged[Cdecl]<nint, void> DisplayListRelease =
        (delegate* unmanaged[Cdecl]<nint, void>)GetExport("ImpellerDisplayListRelease");

    internal static readonly delegate* unmanaged[Cdecl]<nint> PaintNew =
        (delegate* unmanaged[Cdecl]<nint>)GetExport("ImpellerPaintNew");

    internal static readonly delegate* unmanaged[Cdecl]<nint, ImpellerColor*, void> PaintSetColor =
        (delegate* unmanaged[Cdecl]<nint, ImpellerColor*, void>)GetExport("ImpellerPaintSetColor");

    internal static readonly delegate* unmanaged[Cdecl]<nint, void> PaintRelease =
        (delegate* unmanaged[Cdecl]<nint, void>)GetExport("ImpellerPaintRelease");

    internal static readonly delegate* unmanaged[Cdecl]<nint, ImpellerTextureDescriptor, NativeImpellerMapping, nint, nint> TextureCreateWithContentsNew =
        (delegate* unmanaged[Cdecl]<nint, ImpellerTextureDescriptor, NativeImpellerMapping, nint, nint>)GetExport("ImpellerTextureCreateWithContentsNew");

    internal static readonly delegate* unmanaged[Cdecl]<nint, void> TextureRelease =
        (delegate* unmanaged[Cdecl]<nint, void>)GetExport("ImpellerTextureRelease");

    private static nint LoadLibrary()
    {
        if (NativeLibrary.TryLoad(ImpellerLibraryName, out var handle))
        {
            return handle;
        }

        var baseDirectory = AppContext.BaseDirectory;
        foreach (var candidate in GetPlatformCandidates(baseDirectory))
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out handle))
            {
                return handle;
            }
        }

        throw new InvalidOperationException($"Unable to load native Impeller library '{ImpellerLibraryName}'.");
    }

    private static string[] GetPlatformCandidates(string baseDirectory)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new[]
            {
                Path.Combine(baseDirectory, "libimpeller.dylib"),
                Path.Combine(baseDirectory, "impeller.dylib"),
                Path.Combine(baseDirectory, "runtimes", "osx-arm64", "native", "libimpeller.dylib"),
                Path.Combine(baseDirectory, "runtimes", "osx-x64", "native", "libimpeller.dylib"),
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new[]
            {
                Path.Combine(baseDirectory, "impeller.dll"),
                Path.Combine(baseDirectory, "runtimes", "win-x64", "native", "impeller.dll"),
                Path.Combine(baseDirectory, "runtimes", "win-x86", "native", "impeller.dll"),
                Path.Combine(baseDirectory, "runtimes", "win-arm64", "native", "impeller.dll"),
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new[]
            {
                Path.Combine(baseDirectory, "libimpeller.so"),
                Path.Combine(baseDirectory, "impeller.so"),
                Path.Combine(baseDirectory, "runtimes", "linux-x64", "native", "libimpeller.so"),
                Path.Combine(baseDirectory, "runtimes", "linux-arm64", "native", "libimpeller.so"),
            };
        }

        return Array.Empty<string>();
    }

    private static nint GetExport(string exportName)
    {
        if (!NativeLibrary.TryGetExport(LibraryHandle, exportName, out var address))
        {
            throw new MissingMethodException($"Unable to locate native export '{exportName}'.");
        }

        return address;
    }
}
