using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal readonly struct Utf8String : IDisposable
{
    public IntPtr Pointer { get; }

    public Utf8String(string? value)
    {
        Pointer = value is null ? IntPtr.Zero : Marshal.StringToCoTaskMemUTF8(value);
    }

    public void Dispose()
    {
        if (Pointer != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(Pointer);
        }
    }
}
