using System.Diagnostics;

namespace ImpellerSharp.Interop;

internal static class ImpellerDiagnostics
{
    internal static readonly ActivitySource ActivitySource = new("ImpellerSharp.Interop");

    internal static void ContextCreated(string backend, uint version)
    {
        ImpellerEventSource.Log.ContextCreated(backend, version);
    }

    internal static void TextureCreated(in ImpellerTextureDescriptor descriptor)
    {
        ImpellerEventSource.Log.TextureCreated(descriptor.Size.Width, descriptor.Size.Height, (int)descriptor.PixelFormat);
    }

    internal static void SurfaceDrawDisplayList(bool success)
    {
        ImpellerEventSource.Log.SurfaceDrawDisplayList(success);
    }
}
