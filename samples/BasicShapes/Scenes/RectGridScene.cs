using System;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Samples.BasicShapes.Scenes;

internal sealed class RectGridScene : IScene
{
    private ImpellerPaintHandle? _fillPaint;
    private ImpellerPaintHandle? _backgroundPaint;
    private ImpellerPaintHandle? _strokePaint;
    private ImpellerPathHandle? _tilePath;
    private ImpellerPathHandle? _tileOutlinePath;
    private ImpellerPathHandle? _badgePath;
    private ImpellerRect[] _rects = Array.Empty<ImpellerRect>();
    private ImpellerRoundingRadii _tileRadii;
    private ImpellerRect _tilePathBounds;
    private ImpellerRect _badgeBounds;

    public string Name => "rects";

    public void Initialize(ImpellerContextHandle context)
    {
        _fillPaint = ImpellerPaintHandle.Create();
        _fillPaint.SetColor(new ImpellerColor(0.95f, 0.3f, 0.2f, 1f));

        _backgroundPaint = ImpellerPaintHandle.Create();
        _backgroundPaint.SetColor(new ImpellerColor(0.08f, 0.08f, 0.1f, 1f));

        _strokePaint = ImpellerPaintHandle.Create();
        _strokePaint.SetColor(new ImpellerColor(0.98f, 0.98f, 1f, 1f));
        _strokePaint.SetDrawStyle(ImpellerDrawStyle.Stroke);
        _strokePaint.SetStrokeWidth(2.5f);
        _strokePaint.SetStrokeMiter(4f);

        _tileRadii = ImpellerRoundingRadii.Uniform(12f);

        using (var pathBuilder = ImpellerPathBuilderHandle.Create())
        {
            var canonicalTile = new ImpellerRect(-TileWidth / 2f, -TileHeight / 2f, TileWidth, TileHeight);
            pathBuilder.AddRoundedRect(canonicalTile, _tileRadii);
            pathBuilder.ClosePath();
            _tileOutlinePath = pathBuilder.CopyPath();
            _tilePath = pathBuilder.TakePath();
        }

        _tilePathBounds = _tilePath?.GetBounds() ?? default;

        using (var badgeBuilder = ImpellerPathBuilderHandle.Create())
        {
            var barRect = new ImpellerRect(-14f, -6f, 28f, 12f);
            badgeBuilder.AddRect(barRect);

            var arcBounds = new ImpellerRect(-14f, -14f, 28f, 28f);
            badgeBuilder.AddArc(arcBounds, -120f, 120f);

            var capOval = new ImpellerRect(-6f, -6f, 12f, 12f);
            badgeBuilder.AddOval(capOval);
            badgeBuilder.ClosePath();
            _badgePath = badgeBuilder.TakePath();
        }

        _badgeBounds = _badgePath?.GetBounds() ?? default;

        _rects = BuildRectangles(256);
    }

    public ImpellerDisplayListHandle CreateDisplayList(ImpellerContextHandle context, int frameIndex)
    {
        using var builder = ImpellerDisplayListBuilderHandle.Create();

        var viewport = new ImpellerRect(24f, 24f, 1280f - 48f, 720f - 48f);
        var viewportRadii = ImpellerRoundingRadii.Uniform(28f);
        builder.DrawRoundedRect(viewport, viewportRadii, _backgroundPaint!);

        var innerViewport = new ImpellerRect(viewport.X + 24f, viewport.Y + 24f, viewport.Width - 48f, viewport.Height - 48f);
        var innerRadii = ImpellerRoundingRadii.Uniform(18f);
        builder.DrawRoundedRectDifference(viewport, viewportRadii, innerViewport, innerRadii, _backgroundPaint!);

        var paint = _fillPaint!;

        for (var i = 0; i < _rects.Length; i++)
        {
            ref readonly var rect = ref _rects[i];

            if (_tilePath is not null)
            {
                builder.Save();
                builder.Translate(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                builder.DrawShadow(
                    _tilePath,
                    new ImpellerColor(0f, 0f, 0f, 0.35f),
                    elevation: 6f,
                    occluderIsTransparent: false,
                    devicePixelRatio: 1f);
                builder.Restore();
            }

            builder.DrawRoundedRect(rect, _tileRadii, paint);
            builder.DrawRoundedRect(rect, _tileRadii, _strokePaint!);

            if (_badgePath is not null && i % 32 == 0)
            {
                builder.Save();
                builder.Translate(rect.X + rect.Width - 18f, rect.Y + 18f);
                builder.DrawPath(_badgePath, _strokePaint!);
                builder.Restore();
            }

            if (_tileOutlinePath is not null && i == 0)
            {
                builder.Save();
                builder.Translate(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                builder.DrawPath(_tileOutlinePath, _strokePaint!);
                builder.Restore();
            }
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

        return FormattableString.Invariant(
            $"rects:count={_rects.Length};tile={_tilePathBounds.Width:F1}x{_tilePathBounds.Height:F1};badge={_badgeBounds.Width:F1}x{_badgeBounds.Height:F1};sumX={sumX:F2};sumY={sumY:F2};sumW={sumWidth:F2};sumH={sumHeight:F2};frame={frameIndex}");
    }

    public void Dispose()
    {
        _fillPaint?.Dispose();
        _fillPaint = null;

        _backgroundPaint?.Dispose();
        _backgroundPaint = null;

        _strokePaint?.Dispose();
        _strokePaint = null;

        _tilePath?.Dispose();
        _tilePath = null;

        _tileOutlinePath?.Dispose();
        _tileOutlinePath = null;

        _badgePath?.Dispose();
        _badgePath = null;
    }

    private static ImpellerRect[] BuildRectangles(int count)
    {
        var rects = new ImpellerRect[count];
        const float baseX = 64f;
        const float baseY = 64f;
        const float width = TileWidth;
        const float height = TileHeight;
        const float spacing = 16f;
        const int columns = 12;

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

    private const float TileWidth = 80f;
    private const float TileHeight = 80f;
}
