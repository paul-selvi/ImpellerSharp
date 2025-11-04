using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using AvaloniaImpellerApp.Rendering;
using Avalonia.Controls.Primitives;
using ImpellerSharp.Avalonia.Controls;
using ImpellerSharp.Avalonia.Linux.Controls;
using ImpellerSharp.Avalonia.Mac.Controls;
using ImpellerSharp.Avalonia.Windows.Controls;
using ImpellerSharp.Interop;
using SkiaSharp;

namespace AvaloniaImpellerApp;

public partial class MainWindow : Window
{
    private readonly ImpellerRenderer _impeller = new();
    private MetalNativeHost? _metalHost;
    private ImpellerSkiaView? _skiaView;
    private Slider? _complexitySlider;
    private TextBlock? _complexityLabel;
    private TabControl? _sceneTabs;
    private SceneKind _currentScene = SceneKind.BasicDemo;

    private enum SceneKind
    {
        BasicDemo,
        MotionMark
    }

    public MainWindow()
    {
        InitializeComponent();

        if (this.FindControl<ContentControl>("RenderHost") is { } host)
        {
            if (OperatingSystem.IsMacOS())
            {
                _metalHost = new MetalNativeHost();
                host.Content = _metalHost;
                _metalHost.RenderMetal += OnRenderMetal;
                _metalHost.DetachedFromVisualTree += ViewOnDetachedFromVisualTree;
            }
            else if (OperatingSystem.IsWindows())
            {
                _skiaView = new Win32ImpellerView();
                host.Content = _skiaView;
                _skiaView.RenderSkia += OnRenderSkia;
                _skiaView.DetachedFromVisualTree += ViewOnDetachedFromVisualTree;
            }
            else if (OperatingSystem.IsLinux())
            {
                _skiaView = new LinuxImpellerView();
                host.Content = _skiaView;
                _skiaView.RenderSkia += OnRenderSkia;
                _skiaView.DetachedFromVisualTree += ViewOnDetachedFromVisualTree;
            }
            else
            {
                _skiaView = new ImpellerSkiaView();
                host.Content = _skiaView;
                _skiaView.RenderSkia += OnRenderSkia;
                _skiaView.DetachedFromVisualTree += ViewOnDetachedFromVisualTree;
            }
        }

        _sceneTabs = this.FindControl<TabControl>("SceneTabs");
        if (_sceneTabs is not null)
        {
            _sceneTabs.SelectionChanged += SceneTabsOnSelectionChanged;
            _sceneTabs.SelectedIndex = 0;
        }

        UpdateScene(SceneKind.BasicDemo);

        Closed += (_, _) =>
        {
            if (_complexitySlider is not null)
            {
                _complexitySlider.PropertyChanged -= ComplexitySliderOnPropertyChanged;
            }

            if (_sceneTabs is not null)
            {
                _sceneTabs.SelectionChanged -= SceneTabsOnSelectionChanged;
            }

            _impeller.Dispose();
        };
    }

    private void ComplexitySliderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == RangeBase.ValueProperty && e.NewValue is double value)
        {
            ApplyComplexity(value);
        }
    }

    private void ApplyComplexity(double value)
    {
        var complexity = Math.Clamp((int)Math.Round(value), 0, 24);
        if (_complexityLabel is not null)
        {
            _complexityLabel.Text = _currentScene == SceneKind.MotionMark ? complexity.ToString() : string.Empty;
        }

        _impeller.SetComplexity(complexity);
        RequestFrame();
    }

    private void SceneTabsOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var index = _sceneTabs?.SelectedIndex ?? 0;
        UpdateScene(index == 1 ? SceneKind.MotionMark : SceneKind.BasicDemo);
    }

    private void UpdateScene(SceneKind scene)
    {
        if (_currentScene == scene)
        {
            return;
        }

        _currentScene = scene;
        _impeller.SetScene(scene);

        if (scene == SceneKind.MotionMark)
        {
            EnsureMotionControls();
            if (_complexitySlider is { } slider)
            {
                ApplyComplexity(slider.Value);
            }
        }
        else if (_complexityLabel is not null)
        {
            _complexityLabel.Text = string.Empty;
        }

        RequestFrame();
    }

    private void EnsureMotionControls()
    {
        if (_complexitySlider is null)
        {
            var slider = this.FindControl<Slider>("ComplexitySlider");
            if (slider is not null)
            {
                _complexitySlider = slider;
                slider.PropertyChanged += ComplexitySliderOnPropertyChanged;
            }
        }

        _complexityLabel ??= this.FindControl<TextBlock>("ComplexityValue");
    }

    private void RequestFrame()
    {
        _skiaView?.InvalidateVisual();
    }

    private void OnRenderMetal(object? sender, MetalRenderEventArgs e)
    {
        var rendered = _impeller.TryRenderMetal(e);
        if (rendered)
        {
            return;
        }
    }

    private void OnRenderSkia(object? sender, ImpellerSkiaRenderEventArgs e)
    {
        var deviceMessage = e.MetalDevice is not null
            ? "Metal context"
            : e.VulkanContext is not null
                ? "Vulkan context"
                : "Software / OpenGL";

        var rendered = _impeller.TryRenderSkia(e);

        e.ResetPlatformLease();

        if (rendered)
        {
            using var overlayPaint = new SKPaint { IsAntialias = true, Color = SKColors.White, TextSize = 18 };
            e.Canvas.DrawText("Impeller", 32, 48, overlayPaint);
            return;
        }

        var canvas = e.Canvas;
        canvas.Clear(SKColors.Black);

        using var paint = new SKPaint { IsAntialias = true };

        paint.Color = SKColors.DeepSkyBlue;
        canvas.DrawRoundRect(40, 40, 300, 200, 24, 24, paint);

        paint.Color = SKColors.White;
        paint.TextSize = 24;
        canvas.DrawText("Avalonia + Skia lease", 60, 90, paint);

        paint.TextSize = 16;
        canvas.DrawText(deviceMessage, 60, 120, paint);

        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 4;
        paint.Color = SKColors.Orange;
        canvas.DrawCircle(200, 200, 60, paint);
    }

    private void ViewOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _impeller.Dispose();
    }

    private sealed class ImpellerRenderer : IDisposable
    {
        private ImpellerContextHandle? _context;
        private SceneKind _scene = SceneKind.BasicDemo;
        private readonly Channel<WorkerMessage> _workerChannel = Channel.CreateUnbounded<WorkerMessage>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });
        private readonly Task _workerTask;
        private ImpellerDisplayListHandle? _currentDisplayList;
        private ImpellerDisplayListHandle? _nextDisplayList;

        public ImpellerRenderer()
        {
            _workerTask = Task.Run(() => RunWorkerAsync(_workerChannel.Reader));
        }

        public void SetComplexity(int complexity)
        {
            _workerChannel.Writer.TryWrite(WorkerMessage.ForComplexity(complexity));
        }

        public void SetScene(SceneKind scene)
        {
            _scene = scene;
            _workerChannel.Writer.TryWrite(WorkerMessage.ForScene(scene));
        }

        public bool TryRenderSkia(ImpellerSkiaRenderEventArgs args)
        {
            if (!args.TryGetMetalRenderTarget(out var info))
            {
                return false;
            }

            EnsureContext();

            if (_context is null)
            {
                return false;
            }

            try
            {
                using var surface = ImpellerSurfaceHandle.WrapMetalTexture(_context, info.Texture);

                var width = MathF.Max(1f, info.Size.Width);
                var height = MathF.Max(1f, info.Size.Height);

                return RenderSurface(surface, width, height);
            }
            catch (ImpellerInteropException ex)
            {
                Console.WriteLine($"[Impeller] Failed to wrap Skia metal texture: {ex.Message}");
                return false;
            }
        }

        public bool TryRenderMetal(MetalRenderEventArgs args)
        {
            EnsureContext();

            if (_context is null)
            {
                return false;
            }

            try
            {
                using var surface = ImpellerSurfaceHandle.WrapMetalDrawable(_context, args.Drawable);

                var width = MathF.Max(1f, args.PixelSize.Width);
                var height = MathF.Max(1f, args.PixelSize.Height);

                var rendered = RenderSurface(surface, width, height);
                if (rendered)
                {
                    args.MarkRendered();
                }

                return rendered;
            }
            catch (ImpellerInteropException ex)
            {
                Console.WriteLine($"[Impeller] Failed to wrap CAMetalDrawable: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _workerChannel.Writer.TryWrite(WorkerMessage.Stop());
            _workerChannel.Writer.TryComplete();
            try
            {
                _workerTask.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Impeller] Render worker shutdown error: {ex}");
            }

            _context?.Dispose();
            _context = null;
            _currentDisplayList?.Dispose();
            _currentDisplayList = null;

            var pending = Interlocked.Exchange(ref _nextDisplayList, null);
            pending?.Dispose();
        }

        private void EnsureContext()
        {
            _context ??= ImpellerContextHandle.CreateMetal();
        }

        private bool RenderSurface(ImpellerSurfaceHandle surface, float width, float height)
        {
            if (width <= 0f || height <= 0f)
            {
                RequestRender(width, height);
                return false;
            }

            RequestRender(width, height);
            UpdateDisplayList();

            var displayList = _currentDisplayList;
            if (displayList is null || displayList.IsInvalid)
            {
                return false;
            }

            if (!surface.DrawDisplayList(displayList))
            {
                return false;
            }

            return surface.Present();
        }

        private void RequestRender(float width, float height)
        {
            _workerChannel.Writer.TryWrite(WorkerMessage.ForRender(width, height, _scene));
        }

        private void UpdateDisplayList()
        {
            var next = Interlocked.Exchange(ref _nextDisplayList, null);
            if (next is null)
            {
                return;
            }

            _currentDisplayList?.Dispose();
            _currentDisplayList = next;
        }

        private async Task RunWorkerAsync(ChannelReader<WorkerMessage> reader)
        {
            try
            {
                using var basicScene = new BasicDemoScene();
                using var motionScene = new MotionMarkSimulation();
                IImpellerScene activeScene = basicScene;
                var activeKind = SceneKind.BasicDemo;

                var lastWidth = 0f;
                var lastHeight = 0f;
                var hasSize = false;
                var rebuildRequested = false;

                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var message))
                    {
                        switch (message.Kind)
                        {
                            case WorkerMessageKind.Render:
                                lastWidth = message.Width;
                                lastHeight = message.Height;
                                hasSize = lastWidth > 0f && lastHeight > 0f;
                                if (message.Scene != activeKind)
                                {
                                    activeKind = message.Scene;
                                    activeScene = activeKind == SceneKind.MotionMark
                                        ? (IImpellerScene)motionScene
                                        : basicScene;
                                }
                                rebuildRequested = true;
                                break;

                            case WorkerMessageKind.Complexity:
                                motionScene.SetComplexity(message.Complexity);
                                if (activeKind == SceneKind.MotionMark && hasSize)
                                {
                                    rebuildRequested = true;
                                }
                                break;

                            case WorkerMessageKind.Scene:
                                activeKind = message.Scene;
                                activeScene = activeKind == SceneKind.MotionMark
                                    ? (IImpellerScene)motionScene
                                    : basicScene;
                                if (hasSize)
                                {
                                    rebuildRequested = true;
                                }
                                break;

                            case WorkerMessageKind.Stop:
                                PublishDisplayList(null);
                                return;
                        }
                    }

                    if (!rebuildRequested)
                    {
                        continue;
                    }

                    rebuildRequested = false;

                    if (!hasSize)
                    {
                        PublishDisplayList(null);
                        continue;
                    }

                    var cullRect = new ImpellerRect(0f, 0f, lastWidth, lastHeight);
                    using var builder = ImpellerDisplayListBuilderHandle.Create(cullRect);

                    var rendered = false;
                    try
                    {
                        rendered = activeScene.Render(builder, lastWidth, lastHeight);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Impeller] Scene render failed: {ex}");
                    }

                    if (!rendered)
                    {
                        PublishDisplayList(null);
                        continue;
                    }

                    var displayList = builder.Build();
                    PublishDisplayList(displayList);
                }
            }
            catch (ChannelClosedException)
            {
                PublishDisplayList(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Impeller] Render worker crashed: {ex}");
                PublishDisplayList(null);
            }
        }

        private void PublishDisplayList(ImpellerDisplayListHandle? displayList)
        {
            var previous = Interlocked.Exchange(ref _nextDisplayList, displayList);
            previous?.Dispose();
        }

        private readonly struct WorkerMessage
        {
            private WorkerMessage(WorkerMessageKind kind, float width, float height, int complexity, SceneKind scene)
            {
                Kind = kind;
                Width = width;
                Height = height;
                Complexity = complexity;
                Scene = scene;
            }

            public WorkerMessageKind Kind { get; }
            public float Width { get; }
            public float Height { get; }
            public int Complexity { get; }
            public SceneKind Scene { get; }

            public static WorkerMessage ForRender(float width, float height, SceneKind scene) =>
                new(WorkerMessageKind.Render, width, height, 0, scene);

            public static WorkerMessage ForComplexity(int complexity) =>
                new(WorkerMessageKind.Complexity, 0f, 0f, complexity, SceneKind.BasicDemo);

            public static WorkerMessage ForScene(SceneKind scene) =>
                new(WorkerMessageKind.Scene, 0f, 0f, 0, scene);

            public static WorkerMessage Stop() =>
                new(WorkerMessageKind.Stop, 0f, 0f, 0, SceneKind.BasicDemo);
        }

        private enum WorkerMessageKind : byte
        {
            Render,
            Complexity,
            Scene,
            Stop
        }
    }
}
