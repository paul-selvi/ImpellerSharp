using System;
using System.IO;
using ImpellerSharp.Interop;
using ImpellerSharp.Samples.BasicShapes.Scenes;

namespace ImpellerSharp.Samples.BasicShapes;

internal static partial class Program
{
    static partial void RunPlatformSample(SampleOptions options)
    {
        if (!OperatingSystem.IsMacOS())
        {
            Console.WriteLine("Mac-specific sample invoked on non-macOS platform.");
            return;
        }

        var backendPreference = options.Backend;
        var chosenBackend = backendPreference == "auto" ? "metal" : backendPreference;
        if (!string.Equals(chosenBackend, "metal", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine($"Backend '{backendPreference}' is not supported for the interactive macOS sample. Falling back to Metal.");
            chosenBackend = "metal";
        }

        Console.WriteLine($"Scene: {options.Scene}");
        Console.WriteLine($"Frame limit: {options.FrameLimit?.ToString() ?? "unbounded"}");
        if (!string.IsNullOrEmpty(options.CapturePath))
        {
            Console.WriteLine($"Capture: {Path.GetFullPath(options.CapturePath)} (first frame)");
        }

        using var host = MacMetalHost.Create(1280, 720, "Impeller BasicShapes");
        using var context = ImpellerContextHandle.CreateMetal();

        IScene scene;
        try
        {
            scene = SceneFactory.Create(options.Scene);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return;
        }

        using (scene)
        {
            try
            {
                scene.Initialize(context);
            }
            catch (Exception initEx)
            {
                Console.Error.WriteLine($"Scene initialization failed: {initEx.Message}");
                return;
            }

            Console.WriteLine("Entering render loop. Close the window to exit.");

            var frameIndex = 0;
            var captureWritten = false;

            while (!host.ShouldClose && (!options.FrameLimit.HasValue || frameIndex < options.FrameLimit.Value))
            {
                host.PumpEvents();

                if (!host.TryAcquireDrawable(out var drawable))
                {
                    continue;
                }

                using (var displayList = scene.CreateDisplayList(context, frameIndex))
                {
                    using var surface = ImpellerSurfaceHandle.WrapMetalDrawable(context, drawable);
                    if (surface.DrawDisplayList(displayList))
                    {
                        surface.Present();
                    }
                }

                if (!captureWritten && !string.IsNullOrEmpty(options.CapturePath))
                {
                    try
                    {
                        MacCapture.CaptureWindow(host.WindowNumber, options.CapturePath!);
                        captureWritten = true;
                        Console.WriteLine($"Captured frame to {options.CapturePath}");
                    }
                    catch (Exception captureEx)
                    {
                        Console.Error.WriteLine($"Capture failed: {captureEx.Message}");
                        captureWritten = true;
                    }
                }

                host.ReleaseDrawable(drawable);
                frameIndex++;
            }
        }

        Console.WriteLine("Render loop exited.");
    }
}
