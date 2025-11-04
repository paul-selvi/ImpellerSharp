using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.VisualTree;

namespace ImpellerSharp.Avalonia.Controls;

/// <summary>
/// Avalonia control that exposes Skia lease rendering callbacks compatible with Impeller.
/// </summary>
public class ImpellerSkiaView : Control
{
    private CompositionCustomVisual? _customVisual;
    private ImpellerVisualHandler? _handler;

    /// <summary>
    /// Raised when a new Skia lease is available for rendering.
    /// </summary>
    public event EventHandler<ImpellerSkiaRenderEventArgs>? RenderSkia;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachCompositionVisual();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachCompositionVisual();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty)
        {
            UpdateCompositionSize(Bounds.Size);
        }
    }

    private void AttachCompositionVisual()
    {
        var rootVisual = ElementComposition.GetElementVisual(this);
        if (rootVisual is null)
        {
            return;
        }

        if (_customVisual != null)
        {
            DetachCompositionVisual();
        }

        _handler = new ImpellerVisualHandler(this);
        _customVisual = rootVisual.Compositor.CreateCustomVisual(_handler);
        UpdateCompositionSize(Bounds.Size);
        ElementComposition.SetElementChildVisual(this, _customVisual);
    }

    private void DetachCompositionVisual()
    {
        _customVisual?.SendHandlerMessage(ImpellerVisualHandler.DisposeMessage.Instance);
        ElementComposition.SetElementChildVisual(this, null);
        _customVisual = null;
        _handler = null;
    }

    private void UpdateCompositionSize(Size size)
    {
        if (_customVisual is not null)
        {
            _customVisual.Size = new Vector(size.Width, size.Height);
        }
    }

    private sealed class ImpellerVisualHandler : CompositionCustomVisualHandler
    {
        private readonly ImpellerSkiaView _owner;

        public ImpellerVisualHandler(ImpellerSkiaView owner)
        {
            _owner = owner;
        }

        public override void OnRender(ImmediateDrawingContext drawingContext)
        {
            var handler = _owner.RenderSkia;
            if (handler is null)
            {
                return;
            }

            if (!drawingContext.TryGetFeature<ISkiaSharpApiLeaseFeature>(out var leaseFeature))
            {
                return;
            }

            using var lease = leaseFeature.Lease();
            var args = new ImpellerSkiaRenderEventArgs(lease);
            try
            {
                handler(_owner, args);
            }
            finally
            {
                args.Dispose();
            }
        }

        public override void OnMessage(object message)
        {
            if (ReferenceEquals(message, DisposeMessage.Instance))
            {
                // nothing to dispose currently
            }
        }

        public sealed class DisposeMessage
        {
            public static readonly DisposeMessage Instance = new();

            private DisposeMessage()
            {
            }
        }
    }
}
