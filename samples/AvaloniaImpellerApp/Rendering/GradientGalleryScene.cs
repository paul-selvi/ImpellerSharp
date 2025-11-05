using System;
using ImpellerSharp.Interop;

namespace AvaloniaImpellerApp.Rendering;

internal sealed class GradientGalleryScene : IImpellerScene
{
    private readonly ImpellerPaintHandle _backgroundPaint;
    private readonly ImpellerPaintHandle _linearPaint;
    private readonly ImpellerPaintHandle _radialPaint;
    private readonly ImpellerPaintHandle _sweepPaint;
    private readonly ImpellerTypographyContextHandle _typography;
    private readonly ImpellerParagraphStyleHandle _captionStyle;

    private ImpellerColorSourceHandle? _linearGradient;
    private ImpellerColorSourceHandle? _radialGradient;
    private ImpellerColorSourceHandle? _sweepGradient;

    private ImpellerParagraphHandle? _linearCaption;
    private ImpellerParagraphHandle? _radialCaption;
    private ImpellerParagraphHandle? _sweepCaption;
    private float _captionWidth;

    public GradientGalleryScene()
    {
        _backgroundPaint = ImpellerPaintHandle.Create();
        _backgroundPaint.SetColor(new ImpellerColor(0.07f, 0.09f, 0.13f, 1f));

        _linearPaint = ImpellerPaintHandle.Create();
        _radialPaint = ImpellerPaintHandle.Create();
        _sweepPaint = ImpellerPaintHandle.Create();

        _typography = ImpellerTypographyContextHandle.Create();
        _captionStyle = ImpellerParagraphStyleHandle.Create();
        _captionStyle.SetFontSize(18f);
        _captionStyle.SetTextAlignment(ImpellerTextAlignment.Center);
    }

    public bool Render(ImpellerDisplayListBuilderHandle builder, float width, float height)
    {
        builder.DrawPaint(_backgroundPaint);

        var margin = 32f;
        var gap = 24f;
        var columns = 3f;
        var cardWidth = MathF.Max(160f, (width - margin * 2f - gap * (columns - 1f)) / columns);
        var cardHeight = MathF.Min(220f, MathF.Max(140f, height * 0.45f));
        var top = margin;

        EnsureCaptions(cardWidth - 16f);

        var linearRect = new ImpellerRect(margin, top, cardWidth, cardHeight);
        DrawLinearCard(builder, linearRect);
        DrawCaption(builder, _linearCaption, linearRect, 0f);

        var radialX = margin + cardWidth + gap;
        var radialRect = new ImpellerRect(radialX, top, cardWidth, cardHeight);
        DrawRadialCard(builder, radialRect);
        DrawCaption(builder, _radialCaption, radialRect, 0f);

        var sweepX = radialX + cardWidth + gap;
        var sweepRect = new ImpellerRect(sweepX, top, cardWidth, cardHeight);
        DrawSweepCard(builder, sweepRect);
        DrawCaption(builder, _sweepCaption, sweepRect, 0f);

        return true;
    }

    private void DrawLinearCard(ImpellerDisplayListBuilderHandle builder, in ImpellerRect rect)
    {
        var start = new ImpellerPoint(rect.X, rect.Y);
        var end = new ImpellerPoint(rect.X + rect.Width, rect.Y + rect.Height);

        if (_linearGradient is null ||
            !PointsAreClose(start, _linearStart) ||
            !PointsAreClose(end, _linearEnd))
        {
            var gradient = ImpellerColorSourceHandle.CreateLinearGradient(
                start,
                end,
                new[]
                {
                    new ImpellerColor(0.98f, 0.31f, 0.52f, 1f),
                    new ImpellerColor(0.27f, 0.52f, 0.98f, 1f),
                },
                new[] { 0f, 1f },
                ImpellerTileMode.Clamp);

            _linearPaint.SetColorSource(gradient);
            _linearGradient?.Dispose();
            _linearGradient = gradient;
            _linearStart = start;
            _linearEnd = end;
        }

        builder.DrawRect(rect, _linearPaint);
    }

    private void DrawRadialCard(ImpellerDisplayListBuilderHandle builder, in ImpellerRect rect)
    {
        var center = new ImpellerPoint(rect.X + rect.Width * 0.5f, rect.Y + rect.Height * 0.5f);
        var radius = MathF.Min(rect.Width, rect.Height) * 0.55f;

        if (_radialGradient is null ||
            !PointsAreClose(center, _radialCenter) ||
            MathF.Abs(radius - _radialRadius) > 0.5f)
        {
            var gradient = ImpellerColorSourceHandle.CreateRadialGradient(
                center,
                radius,
                new[]
                {
                    new ImpellerColor(0.15f, 0.9f, 0.74f, 1f),
                    new ImpellerColor(0.05f, 0.2f, 0.3f, 1f),
                },
                new[] { 0f, 1f },
                ImpellerTileMode.Clamp);

            _radialPaint.SetColorSource(gradient);
            _radialGradient?.Dispose();
            _radialGradient = gradient;
            _radialCenter = center;
            _radialRadius = radius;
        }

        builder.DrawRect(rect, _radialPaint);
    }

    private void DrawSweepCard(ImpellerDisplayListBuilderHandle builder, in ImpellerRect rect)
    {
        var center = new ImpellerPoint(rect.X + rect.Width * 0.5f, rect.Y + rect.Height * 0.5f);

        if (_sweepGradient is null || !PointsAreClose(center, _sweepCenter))
        {
            var gradient = ImpellerColorSourceHandle.CreateSweepGradient(
                center,
                0f,
                360f,
                new[]
                {
                    new ImpellerColor(1f, 0.52f, 0.18f, 1f),
                    new ImpellerColor(0.75f, 0.3f, 0.9f, 1f),
                    new ImpellerColor(0.2f, 0.78f, 1f, 1f),
                    new ImpellerColor(1f, 0.52f, 0.18f, 1f),
                },
                new[] { 0f, 0.35f, 0.7f, 1f },
                ImpellerTileMode.Clamp);

            _sweepPaint.SetColorSource(gradient);
            _sweepGradient?.Dispose();
            _sweepGradient = gradient;
            _sweepCenter = center;
        }

        builder.DrawRect(rect, _sweepPaint);
    }

    private void DrawCaption(
        ImpellerDisplayListBuilderHandle builder,
        ImpellerParagraphHandle? paragraph,
        in ImpellerRect above,
        float offsetY)
    {
        if (paragraph is null)
        {
            return;
        }

        var x = above.X + (above.Width - _captionWidth) * 0.5f;
        var y = above.Y + above.Height + 16f + offsetY;
        builder.DrawParagraph(paragraph, new ImpellerPoint(x, y));
    }

    private void EnsureCaptions(float width)
    {
        if (MathF.Abs(width - _captionWidth) < 1f &&
            _linearCaption is not null &&
            _radialCaption is not null &&
            _sweepCaption is not null)
        {
            return;
        }

        _linearCaption?.Dispose();
        _radialCaption?.Dispose();
        _sweepCaption?.Dispose();

        _linearCaption = BuildCaption("Linear gradient across the card", width);
        _radialCaption = BuildCaption("Radial focus with soft falloff", width);
        _sweepCaption = BuildCaption("Sweep gradient cycling hues", width);
        _captionWidth = width;
    }

    private ImpellerParagraphHandle BuildCaption(string text, float width)
    {
        using var builder = ImpellerParagraphBuilderHandle.Create(_typography);
        builder.PushStyle(_captionStyle);
        builder.AddText(text);
        return builder.Build(width);
    }

    private static bool PointsAreClose(in ImpellerPoint first, in ImpellerPoint second)
    {
        const float epsilon = 0.5f;
        return MathF.Abs(first.X - second.X) < epsilon &&
               MathF.Abs(first.Y - second.Y) < epsilon;
    }

    public void Dispose()
    {
        _linearCaption?.Dispose();
        _radialCaption?.Dispose();
        _sweepCaption?.Dispose();
        _captionStyle.Dispose();
        _typography.Dispose();

        _linearPaint.SetColorSource(null);
        _radialPaint.SetColorSource(null);
        _sweepPaint.SetColorSource(null);

        _linearGradient?.Dispose();
        _radialGradient?.Dispose();
        _sweepGradient?.Dispose();

        _linearPaint.Dispose();
        _radialPaint.Dispose();
        _sweepPaint.Dispose();
        _backgroundPaint.Dispose();
    }

    private ImpellerPoint _linearStart;
    private ImpellerPoint _linearEnd;
    private ImpellerPoint _radialCenter;
    private float _radialRadius;
    private ImpellerPoint _sweepCenter;
}
