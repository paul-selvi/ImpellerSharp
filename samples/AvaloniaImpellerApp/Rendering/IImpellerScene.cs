using System;
using ImpellerSharp.Interop;

namespace AvaloniaImpellerApp.Rendering;

internal interface IImpellerScene : IDisposable
{
    bool Render(ImpellerDisplayListBuilderHandle builder, float width, float height);
}
