using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static unsafe class ImpellerCommandPointers
{
    private static readonly nint LibraryHandle = LoadLibrary();

    internal static readonly delegate* unmanaged[Cdecl]<nint, nint, byte>
        SurfaceDrawDisplayList = (delegate* unmanaged[Cdecl]<nint, nint, byte>)
            GetExport(nameof(ImpellerNative.ImpellerSurfaceDrawDisplayList));

    private static nint LoadLibrary()
    {
        if (NativeLibrary.TryLoad(ImpellerLibrary.Name, out var handle))
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

        throw new ImpellerInteropException($"Unable to load native library '{ImpellerLibrary.Name}'.");
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
                Path.Combine(baseDirectory, "runtimes", "linux-arm", "native", "libimpeller.so"),
                Path.Combine(baseDirectory, "runtimes", "linux-arm64", "native", "libimpeller.so"),
            };
        }

        return Array.Empty<string>();
    }

    private static nint GetExport(string managedName)
    {
        // Managed name matches the entry point for the exports we cache.
        if (!NativeLibrary.TryGetExport(LibraryHandle, managedName, out var address))
        {
            throw new ImpellerInteropException($"Unable to locate native export '{managedName}'.");
        }

        return address;
    }
}
