using System;
using ImpellerSharp.Interop;

namespace AvaloniaImpellerApp.Rendering;

internal sealed class BlendModesScene : IImpellerScene
{
    private readonly ImpellerPaintHandle _backgroundPaint;
    private ImpellerColorSourceHandle? _backgroundGradient;
    private ImpellerPoint _backgroundStart;
    private ImpellerPoint _backgroundEnd;

    private readonly ImpellerPaintHandle _cardPaint;
    private readonly ImpellerPaintHandle _borderPaint;

    private readonly ImpellerPaintHandle _additivePaintA;
    private readonly ImpellerPaintHandle _additivePaintB;
    private readonly ImpellerPaintHandle _additivePaintC;

    private readonly ImpellerPaintHandle _multiplyBasePaint;
    private readonly ImpellerPaintHandle _multiplyOverlayPaintA;
    private readonly ImpellerPaintHandle _multiplyOverlayPaintB;

    private readonly ImpellerPaintHandle _duotoneBasePaint;
    private readonly ImpellerPaintHandle _duotoneLayerPaint;
    private readonly ImpellerColorFilterHandle _duotoneFilter;
    private ImpellerColorSourceHandle? _duotoneGradient;
    private ImpellerPoint _duotoneStart;
    private ImpellerPoint _duotoneEnd;

    private readonly ImpellerTypographyContextHandle _typography;
    private readonly ImpellerParagraphStyleHandle _captionStyle;
    private ImpellerParagraphHandle? _additiveCaption;
    private ImpellerParagraphHandle? _multiplyCaption;
    private ImpellerParagraphHandle? _duotoneCaption;
    private float _captionWidth;

    public BlendModesScene()
    {
        _backgroundPaint = ImpellerPaintHandle.Create();

        _cardPaint = ImpellerPaintHandle.Create();
        _cardPaint.SetColor(new ImpellerColor(0.13f, 0.18f, 0.26f, 0.92f));

        _borderPaint = ImpellerPaintHandle.Create();
        _borderPaint.SetDrawStyle(ImpellerDrawStyle.Stroke);
        _borderPaint.SetStrokeWidth(2.5f);
        _borderPaint.SetColor(new ImpellerColor(0.82f, 0.88f, 1f, 0.25f));

        _additivePaintA = CreateBlendPaint(new ImpellerColor(0.95f, 0.4f, 0.6f, 0.75f), ImpellerBlendMode.Screen);
        _additivePaintB = CreateBlendPaint(new ImpellerColor(0.3f, 0.75f, 1f, 0.75f), ImpellerBlendMode.Screen);
        _additivePaintC = CreateBlendPaint(new ImpellerColor(0.96f, 0.85f, 0.4f, 0.75f), ImpellerBlendMode.Plus);

        _multiplyBasePaint = ImpellerPaintHandle.Create();
        _multiplyBasePaint.SetColor(new ImpellerColor(0.22f, 0.28f, 0.38f, 0.6f));

        _multiplyOverlayPaintA = CreateBlendPaint(new ImpellerColor(0.9f, 0.6f, 0.2f, 0.95f), ImpellerBlendMode.Multiply);
        _multiplyOverlayPaintB = CreateBlendPaint(new ImpellerColor(0.2f, 0.6f, 0.85f, 0.95f), ImpellerBlendMode.Multiply);

        _duotoneBasePaint = ImpellerPaintHandle.Create();
        _duotoneLayerPaint = ImpellerPaintHandle.Create();
        _duotoneLayerPaint.SetBlendMode(ImpellerBlendMode.SourceOver);

        _duotoneFilter = ImpellerColorFilterHandle.CreateColorMatrix(BuildDuotoneMatrix());
        _duotoneLayerPaint.SetColorFilter(_duotoneFilter);

        _typography = ImpellerTypographyContextHandle.Create();
        _captionStyle = ImpellerParagraphStyleHandle.Create();
        _captionStyle.SetFontSize(18f);
        _captionStyle.SetTextAlignment(ImpellerTextAlignment.Center);
    }

    public bool Render(ImpellerDisplayListBuilderHandle builder, float width, float height)
    {
        if (width <= 0f || height <= 0f)
        {
            return false;
        }

        EnsureBackgroundGradient(width, height);
        builder.DrawRect(new ImpellerRect(0f, 0f, width, height), _backgroundPaint);

        const float margin = 32f;
        const float gap = 24f;
        const float columns = 3f;
        var cardWidth = MathF.Min(300f, MathF.Max(180f, (width - margin * 2f - gap * (columns - 1f)) / columns));
        var cardHeight = MathF.Min(280f, MathF.Max(190f, height * 0.55f));
        var radius = MathF.Min(24f, cardWidth * 0.14f);

        var totalRowWidth = cardWidth * columns + gap * (columns - 1f);
        var startX = margin + MathF.Max(0f, (width - margin * 2f - totalRowWidth) * 0.5f);
        var top = margin;

        EnsureCaptions(cardWidth - 18f);

        var additiveRect = new ImpellerRect(startX, top, cardWidth, cardHeight);
        DrawAdditiveCard(builder, additiveRect, radius, _additiveCaption);

        var multiplyX = startX + cardWidth + gap;
        var multiplyRect = new ImpellerRect(multiplyX, top, cardWidth, cardHeight);
        DrawMultiplyCard(builder, multiplyRect, radius, _multiplyCaption);

        var duotoneX = multiplyX + cardWidth + gap;
        var duotoneRect = new ImpellerRect(duotoneX, top, cardWidth, cardHeight);
        DrawDuotoneCard(builder, duotoneRect, radius, _duotoneCaption);

        return true;
    }

    private void DrawAdditiveCard(
        ImpellerDisplayListBuilderHandle builder,
        in ImpellerRect rect,
        float radius,
        ImpellerParagraphHandle? caption)
    {
        DrawRoundedRect(builder, rect, radius, _cardPaint);
        DrawRoundedRect(builder, rect, radius, _borderPaint);

        builder.Save();
        builder.ClipRoundedRect(rect, ImpellerRoundingRadii.Uniform(radius), ImpellerClipOperation.Intersect);

        var center = new ImpellerPoint(rect.X + rect.Width * 0.5f, rect.Y + rect.Height * 0.52f);
        var offsetX = rect.Width * 0.26f;
        var offsetY = rect.Height * 0.22f;
        var circleRadius = rect.Width * 0.32f;

        DrawCircle(builder, new ImpellerPoint(center.X - offsetX, center.Y - offsetY), circleRadius, _additivePaintA);
        DrawCircle(builder, new ImpellerPoint(center.X + offsetX, center.Y - offsetY), circleRadius, _additivePaintB);
        DrawCircle(builder, new ImpellerPoint(center.X, center.Y + offsetY), circleRadius, _additivePaintC);

        builder.Restore();

        DrawCaption(builder, caption, rect);
    }

    private void DrawMultiplyCard(
        ImpellerDisplayListBuilderHandle builder,
        in ImpellerRect rect,
        float radius,
        ImpellerParagraphHandle? caption)
    {
        DrawRoundedRect(builder, rect, radius, _cardPaint);

        builder.Save();
        var radii = ImpellerRoundingRadii.Uniform(radius);
        builder.ClipRoundedRect(rect, radii, ImpellerClipOperation.Intersect);

        builder.DrawRect(rect, _multiplyBasePaint);

        var firstRect = new ImpellerRect(
            rect.X + rect.Width * 0.1f,
            rect.Y + rect.Height * 0.14f,
            rect.Width * 0.58f,
            rect.Height * 0.56f);
        builder.DrawRect(firstRect, _multiplyOverlayPaintA);

        var secondRect = new ImpellerRect(
            rect.X + rect.Width * 0.32f,
            rect.Y + rect.Height * 0.32f,
            rect.Width * 0.54f,
            rect.Height * 0.5f);
        builder.DrawRect(secondRect, _multiplyOverlayPaintB);

        var diagonal = new ImpellerRect(
            rect.X - rect.Width * 0.1f,
            rect.Y + rect.Height * 0.52f,
            rect.Width * 1.4f,
            rect.Height * 0.22f);
        builder.Save();
        builder.ClipRoundedRect(diagonal, ImpellerRoundingRadii.Uniform(diagonal.Height * 0.5f), ImpellerClipOperation.Intersect);
        builder.DrawRect(diagonal, _borderPaint);
        builder.Restore();

        builder.Restore();

        DrawCaption(builder, caption, rect);
    }

    private void DrawDuotoneCard(
        ImpellerDisplayListBuilderHandle builder,
        in ImpellerRect rect,
        float radius,
        ImpellerParagraphHandle? caption)
    {
        DrawRoundedRect(builder, rect, radius, _cardPaint);

        EnsureDuotoneGradient(rect);

        builder.Save();
        var radii = ImpellerRoundingRadii.Uniform(radius);
        builder.ClipRoundedRect(rect, radii, ImpellerClipOperation.Intersect);

        builder.SaveLayer(rect, _duotoneLayerPaint, null);
        builder.DrawRect(rect, _duotoneBasePaint);

        var waveRect = new ImpellerRect(
            rect.X,
            rect.Y + rect.Height * 0.35f,
            rect.Width,
            rect.Height * 0.45f);
        builder.Save();
        var waveRadius = ImpellerRoundingRadii.Uniform(waveRect.Height * 0.42f);
        builder.ClipRoundedRect(waveRect, waveRadius, ImpellerClipOperation.Intersect);
        builder.DrawRect(waveRect, _borderPaint);
        builder.Restore();

        builder.Restore();
        builder.Restore();

        DrawRoundedRect(builder, rect, radius, _borderPaint);
        DrawCaption(builder, caption, rect);
    }

    private void DrawCaption(
        ImpellerDisplayListBuilderHandle builder,
        ImpellerParagraphHandle? caption,
        in ImpellerRect rect)
    {
        if (caption is null)
        {
            return;
        }

        var x = rect.X + (rect.Width - _captionWidth) * 0.5f;
        var y = rect.Y + rect.Height + 18f;
        builder.DrawParagraph(caption, new ImpellerPoint(x, y));
    }

    private void EnsureBackgroundGradient(float width, float height)
    {
        var start = new ImpellerPoint(0f, height * 0.3f);
        var end = new ImpellerPoint(width, height);

        if (_backgroundGradient is not null &&
            PointsAreClose(start, _backgroundStart) &&
            PointsAreClose(end, _backgroundEnd))
        {
            return;
        }

        var gradient = ImpellerColorSourceHandle.CreateLinearGradient(
            start,
            end,
            new[]
            {
                new ImpellerColor(0.05f, 0.06f, 0.1f, 1f),
                new ImpellerColor(0.12f, 0.14f, 0.2f, 1f),
                new ImpellerColor(0.18f, 0.22f, 0.32f, 1f),
            },
            new[] { 0f, 0.55f, 1f },
            ImpellerTileMode.Clamp);

        _backgroundPaint.SetColorSource(gradient);
        _backgroundGradient?.Dispose();
        _backgroundGradient = gradient;
        _backgroundStart = start;
        _backgroundEnd = end;
    }

    private void EnsureDuotoneGradient(in ImpellerRect rect)
    {
        var start = new ImpellerPoint(rect.X, rect.Y);
        var end = new ImpellerPoint(rect.X + rect.Width, rect.Y + rect.Height);

        if (_duotoneGradient is not null &&
            PointsAreClose(start, _duotoneStart) &&
            PointsAreClose(end, _duotoneEnd))
        {
            return;
        }

        var gradient = ImpellerColorSourceHandle.CreateLinearGradient(
            start,
            end,
            new[]
            {
                new ImpellerColor(0.22f, 0.56f, 0.92f, 1f),
                new ImpellerColor(0.92f, 0.32f, 0.54f, 1f),
            },
            new[] { 0f, 1f },
            ImpellerTileMode.Clamp);

        _duotoneBasePaint.SetColorSource(gradient);
        _duotoneGradient?.Dispose();
        _duotoneGradient = gradient;
        _duotoneStart = start;
        _duotoneEnd = end;
    }

    private void EnsureCaptions(float width)
    {
        if (MathF.Abs(width - _captionWidth) < 1f &&
            _additiveCaption is not null &&
            _multiplyCaption is not null &&
            _duotoneCaption is not null)
        {
            return;
        }

        _additiveCaption?.Dispose();
        _multiplyCaption?.Dispose();
        _duotoneCaption?.Dispose();

        _additiveCaption = BuildCaption("Additive blending (screen/plus)", width);
        _multiplyCaption = BuildCaption("Multiply blending for shading", width);
        _duotoneCaption = BuildCaption("Color matrix duotone grading", width);
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
        _additiveCaption?.Dispose();
        _multiplyCaption?.Dispose();
        _duotoneCaption?.Dispose();

        _captionStyle.Dispose();
        _typography.Dispose();

        _duotoneLayerPaint.Dispose();
        _duotoneFilter.Dispose();
        _duotoneBasePaint.SetColorSource(null);
        _duotoneGradient?.Dispose();
        _duotoneBasePaint.Dispose();

        _multiplyOverlayPaintB.Dispose();
        _multiplyOverlayPaintA.Dispose();
        _multiplyBasePaint.Dispose();

        _additivePaintC.Dispose();
        _additivePaintB.Dispose();
        _additivePaintA.Dispose();

        _borderPaint.Dispose();
        _cardPaint.Dispose();

        _backgroundPaint.SetColorSource(null);
        _backgroundGradient?.Dispose();
        _backgroundPaint.Dispose();
    }

    private static ImpellerPaintHandle CreateBlendPaint(ImpellerColor color, ImpellerBlendMode blendMode)
    {
        var paint = ImpellerPaintHandle.Create();
        paint.SetColor(color);
        paint.SetBlendMode(blendMode);
        return paint;
    }

    private static void DrawRoundedRect(
        ImpellerDisplayListBuilderHandle builder,
        in ImpellerRect rect,
        float radius,
        ImpellerPaintHandle paint)
    {
        builder.Save();
        builder.ClipRoundedRect(rect, ImpellerRoundingRadii.Uniform(radius), ImpellerClipOperation.Intersect);
        builder.DrawRect(rect, paint);
        builder.Restore();
    }

    private static void DrawCircle(
        ImpellerDisplayListBuilderHandle builder,
        in ImpellerPoint center,
        float radius,
        ImpellerPaintHandle paint)
    {
        var bounds = new ImpellerRect(center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        builder.Save();
        builder.ClipOval(bounds, ImpellerClipOperation.Intersect);
        builder.DrawRect(bounds, paint);
        builder.Restore();
    }

    private static ImpellerColorMatrix BuildDuotoneMatrix()
    {
        Span<float> values = stackalloc float[ImpellerColorMatrix.ElementCount];
        values.Clear();

        // Transform to a warm duotone while preserving alpha.
        values[0] = 0.9f;
        values[1] = 0.05f;
        values[2] = 0.0f;
        values[4] = 0.05f;

        values[5] = 0.1f;
        values[6] = 0.85f;
        values[7] = 0.05f;
        values[9] = 0.02f;

        values[10] = 0.05f;
        values[11] = 0.18f;
        values[12] = 0.75f;
        values[14] = 0.04f;

        values[18] = 1f;

        var matrix = new ImpellerColorMatrix();
        matrix.Set(values);
        return matrix;
    }

    private static bool PointsAreClose(in ImpellerPoint first, in ImpellerPoint second)
    {
        const float epsilon = 0.5f;
        return MathF.Abs(first.X - second.X) < epsilon &&
               MathF.Abs(first.Y - second.Y) < epsilon;
    }
}
