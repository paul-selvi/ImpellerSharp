using System;
using ImpellerSharp.Interop;

namespace AvaloniaImpellerApp.Rendering;

internal sealed class TypographyGalleryScene : IImpellerScene
{
    private readonly ImpellerPaintHandle _backgroundPaint;
    private readonly ImpellerPaintHandle _cardPaint;
    private readonly ImpellerPaintHandle _accentPaint;

    private readonly ImpellerTypographyContextHandle _typography;
    private readonly ImpellerParagraphStyleHandle _headlineStyle;
    private readonly ImpellerParagraphStyleHandle _bodyLeftStyle;
    private readonly ImpellerParagraphStyleHandle _bodyCenterStyle;
    private readonly ImpellerParagraphStyleHandle _bodyRightStyle;
    private readonly ImpellerParagraphStyleHandle _captionStyle;

    private ImpellerParagraphHandle? _headline;
    private ImpellerParagraphHandle? _leftColumn;
    private ImpellerParagraphHandle? _centerColumn;
    private ImpellerParagraphHandle? _rightColumn;
    private ImpellerParagraphHandle? _caption;

    private float _headlineWidth;
    private float _columnWidth;

    public TypographyGalleryScene()
    {
        _backgroundPaint = ImpellerPaintHandle.Create();
        _backgroundPaint.SetColor(new ImpellerColor(0.04f, 0.05f, 0.08f, 1f));

        _cardPaint = ImpellerPaintHandle.Create();
        _cardPaint.SetColor(new ImpellerColor(0.12f, 0.18f, 0.26f, 0.85f));

        _accentPaint = ImpellerPaintHandle.Create();
        _accentPaint.SetColor(new ImpellerColor(0.23f, 0.62f, 0.92f, 1f));
        _accentPaint.SetStrokeWidth(3f);
        _accentPaint.SetDrawStyle(ImpellerDrawStyle.Stroke);

        _typography = ImpellerTypographyContextHandle.Create();

        _headlineStyle = ImpellerParagraphStyleHandle.Create();
        _headlineStyle.SetFontSize(42f);
        _headlineStyle.SetFontWeight(ImpellerFontWeight.W700);

        _bodyLeftStyle = ImpellerParagraphStyleHandle.Create();
        _bodyLeftStyle.SetFontSize(18f);
        _bodyLeftStyle.SetHeight(1.4f);
        _bodyLeftStyle.SetTextAlignment(ImpellerTextAlignment.Left);

        _bodyCenterStyle = ImpellerParagraphStyleHandle.Create();
        _bodyCenterStyle.SetFontSize(18f);
        _bodyCenterStyle.SetHeight(1.45f);
        _bodyCenterStyle.SetTextAlignment(ImpellerTextAlignment.Center);

        _bodyRightStyle = ImpellerParagraphStyleHandle.Create();
        _bodyRightStyle.SetFontSize(18f);
        _bodyRightStyle.SetHeight(1.45f);
        _bodyRightStyle.SetTextAlignment(ImpellerTextAlignment.Right);

        _captionStyle = ImpellerParagraphStyleHandle.Create();
        _captionStyle.SetFontSize(16f);
        _captionStyle.SetTextAlignment(ImpellerTextAlignment.Center);
    }

    public bool Render(ImpellerDisplayListBuilderHandle builder, float width, float height)
    {
        builder.DrawPaint(_backgroundPaint);

        var margin = 36f;
        var contentWidth = MathF.Max(320f, width - margin * 2f);
        var gap = 20f;
        var columnAreaWidth = contentWidth;
        var columnWidth = MathF.Max(150f, (columnAreaWidth - gap * 2f) / 3f);

        EnsureParagraphs(contentWidth, columnWidth);

        var originX = margin;
        var originY = margin;

        if (_headline is not null)
        {
            builder.DrawParagraph(_headline, new ImpellerPoint(originX, originY));
            originY += _headline.GetHeight() + 28f;
        }

        var highlightRect = new ImpellerRect(
            margin,
            originY - 16f,
            columnAreaWidth,
            MathF.Min(height - originY - margin, columnWidth * 1.5f));

        builder.DrawRect(highlightRect, _cardPaint);

        var columnTop = originY;
        var columnLeft = margin;

        DrawColumn(builder, _leftColumn, columnLeft, columnTop, columnWidth);
        columnLeft += columnWidth + gap;

        DrawColumn(builder, _centerColumn, columnLeft, columnTop, columnWidth);
        columnLeft += columnWidth + gap;

        DrawColumn(builder, _rightColumn, columnLeft, columnTop, columnWidth);

        var baselineY = columnTop + columnWidth * 1.1f;
        var baselineLeft = margin;
        var baselineRight = margin + columnAreaWidth;
        builder.DrawLine(
            new ImpellerPoint(baselineLeft, baselineY),
            new ImpellerPoint(baselineRight, baselineY),
            _accentPaint);

        if (_caption is not null)
        {
            var captionY = baselineY + 20f;
            var captionX = margin + (columnAreaWidth - _captionWidth) * 0.5f;
            builder.DrawParagraph(_caption, new ImpellerPoint(captionX, captionY));
        }

        return true;
    }

    private void DrawColumn(
        ImpellerDisplayListBuilderHandle builder,
        ImpellerParagraphHandle? paragraph,
        float x,
        float y,
        float width)
    {
        if (paragraph is null)
        {
            return;
        }

        builder.DrawParagraph(paragraph, new ImpellerPoint(x, y));

        var lineTop = y - 10f;
        builder.DrawDashedLine(
            new ImpellerPoint(x, lineTop),
            new ImpellerPoint(x + width, lineTop),
            12f,
            8f,
            _accentPaint);
    }

    private void EnsureParagraphs(float headlineWidth, float columnWidth)
    {
        var headlineNeedsUpdate = MathF.Abs(headlineWidth - _headlineWidth) > 1f;
        var columnNeedsUpdate = MathF.Abs(columnWidth - _columnWidth) > 1f;

        if (!headlineNeedsUpdate && !columnNeedsUpdate &&
            _headline is not null &&
            _leftColumn is not null &&
            _centerColumn is not null &&
            _rightColumn is not null &&
            _caption is not null)
        {
            return;
        }

        _headline?.Dispose();
        _leftColumn?.Dispose();
        _centerColumn?.Dispose();
        _rightColumn?.Dispose();
        _caption?.Dispose();

        _headline = BuildParagraph(_headlineStyle,
            "Typography and layout with Impeller paragraphs",
            headlineWidth);

        const string sampleText =
            "ImpellerSharp exposes Flutter's Impeller engine primitives for rich typography, precise glyph metrics, " +
            "and advanced layout. Paragraphs flow text within bounds and support alignment, font variations, and decorations.";

        _leftColumn = BuildParagraph(_bodyLeftStyle, sampleText, columnWidth);
        _centerColumn = BuildParagraph(_bodyCenterStyle, sampleText, columnWidth);
        _rightColumn = BuildParagraph(_bodyRightStyle, sampleText, columnWidth);

        _caption = BuildParagraph(_captionStyle,
            "Three columns demonstrate left, center, and right alignment using a shared paragraph builder.",
            columnWidth * 2f + 20f);

        _headlineWidth = headlineWidth;
        _columnWidth = columnWidth;
        _captionWidth = columnWidth * 2f + 20f;
    }

    private ImpellerParagraphHandle BuildParagraph(
        ImpellerParagraphStyleHandle style,
        string text,
        float width)
    {
        using var builder = ImpellerParagraphBuilderHandle.Create(_typography);
        builder.PushStyle(style);
        builder.AddText(text);
        return builder.Build(width);
    }

    public void Dispose()
    {
        _headline?.Dispose();
        _leftColumn?.Dispose();
        _centerColumn?.Dispose();
        _rightColumn?.Dispose();
        _caption?.Dispose();

        _captionStyle.Dispose();
        _bodyRightStyle.Dispose();
        _bodyCenterStyle.Dispose();
        _bodyLeftStyle.Dispose();
        _headlineStyle.Dispose();
        _typography.Dispose();

        _accentPaint.Dispose();
        _cardPaint.Dispose();
        _backgroundPaint.Dispose();
    }

    private float _captionWidth;
}
