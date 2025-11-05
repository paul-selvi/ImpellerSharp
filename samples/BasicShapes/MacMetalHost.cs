using System;
using Silk.NET.GLFW;

namespace ImpellerSharp.Samples.BasicShapes;

internal sealed unsafe class MacMetalHost : IDisposable
{
    private readonly Glfw _glfw;
    private readonly WindowHandle* _window;
    private readonly delegate* unmanaged[Cdecl]<WindowHandle*, nint> _getCocoaWindow;
    private readonly nint _nsWindow;
    private nint _metalLayer;
    private bool _disposed;

    private MacMetalHost(
        Glfw glfw,
        WindowHandle* window,
        delegate* unmanaged[Cdecl]<WindowHandle*, nint> getCocoaWindow,
        nint nsWindow,
        nint metalLayer)
    {
        _glfw = glfw;
        _window = window;
        _getCocoaWindow = getCocoaWindow;
        _nsWindow = nsWindow;
        _metalLayer = metalLayer;
    }

    public static MacMetalHost Create(int width, int height, string title)
    {
        var glfw = Glfw.GetApi();
        if (!glfw.Init())
        {
            throw new InvalidOperationException("GLFW initialization failed.");
        }

        glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        glfw.WindowHint(WindowHintBool.Resizable, false);
        glfw.WindowHint(WindowHintBool.Visible, true);
        glfw.WindowHint(WindowHintBool.Decorated, true);

        var window = glfw.CreateWindow(width, height, title, null, null);
        if (window is null)
        {
            glfw.Terminate();
            throw new InvalidOperationException("Unable to create GLFW window.");
        }

        var proc = (delegate* unmanaged[Cdecl]<WindowHandle*, nint>)glfw.GetProcAddress("glfwGetCocoaWindow");
        if (proc == null)
        {
            glfw.DestroyWindow(window);
            glfw.Terminate();
            throw new InvalidOperationException("glfwGetCocoaWindow is unavailable in this GLFW build.");
        }

        using var pool = ObjectiveCRuntime.PushAutoreleasePool();

        var nsWindow = proc(window);
        if (nsWindow == nint.Zero)
        {
            glfw.DestroyWindow(window);
            glfw.Terminate();
            throw new InvalidOperationException("Failed to obtain NSWindow from GLFW.");
        }

        var contentView = ObjectiveCRuntime.objc_msgSend_IntPtr(nsWindow, Selectors.ContentView);
        if (contentView == nint.Zero)
        {
            glfw.DestroyWindow(window);
            glfw.Terminate();
            throw new InvalidOperationException("NSWindow contentView was null.");
        }

        var metalLayer = CreateMetalLayer(width, height, nsWindow);
        ObjectiveCRuntime.objc_msgSend_Void_Bool(contentView, Selectors.SetWantsLayer, true);
        ObjectiveCRuntime.objc_msgSend_Void_IntPtr(contentView, Selectors.SetLayer, metalLayer);

        return new MacMetalHost(glfw, window, proc, nsWindow, metalLayer);
    }

    public bool ShouldClose => _glfw.WindowShouldClose(_window);

    public nint NSWindow => _nsWindow;

    public nuint WindowNumber => _nsWindow == nint.Zero
        ? 0
        : ObjectiveCRuntime.objc_msgSend_NUInt(_nsWindow, Selectors.WindowNumber);

    public void PumpEvents()
    {
        _glfw.PollEvents();
    }

    public bool TryAcquireDrawable(out nint drawable)
    {
        drawable = nint.Zero;
        if (_metalLayer == nint.Zero)
        {
            return false;
        }

        using var pool = ObjectiveCRuntime.PushAutoreleasePool();
        drawable = ObjectiveCRuntime.objc_msgSend_IntPtr(_metalLayer, Selectors.NextDrawable);
        return drawable != nint.Zero;
    }

    public void ReleaseDrawable(nint drawable)
    {
        if (drawable != nint.Zero)
        {
            ObjectiveCRuntime.Release(drawable);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_metalLayer != nint.Zero)
        {
            ObjectiveCRuntime.Release(_metalLayer);
            _metalLayer = nint.Zero;
        }

        if (_window is not null)
        {
            _glfw.DestroyWindow(_window);
        }

        _glfw.Terminate();
    }

    private static nint CreateMetalLayer(int width, int height, nint nsWindow)
    {
        var layerClass = ObjectiveCRuntime.GetClass("CAMetalLayer");
        if (layerClass == nint.Zero)
        {
            throw new InvalidOperationException("CAMetalLayer class could not be located.");
        }

        var layerAlloc = ObjectiveCRuntime.objc_msgSend_IntPtr(layerClass, Selectors.Alloc);
        var layer = ObjectiveCRuntime.objc_msgSend_IntPtr(layerAlloc, Selectors.Init);
        if (layer == nint.Zero)
        {
            throw new InvalidOperationException("Failed to instantiate CAMetalLayer.");
        }

        var device = ObjectiveCRuntime.MTLCreateSystemDefaultDevice();
        if (device == nint.Zero)
        {
            throw new InvalidOperationException("MTLCreateSystemDefaultDevice returned null.");
        }

        ObjectiveCRuntime.objc_msgSend_Void_IntPtr(layer, Selectors.SetDevice, device);
        ObjectiveCRuntime.objc_msgSend_Void_Bool(layer, Selectors.SetPresentsWithTransaction, false);

        var size = new CGSize(width, height);
        ObjectiveCRuntime.objc_msgSend_Void_CGSize(layer, Selectors.SetDrawableSize, size);
        ObjectiveCRuntime.objc_msgSend_Void_CGRect(layer, Selectors.SetFrame, new CGRect(new CGPoint(0, 0), size));

        var scale = ObjectiveCRuntime.objc_msgSend_Double(nsWindow, Selectors.BackingScaleFactor);
        if (scale <= 0)
        {
            scale = 1.0;
        }

        ObjectiveCRuntime.objc_msgSend_Void_Double(layer, Selectors.SetContentsScale, scale);

        ObjectiveCRuntime.objc_msgSend_Void_Bool(layer, Selectors.SetDisplaySyncEnabled, true);

        return layer;
    }

    private static class Selectors
    {
        internal static readonly nint Alloc = ObjectiveCRuntime.Selector("alloc");
        internal static readonly nint Init = ObjectiveCRuntime.Selector("init");
        internal static readonly nint ContentView = ObjectiveCRuntime.Selector("contentView");
        internal static readonly nint SetWantsLayer = ObjectiveCRuntime.Selector("setWantsLayer:");
        internal static readonly nint SetLayer = ObjectiveCRuntime.Selector("setLayer:");
        internal static readonly nint SetDevice = ObjectiveCRuntime.Selector("setDevice:");
        internal static readonly nint SetPresentsWithTransaction = ObjectiveCRuntime.Selector("setPresentsWithTransaction:");
        internal static readonly nint SetDrawableSize = ObjectiveCRuntime.Selector("setDrawableSize:");
        internal static readonly nint SetFrame = ObjectiveCRuntime.Selector("setFrame:");
        internal static readonly nint NextDrawable = ObjectiveCRuntime.Selector("nextDrawable");
        internal static readonly nint SetContentsScale = ObjectiveCRuntime.Selector("setContentsScale:");
        internal static readonly nint BackingScaleFactor = ObjectiveCRuntime.Selector("backingScaleFactor");
        internal static readonly nint SetDisplaySyncEnabled = ObjectiveCRuntime.Selector("setDisplaySyncEnabled:");
        internal static readonly nint WindowNumber = ObjectiveCRuntime.Selector("windowNumber");
    }
}
