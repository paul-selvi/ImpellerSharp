using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct ImpellerPoint
{
    public float X;
    public float Y;

    public ImpellerPoint(float x, float y)
    {
        X = x;
        Y = y;
    }
}
