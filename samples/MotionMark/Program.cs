using System;
using System.IO;
using ImpellerSharp.Interop;
using Silk.NET.GLFW;
using System.Runtime.InteropServices;
using AvaloniaImpellerApp.Rendering;

namespace ImpellerSharp.Samples.MotionMark;

internal static class Program
{
    private const int InitialWidth = 1280;
    private const int InitialHeight = 720;

    private static Glfw? s_glfw;
    private static GlfwCallbacks.ErrorCallback? s_errorCallback;

    private static unsafe int Main()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Console.Error.WriteLine("MotionMark sample currently supports macOS Metal backends only.");
            return 1;
        }

        s_glfw = Glfw.GetApi();
        s_errorCallback = ErrorCallback;
        s_glfw.SetErrorCallback(s_errorCallback);

        if (!s_glfw.Init())
        {
            Console.Error.WriteLine("Failed to initialise GLFW.");
            return 1;
        }

        GlfwNative.Initialise(s_glfw);

        try
        {
            s_glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
            s_glfw.WindowHint(WindowHintBool.Decorated, true);
            s_glfw.WindowHint(WindowHintBool.Visible, true);

            WindowHandle* window = s_glfw.CreateWindow(
                InitialWidth,
                InitialHeight,
                "Impeller MotionMark (Metal)",
                null,
                null);

            if (window is null)
            {
                Console.Error.WriteLine("GLFW window creation failed.");
                return 1;
            }

            var cocoaWindow = GlfwNative.GetCocoaWindow(window);
            if (cocoaWindow == IntPtr.Zero)
            {
                Console.Error.WriteLine("glfwGetCocoaWindow returned null.");
                s_glfw.DestroyWindow(window);
                return 1;
            }

            var layerContext = MetalInterop.AttachMetalLayer(cocoaWindow);
            if (layerContext.Layer == IntPtr.Zero)
            {
                Console.Error.WriteLine("Could not attach CAMetalLayer to the GLFW window.");
                s_glfw.DestroyWindow(window);
                return 1;
            }

            EnsureImpellerNativeLoaded();

            using var context = ImpellerContextHandle.CreateMetal();
            using var simulation = new MotionMarkSimulation();
            simulation.SetComplexity(9);

            while (!s_glfw.WindowShouldClose(window))
            {
                //s_glfw.WaitEventsTimeout(1.0 / 120.0);
                s_glfw.PollEvents();

                s_glfw.GetWindowSize(window, out var windowWidth, out var windowHeight);
                s_glfw.GetFramebufferSize(window, out var framebufferWidth, out var framebufferHeight);

                if (framebufferWidth <= 0 || framebufferHeight <= 0 || windowWidth <= 0 || windowHeight <= 0)
                {
                    continue;
                }

                var scale = windowWidth > 0
                    ? (double)framebufferWidth / Math.Max(1, windowWidth)
                    : 1d;

                MetalInterop.UpdateLayerGeometry(
                    layerContext,
                    windowWidth,
                    windowHeight,
                    framebufferWidth,
                    framebufferHeight,
                    scale);

                IntPtr drawable;
                using (MetalInterop.AutoreleaseScope())
                {
                    drawable = MetalInterop.NextDrawable(layerContext.Layer);
                    if (drawable == IntPtr.Zero)
                    {
                        continue;
                    }

                    MetalInterop.Retain(drawable);
                }

                try
                {
                    using var surface = ImpellerSurfaceHandle.WrapMetalDrawable(context, drawable);
                    using var builder = ImpellerDisplayListBuilderHandle.Create();

                    if (!simulation.Render(builder, framebufferWidth, framebufferHeight))
                    {
                        continue;
                    }

                    using var displayList = builder.Build();
                    surface.DrawDisplayList(displayList);
                    surface.Present();
                }
                catch (ImpellerInteropException ex)
                {
                    Console.Error.WriteLine($"Impeller interop error: {ex.Message}");
                    break;
                }
                finally
                {
                    MetalInterop.Release(drawable);
                }
            }

            MetalInterop.Detach(layerContext);
            s_glfw.DestroyWindow(window);

            return 0;
        }
        finally
        {
            s_glfw.Terminate();
            s_glfw.Dispose();
        }
    }

    private static void ErrorCallback(ErrorCode error, string description)
    {
        Console.Error.WriteLine($"[GLFW] {error}: {description}");
    }

    private static void EnsureImpellerNativeLoaded()
    {
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "libimpeller.dylib"),
            Path.Combine(baseDir, "impeller.dylib"),
            Path.Combine(baseDir, "runtimes", "osx-arm64", "native", "libimpeller.dylib"),
            Path.Combine(baseDir, "runtimes", "osx-x64", "native", "libimpeller.dylib"),
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                NativeLibrary.Load(candidate);
                return;
            }
        }

        Console.Error.WriteLine("Warning: Unable to locate libimpeller.dylib. Set DYLD_LIBRARY_PATH or copy the native binaries next to the executable.");
    }
}
