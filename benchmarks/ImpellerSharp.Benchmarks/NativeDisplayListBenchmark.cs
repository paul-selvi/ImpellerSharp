using System;
using BenchmarkDotNet.Attributes;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public unsafe class NativeDisplayListBenchmark : IDisposable
{
    private nint _paint;
    private ImpellerRect[] _rects = Array.Empty<ImpellerRect>();
    private bool _initialized;
    private string? _skipReason;

    [Params(256, 1024)]
    public int RectCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        try
        {
            _paint = NativeDisplayListInterop.PaintNew();
            if (_paint == nint.Zero)
            {
                throw new InvalidOperationException("ImpellerPaintNew returned null.");
            }

            var color = new ImpellerColor(1f, 0.2f, 0.1f, 1f);
            NativeDisplayListInterop.PaintSetColor(_paint, &color);
            _rects = RectSequenceCache.GetRects(RectCount);
            _initialized = true;
        }
        catch (Exception ex)
        {
            _skipReason = ex.Message;
            _initialized = false;
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Dispose();
    }

    [Benchmark(Description = "Native ABI display list build (rects)")]
    public int BuildDisplayListNative()
    {
        EnsureInitialized();

        var builder = NativeDisplayListInterop.DisplayListBuilderNew(null);
        if (builder == nint.Zero)
        {
            throw new InvalidOperationException("ImpellerDisplayListBuilderNew returned null.");
        }

        var paint = _paint;
        var rects = _rects;
        if (rects.Length != RectCount)
        {
            rects = _rects = RectSequenceCache.GetRects(RectCount);
        }

        try
        {
            for (var i = 0; i < rects.Length; i++)
            {
                var rect = rects[i];
                NativeDisplayListInterop.DisplayListBuilderDrawRect(builder, &rect, paint);
            }

            var displayList = NativeDisplayListInterop.DisplayListBuilderCreateDisplayListNew(builder);
            if (displayList == nint.Zero)
            {
                throw new InvalidOperationException("ImpellerDisplayListBuilderCreateDisplayListNew returned null.");
            }

            NativeDisplayListInterop.DisplayListRelease(displayList);
        }
        finally
        {
            NativeDisplayListInterop.DisplayListBuilderRelease(builder);
        }

        return rects.Length;
    }

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Native benchmark prerequisites are missing: {_skipReason ?? "unknown error resolving native exports."} " +
            "Ensure the Impeller native library is available and built before running benchmarks.");
    }

    public void Dispose()
    {
        if (_paint != nint.Zero)
        {
            NativeDisplayListInterop.PaintRelease(_paint);
            _paint = nint.Zero;
        }
    }
}
