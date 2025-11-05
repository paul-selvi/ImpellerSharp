using System;
using ImpellerSharp.Interop;

namespace AvaloniaImpellerApp.Rendering;

internal sealed class BasicDemoScene : IImpellerScene
{
    private readonly ImpellerPaintHandle _backgroundPaint;
    private readonly ImpellerPaintHandle _rectPaint;
    private readonly ImpellerPaintHandle _strokePaint;
    private ImpellerColorSourceHandle? _gradient;
    private ImpellerPoint _gradientStart;
    private ImpellerPoint _gradientEnd;
    private readonly ImpellerTypographyContextHandle _typography;
    private readonly ImpellerParagraphStyleHandle _paragraphStyle;
    private ImpellerParagraphHandle? _paragraph;
    private float _paragraphWidth;

    private readonly ImpellerPathHandle _starPath;

    private static readonly ImpellerColor[] s_gradientColors =
    {
        new ImpellerColor(0.2f, 0.6f, 0.9f, 1f),
        new ImpellerColor(0.8f, 0.2f, 0.8f, 1f),
    };

    private static readonly float[] s_gradientStops = { 0f, 1f };

    public BasicDemoScene()
    {
        _backgroundPaint = ImpellerPaintHandle.Create();
        _backgroundPaint.SetColor(new ImpellerColor(0.05f, 0.07f, 0.1f, 1f));

        _rectPaint = ImpellerPaintHandle.Create();

        _strokePaint = ImpellerPaintHandle.Create();
        _strokePaint.SetDrawStyle(ImpellerDrawStyle.Stroke);
        _strokePaint.SetStrokeCap(ImpellerStrokeCap.Round);
        _strokePaint.SetStrokeJoin(ImpellerStrokeJoin.Round);
        _strokePaint.SetStrokeWidth(6f);
        _strokePaint.SetColor(new ImpellerColor(1f, 0.9f, 0.4f, 1f));

        _typography = ImpellerTypographyContextHandle.Create();
        _paragraphStyle = ImpellerParagraphStyleHandle.Create();
        _paragraphStyle.SetFontSize(32f);
        _paragraphStyle.SetTextAlignment(ImpellerTextAlignment.Center);

        _starPath = CreateStarPath();
    }

    public bool Render(ImpellerDisplayListBuilderHandle builder, float width, float height)
    {
        builder.DrawPaint(_backgroundPaint);

        var margin = 48f;
        var rectHeight = MathF.Min(height * 0.35f, height - margin * 2f);
        var rect = new ImpellerRect(margin, margin, MathF.Max(0f, width - margin * 2f), rectHeight);
        EnsureGradient(rect);
        builder.DrawRect(rect, _rectPaint);
        var rectBottom = rect.Y + rect.Height;

        EnsureParagraph(MathF.Max(200f, width - margin * 2f));

        if (_paragraph is not null)
        {
            var paragraphX = (width - _paragraphWidth) * 0.5f;
            var paragraphY = rectBottom + 32f;
            builder.DrawParagraph(_paragraph, new ImpellerPoint(paragraphX, paragraphY));
        }

        var centerX = width * 0.5f;
        var centerY = rectBottom + (height - rectBottom) * 0.55f;
        var rotation = (float)(DateTime.UtcNow.TimeOfDay.TotalSeconds * 30.0 % 360.0);

        builder.Save();
        builder.Translate(centerX, centerY);
        builder.Rotate(rotation);
        builder.DrawPath(_starPath, _strokePaint);
        builder.Restore();

        return true;
    }

    private void EnsureParagraph(float maxWidth)
    {
        var targetWidth = MathF.Max(200f, MathF.Min(maxWidth, 600f));
        if (_paragraph is not null && MathF.Abs(targetWidth - _paragraphWidth) < 1f)
        {
            return;
        }

        _paragraph?.Dispose();
        using var builder = ImpellerParagraphBuilderHandle.Create(_typography);
        builder.PushStyle(_paragraphStyle);
        builder.AddText("ImpellerSharp\nBasic Demo");
        _paragraph = builder.Build(targetWidth);
        _paragraphWidth = targetWidth;
    }

    private void EnsureGradient(in ImpellerRect rect)
    {
        var start = new ImpellerPoint(rect.X, rect.Y);
        var end = new ImpellerPoint(rect.X, rect.Y + rect.Height);

        if (_gradient is not null &&
            PointsAreClose(start, _gradientStart) &&
            PointsAreClose(end, _gradientEnd))
        {
            return;
        }

        var gradient = ImpellerColorSourceHandle.CreateLinearGradient(
            start,
            end,
            s_gradientColors,
            s_gradientStops,
            ImpellerTileMode.Clamp);

        _rectPaint.SetColorSource(gradient);

        _gradient?.Dispose();
        _gradient = gradient;
        _gradientStart = start;
        _gradientEnd = end;
    }

    private static bool PointsAreClose(in ImpellerPoint first, in ImpellerPoint second)
    {
        const float epsilon = 0.5f;
        return MathF.Abs(first.X - second.X) < epsilon &&
               MathF.Abs(first.Y - second.Y) < epsilon;
    }

    private static ImpellerPathHandle CreateStarPath()
    {
        using var pathBuilder = ImpellerPathBuilderHandle.Create();
        const float outer = 120f;
        const float inner = 52f;
        const int points = 10;
        for (int i = 0; i < points; i++)
        {
            var angle = -MathF.PI / 2f + i * MathF.PI / 5f;
            var radius = i % 2 == 0 ? outer : inner;
            var point = new ImpellerPoint(
                radius * MathF.Cos(angle),
                radius * MathF.Sin(angle));
            if (i == 0)
            {
                pathBuilder.MoveTo(point);
            }
            else
            {
                pathBuilder.LineTo(point);
            }
        }

        pathBuilder.LineTo(new ImpellerPoint(0f, -outer));
        return pathBuilder.TakePath();
    }

    public void Dispose()
    {
        _paragraph?.Dispose();
        _paragraphStyle.Dispose();
        _typography.Dispose();
        _strokePaint.Dispose();
        _rectPaint.SetColorSource(null);
        _gradient?.Dispose();
        _rectPaint.Dispose();
        _backgroundPaint.Dispose();
        _starPath.Dispose();
    }
}
