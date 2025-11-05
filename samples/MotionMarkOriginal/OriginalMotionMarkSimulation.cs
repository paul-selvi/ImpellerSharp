using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Samples.MotionMarkOriginal;

internal sealed class OriginalMotionMarkSimulation : IDisposable
{
    private const int MaxTiles = 4096;
    private const int TotalRows = 71;

    private readonly Random _random = new((int)Environment.TickCount64);
    private readonly List<Tile> _tiles = new();
    private readonly ImpellerPaintHandle _backgroundPaint;
    private readonly ImpellerPaintHandle _fillPaint;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private Vector2 _layoutSize;
    private int _centerSpiralCount;
    private int _sidePanelCount;
    private int _targetTileCount;

    public OriginalMotionMarkSimulation()
    {
        _backgroundPaint = ImpellerPaintHandle.Create();
        _backgroundPaint.SetColor(new ImpellerColor(0.06f, 0.08f, 0.12f, 1f));

        _fillPaint = ImpellerPaintHandle.Create();

        SetComplexity(6);
    }

    public void SetComplexity(int complexity)
    {
        complexity = Math.Clamp(complexity, 0, 24);
        _targetTileCount = Math.Clamp((complexity + 1) * 64, 32, MaxTiles);
    }

    public bool Render(ImpellerDisplayListBuilderHandle builder, float width, float height)
    {
        if (builder is null || builder.IsInvalid)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (width <= 0f || height <= 0f)
        {
            return false;
        }

        EnsureTiles(width, height);

        if (_tiles.Count == 0)
        {
            return false;
        }

        builder.DrawPaint(_backgroundPaint);

        var activeCount = Math.Min(_targetTileCount, _tiles.Count);
        if (activeCount <= 0)
        {
            return true;
        }

        var distanceFactor = ComputeDistanceFactor(activeCount);
        var progress = (float)(_stopwatch.Elapsed.TotalMilliseconds % 10000.0) / 10000f;
        var bounce = MathF.Sin(2f * MathF.Abs(0.5f - progress));
        var lightnessBase = Lerp(bounce, 20f, 50f);
        var hueDegrees = Lerp(progress, 0f, 360f);

        for (var i = 0; i < activeCount; ++i)
        {
            var tile = _tiles[i];

            tile.Rotation = (tile.Rotation + tile.Step) % 360f;

            var influence = MathF.Max(0.01f, 1f - tile.Distance * distanceFactor);
            var tangent = MathF.Tan(MathF.Min(influence / 1.25f, 1.2f));
            var lightness = Clamp(lightnessBase * tangent / 100f, 0f, 1f);
            var alpha = Clamp(influence, 0f, 1f);
            var color = FromHsl(hueDegrees / 360f, 1f, lightness, alpha);

            _fillPaint.SetColor(color);

            var half = tile.Size * 0.5f;
            var cx = tile.Position.X + half;
            var cy = tile.Position.Y + half;

            builder.Save();
            builder.Translate(cx, cy);
            builder.Rotate(tile.Rotation);
            builder.Translate(-half, -half);
            builder.DrawRect(new ImpellerRect(0f, 0f, tile.Size, tile.Size), _fillPaint);
            builder.Restore();

            _tiles[i] = tile;
        }

        return true;
    }

    private void EnsureTiles(float width, float height)
    {
        var size = new Vector2(width, height);
        if (_tiles.Count == 0 || Vector2.DistanceSquared(size, _layoutSize) > 4f)
        {
            BuildTiles(width, height);
            _layoutSize = size;
        }
    }

    private void BuildTiles(float width, float height)
    {
        _tiles.Clear();

        var tileSize = MathF.Max(4f, MathF.Round(height / TotalRows));
        if (tileSize <= 0f)
        {
            return;
        }

        var tileStride = tileSize;
        var startX = MathF.Round((width - tileSize) / 2f);
        var startY = MathF.Round((height - tileSize) / 2f);

        var maxSide = Math.Max(1, (int)(MathF.Floor(startY / tileStride) * 2f + 1f));
        _centerSpiralCount = maxSide * maxSide;

        var direction = 0;
        var spiralCounter = 2;
        var nextIndex = 1;
        var currentX = startX;
        var currentY = startY;
        var center = new Vector2(width * 0.5f, height * 0.5f);

        for (var i = 0; i < _centerSpiralCount; ++i)
        {
            AddTile(currentX, currentY, tileSize, center);

            if (i == nextIndex)
            {
                direction = (direction + 1) % 4;
                spiralCounter++;
                nextIndex += spiralCounter >> 1;
            }

            switch (direction)
            {
                case 0:
                    currentX += tileStride;
                    break;
                case 1:
                    currentY -= tileStride;
                    break;
                case 2:
                    currentX -= tileStride;
                    break;
                default:
                    currentY += tileStride;
                    break;
            }
        }

        var span = Math.Max(0, (int)MathF.Floor((width - currentX) / tileStride));
        _sidePanelCount = maxSide * span * 2;

        for (var i = 0; i < _sidePanelCount; ++i)
        {
            var column = MathF.Floor(i / (float)maxSide);
            var sideX = currentX + MathF.Floor(column / 2f) * tileStride;
            var sideY = currentY - tileStride * (i % maxSide);

            if (((int)column % 2) == 1)
            {
                sideX = width - sideX - tileSize + 1f;
            }

            AddTile(sideX, sideY, tileSize, center);
        }
    }

    private void AddTile(float x, float y, float size, Vector2 sceneCenter)
    {
        var half = size * 0.5f;
        var tileCenter = new Vector2(x + half, y + half);
        var distance = Vector2.Distance(sceneCenter, tileCenter) / Math.Max(1f, size);

        _tiles.Add(new Tile
        {
            Position = new Vector2(x, y),
            Size = size,
            Rotation = _random.Next(0, 360),
            Step = MathF.Max(3f, distance / 1.5f),
            Distance = distance,
        });
    }

    private float ComputeDistanceFactor(int activeCount)
    {
        if (activeCount <= 0)
        {
            return 0f;
        }

        var sqrt = MathF.Sqrt(MathF.Max(1f, activeCount));
        var overflow = MathF.Max(0, activeCount - _centerSpiralCount);
        var panelRatio = _sidePanelCount > 0 ? overflow / (float)_sidePanelCount : 0f;
        return 1.5f * (1f - 0.5f * panelRatio) / sqrt;
    }

    private static float Lerp(float t, float a, float b) => a + (b - a) * t;

    private static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }
        if (value > max)
        {
            return max;
        }
        return value;
    }

    private static ImpellerColor FromHsl(float h, float s, float l, float a)
    {
        float r, g, b;

        if (s <= 0f)
        {
            r = g = b = l;
        }
        else
        {
            var q = l < 0.5f ? l * (1f + s) : l + s - l * s;
            var p = 2f * l - q;
            r = HueToRgb(p, q, h + 1f / 3f);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1f / 3f);
        }

        return new ImpellerColor(r, g, b, Clamp(a, 0f, 1f));
    }

    private static float HueToRgb(float p, float q, float t)
    {
        if (t < 0f)
        {
            t += 1f;
        }
        if (t > 1f)
        {
            t -= 1f;
        }

        if (t < 1f / 6f)
        {
            return p + (q - p) * 6f * t;
        }
        if (t < 0.5f)
        {
            return q;
        }
        if (t < 2f / 3f)
        {
            return p + (q - p) * (2f / 3f - t) * 6f;
        }

        return p;
    }

    public void Dispose()
    {
        _backgroundPaint.Dispose();
        _fillPaint.Dispose();
    }

    private struct Tile
    {
        public Vector2 Position;
        public float Size;
        public float Rotation;
        public float Step;
        public float Distance;
    }
}
