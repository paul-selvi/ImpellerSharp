using System;
using BenchmarkDotNet.Attributes;
using ImpellerSharp.Interop;
using ImpellerSharp.Interop.Hosting;

namespace ImpellerSharp.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class SurfacePresentBenchmark : IDisposable
{
    private MetalGlfwAppHost? _host;
    private bool _initialized;
    private string? _skipReason;

    [GlobalSetup]
    public void GlobalSetup()
    {
        if (!OperatingSystem.IsMacOS())
        {
            _skipReason = "Surface present benchmark currently requires macOS Metal backend.";
            _initialized = false;
            return;
        }

        _host = new MetalGlfwAppHost(new MetalGlfwHostOptions
        {
            Title = "ImpellerSharp.Benchmarks.SurfacePresent",
            Width = 640,
            Height = 480,
            Visible = false,
        });

        _initialized = true;
    }

    [GlobalCleanup]
    public void Dispose()
    {
        _host?.Dispose();
        _host = null;
    }

    [Benchmark(Description = "Managed surface draw + present loop")]
    public int ManagedSurfacePresent()
    {
        EnsureInitialized();

        if (_host is null)
        {
            throw new InvalidOperationException("Host not initialised.");
        }

        var frameCount = 0;

        var result = _host.Run((builder, width, height) =>
        {
            if (frameCount >= 60)
            {
                return false;
            }

            using var paint = CreateClearPaint();
            builder.DrawRect(new ImpellerRect(0, 0, width, height), paint);
            frameCount++;
            return true;
        });

        if (result != 0)
        {
            throw new InvalidOperationException($"MetalGlfwAppHost returned exit code {result}.");
        }

        return frameCount;
    }

    private static ImpellerPaintHandle CreateClearPaint()
    {
        var paint = ImpellerPaintHandle.Create();
        paint.SetColor(new ImpellerColor(0f, 0f, 0f, 1f));
        return paint;
    }

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Surface present benchmark prerequisites missing: {_skipReason ?? "unknown error"}");
    }
}
