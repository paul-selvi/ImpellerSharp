using System;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Samples.BasicShapes;

internal static partial class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Impeller BasicShapes sample bootstrap");

        try
        {
            SampleOptions options;
            try
            {
                options = SampleOptions.Parse(args);
            }
            catch (Exception parseEx)
            {
                Console.Error.WriteLine(parseEx.Message);
                SampleOptions.PrintUsage();
                return;
            }

            if (options.Headless)
            {
                if (!HeadlessRunner.Run(options))
                {
                    Environment.ExitCode = 1;
                }
                return;
            }

            if (!OperatingSystem.IsMacOS())
            {
                Console.Error.WriteLine("Interactive mode is currently only implemented for macOS. Use --headless for golden exports.");
                Environment.ExitCode = 1;
                return;
            }

            RunPlatformSample(options);
        }
        catch (ImpellerInteropException ex)
        {
            Console.Error.WriteLine($"Impeller initialization failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled exception: {ex}");
        }
    }

    static partial void RunPlatformSample(SampleOptions options);
}
