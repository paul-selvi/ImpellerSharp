using Avalonia;

namespace AvaloniaImpellerApp;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .UseSkia()
        .With(new AvaloniaNativePlatformOptions()
        {
            RenderingMode = 
            [
                AvaloniaNativeRenderingMode.Metal
            ]
        })
        .LogToTrace();
}
