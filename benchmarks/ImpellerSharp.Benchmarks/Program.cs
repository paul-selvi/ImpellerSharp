using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Running;

namespace ImpellerSharp.Benchmarks;

internal static class Program
{
    private static void Main(string[] args)
    {
        var artifactsPath = Path.Combine(AppContext.BaseDirectory, "BenchmarkArtifacts");
        Directory.CreateDirectory(artifactsPath);

        var config = ManualConfig
            .Create(DefaultConfig.Instance)
            .WithArtifactsPath(artifactsPath)
            .AddExporter(MarkdownExporter.GitHub, JsonExporter.Default)
            .WithOption(ConfigOptions.DisableOptimizationsValidator, true);

        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, config);
    }
}
