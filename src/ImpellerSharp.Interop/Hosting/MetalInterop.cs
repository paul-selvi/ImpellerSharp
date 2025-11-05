using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop.Hosting;

internal static class MetalInterop
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";
    private const string MetalFramework = "/System/Library/Frameworks/Metal.framework/Metal";
    private const nuint MTLPixelFormatBGRA8Unorm = 80;

    private static readonly nint CAMetalLayerClass;
    private static readonly nint NSAutoreleasePoolClass;

    private static readonly nint selAlloc;
    private static readonly nint selInit;
    private static readonly nint selDrain;
    private static readonly nint selRetain;
    private static readonly nint selRelease;
    private static readonly nint selLayer;
    private static readonly nint selSetLayer;
    private static readonly nint selSetWantsLayer;
    private static readonly nint selSetNeedsDisplay;
    private static readonly nint selContentView;
    private static readonly nint selSetDevice;
    private static readonly nint selSetPixelFormat;
    private static readonly nint selSetFramebufferOnly;
    private static readonly nint selSetPresentsWithTransaction;
    private static readonly nint selSetDisplaySyncEnabled;
    private static readonly nint selSetDrawableSize;
    private static readonly nint selSetContentsScale;
    private static readonly nint selSetFrame;
    private static readonly nint selRespondsToSelector;
    private static readonly nint selNextDrawable;
    private static readonly nint selBackingScaleFactor;

    static MetalInterop()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        CAMetalLayerClass = objc_getClass("CAMetalLayer");
        NSAutoreleasePoolClass = objc_getClass("NSAutoreleasePool");

        selAlloc = sel_registerName("alloc");
        selInit = sel_registerName("init");
        selDrain = sel_registerName("drain");
        selRetain = sel_registerName("retain");
        selRelease = sel_registerName("release");
        selLayer = sel_registerName("layer");
        selSetLayer = sel_registerName("setLayer:");
        selSetWantsLayer = sel_registerName("setWantsLayer:");
        selSetNeedsDisplay = sel_registerName("setNeedsDisplay:");
        selContentView = sel_registerName("contentView");
        selSetDevice = sel_registerName("setDevice:");
        selSetPixelFormat = sel_registerName("setPixelFormat:");
        selSetFramebufferOnly = sel_registerName("setFramebufferOnly:");
        selSetPresentsWithTransaction = sel_registerName("setPresentsWithTransaction:");
        selSetDisplaySyncEnabled = sel_registerName("setDisplaySyncEnabled:");
        selSetDrawableSize = sel_registerName("setDrawableSize:");
        selSetContentsScale = sel_registerName("setContentsScale:");
        selSetFrame = sel_registerName("setFrame:");
        selRespondsToSelector = sel_registerName("respondsToSelector:");
        selNextDrawable = sel_registerName("nextDrawable");
        selBackingScaleFactor = sel_registerName("backingScaleFactor");
    }

    public static MetalLayerContext AttachMetalLayer(nint window)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return default;
        }

        using var pool = AutoreleaseScope();

        var contentView = objc_msgSend_IntPtr(window, selContentView);
        if (contentView == nint.Zero)
        {
            return default;
        }

        var layer = objc_msgSend_IntPtr(CAMetalLayerClass, selLayer);
        if (layer == nint.Zero)
        {
            return default;
        }

        var device = MTLCreateSystemDefaultDevice();
        if (device == nint.Zero)
        {
            return default;
        }

        objc_msgSend_Void_IntPtr(layer, selSetDevice, device);
        objc_msgSend_nuint(layer, selSetPixelFormat, MTLPixelFormatBGRA8Unorm);
        objc_msgSend_bool(layer, selSetFramebufferOnly, false);

        if (RespondsToSelector(layer, selSetPresentsWithTransaction))
        {
            objc_msgSend_bool(layer, selSetPresentsWithTransaction, false);
        }

        if (RespondsToSelector(layer, selSetDisplaySyncEnabled))
        {
            objc_msgSend_bool(layer, selSetDisplaySyncEnabled, true);
        }

        objc_msgSend_Void_IntPtr(contentView, selSetLayer, layer);
        objc_msgSend_bool(contentView, selSetWantsLayer, true);
        objc_msgSend_bool(contentView, selSetNeedsDisplay, true);

        Retain(layer);

        var initialScale = GetBackingScaleFactor(window);
        if (initialScale <= 0)
        {
            initialScale = 1d;
        }

        return new MetalLayerContext(window, contentView, layer, initialScale);
    }

    public static void UpdateLayerGeometry(
        MetalLayerContext context,
        int logicalWidth,
        int logicalHeight,
        int pixelWidth,
        int pixelHeight,
        double scaling)
    {
        if (context.Layer == nint.Zero)
        {
            return;
        }

        var scale = scaling > 0 ? scaling : context.InitialScale;
        if (scale <= 0)
        {
            scale = 1d;
        }

        SetContentsScale(context.Layer, scale);
        SetDrawableSize(context.Layer, pixelWidth, pixelHeight);

        var frame = new CGRect
        {
            Origin = new CGPoint { X = 0, Y = 0 },
            Size = new CGSize { Width = logicalWidth, Height = logicalHeight }
        };

        objc_msgSend_CGRect(context.Layer, selSetFrame, frame);
    }

    public static IntPtr NextDrawable(nint layer) =>
        layer == nint.Zero ? nint.Zero : objc_msgSend_IntPtr(layer, selNextDrawable);

    public static void Retain(nint handle)
    {
        if (handle != nint.Zero)
        {
            objc_msgSend_void(handle, selRetain);
        }
    }

    public static void Release(nint handle)
    {
        if (handle != nint.Zero)
        {
            objc_msgSend_void(handle, selRelease);
        }
    }

    public static void Detach(in MetalLayerContext context)
    {
        if (context.View != nint.Zero)
        {
            objc_msgSend_Void_IntPtr(context.View, selSetLayer, nint.Zero);
        }

        if (context.Layer != nint.Zero)
        {
            Release(context.Layer);
        }
    }

    public static IDisposable AutoreleaseScope() =>
        OperatingSystem.IsMacOS() ? new AutoreleasePoolScope() : DummyDisposable.Instance;

    private static void SetDrawableSize(nint layer, int width, int height)
    {
        if (layer == nint.Zero)
        {
            return;
        }

        var size = new CGSize { Width = width, Height = height };
        objc_msgSend_CGSize(layer, selSetDrawableSize, size);
    }

    private static void SetContentsScale(nint layer, double scaling)
    {
        if (layer == nint.Zero || scaling <= 0)
        {
            return;
        }

        objc_msgSend_double(layer, selSetContentsScale, scaling);
    }

    private static double GetBackingScaleFactor(nint window)
    {
        if (window == nint.Zero || selBackingScaleFactor == nint.Zero)
        {
            return 1d;
        }

        return objc_msgSend_double_return(window, selBackingScaleFactor);
    }

    private static bool RespondsToSelector(nint obj, nint selector)
    {
        if (obj == nint.Zero || selector == nint.Zero)
        {
            return false;
        }

        return objc_msgSend_bool_return(obj, selRespondsToSelector, selector);
    }

    private sealed class DummyDisposable : IDisposable
    {
        public static readonly DummyDisposable Instance = new();
        public void Dispose()
        {
        }
    }

    private readonly struct AutoreleasePoolScope : IDisposable
    {
        private readonly nint _pool;

        public AutoreleasePoolScope()
        {
            if (!OperatingSystem.IsMacOS() || NSAutoreleasePoolClass == nint.Zero)
            {
                _pool = nint.Zero;
                return;
            }

            _pool = objc_msgSend_IntPtr(objc_msgSend_IntPtr(NSAutoreleasePoolClass, selAlloc), selInit);
        }

        public void Dispose()
        {
            if (_pool != nint.Zero)
            {
                objc_msgSend_void(_pool, selDrain);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGSize
    {
        public double Width;
        public double Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public CGPoint Origin;
        public CGSize Size;
    }

    [DllImport(MetalFramework, EntryPoint = "MTLCreateSystemDefaultDevice")]
    private static extern nint MTLCreateSystemDefaultDevice();

    [DllImport(ObjCLibrary, EntryPoint = "objc_getClass")]
    private static extern nint objc_getClass(string name);

    [DllImport(ObjCLibrary, EntryPoint = "sel_registerName")]
    private static extern nint sel_registerName(string name);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern nint objc_msgSend_IntPtr(nint receiver, nint selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(nint receiver, nint selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_bool(nint receiver, nint selector, bool value);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_double(nint receiver, nint selector, double value);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_nuint(nint receiver, nint selector, nuint value);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_Void_IntPtr(nint receiver, nint selector, nint argument);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_CGSize(nint receiver, nint selector, CGSize value);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_CGRect(nint receiver, nint selector, CGRect value);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool objc_msgSend_bool_return(nint receiver, nint selector, nint argument);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern double objc_msgSend_double_return(nint receiver, nint selector);
}

internal readonly struct MetalLayerContext
{
    public MetalLayerContext(nint window, nint view, nint layer, double initialScale)
    {
        Window = window;
        View = view;
        Layer = layer;
        InitialScale = initialScale;
    }

    public nint Window { get; }

    public nint View { get; }

    public nint Layer { get; }

    public double InitialScale { get; }
}
