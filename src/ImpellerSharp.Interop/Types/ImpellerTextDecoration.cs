using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct ImpellerTextDecoration
{
    public ImpellerTextDecorationType Types;
    public ImpellerColor Color;
    public ImpellerTextDecorationStyle Style;
    public float ThicknessMultiplier;

    public static ImpellerTextDecoration None => new()
    {
        Types = ImpellerTextDecorationType.None,
        Color = new ImpellerColor(0f, 0f, 0f, 0f),
        Style = ImpellerTextDecorationStyle.Solid,
        ThicknessMultiplier = 1f,
    };
}
