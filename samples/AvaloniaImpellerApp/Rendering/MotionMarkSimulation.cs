using System;
using System.Collections.Generic;
using ImpellerSharp.Interop;

namespace AvaloniaImpellerApp.Rendering;

internal sealed class MotionMarkSimulation : IImpellerScene
{
    private const int GridWidth = 80;
    private const int GridHeight = 40;

    private static readonly ImpellerColor[] Palette =
    {
        CreateColor(0x10, 0x10, 0x10),
        CreateColor(0x80, 0x80, 0x80),
        CreateColor(0xC0, 0xC0, 0xC0),
        CreateColor(0x10, 0x10, 0x10),
        CreateColor(0x80, 0x80, 0x80),
        CreateColor(0xC0, 0xC0, 0xC0),
        CreateColor(0xE0, 0x10, 0x40),
    };

    private static readonly (int Dx, int Dy)[] Offsets =
    {
        (-4, 0),
        (2, 0),
        (1, -2),
        (1, 2),
    };

    private readonly Random _random = new((int)Environment.TickCount64);
    private readonly List<Element> _elements = new();
    private GridPoint _lastPoint = new(GridWidth / 2, GridHeight / 2);
    private readonly ImpellerPaintHandle _backgroundPaint;
    private readonly ImpellerPaintHandle _strokePaint;
    private int _complexity = 8;

    public MotionMarkSimulation()
    {
        _backgroundPaint = ImpellerPaintHandle.Create();
        _backgroundPaint.SetColor(new ImpellerColor(12f / 255f, 16f / 255f, 24f / 255f, 1f));
        _strokePaint = ImpellerPaintHandle.Create();
        _strokePaint.SetDrawStyle(ImpellerDrawStyle.Stroke);
        _strokePaint.SetStrokeCap(ImpellerStrokeCap.Round);
        _strokePaint.SetStrokeJoin(ImpellerStrokeJoin.Round);

        ResizeElements(ComputeElementCount(_complexity));
    }

    public int Complexity => _complexity;

    public void SetComplexity(int complexity)
    {
        complexity = Math.Clamp(complexity, 0, 24);
        if (complexity == _complexity)
        {
            return;
        }

        _complexity = complexity;
        ResizeElements(ComputeElementCount(_complexity));
    }

    public bool Render(ImpellerDisplayListBuilderHandle builder, float width, float height)
    {
        if (builder is null || builder.IsInvalid)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.DrawPaint(_backgroundPaint);

        if (_elements.Count == 0 || width <= 0f || height <= 0f)
        {
            return false;
        }

        var scaleX = width / (GridWidth + 1f);
        var scaleY = height / (GridHeight + 1f);
        var uniformScale = MathF.Min(scaleX, scaleY);
        uniformScale = MathF.Max(0f, uniformScale);
        if (uniformScale <= 0f)
        {
            return false;
        }

        var offsetX = (width - uniformScale * (GridWidth + 1f)) * 0.5f;
        var offsetY = (height - uniformScale * (GridHeight + 1f)) * 0.5f;

        using var pathBuilder = ImpellerPathBuilderHandle.Create();
        var pathStarted = false;

        for (var i = 0; i < _elements.Count; i++)
        {
            var element = _elements[i];

            if (!pathStarted)
            {
                var startPoint = ToPoint(element.Start, uniformScale, offsetX, offsetY);
                pathBuilder.MoveTo(startPoint);
                pathStarted = true;
            }

            switch (element.Kind)
            {
                case SegmentKind.Line:
                    pathBuilder.LineTo(ToPoint(element.End, uniformScale, offsetX, offsetY));
                    break;

                case SegmentKind.Quad:
                    pathBuilder.QuadraticCurveTo(
                        ToPoint(element.Control1, uniformScale, offsetX, offsetY),
                        ToPoint(element.End, uniformScale, offsetX, offsetY));
                    break;

                case SegmentKind.Cubic:
                    pathBuilder.CubicCurveTo(
                        ToPoint(element.Control1, uniformScale, offsetX, offsetY),
                        ToPoint(element.Control2, uniformScale, offsetX, offsetY),
                        ToPoint(element.End, uniformScale, offsetX, offsetY));
                    break;
            }

            var finalize = element.Split || i + 1 == _elements.Count;
            if (finalize)
            {
                using var path = pathBuilder.TakePath();
                _strokePaint.SetColor(element.Color);
                _strokePaint.SetStrokeWidth(element.Width);
                builder.DrawPath(path, _strokePaint);
                pathStarted = false;
            }

            if (_random.NextDouble() > 0.995)
            {
                element = element.ToggleSplit();
                _elements[i] = element;
            }
        }

        return true;
    }

    public void Dispose()
    {
        _backgroundPaint.Dispose();
        _strokePaint.Dispose();
    }

    private static int ComputeElementCount(int complexity)
    {
        if (complexity < 10)
        {
            return (complexity + 1) * 1000;
        }

        var extended = (complexity - 8) * 10000;
        return Math.Min(extended, 120000);
    }

    private void ResizeElements(int targetCount)
    {
        var current = _elements.Count;
        if (targetCount == current)
        {
            return;
        }

        if (targetCount < current)
        {
            _elements.RemoveRange(targetCount, current - targetCount);
            _lastPoint = targetCount > 0 ? _elements[_elements.Count - 1].End : new GridPoint(GridWidth / 2, GridHeight / 2);
            return;
        }

        if (current == 0)
        {
            _lastPoint = new GridPoint(GridWidth / 2, GridHeight / 2);
        }
        else
        {
            _lastPoint = _elements[_elements.Count - 1].End;
        }

        _elements.Capacity = Math.Max(_elements.Capacity, targetCount);

        for (var i = current; i < targetCount; i++)
        {
            var element = CreateRandomElement(_lastPoint);
            _elements.Add(element);
            _lastPoint = element.End;
        }
    }

    private Element CreateRandomElement(GridPoint last)
    {
        var segmentType = _random.Next(0, 4);
        var next = RandomPoint(last);

        var element = new Element
        {
            Start = last,
            Color = Palette[_random.Next(Palette.Length)],
            Width = (float)(Math.Pow(_random.NextDouble(), 5.0) * 20.0 + 1.0f),
            Split = _random.NextDouble() < 0.5,
        };

        if (segmentType < 2)
        {
            element.Kind = SegmentKind.Line;
            element.End = next;
        }
        else if (segmentType == 2)
        {
            var p2 = RandomPoint(next);
            element.Kind = SegmentKind.Quad;
            element.Control1 = next;
            element.End = p2;
        }
        else
        {
            var p2 = RandomPoint(next);
            var p3 = RandomPoint(next);
            element.Kind = SegmentKind.Cubic;
            element.Control1 = next;
            element.Control2 = p2;
            element.End = p3;
        }

        return element;
    }

    private GridPoint RandomPoint(GridPoint last)
    {
        var (dx, dy) = Offsets[_random.Next(Offsets.Length)];
        var x = last.X + dx;
        if (x < 0 || x > GridWidth)
        {
            x -= dx * 2;
        }

        var y = last.Y + dy;
        if (y < 0 || y > GridHeight)
        {
            y -= dy * 2;
        }

        return new GridPoint(x, y);
    }

    private static ImpellerPoint ToPoint(GridPoint point, float scale, float offsetX, float offsetY)
    {
        var px = offsetX + (point.X + 0.5f) * scale;
        var py = offsetY + (point.Y + 0.5f) * scale;
        return new ImpellerPoint(px, py);
    }

    private static ImpellerColor CreateColor(byte r, byte g, byte b)
    {
        return new ImpellerColor(r / 255f, g / 255f, b / 255f, 1f);
    }

    private readonly record struct GridPoint(int X, int Y);

    private enum SegmentKind : byte
    {
        Line,
        Quad,
        Cubic,
    }

    private struct Element
    {
        public SegmentKind Kind;
        public GridPoint Start;
        public GridPoint Control1;
        public GridPoint Control2;
        public GridPoint End;
        public ImpellerColor Color;
        public float Width;
        public bool Split;

        public Element ToggleSplit()
        {
            var copy = this;
            copy.Split = !copy.Split;
            return copy;
        }
    }
}
