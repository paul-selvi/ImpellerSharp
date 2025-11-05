using System;
using System.Security.Cryptography;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Samples.BasicShapes.Scenes;

internal sealed class TextureStreamingScene : IScene
{
    private readonly Random _random = new(1337);
    private byte[] _pixelBuffer = Array.Empty<byte>();
    private ImpellerTextureDescriptor _descriptor;
    private ImpellerTextureHandle? _texture;
    private ImpellerPaintHandle? _borderPaint;
    private ImpellerRect _destinationRect;
    private ImpellerRect _sourceRect;
    private string _lastDigest = string.Empty;

    public string Name => "stream";

    public void Initialize(ImpellerContextHandle context)
    {
        _descriptor = new ImpellerTextureDescriptor(
            ImpellerPixelFormat.Rgba8888,
            new ImpellerISize(256, 256));

        var pixelCount = (int)(_descriptor.Size.Width * _descriptor.Size.Height * 4);
        _pixelBuffer = new byte[pixelCount];

        _sourceRect = new ImpellerRect(0, 0, _descriptor.Size.Width, _descriptor.Size.Height);
        _destinationRect = new ImpellerRect(256, 160, _descriptor.Size.Width, _descriptor.Size.Height);

        _borderPaint = ImpellerPaintHandle.Create();
        _borderPaint.SetColor(new ImpellerColor(0.95f, 0.95f, 0.95f, 1f));
        _borderPaint.SetStrokeWidth(4f);
        _borderPaint.SetDrawStyle(ImpellerDrawStyle.Stroke);
    }

    public ImpellerDisplayListHandle CreateDisplayList(ImpellerContextHandle context, int frameIndex)
    {
        RefreshTexture(context, frameIndex);

        using var builder = ImpellerDisplayListBuilderHandle.Create();

        // Draw gradient background.
        using var backgroundPaint = ImpellerPaintHandle.Create();
        backgroundPaint.SetColor(new ImpellerColor(0.05f, 0.07f, 0.1f, 1f));
        builder.DrawRect(new ImpellerRect(0, 0, 1280, 720), backgroundPaint);

        if (_texture is not null)
        {
            builder.DrawTextureRect(
                _texture,
                _sourceRect,
                _destinationRect,
                ImpellerTextureSampling.Linear);
        }

        builder.DrawRect(_destinationRect, _borderPaint!);

        return builder.Build();
    }

    public string DescribeFrame(int frameIndex)
    {
        if (string.IsNullOrEmpty(_lastDigest))
        {
            return $"stream:digest=unavailable;frame={frameIndex}";
        }

        return $"stream:digest={_lastDigest};frame={frameIndex}";
    }

    public void Dispose()
    {
        _texture?.Dispose();
        _texture = null;

        _borderPaint?.Dispose();
        _borderPaint = null;
    }

    private void RefreshTexture(ImpellerContextHandle context, int frameIndex)
    {
        Span<byte> pixels = _pixelBuffer;
        var width = (int)_descriptor.Size.Width;
        var height = (int)_descriptor.Size.Height;
        var stride = width * 4;
        var phase = frameIndex % 360;

        for (var y = 0; y < height; y++)
        {
            var row = pixels.Slice(y * stride, stride);
            for (var x = 0; x < width; x++)
            {
                var offset = x * 4;
                var u = (byte)((x + phase) % 256);
                var v = (byte)((y * 2 + phase) % 256);
                row[offset + 0] = u;
                row[offset + 1] = v;
                row[offset + 2] = (byte)_random.Next(32, 220);
                row[offset + 3] = 255;
            }
        }

        _lastDigest = ComputeDigest(pixels);

        if (SceneExecutionContext.Headless)
        {
            return;
        }

        var nextTexture = context.CreateTexture(_descriptor, pixels);

        _texture?.Dispose();
        _texture = nextTexture;
    }

    private static string ComputeDigest(Span<byte> pixels)
    {
        var hash = SHA256.HashData(pixels);
        return Convert.ToHexString(hash);
    }
}
