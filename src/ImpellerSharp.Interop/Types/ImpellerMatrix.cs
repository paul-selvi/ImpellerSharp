using System;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ImpellerMatrix
{
    private fixed float _m[16];

    public Span<float> AsSpan()
    {
        fixed (float* ptr = _m)
        {
            return new Span<float>(ptr, 16);
        }
    }

    public void Set(ReadOnlySpan<float> values)
    {
        if (values.Length != 16)
        {
            throw new ArgumentException("Matrix requires exactly 16 values.", nameof(values));
        }

        values.CopyTo(AsSpan());
    }

    public static ImpellerMatrix Identity
    {
        get
        {
            var matrix = new ImpellerMatrix();
            var span = matrix.AsSpan();
            span.Clear();
            span[0] = 1f;
            span[5] = 1f;
            span[10] = 1f;
            span[15] = 1f;
            return matrix;
        }
    }
}
