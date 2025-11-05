using System;
using System.Runtime.InteropServices;
using Silk.NET.GLFW;

namespace ImpellerSharp.Interop.Hosting;

internal static unsafe class GlfwNative
{
    private static bool s_libraryLoaded;
    private static IntPtr s_libraryHandle;
    private static delegate* unmanaged<WindowHandle*, IntPtr> s_getCocoaWindow;

    public static void Initialise(Glfw glfw)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        if (s_libraryLoaded && s_getCocoaWindow != null)
        {
            return;
        }

        if (!TryLoadLibrary())
        {
            throw new InvalidOperationException("Unable to load GLFW native library; ensure Ultz.Native.GLFW is present.");
        }

        var export = NativeLibrary.GetExport(s_libraryHandle, "glfwGetCocoaWindow");
        if (export == IntPtr.Zero)
        {
            throw new InvalidOperationException("glfwGetCocoaWindow export not found in GLFW library.");
        }

        s_getCocoaWindow = (delegate* unmanaged<WindowHandle*, IntPtr>)export;
        s_libraryLoaded = true;
    }

    public static IntPtr GetCocoaWindow(WindowHandle* window)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return IntPtr.Zero;
        }

        if (s_getCocoaWindow == null)
        {
            throw new InvalidOperationException("Call GlfwNative.Initialise before requesting the native window handle.");
        }

        return s_getCocoaWindow(window);
    }

    private static bool TryLoadLibrary()
    {
        if (s_libraryHandle != IntPtr.Zero)
        {
            return true;
        }

        bool TryLoad(string name)
        {
            if (NativeLibrary.TryLoad(name, out var handle))
            {
                s_libraryHandle = handle;
                return true;
            }

            return false;
        }

        return
            TryLoad("libglfw.3.dylib") ||
            TryLoad("libglfw.dylib") ||
            TryLoad("glfw") ||
            TryLoad(System.IO.Path.Combine(AppContext.BaseDirectory, "libglfw.3.dylib")) ||
            TryLoad(System.IO.Path.Combine(AppContext.BaseDirectory, "runtimes", "osx-arm64", "native", "libglfw.3.dylib")) ||
            TryLoad(System.IO.Path.Combine(AppContext.BaseDirectory, "runtimes", "osx-x64", "native", "libglfw.3.dylib"));
    }
}
