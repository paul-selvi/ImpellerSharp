using System;

namespace ImpellerSharp.Interop.Hosting;

/// <summary>
/// Options used to configure <see cref="MetalGlfwAppHost"/>.
/// </summary>
public sealed class MetalGlfwHostOptions
{
    /// <summary>
    /// Gets or sets the window title.
    /// </summary>
    public string Title { get; set; } = "Impeller Application";

    /// <summary>
    /// Gets or sets the initial window width in logical pixels.
    /// </summary>
    public int Width { get; set; } = 1280;

    /// <summary>
    /// Gets or sets the initial window height in logical pixels.
    /// </summary>
    public int Height { get; set; } = 720;

    /// <summary>
    /// Gets or sets a callback invoked when GLFW reports an error.
    /// Defaults to writing to <see cref="Console.Error"/>.
    /// </summary>
    public Action<string>? ErrorLogger { get; set; }
}
