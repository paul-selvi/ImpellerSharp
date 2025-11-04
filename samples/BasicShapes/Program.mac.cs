using System.Runtime.InteropServices;

namespace ImpellerSharp.Samples.BasicShapes;

internal static partial class Program
{
    private static partial void RunPlatformSample()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Console.WriteLine("Mac-specific sample invoked on non-macOS platform.");
            return;
        }

        Console.WriteLine("Creating GLFW window and Metal surface (stub).");

        // TODO: P/Invoke into GLFW + Metal to create CAMetalLayer and wrap drawable.

        // For now, demonstrate display list construction only.
        using var context = ImpellerContextHandle.CreateMetal();
        using var paint = ImpellerPaintHandle.Create();
        using var builder = ImpellerDisplayListBuilderHandle.Create();

        paint.SetColor(new ImpellerColor(1f, 0f, 0f, 1f));
        builder.DrawRect(new ImpellerRect(10, 10, 100, 100), paint);

        using var displayList = builder.Build();

        Console.WriteLine("Display list created; integrate surface draw when GLFW binding is available.");
    }
}
