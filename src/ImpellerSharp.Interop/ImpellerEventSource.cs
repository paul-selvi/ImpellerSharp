using System.Diagnostics.Tracing;

namespace ImpellerSharp.Interop;

[EventSource(Name = "ImpellerSharp.Interop")]
internal sealed class ImpellerEventSource : EventSource
{
    internal static readonly ImpellerEventSource Log = new();

    private ImpellerEventSource()
    {
    }

    [Event(1, Level = EventLevel.Informational, Message = "Context created: {0}, version {1}")]
    public void ContextCreated(string backend, uint version)
    {
        WriteEvent(1, backend, version);
    }

    [Event(2, Level = EventLevel.Informational, Message = "Texture created {0}x{1}, format {2}")]
    public void TextureCreated(long width, long height, int pixelFormat)
    {
        WriteEvent(2, width, height, pixelFormat);
    }

    [Event(3, Level = EventLevel.Informational, Message = "Surface draw display list success={0}")]
    public void SurfaceDrawDisplayList(bool success)
    {
        WriteEvent(3, success);
    }
}
