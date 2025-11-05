using System;
using System.Collections.Generic;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Benchmarks;

internal static class RectSequenceCache
{
    private static readonly object Gate = new();
    private static readonly Dictionary<int, ImpellerRect[]> Cache = new();
    private const float BaseX = 10f;
    private const float BaseY = 10f;
    private const float Width = 32f;
    private const float Height = 32f;
    private const float Spacing = 4f;
    private const int Columns = 32;

    internal static ImpellerRect[] GetRects(int count)
    {
        lock (Gate)
        {
            if (!Cache.TryGetValue(count, out var rects))
            {
                rects = BuildRectSequence(count);
                Cache[count] = rects;
            }

            return rects;
        }
    }

    private static ImpellerRect[] BuildRectSequence(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Rectangle count must be positive.");
        }

        var rects = new ImpellerRect[count];

        for (var i = 0; i < count; i++)
        {
            var column = i % Columns;
            var row = i / Columns;

            rects[i] = new ImpellerRect(
                BaseX + column * (Width + Spacing),
                BaseY + row * (Height + Spacing),
                Width,
                Height);
        }

        return rects;
    }
}
