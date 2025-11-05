using System;
using ImpellerSharp.Interop;

namespace AvaloniaImpellerApp.Rendering;

internal sealed class LayerEffectsScene : IImpellerScene
{
    private readonly ImpellerPaintHandle _backgroundPaint;
    private ImpellerColorSourceHandle? _backgroundGradient;
    private ImpellerPoint _backgroundStart;
    private ImpellerPoint _backgroundEnd;

    private readonly ImpellerPaintHandle _cardPaint;
    private readonly ImpellerPaintHandle _highlightPaint;
    private readonly ImpellerPaintHandle _shadowPaint;
    private readonly ImpellerPaintHandle _frostFillPaint;
    private readonly ImpellerPaintHandle _frostBorderPaint;
    private readonly ImpellerPaintHandle _glowBasePaint;
    private readonly ImpellerPaintHandle _glowAccentPaint;
    private readonly ImpellerPaintHandle _glowLayerPaint;

    private readonly ImpellerMaskFilterHandle _shadowBlur;
    private readonly ImpellerImageFilterHandle _backdropBlur;
    private readonly ImpellerImageFilterHandle _glowBlur;

    private readonly ImpellerTypographyContextHandle _typography;
    private readonly ImpellerParagraphStyleHandle _captionStyle;
    private ImpellerParagraphHandle? _shadowCaption;
    private ImpellerParagraphHandle? _frostCaption;
    private ImpellerParagraphHandle? _glowCaption;
    private float _captionWidth;

    public LayerEffectsScene()
    {
        _backgroundPaint = ImpellerPaintHandle.Create();

        _cardPaint = ImpellerPaintHandle.Create();
        _cardPaint.SetColor(new ImpellerColor(0.14f, 0.2f, 0.3f, 0.92f));

        _highlightPaint = ImpellerPaintHandle.Create();
        _highlightPaint.SetColor(new ImpellerColor(1f, 1f, 1f, 0.12f));

        _shadowPaint = ImpellerPaintHandle.Create();
        _shadowPaint.SetColor(new ImpellerColor(0f, 0f, 0f, 0.45f));
        _shadowBlur = ImpellerMaskFilterHandle.CreateBlur(ImpellerBlurStyle.Normal, 12f);
        _shadowPaint.SetMaskFilter(_shadowBlur);

        _frostFillPaint = ImpellerPaintHandle.Create();
        _frostFillPaint.SetColor(new ImpellerColor(0.35f, 0.55f, 0.82f, 0.25f));

        _frostBorderPaint = ImpellerPaintHandle.Create();
        _frostBorderPaint.SetDrawStyle(ImpellerDrawStyle.Stroke);
        _frostBorderPaint.SetStrokeWidth(2f);
        _frostBorderPaint.SetColor(new ImpellerColor(0.82f, 0.9f, 1f, 0.35f));

        _backdropBlur = ImpellerImageFilterHandle.CreateBlur(18f, 18f, ImpellerTileMode.Decal);

        _glowBasePaint = ImpellerPaintHandle.Create();
        _glowBasePaint.SetColor(new ImpellerColor(0.95f, 0.56f, 1f, 0.72f));

        _glowAccentPaint = ImpellerPaintHandle.Create();
        _glowAccentPaint.SetColor(new ImpellerColor(0.32f, 0.78f, 1f, 0.68f));

        _glowLayerPaint = ImpellerPaintHandle.Create();
        _glowLayerPaint.SetBlendMode(ImpellerBlendMode.Screen);
        _glowBlur = ImpellerImageFilterHandle.CreateBlur(22f, 22f, ImpellerTileMode.Clamp);
        _glowLayerPaint.SetImageFilter(_glowBlur);

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
        var backdropRect = new ImpellerRect(0f, 0f, width, height);
        builder.DrawRect(backdropRect, _backgroundPaint);

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

        var shadowRect = new ImpellerRect(startX, top, cardWidth, cardHeight);
        DrawShadowCard(builder, shadowRect, radius, _shadowCaption);

        var frostX = startX + cardWidth + gap;
        var frostRect = new ImpellerRect(frostX, top, cardWidth, cardHeight);
        DrawFrostedCard(builder, frostRect, radius, _frostCaption);

        var glowX = frostX + cardWidth + gap;
        var glowRect = new ImpellerRect(glowX, top, cardWidth, cardHeight);
        DrawGlowCard(builder, glowRect, radius, _glowCaption);

        return true;
    }

    private void DrawShadowCard(
        ImpellerDisplayListBuilderHandle builder,
        in ImpellerRect rect,
        float radius,
        ImpellerParagraphHandle? caption)
    {
        var shadowRect = new ImpellerRect(rect.X + 12f, rect.Y + 16f, rect.Width, rect.Height);
        DrawRoundedRect(builder, shadowRect, radius, _shadowPaint);

        DrawRoundedRect(builder, rect, radius, _cardPaint);

        builder.Save();
        var radii = ImpellerRoundingRadii.Uniform(radius);
        builder.ClipRoundedRect(rect, radii, ImpellerClipOperation.Intersect);
        var highlightRect = new ImpellerRect(rect.X, rect.Y, rect.Width, rect.Height * 0.45f);
        builder.ClipRect(highlightRect, ImpellerClipOperation.Intersect);
        builder.DrawRect(rect, _highlightPaint);
        builder.Restore();

        DrawCaption(builder, caption, rect);
    }

    private void DrawFrostedCard(
        ImpellerDisplayListBuilderHandle builder,
        in ImpellerRect rect,
        float radius,
        ImpellerParagraphHandle? caption)
    {
        var radii = ImpellerRoundingRadii.Uniform(radius);

        builder.Save();
        builder.ClipRoundedRect(rect, radii, ImpellerClipOperation.Intersect);
        builder.SaveLayer(rect, null, _backdropBlur);
        builder.DrawRect(rect, _frostFillPaint);

        var sweep = new ImpellerRect(
            rect.X - rect.Width * 0.15f,
            rect.Y + rect.Height * 0.12f,
            rect.Width * 1.4f,
            rect.Height * 0.42f);

        builder.Save();
        builder.ClipRoundedRect(sweep, ImpellerRoundingRadii.Uniform(sweep.Height * 0.5f), ImpellerClipOperation.Intersect);
        builder.DrawRect(sweep, _highlightPaint);
        builder.Restore();

        var bottomGlow = new ImpellerRect(
            rect.X + rect.Width * 0.18f,
            rect.Y + rect.Height * 0.58f,
            rect.Width * 0.64f,
            rect.Height * 0.28f);
        builder.Save();
        builder.ClipRoundedRect(bottomGlow, ImpellerRoundingRadii.Uniform(bottomGlow.Height * 0.5f), ImpellerClipOperation.Intersect);
        builder.DrawRect(bottomGlow, _highlightPaint);
        builder.Restore();

        builder.Restore();
        builder.Restore();

        DrawRoundedRect(builder, rect, radius, _frostBorderPaint);
        DrawCaption(builder, caption, rect);
    }

    private void DrawGlowCard(
        ImpellerDisplayListBuilderHandle builder,
        in ImpellerRect rect,
        float radius,
        ImpellerParagraphHandle? caption)
    {
        var radii = ImpellerRoundingRadii.Uniform(radius);
        DrawRoundedRect(builder, rect, radius, _cardPaint);

        builder.Save();
        builder.ClipRoundedRect(rect, radii, ImpellerClipOperation.Intersect);

        builder.SaveLayer(rect, _glowLayerPaint, null);

        var primaryCenter = new ImpellerPoint(rect.X + rect.Width * 0.38f, rect.Y + rect.Height * 0.42f);
        var secondaryCenter = new ImpellerPoint(rect.X + rect.Width * 0.62f, rect.Y + rect.Height * 0.58f);
        DrawCircle(builder, primaryCenter, rect.Width * 0.28f, _glowBasePaint);
        DrawCircle(builder, secondaryCenter, rect.Width * 0.22f, _glowAccentPaint);

        builder.Restore();

        var accentRect = new ImpellerRect(
            rect.X + rect.Width * 0.18f,
            rect.Y + rect.Height * 0.18f,
            rect.Width * 0.64f,
            rect.Height * 0.18f);
        builder.Save();
        builder.ClipRect(accentRect, ImpellerClipOperation.Intersect);
        builder.DrawRect(rect, _highlightPaint);
        builder.Restore();

        builder.Restore();

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
        var start = new ImpellerPoint(0f, 0f);
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
                new ImpellerColor(0.05f, 0.07f, 0.12f, 1f),
                new ImpellerColor(0.12f, 0.16f, 0.24f, 1f),
            },
            new[] { 0f, 1f },
            ImpellerTileMode.Clamp);

        _backgroundPaint.SetColorSource(gradient);
        _backgroundGradient?.Dispose();
        _backgroundGradient = gradient;
        _backgroundStart = start;
        _backgroundEnd = end;
    }

    private void EnsureCaptions(float width)
    {
        if (MathF.Abs(width - _captionWidth) < 1f &&
            _shadowCaption is not null &&
            _frostCaption is not null &&
            _glowCaption is not null)
        {
            return;
        }

        _shadowCaption?.Dispose();
        _frostCaption?.Dispose();
        _glowCaption?.Dispose();

        _shadowCaption = BuildCaption("Mask filter drop shadow", width);
        _frostCaption = BuildCaption("Backdrop blur frosted glass", width);
        _glowCaption = BuildCaption("Layered glow with SaveLayer", width);
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
        _shadowCaption?.Dispose();
        _frostCaption?.Dispose();
        _glowCaption?.Dispose();

        _captionStyle.Dispose();
        _typography.Dispose();

        _glowLayerPaint.Dispose();
        _glowBlur.Dispose();

        _glowAccentPaint.Dispose();
        _glowBasePaint.Dispose();

        _frostBorderPaint.Dispose();
        _frostFillPaint.Dispose();

        _backdropBlur.Dispose();

        _shadowPaint.Dispose();
        _shadowBlur.Dispose();

        _highlightPaint.Dispose();
        _cardPaint.Dispose();

        _backgroundPaint.SetColorSource(null);
        _backgroundGradient?.Dispose();
        _backgroundPaint.Dispose();
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

    private static bool PointsAreClose(in ImpellerPoint first, in ImpellerPoint second)
    {
        const float epsilon = 0.5f;
        return MathF.Abs(first.X - second.X) < epsilon &&
               MathF.Abs(first.Y - second.Y) < epsilon;
    }
}
