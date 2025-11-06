using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ImpellerSharp.Interop;
using ImpellerSharp.Samples.BasicShapes.Scenes;

namespace ImpellerSharp.Samples.BasicShapes;

internal static class HeadlessRunner
{
    internal static bool Run(SampleOptions options)
    {
        if (!BackendContextFactory.TryCreateContext(options, out var context, out var backend, out var error))
        {
            Console.Error.WriteLine(error ?? "Failed to create Impeller context.");
            return false;
        }

        using (context)
        {
            IScene scene;
            try
            {
                scene = SceneFactory.Create(options.Scene);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false;
            }

            SceneExecutionContext.Headless = true;
            SceneExecutionContext.LastVulkanSwapchainProbe = null;
            try
            {
                using (scene)
                {
                    try
                    {
                        scene.Initialize(context!);
                    }
                    catch (Exception initEx)
                    {
                        Console.Error.WriteLine($"Scene initialization failed: {initEx.Message}");
                        return false;
                    }

                    var frameCount = options.FrameLimit ?? 1;
                    var digests = new List<FrameDigest>(frameCount);

                    for (var frame = 0; frame < frameCount; frame++)
                    {
                        using var displayList = scene.CreateDisplayList(context!, frame);
                        digests.Add(new FrameDigest(frame, scene.DescribeFrame(frame)));
                    }

                    Console.WriteLine($"Headless run complete. Scene={options.Scene}, Backend={backend}, Frames={frameCount}");
                    if (!string.IsNullOrEmpty(SceneExecutionContext.LastVulkanSwapchainProbe))
                    {
                        Console.WriteLine($"Vulkan swapchain probe: {SceneExecutionContext.LastVulkanSwapchainProbe}");
                    }

                    if (!string.IsNullOrEmpty(options.OutputPath))
                    {
                        WriteGoldenArtifact(options.OutputPath!, backend, options.Scene, digests);
                        Console.WriteLine($"Golden artifact written to {Path.GetFullPath(options.OutputPath!)}");
                    }
                }
            }
            finally
            {
                SceneExecutionContext.Headless = false;
                SceneExecutionContext.LastVulkanSwapchainProbe = null;
            }
        }

        return true;
    }

    private static void WriteGoldenArtifact(string path, string backend, string scene, IReadOnlyList<FrameDigest> digests)
    {
        var payload = new GoldenArtifact
        {
            Scene = scene,
            Backend = backend,
            GeneratedAt = DateTimeOffset.UtcNow,
            Frames = digests,
            SwapchainProbe = SceneExecutionContext.LastVulkanSwapchainProbe,
        };

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, payload, options);
    }

    private sealed record GoldenArtifact
    {
        public string Scene { get; init; } = string.Empty;

        public string Backend { get; init; } = string.Empty;

        public DateTimeOffset GeneratedAt { get; init; }

        public IReadOnlyList<FrameDigest> Frames { get; init; } = Array.Empty<FrameDigest>();

        public string? SwapchainProbe { get; init; }
    }

    private sealed record FrameDigest(int Frame, string Summary);
}
