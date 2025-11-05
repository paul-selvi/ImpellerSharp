using System;
using BenchmarkDotNet.Attributes;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class DisplayListBuilderBenchmark : IDisposable
{
    private ImpellerPaintHandle? _paint;
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
            _paint = ImpellerPaintHandle.Create();
            _paint.SetColor(new ImpellerColor(1f, 0f, 0f, 1f));
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

    [Benchmark(Description = "Managed display list build (rects)")]
    public int BuildDisplayList()
    {
        EnsureInitialized();

        using var builder = ImpellerDisplayListBuilderHandle.Create();

        var paint = _paint!;
        var rects = _rects;
        if (rects.Length != RectCount)
        {
            rects = _rects = RectSequenceCache.GetRects(RectCount);
        }

        for (var i = 0; i < rects.Length; i++)
        {
            builder.DrawRect(rects[i], paint);
        }

        using var displayList = builder.Build();
        return RectCount;
    }

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Benchmark prerequisites are missing: {_skipReason ?? "unknown error creating Impeller handles."} " +
            "Ensure the Impeller native library is discoverable before running benchmarks.");
    }

    public void Dispose()
    {
        _paint?.Dispose();
        _paint = null;
    }
}
