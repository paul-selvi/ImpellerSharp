using System;
using ImpellerSharp.Interop;

namespace ImpellerSharp.Samples.BasicShapes.Scenes;

internal interface IScene : IDisposable
{
    string Name { get; }

    void Initialize(ImpellerContextHandle context);

    ImpellerDisplayListHandle CreateDisplayList(ImpellerContextHandle context, int frameIndex);

    string DescribeFrame(int frameIndex);
}
