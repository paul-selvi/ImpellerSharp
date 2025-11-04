using System;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Samples.BasicShapes;

internal static partial class Program
{
    private static void Main()
    {
        Console.WriteLine("Impeller BasicShapes sample bootstrap");

        try
        {
            RunPlatformSample();
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

    static partial void RunPlatformSample();
}
