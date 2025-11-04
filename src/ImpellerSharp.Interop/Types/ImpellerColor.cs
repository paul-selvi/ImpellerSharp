using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct ImpellerColor
{
    public float Red;
    public float Green;
    public float Blue;
    public float Alpha;
    public ImpellerColorSpace ColorSpace;

    public ImpellerColor(float red, float green, float blue, float alpha, ImpellerColorSpace colorSpace = ImpellerColorSpace.SRgb)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
        ColorSpace = colorSpace;
    }
}
