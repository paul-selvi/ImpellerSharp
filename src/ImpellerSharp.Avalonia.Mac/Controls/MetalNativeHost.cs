using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;

namespace ImpellerSharp.Avalonia.Mac.Controls;

/// <summary>
/// macOS-native host that provides CAMetalDrawable interop for Impeller.
/// </summary>
public sealed class MetalNativeHost : NativeControlHost
{
    private const double TargetFrameRate = 60d;

    private readonly PixelSize _defaultPixelSize = new(1, 1);

    private DispatcherTimer? _renderTimer;
    private PixelSize _currentPixelSize;
    private double _currentScaling = 1d;
    private nint _viewHandle;
    private nint _layerHandle;

    public event EventHandler<MetalRenderEventArgs>? RenderMetal;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle? parent)
    {
        if (!OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("MetalNativeHost is only supported on macOS.");
        }

        _viewHandle = MetalView.Create();
        _layerHandle = MetalView.GetLayer(_viewHandle);

        UpdateDrawableSize(force: true);
        StartRenderLoop();

        return new PlatformHandle(_viewHandle, "NSView");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        StopRenderLoop();

        var view = control.Handle;
        if (view != nint.Zero)
        {
            MetalView.Destroy(view);
        }

        _viewHandle = nint.Zero;
        _layerHandle = nint.Zero;

        base.DestroyNativeControlCore(control);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        StartRenderLoop();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopRenderLoop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty)
        {
            if (!OperatingSystem.IsMacOS())
            {
                return;
            }

            UpdateDrawableSize(force: true);
        }
    }

    private void StartRenderLoop()
    {
        if (_renderTimer is not null || _viewHandle == nint.Zero)
        {
            return;
        }

        _currentPixelSize = _defaultPixelSize;
        _currentScaling = 1d;

        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1d / TargetFrameRate)
        };

        _renderTimer.Tick += OnRenderTimerTick;
        _renderTimer.Start();
    }

    private void StopRenderLoop()
    {
        if (_renderTimer is null)
        {
            return;
        }

        _renderTimer.Stop();
        _renderTimer.Tick -= OnRenderTimerTick;
        _renderTimer = null;
    }

    private void OnRenderTimerTick(object? sender, EventArgs e)
    {
        if (_viewHandle == nint.Zero || _layerHandle == nint.Zero)
        {
            return;
        }

        var handler = RenderMetal;
        if (handler is null)
        {
            return;
        }

        if (!UpdateDrawableSize(force: false))
        {
            return;
        }

        if (_currentPixelSize.Width <= 0 || _currentPixelSize.Height <= 0)
        {
            return;
        }

        var drawable = MetalView.NextDrawable(_layerHandle);
        if (drawable == nint.Zero)
        {
            return;
        }

        MetalView.Retain(drawable);

        try
        {
            var texture = MetalView.GetDrawableTexture(drawable);
            if (texture == nint.Zero)
            {
                return;
            }

            var device = MetalView.GetDevice(_layerHandle);
            var args = new MetalRenderEventArgs(
                _viewHandle,
                _layerHandle,
                drawable,
                texture,
                device,
                _currentPixelSize,
                _currentScaling);

            handler(this, args);

            if (!args.Rendered)
            {
                MetalView.PresentDrawable(drawable);
            }
        }
        finally
        {
            MetalView.Release(drawable);
        }
    }

    private bool UpdateDrawableSize(bool force)
    {
        if (_viewHandle == nint.Zero || _layerHandle == nint.Zero)
        {
            return false;
        }

        var bounds = Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return false;
        }

        var scaling = VisualRoot?.RenderScaling ?? 1d;
        var pixelWidth = Math.Max(1, (int)Math.Round(bounds.Width * scaling));
        var pixelHeight = Math.Max(1, (int)Math.Round(bounds.Height * scaling));

        if (!force &&
            pixelWidth == _currentPixelSize.Width &&
            pixelHeight == _currentPixelSize.Height &&
            Math.Abs(scaling - _currentScaling) < 0.0001)
        {
            return true;
        }

        _currentPixelSize = new PixelSize(pixelWidth, pixelHeight);
        _currentScaling = scaling;

        MetalView.SetFrame(_viewHandle, bounds.Width, bounds.Height);
        MetalView.SetContentsScale(_layerHandle, scaling);
        MetalView.SetDrawableSize(_layerHandle, pixelWidth, pixelHeight);

        return true;
    }
}
