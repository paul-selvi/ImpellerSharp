using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Samples.BasicShapes;

internal static class ObjectiveCRuntime
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";
    private const string FoundationLibrary = "/System/Library/Frameworks/Foundation.framework/Foundation";
    private const string MetalLibrary = "/System/Library/Frameworks/Metal.framework/Metal";

    [DllImport(ObjCLibrary, EntryPoint = "sel_registerName")]
    internal static extern nint Selector(string name);

    [DllImport(ObjCLibrary, EntryPoint = "objc_getClass")]
    internal static extern nint GetClass(string name);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    internal static extern nint objc_msgSend_IntPtr(nint receiver, nint selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    internal static extern void objc_msgSend_Void_IntPtr(nint receiver, nint selector, nint arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    internal static extern void objc_msgSend_Void_Bool(nint receiver, nint selector, bool value);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    internal static extern void objc_msgSend_Void_Double(nint receiver, nint selector, double value);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    internal static extern void objc_msgSend_Void_CGSize(nint receiver, nint selector, CGSize size);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    internal static extern void objc_msgSend_Void_CGRect(nint receiver, nint selector, CGRect rect);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    internal static extern double objc_msgSend_Double(nint receiver, nint selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    internal static extern nuint objc_msgSend_NUInt(nint receiver, nint selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_release")]
    internal static extern void Release(nint obj);

    [DllImport(ObjCLibrary, EntryPoint = "objc_retain")]
    internal static extern nint Retain(nint obj);

    [DllImport(ObjCLibrary, EntryPoint = "objc_autoreleasePoolPush")]
    private static extern nint AutoreleasePoolPush();

    [DllImport(ObjCLibrary, EntryPoint = "objc_autoreleasePoolPop")]
    private static extern void AutoreleasePoolPop(nint state);

    [DllImport(MetalLibrary, EntryPoint = "MTLCreateSystemDefaultDevice")]
    internal static extern nint MTLCreateSystemDefaultDevice();

    internal static AutoreleasePoolScope PushAutoreleasePool()
    {
        return new AutoreleasePoolScope(AutoreleasePoolPush());
    }

    internal readonly ref struct AutoreleasePoolScope
    {
        private readonly nint _state;

        internal AutoreleasePoolScope(nint state)
        {
            _state = state;
        }

        public void Dispose()
        {
            if (_state != nint.Zero)
            {
                AutoreleasePoolPop(_state);
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct CGSize
{
    public double Width;
    public double Height;

    public CGSize(double width, double height)
    {
        Width = width;
        Height = height;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct CGPoint
{
    public double X;
    public double Y;

    public CGPoint(double x, double y)
    {
        X = x;
        Y = y;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct CGRect
{
    public CGPoint Origin;
    public CGSize Size;

    public CGRect(CGPoint origin, CGSize size)
    {
        Origin = origin;
        Size = size;
    }
}
