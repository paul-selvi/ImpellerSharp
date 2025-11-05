using System;
using ImpellerSharp.Interop;

namespace AvaloniaImpellerApp.Rendering;

internal sealed class VectorPathsGalleryScene : IImpellerScene
{
    private readonly ImpellerPaintHandle _backgroundPaint;
    private readonly ImpellerPaintHandle _cardPaint;
    private readonly ImpellerPaintHandle _fillPaint;
    private readonly ImpellerPaintHandle _strokePaint;
    private readonly ImpellerPaintHandle _accentStrokePaint;

    private readonly ImpellerTypographyContextHandle _typography;
    private readonly ImpellerParagraphStyleHandle _captionStyle;

    private readonly ImpellerPathHandle _starPath;
    private readonly ImpellerPathHandle _orbitPath;
    private readonly ImpellerPathHandle _wavePath;

    private ImpellerParagraphHandle? _starCaption;
    private ImpellerParagraphHandle? _orbitCaption;
    private ImpellerParagraphHandle? _waveCaption;
    private float _captionWidth;

    public VectorPathsGalleryScene()
    {
        _backgroundPaint = ImpellerPaintHandle.Create();
        _backgroundPaint.SetColor(new ImpellerColor(0.06f, 0.07f, 0.1f, 1f));

        _cardPaint = ImpellerPaintHandle.Create();
        _cardPaint.SetColor(new ImpellerColor(0.11f, 0.15f, 0.2f, 0.92f));

        _fillPaint = ImpellerPaintHandle.Create();
        _fillPaint.SetColor(new ImpellerColor(0.66f, 0.45f, 0.92f, 0.95f));

        _strokePaint = ImpellerPaintHandle.Create();
        _strokePaint.SetDrawStyle(ImpellerDrawStyle.Stroke);
        _strokePaint.SetStrokeWidth(6f);
        _strokePaint.SetStrokeJoin(ImpellerStrokeJoin.Round);
        _strokePaint.SetStrokeCap(ImpellerStrokeCap.Round);
        _strokePaint.SetColor(new ImpellerColor(0.94f, 0.8f, 0.22f, 1f));

        _accentStrokePaint = ImpellerPaintHandle.Create();
        _accentStrokePaint.SetDrawStyle(ImpellerDrawStyle.Stroke);
        _accentStrokePaint.SetStrokeWidth(3f);
        _accentStrokePaint.SetStrokeJoin(ImpellerStrokeJoin.Round);
        _accentStrokePaint.SetStrokeCap(ImpellerStrokeCap.Round);
        _accentStrokePaint.SetColor(new ImpellerColor(0.32f, 0.87f, 0.98f, 1f));

        _typography = ImpellerTypographyContextHandle.Create();
        _captionStyle = ImpellerParagraphStyleHandle.Create();
        _captionStyle.SetFontSize(18f);
        _captionStyle.SetTextAlignment(ImpellerTextAlignment.Center);

        _starPath = CreateStarPath();
        _orbitPath = CreateOrbitPath();
        _wavePath = CreateWavePath();
    }

    public bool Render(ImpellerDisplayListBuilderHandle builder, float width, float height)
    {
        builder.DrawPaint(_backgroundPaint);

        var margin = 32f;
        var gap = 24f;
        var columns = 3f;
        var cardWidth = MathF.Max(160f, (width - margin * 2f - gap * (columns - 1f)) / columns);
        var cardHeight = MathF.Min(240f, MathF.Max(160f, height * 0.45f));
        var top = margin;

        EnsureCaptions(cardWidth - 18f);

        DrawCard(builder, _starPath, _fillPaint, _strokePaint, margin, top, cardWidth, cardHeight, _starCaption);

        var orbitX = margin + cardWidth + gap;
        DrawCard(builder, _orbitPath, _fillPaint, _accentStrokePaint, orbitX, top, cardWidth, cardHeight, _orbitCaption, rotation: 22f);

        var waveX = orbitX + cardWidth + gap;
        DrawCard(builder, _wavePath, _fillPaint, _accentStrokePaint, waveX, top, cardWidth, cardHeight, _waveCaption, oscillate: true);

        return true;
    }

    private void DrawCard(
        ImpellerDisplayListBuilderHandle builder,
        ImpellerPathHandle path,
        ImpellerPaintHandle fill,
        ImpellerPaintHandle stroke,
        float x,
        float y,
        float width,
        float height,
        ImpellerParagraphHandle? caption,
        float rotation = 0f,
        bool oscillate = false)
    {
        var rect = new ImpellerRect(x, y, width, height);
        builder.DrawRect(rect, _cardPaint);

        var centerX = rect.X + rect.Width * 0.5f;
        var centerY = rect.Y + rect.Height * 0.5f;
        var scale = MathF.Min(rect.Width, rect.Height) * 0.0035f;

        if (oscillate)
        {
            var time = (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;
            rotation = MathF.Sin(time * 1.2f) * 14f;
            scale *= 1f + MathF.Sin(time * 2.1f) * 0.08f;
        }

        builder.Save();
        builder.Translate(centerX, centerY);
        builder.Rotate(rotation);
        builder.Scale(scale, scale);
        builder.DrawPath(path, fill);
        builder.DrawPath(path, stroke);
        builder.Restore();

        if (caption is not null)
        {
            var captionX = rect.X + (rect.Width - _captionWidth) * 0.5f;
            var captionY = rect.Y + rect.Height + 16f;
            builder.DrawParagraph(caption, new ImpellerPoint(captionX, captionY));
        }
    }

    private void EnsureCaptions(float width)
    {
        if (MathF.Abs(width - _captionWidth) < 1f &&
            _starCaption is not null &&
            _orbitCaption is not null &&
            _waveCaption is not null)
        {
            return;
        }

        _starCaption?.Dispose();
        _orbitCaption?.Dispose();
        _waveCaption?.Dispose();

        _starCaption = BuildCaption("Path fills + strokes", width);
        _orbitCaption = BuildCaption("Transforms and rotation", width);
        _waveCaption = BuildCaption("Animated path scaling", width);
        _captionWidth = width;
    }

    private ImpellerParagraphHandle BuildCaption(string text, float width)
    {
        using var builder = ImpellerParagraphBuilderHandle.Create(_typography);
        builder.PushStyle(_captionStyle);
        builder.AddText(text);
        return builder.Build(width);
    }

    public void Dispose()
    {
        _starCaption?.Dispose();
        _orbitCaption?.Dispose();
        _waveCaption?.Dispose();

        _captionStyle.Dispose();
        _typography.Dispose();

        _accentStrokePaint.Dispose();
        _strokePaint.Dispose();
        _fillPaint.Dispose();
        _cardPaint.Dispose();
        _backgroundPaint.Dispose();

        _starPath.Dispose();
        _orbitPath.Dispose();
        _wavePath.Dispose();
    }

    private static ImpellerPathHandle CreateStarPath()
    {
        using var builder = ImpellerPathBuilderHandle.Create();
        const float outer = 180f;
        const float inner = 72f;
        const int points = 10;

        for (var i = 0; i < points; i++)
        {
            var angle = -MathF.PI / 2f + i * MathF.PI / 5f;
            var radius = i % 2 == 0 ? outer : inner;
            var point = new ImpellerPoint(radius * MathF.Cos(angle), radius * MathF.Sin(angle));
            if (i == 0)
            {
                builder.MoveTo(point);
            }
            else
            {
                builder.LineTo(point);
            }
        }

        builder.LineTo(new ImpellerPoint(0f, -outer));
        return builder.TakePath();
    }

    private static ImpellerPathHandle CreateOrbitPath()
    {
        using var builder = ImpellerPathBuilderHandle.Create();
        builder.MoveTo(new ImpellerPoint(-200f, -40f));
        builder.CubicCurveTo(
            new ImpellerPoint(-140f, -220f),
            new ImpellerPoint(140f, -220f),
            new ImpellerPoint(200f, -40f));
        builder.CubicCurveTo(
            new ImpellerPoint(260f, 120f),
            new ImpellerPoint(-260f, 120f),
            new ImpellerPoint(-200f, -40f));
        return builder.TakePath();
    }

    private static ImpellerPathHandle CreateWavePath()
    {
        using var builder = ImpellerPathBuilderHandle.Create();
        const float amplitude = 120f;
        const float length = 360f;
        const int segments = 6;

        builder.MoveTo(new ImpellerPoint(-length * 0.5f, 0f));
        for (var i = 0; i < segments; i++)
        {
            var progress = i / (float)segments;
            var x = -length * 0.5f + progress * length;
            var controlX = x + length / segments / 2f;
            var controlY = (i % 2 == 0 ? -1f : 1f) * amplitude;
            var endX = x + length / segments;
            builder.QuadraticCurveTo(
                new ImpellerPoint(controlX, controlY),
                new ImpellerPoint(endX, 0f));
        }

        return builder.TakePath();
    }
}
