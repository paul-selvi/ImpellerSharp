using System;
using System.IO;
using System.Runtime.InteropServices;
using ImpellerSharp.Interop;
using Silk.NET.GLFW;

namespace ImpellerSharp.Interop.Hosting;

/// <summary>
/// Provides a minimal GLFW + Metal host for rendering Impeller scenes on macOS.
/// </summary>
public sealed unsafe class MetalGlfwAppHost : IDisposable
{
    private readonly MetalGlfwHostOptions _options;

    private Glfw? _glfw;
    private WindowHandle* _window;
    private MetalLayerContext _layerContext;
    private GlfwCallbacks.ErrorCallback? _errorCallback;
    private bool _glfwInitialised;
    private bool _disposed;

    public MetalGlfwAppHost(MetalGlfwHostOptions? options = null)
    {
        _options = options ?? new MetalGlfwHostOptions();
    }

    /// <summary>
    /// Runs the host loop using the supplied render callback.
    /// </summary>
    /// <param name="renderFrame">
    /// Called for each frame with the display list builder and framebuffer dimensions.
    /// Return <c>true</c> to present the frame; <c>false</c> skips presentation while keeping the loop alive.
    /// </param>
    /// <returns>Process exit code semantics; zero indicates success.</returns>
    public int Run(Func<ImpellerDisplayListBuilderHandle, float, float, bool> renderFrame)
    {
        if (renderFrame is null)
        {
            throw new ArgumentNullException(nameof(renderFrame));
        }

        ThrowIfDisposed();

        if (!OperatingSystem.IsMacOS())
        {
            Log("MetalGlfwAppHost currently supports macOS Metal backends only.");
            return 1;
        }

        if (!EnsureImpellerNativeLoaded())
        {
            Log("Unable to locate libimpeller.dylib. Build ImpellerSharp.Native artifacts or copy the native binaries into the application output directory.");
            return 1;
        }

        try
        {
            _glfw = Glfw.GetApi();
            _errorCallback = ErrorCallback;
            _glfw.SetErrorCallback(_errorCallback);

            if (!_glfw.Init())
            {
                Log("Failed to initialise GLFW.");
                return 1;
            }

            _glfwInitialised = true;
            GlfwNative.Initialise(_glfw);

            ConfigureWindowHints(_glfw);
            _window = _glfw.CreateWindow(
                _options.Width,
                _options.Height,
                _options.Title,
                null,
                null);

            if (_window is null)
            {
                Log("GLFW window creation failed.");
                return 1;
            }

            var cocoaWindow = GlfwNative.GetCocoaWindow(_window);
            if (cocoaWindow == IntPtr.Zero)
            {
                Log("glfwGetCocoaWindow returned null.");
                return 1;
            }

            _layerContext = MetalInterop.AttachMetalLayer(cocoaWindow);
            if (_layerContext.Layer == IntPtr.Zero)
            {
                Log("Could not attach CAMetalLayer to the GLFW window.");
                return 1;
            }

            using var context = ImpellerContextHandle.CreateMetal();
            return RunLoop(context, renderFrame);
        }
        catch (ImpellerInteropException ex)
        {
            Log($"Impeller interop error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Log($"Unexpected error while running MetalGlfwAppHost: {ex}");
            return 1;
        }
        finally
        {
            Cleanup();
        }
    }

    private int RunLoop(ImpellerContextHandle context, Func<ImpellerDisplayListBuilderHandle, float, float, bool> renderFrame)
    {
        var exitCode = 0;

        while (_window is not null && _glfw is not null && !_glfw.WindowShouldClose(_window))
        {
            _glfw.PollEvents();

            _glfw.GetWindowSize(_window, out var windowWidth, out var windowHeight);
            _glfw.GetFramebufferSize(_window, out var framebufferWidth, out var framebufferHeight);

            if (framebufferWidth <= 0 || framebufferHeight <= 0 || windowWidth <= 0 || windowHeight <= 0)
            {
                continue;
            }

            var scale = windowWidth > 0
                ? (double)framebufferWidth / Math.Max(1, windowWidth)
                : 1d;

            MetalInterop.UpdateLayerGeometry(
                _layerContext,
                windowWidth,
                windowHeight,
                framebufferWidth,
                framebufferHeight,
                scale);

            IntPtr drawable;
            using (MetalInterop.AutoreleaseScope())
            {
                drawable = MetalInterop.NextDrawable(_layerContext.Layer);
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

                if (!renderFrame(builder, framebufferWidth, framebufferHeight))
                {
                    continue;
                }

                using var displayList = builder.Build();
                if (!surface.DrawDisplayList(displayList))
                {
                    Log("Impeller surface draw failed.");
                    exitCode = 1;
                    break;
                }

                if (!surface.Present())
                {
                    Log("Impeller surface present failed.");
                    exitCode = 1;
                    break;
                }
            }
            catch (ImpellerInteropException ex)
            {
                Log($"Impeller interop error: {ex.Message}");
                exitCode = 1;
                break;
            }
            catch (Exception ex)
            {
                Log($"Unexpected error while rendering frame: {ex}");
                exitCode = 1;
                break;
            }
            finally
            {
                MetalInterop.Release(drawable);
            }
        }

        return exitCode;
    }

    private void Cleanup()
    {
        if (_window is not null && _glfw is not null)
        {
            MetalInterop.Detach(_layerContext);
            _glfw.DestroyWindow(_window);
            _window = null;
            _layerContext = default;
        }

        if (_glfw is not null && _glfwInitialised)
        {
            _glfw.Terminate();
            _glfw.Dispose();
            _glfw = null;
            _glfwInitialised = false;
        }
    }

    private static void ConfigureWindowHints(Glfw glfw)
    {
        glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        glfw.WindowHint(WindowHintBool.Decorated, true);
        glfw.WindowHint(WindowHintBool.Visible, true);
    }

    private bool EnsureImpellerNativeLoaded()
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
                return true;
            }
        }

        return false;
    }

    private void ErrorCallback(ErrorCode error, string description)
    {
        Log($"[GLFW] {error}: {description}");
    }

    private void Log(string message)
    {
        if (_options.ErrorLogger is { } logger)
        {
            logger(message);
        }
        else
        {
            Console.Error.WriteLine(message);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MetalGlfwAppHost));
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Cleanup();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
