using System;
using System.Diagnostics;
using System.IO;

namespace ImpellerSharp.Samples.BasicShapes;

internal static class MacCapture
{
    private const string CaptureTool = "/usr/sbin/screencapture";

    internal static void CaptureWindow(nuint windowNumber, string destinationPath)
    {
        if (!OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("Window capture is only supported on macOS.");
        }

        if (windowNumber == 0)
        {
            throw new ArgumentException("Window number must be non-zero for capture.", nameof(windowNumber));
        }

        if (!File.Exists(CaptureTool))
        {
            throw new FileNotFoundException("macOS screencapture utility not found.", CaptureTool);
        }

        var fullPath = Path.GetFullPath(destinationPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = CaptureTool,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            }
        };

        process.StartInfo.ArgumentList.Add("-x");
        process.StartInfo.ArgumentList.Add("-o");
        process.StartInfo.ArgumentList.Add("-l");
        process.StartInfo.ArgumentList.Add(windowNumber.ToString());
        process.StartInfo.ArgumentList.Add(fullPath);

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException($"screencapture failed with exit code {process.ExitCode}: {error}");
        }
    }
}
