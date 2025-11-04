using System;
using Avalonia;

namespace ImpellerSharp.Avalonia.Mac.Controls;

public sealed class MetalRenderEventArgs
{
    internal MetalRenderEventArgs(
        nint view,
        nint layer,
        nint drawable,
        nint texture,
        nint device,
        PixelSize pixelSize,
        double scaling)
    {
        View = view;
        Layer = layer;
        Drawable = drawable;
        Texture = texture;
        Device = device;
        PixelSize = pixelSize;
        Scaling = scaling;
    }

    public nint View { get; }

    public nint Layer { get; }

    public nint Drawable { get; }

    public nint Texture { get; }

    public nint Device { get; }

    public PixelSize PixelSize { get; }

    public double Scaling { get; }

    public Size LogicalSize => Scaling > 0
        ? new Size(PixelSize.Width / Scaling, PixelSize.Height / Scaling)
        : new Size(0, 0);

    public void MarkRendered()
    {
        Rendered = true;
    }

    public bool Rendered { get; private set; }
}
