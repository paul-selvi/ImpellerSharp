using System;
using System.IO;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Samples.BasicShapes.Scenes;

internal sealed class TypographyScene : IScene
{
    private static readonly string[] FontCandidates =
    {
        "/System/Library/Fonts/SFNS.ttf",
        "/System/Library/Fonts/SFNSDisplay.ttf",
        "/System/Library/Fonts/SFNSMono.ttf",
        "/System/Library/Fonts/Supplemental/Arial.ttf",
        "/System/Library/Fonts/Supplemental/Helvetica.ttf",
        "/System/Library/Fonts/Supplemental/Times New Roman.ttf",
    };

    private ImpellerTypographyContextHandle? _typography;
    private ImpellerParagraphHandle? _paragraph;
    private ImpellerPaintHandle? _textPaint;
    private ImpellerPaintHandle? _backgroundPaint;
    private string? _fontFamilyAlias;
    private string _sampleText = string.Empty;

    public string Name => "typography";

    public void Initialize(ImpellerContextHandle context)
    {
        _typography = ImpellerTypographyContextHandle.Create();

        var fontAlias = "SampleFont";
        var fontData = LoadFontBytes(out var resolvedAlias);
        if (!_typography.RegisterFont(fontData, resolvedAlias))
        {
            throw new InvalidOperationException("Failed to register typography font.");
        }

        _fontFamilyAlias = resolvedAlias ?? fontAlias;

        _textPaint = ImpellerPaintHandle.Create();
        _textPaint.SetColor(new ImpellerColor(0.92f, 0.95f, 0.98f, 1f));

        _backgroundPaint = ImpellerPaintHandle.Create();
        _backgroundPaint.SetColor(new ImpellerColor(0.1f, 0.12f, 0.16f, 1f));

        using var paragraphStyle = ImpellerParagraphStyleHandle.Create();
        paragraphStyle.SetFontFamily(_fontFamilyAlias);
        paragraphStyle.SetFontSize(48f);
        paragraphStyle.SetForeground(_textPaint);
        paragraphStyle.SetTextAlignment(ImpellerTextAlignment.Center);
        paragraphStyle.SetTextDirection(ImpellerTextDirection.LeftToRight);

        using var builder = ImpellerParagraphBuilderHandle.Create(_typography);
        builder.PushStyle(paragraphStyle);
        _sampleText = GetSampleText();
        builder.AddText(_sampleText);
        builder.PopStyle();

        _paragraph = builder.Build(960f);
    }

    public ImpellerDisplayListHandle CreateDisplayList(ImpellerContextHandle context, int frameIndex)
    {
        using var builder = ImpellerDisplayListBuilderHandle.Create();
        builder.DrawRect(new ImpellerRect(0, 0, 1280, 720), _backgroundPaint!);

        if (_paragraph is not null)
        {
            builder.DrawParagraph(_paragraph, new ImpellerPoint(160f, 200f));
        }

        return builder.Build();
    }

    public string DescribeFrame(int frameIndex)
    {
        var fontAlias = _fontFamilyAlias ?? "unknown";
        return $"typography:font={fontAlias};length={_sampleText.Length};frame={frameIndex}";
    }

    public void Dispose()
    {
        _paragraph?.Dispose();
        _paragraph = null;

        _typography?.Dispose();
        _typography = null;

        _textPaint?.Dispose();
        _textPaint = null;

        _backgroundPaint?.Dispose();
        _backgroundPaint = null;
    }

    private static byte[] LoadFontBytes(out string? alias)
    {
        foreach (var candidate in FontCandidates)
        {
            if (File.Exists(candidate))
            {
                alias = Path.GetFileNameWithoutExtension(candidate);
                return File.ReadAllBytes(candidate);
            }
        }

        throw new FileNotFoundException("Unable to locate a system font for typography scene.", string.Join(", ", FontCandidates));
    }

    private static string GetSampleText() =>
        "Impeller Typography\n" +
        "Safe handles, spans, and command encoders\n" +
        "rendered directly from .NET.";
}
