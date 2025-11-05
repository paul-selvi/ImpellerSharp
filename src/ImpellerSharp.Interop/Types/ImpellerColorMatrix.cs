using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ImpellerColorMatrix
{
    public const int ElementCount = 20;

    private fixed float _m[ElementCount];

    public Span<float> AsSpan()
    {
        fixed (float* ptr = _m)
        {
            return new Span<float>(ptr, ElementCount);
        }
    }

    public void Set(ReadOnlySpan<float> values)
    {
        if (values.Length != ElementCount)
        {
            throw new ArgumentException("Color matrix requires exactly 20 values.", nameof(values));
        }

        values.CopyTo(AsSpan());
    }

    public static ImpellerColorMatrix Identity
    {
        get
        {
            var matrix = new ImpellerColorMatrix();
            var span = matrix.AsSpan();
            span.Clear();
            span[0] = 1f;
            span[6] = 1f;
            span[12] = 1f;
            span[18] = 1f;
            return matrix;
        }
    }
}
