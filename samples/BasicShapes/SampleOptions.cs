using System;
using System.Collections.Generic;

namespace ImpellerSharp.Samples.BasicShapes;

internal sealed class SampleOptions
{
    private static readonly IReadOnlyDictionary<string, string> SceneAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["basic"] = "rects",
            ["rectangles"] = "rects",
            ["streaming"] = "stream",
            ["textures"] = "stream",
            ["text"] = "typography",
        };

    internal string Scene { get; private set; } = "rects";

    internal int? FrameLimit { get; private set; }

    internal string? CapturePath { get; private set; }

    internal string Backend { get; private set; } = "auto";

    internal bool Headless { get; private set; }

    internal string? OutputPath { get; private set; }

    internal nint? VulkanSurface { get; private set; }

    internal static SampleOptions Parse(string[] args)
    {
        var options = new SampleOptions();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--scene", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-s", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("Missing value for --scene.");
                }

                options.Scene = NormalizeScene(args[++i]);
                continue;
            }

            if (string.Equals(arg, "--backend", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-b", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("Missing value for --backend.");
                }

                options.Backend = NormalizeBackend(args[++i]);
                continue;
            }

            if (string.Equals(arg, "--frames", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-f", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length || !int.TryParse(args[++i], out var frames) || frames <= 0)
                {
                    throw new ArgumentException("Invalid value for --frames. Provide a positive integer.");
                }

                options.FrameLimit = frames;
                continue;
            }

            if (string.Equals(arg, "--capture", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-c", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("Missing value for --capture.");
                }

                options.CapturePath = args[++i];
                continue;
            }

            if (string.Equals(arg, "--headless", StringComparison.OrdinalIgnoreCase))
            {
                options.Headless = true;
                continue;
            }

            if (string.Equals(arg, "--output", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-o", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("Missing value for --output.");
                }

                options.OutputPath = args[++i];
                continue;
            }

            if (string.Equals(arg, "--vk-surface", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("Missing value for --vk-surface.");
                }

                options.VulkanSurface = ParsePointer(args[++i]);
                continue;
            }

            if (string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase))
            {
                PrintUsage();
                Environment.Exit(0);
            }

            throw new ArgumentException($"Unrecognized argument '{arg}'. Use --help for usage.");
        }

        return options;
    }

    internal static void PrintUsage()
    {
        Console.WriteLine("BasicShapes options:");
        Console.WriteLine("  --scene|-s <rects|stream|typography>  Scene to render (default: rects).");
        Console.WriteLine("  --backend|-b <auto|metal|vulkan|opengles>  Rendering backend (default: auto).");
        Console.WriteLine("  --frames|-f <count>                   Optional frame limit (default: run until window close).");
        Console.WriteLine("  --capture|-c <path>                   Capture window to the provided PNG path after first frame (macOS).");
        Console.WriteLine("  --headless                            Run without a window (display lists only).");
        Console.WriteLine("  --output|-o <path>                    When headless, write scene digest JSON to this path.");
        Console.WriteLine("  --vk-surface <pointer>                Hex/decimal VkSurfaceKHR handle for Vulkan swapchain probing (optional).");
        Console.WriteLine("  --help|-h                             Display this help.");
        Console.WriteLine();
        Console.WriteLine("Scene aliases: basic/rectangles -> rects, streaming/textures -> stream, text -> typography.");
    }

    private static string NormalizeScene(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Scene name must not be empty.", nameof(value));
        }

        if (SceneAliases.TryGetValue(value, out var alias))
        {
            return alias;
        }

        return value.ToLowerInvariant();
    }

    private static string NormalizeBackend(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Backend name must not be empty.", nameof(value));
        }

        var normalized = value.ToLowerInvariant();
        return normalized switch
        {
            "auto" => "auto",
            "metal" => "metal",
            "vulkan" => "vulkan",
            "vk" => "vulkan",
            "moltenvk" => "vulkan",
            "opengles" => "opengles",
            "gles" => "opengles",
            _ => throw new ArgumentException($"Unsupported backend '{value}'. Choose auto, metal, vulkan, or opengles.", nameof(value)),
        };
    }

    private static nint ParsePointer(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Pointer value must not be empty.", nameof(value));
        }

        value = value.Trim();

        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (ulong.TryParse(value.AsSpan(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var hex))
            {
                return new nint(unchecked((long)hex));
            }
        }
        else if (long.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var dec))
        {
            return new nint(dec);
        }

        throw new ArgumentException($"Unable to parse pointer value '{value}'. Use hex (0x...) or decimal.", nameof(value));
    }
}
