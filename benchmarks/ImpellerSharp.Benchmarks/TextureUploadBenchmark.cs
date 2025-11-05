using System;
using BenchmarkDotNet.Attributes;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public unsafe class TextureUploadBenchmark : IDisposable
{
    private BenchmarkContextProvider? _contextProvider;
    private ImpellerTextureDescriptor _descriptor;
    private byte[] _pixels = Array.Empty<byte>();
    private bool _initialized;
    private string? _skipReason;

    [Params(128, 256)]
    public int TextureSize { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _contextProvider = new BenchmarkContextProvider();

        if (!_contextProvider.TryEnsureContext(out var failure))
        {
            _skipReason = failure ?? "Unknown context creation failure.";
            _initialized = false;
            return;
        }

        InitialiseTexture(TextureSize);
        _initialized = true;
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Dispose();
    }

    [Benchmark(Description = "Managed texture upload (TextureFactory)")]
    public int ManagedTextureUpload()
    {
        EnsureInitialized();

        var context = _contextProvider!.Context!;
        var descriptor = _descriptor;

        using var texture = context.CreateTexture(descriptor, _pixels);
        return (int)descriptor.Size.Width;
    }

    [Benchmark(Description = "Native texture upload (C ABI)")]
    public int NativeTextureUpload()
    {
        EnsureInitialized();

        var context = _contextProvider!.Context!;
        var descriptor = _descriptor;

        fixed (byte* data = _pixels)
        {
            var mapping = new NativeDisplayListInterop.NativeImpellerMapping
            {
                Data = data,
                Length = (ulong)_pixels.Length,
                OnRelease = null,
            };

            var texture = NativeDisplayListInterop.TextureCreateWithContentsNew(
                context.DangerousGetHandle(),
                descriptor,
                mapping,
                nint.Zero);

            if (texture == nint.Zero)
            {
                throw new InvalidOperationException("ImpellerTextureCreateWithContentsNew returned null.");
            }

            NativeDisplayListInterop.TextureRelease(texture);
        }

        return (int)descriptor.Size.Width;
    }

    public void Dispose()
    {
        _contextProvider?.Dispose();
        _contextProvider = null;
    }

    private void InitialiseTexture(int size)
    {
        _descriptor = new ImpellerTextureDescriptor(
            ImpellerPixelFormat.Rgba8888,
            new ImpellerISize(size, size));

        var totalBytes = checked(size * size * 4);
        _pixels = new byte[totalBytes];

        var seed = new Random(1234);
        seed.NextBytes(_pixels);
    }

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Texture upload benchmark prerequisites are missing: {_skipReason ?? "unknown error."} ");
    }
}
