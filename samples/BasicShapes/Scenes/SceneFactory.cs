using System;
using System.Collections.Generic;

namespace ImpellerSharp.Samples.BasicShapes.Scenes;

internal static class SceneFactory
{
    private static readonly Dictionary<string, Func<IScene>> SceneResolvers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["rects"] = () => new RectGridScene(),
        ["stream"] = () => new TextureStreamingScene(),
        ["typography"] = () => new TypographyScene(),
    };

    internal static IScene Create(string name)
    {
        if (SceneResolvers.TryGetValue(name, out var factory))
        {
            return factory();
        }

        throw new ArgumentException(
            $"Unknown scene '{name}'. Available scenes: {string.Join(", ", SceneResolvers.Keys)}",
            nameof(name));
    }

    internal static IReadOnlyCollection<string> Scenes => SceneResolvers.Keys;
}
