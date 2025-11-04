using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Avalonia.Mac.Controls;

internal static class MetalView
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";
    private const string MetalFramework = "/System/Library/Frameworks/Metal.framework/Metal";

    private static readonly IntPtr NSViewClass;
    private static readonly IntPtr CAMetalLayerClass;
    private static readonly IntPtr NSAutoreleasePoolClass;

    private static readonly IntPtr selAlloc;
    private static readonly IntPtr selInit;
    private static readonly IntPtr selDrain;
    private static readonly IntPtr selRetain;
    private static readonly IntPtr selRelease;
    private static readonly IntPtr selLayer;
    private static readonly IntPtr selSetLayer;
    private static readonly IntPtr selSetWantsLayer;
    private static readonly IntPtr selSetTranslatesAutoresizingMaskIntoConstraints;
    private static readonly IntPtr selSetNeedsDisplay;
    private static readonly IntPtr selSetFrame;
    private static readonly IntPtr selSetDrawableSize;
    private static readonly IntPtr selSetContentsScale;
    private static readonly IntPtr selSetDevice;
    private static readonly IntPtr selDevice;
    private static readonly IntPtr selSetPixelFormat;
    private static readonly IntPtr selSetFramebufferOnly;
    private static readonly IntPtr selSetPresentsWithTransaction;
    private static readonly IntPtr selSetDisplaySyncEnabled;
    private static readonly IntPtr selRespondsToSelector;
    private static readonly IntPtr selNextDrawable;
    private static readonly IntPtr selTexture;
    private static readonly IntPtr selPresent;

    private const nuint MTLPixelFormatBGRA8Unorm = 80;

    static MetalView()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        NSViewClass = objc_getClass("NSView");
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
        selSetTranslatesAutoresizingMaskIntoConstraints = sel_registerName("setTranslatesAutoresizingMaskIntoConstraints:");
        selSetNeedsDisplay = sel_registerName("setNeedsDisplay:");
        selSetFrame = sel_registerName("setFrame:");
        selSetDrawableSize = sel_registerName("setDrawableSize:");
        selSetContentsScale = sel_registerName("setContentsScale:");
        selSetDevice = sel_registerName("setDevice:");
        selDevice = sel_registerName("device");
        selSetPixelFormat = sel_registerName("setPixelFormat:");
        selSetFramebufferOnly = sel_registerName("setFramebufferOnly:");
        selSetPresentsWithTransaction = sel_registerName("setPresentsWithTransaction:");
        selSetDisplaySyncEnabled = sel_registerName("setDisplaySyncEnabled:");
        selRespondsToSelector = sel_registerName("respondsToSelector:");
        selNextDrawable = sel_registerName("nextDrawable");
        selTexture = sel_registerName("texture");
        selPresent = sel_registerName("present");
    }

    public static nint Create()
    {
        if (!OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("MetalView is only available on macOS.");
        }

        using var pool = new AutoreleasePool();

        var view = objc_msgSend_IntPtr(objc_msgSend_IntPtr(NSViewClass, selAlloc), selInit);
        objc_msgSend_bool(view, selSetWantsLayer, true);
        objc_msgSend_bool(view, selSetTranslatesAutoresizingMaskIntoConstraints, true);

        var layer = objc_msgSend_IntPtr(CAMetalLayerClass, selLayer);
        if (layer == nint.Zero)
        {
            throw new InvalidOperationException("Failed to create CAMetalLayer.");
        }

        var device = MTLCreateSystemDefaultDevice();
        if (device == nint.Zero)
        {
            throw new InvalidOperationException("Failed to acquire default Metal device.");
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

        objc_msgSend_Void_IntPtr(view, selSetLayer, layer);
        objc_msgSend_bool(view, selSetNeedsDisplay, true);

        return view;
    }

    public static void Destroy(nint view)
    {
        if (view == nint.Zero)
        {
            return;
        }

        objc_msgSend_Void_IntPtr(view, selSetLayer, nint.Zero);
        objc_msgSend_void(view, selRelease);
    }

    public static nint GetLayer(nint view)
    {
        return view == nint.Zero ? nint.Zero : objc_msgSend_IntPtr(view, selLayer);
    }

    public static void SetFrame(nint view, double width, double height)
    {
        if (view == nint.Zero)
        {
            return;
        }

        var rect = new CGRect
        {
            Origin = new CGPoint { X = 0, Y = 0 },
            Size = new CGSize { Width = width, Height = height }
        };

        objc_msgSend_CGRect(view, selSetFrame, rect);
    }

    public static void SetDrawableSize(nint layer, int pixelWidth, int pixelHeight)
    {
        if (layer == nint.Zero)
        {
            return;
        }

        var size = new CGSize { Width = pixelWidth, Height = pixelHeight };
        objc_msgSend_CGSize(layer, selSetDrawableSize, size);
    }

    public static void SetContentsScale(nint layer, double scaling)
    {
        if (layer == nint.Zero)
        {
            return;
        }

        objc_msgSend_double(layer, selSetContentsScale, scaling);
    }

    public static nint NextDrawable(nint layer) =>
        layer == nint.Zero ? nint.Zero : objc_msgSend_IntPtr(layer, selNextDrawable);

    public static nint GetDrawableTexture(nint drawable) =>
        drawable == nint.Zero ? nint.Zero : objc_msgSend_IntPtr(drawable, selTexture);

    public static void PresentDrawable(nint drawable)
    {
        if (drawable == nint.Zero)
        {
            return;
        }

        objc_msgSend_void(drawable, selPresent);
    }

    public static void Retain(nint handle)
    {
        if (handle == nint.Zero)
        {
            return;
        }

        objc_msgSend_void(handle, selRetain);
    }

    public static void Release(nint handle)
    {
        if (handle == nint.Zero)
        {
            return;
        }

        objc_msgSend_void(handle, selRelease);
    }

    public static nint GetDevice(nint layer) =>
        layer == nint.Zero ? nint.Zero : objc_msgSend_IntPtr(layer, selDevice);

    private static bool RespondsToSelector(nint obj, nint selector)
    {
        if (obj == nint.Zero || selector == nint.Zero)
        {
            return false;
        }

        return objc_msgSend_bool_return(obj, selRespondsToSelector, selector);
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
    private static extern void objc_msgSend_CGSize(nint receiver, nint selector, CGSize value);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_CGRect(nint receiver, nint selector, CGRect value);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_Void_IntPtr(nint receiver, nint selector, nint arg);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool objc_msgSend_bool_return(nint receiver, nint selector, nint argument);

    private readonly struct AutoreleasePool : IDisposable
    {
        private readonly nint _pool;

        public AutoreleasePool()
        {
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
}
