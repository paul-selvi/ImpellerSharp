using System;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Samples.BasicShapes.Scenes;

internal sealed class RectGridScene : IScene
{
    private ImpellerPaintHandle? _fillPaint;
    private ImpellerPaintHandle? _backgroundPaint;
    private ImpellerRect[] _rects = Array.Empty<ImpellerRect>();

    public string Name => "rects";

    public void Initialize(ImpellerContextHandle context)
    {
        _fillPaint = ImpellerPaintHandle.Create();
        _fillPaint.SetColor(new ImpellerColor(0.95f, 0.3f, 0.2f, 1f));

        _backgroundPaint = ImpellerPaintHandle.Create();
        _backgroundPaint.SetColor(new ImpellerColor(0.08f, 0.08f, 0.1f, 1f));

        _rects = BuildRectangles(256);
    }

    public ImpellerDisplayListHandle CreateDisplayList(ImpellerContextHandle context, int frameIndex)
    {
        using var builder = ImpellerDisplayListBuilderHandle.Create();

        var background = new ImpellerRect(0, 0, 1280, 720);
        builder.DrawRect(background, _backgroundPaint!);

        var paint = _fillPaint!;

        for (var i = 0; i < _rects.Length; i++)
        {
            builder.DrawRect(_rects[i], paint);
        }

        return builder.Build();
    }

    public string DescribeFrame(int frameIndex)
    {
        if (_rects.Length == 0)
        {
            return "rects:empty";
        }

        double sumX = 0;
        double sumY = 0;
        double sumWidth = 0;
        double sumHeight = 0;

        for (var i = 0; i < _rects.Length; i++)
        {
            ref readonly var rect = ref _rects[i];
            sumX += rect.X;
            sumY += rect.Y;
            sumWidth += rect.Width;
            sumHeight += rect.Height;
        }

        return $"rects:count={_rects.Length};sumX={sumX:F2};sumY={sumY:F2};sumW={sumWidth:F2};sumH={sumHeight:F2};frame={frameIndex}";
    }

    public void Dispose()
    {
        _fillPaint?.Dispose();
        _fillPaint = null;

        _backgroundPaint?.Dispose();
        _backgroundPaint = null;
    }

    private static ImpellerRect[] BuildRectangles(int count)
    {
        var rects = new ImpellerRect[count];
        const float baseX = 48f;
        const float baseY = 48f;
        const float width = 80f;
        const float height = 80f;
        const float spacing = 16f;
        const int columns = 16;

        for (var i = 0; i < count; i++)
        {
            var column = i % columns;
            var row = i / columns;

            rects[i] = new ImpellerRect(
                baseX + column * (width + spacing),
                baseY + row * (height + spacing),
                width,
                height);
        }

        return rects;
    }
}
