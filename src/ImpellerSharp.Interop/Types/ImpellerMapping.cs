using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ImpellerMapping
{
    public byte* Data;
    public ulong Length;
    public delegate* unmanaged[Cdecl]<nint, void> OnRelease;
}
