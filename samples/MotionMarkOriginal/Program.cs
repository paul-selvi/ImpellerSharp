using System;
using ImpellerSharp.Interop.Hosting;

namespace ImpellerSharp.Samples.MotionMarkOriginal;

internal static class Program
{
    private static int Main()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Console.Error.WriteLine("MotionMarkOriginal sample currently supports macOS Metal backends only.");
            return 1;
        }

        using var simulation = new OriginalMotionMarkSimulation();
        simulation.SetComplexity(12);

        var options = new MetalGlfwHostOptions
        {
            Title = "Impeller MotionMark Classic (Metal)",
            Width = 1280,
            Height = 720,
            ErrorLogger = message => Console.Error.WriteLine(message),
        };

        using var host = new MetalGlfwAppHost(options);
        return host.Run((builder, width, height) => simulation.Render(builder, width, height));
    }
}
