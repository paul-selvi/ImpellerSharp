using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct ImpellerISize
{
    public long Width;
    public long Height;

    public ImpellerISize(long width, long height)
    {
        Width = width;
        Height = height;
    }
}
