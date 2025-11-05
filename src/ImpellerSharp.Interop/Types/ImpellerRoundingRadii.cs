using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct ImpellerRoundingRadii
{
    public ImpellerPoint TopLeft;
    public ImpellerPoint BottomLeft;
    public ImpellerPoint TopRight;
    public ImpellerPoint BottomRight;

    public static ImpellerRoundingRadii Uniform(float radius)
    {
        var point = new ImpellerPoint(radius, radius);
        return new ImpellerRoundingRadii
        {
            TopLeft = point,
            BottomLeft = point,
            TopRight = point,
            BottomRight = point,
        };
    }
}
