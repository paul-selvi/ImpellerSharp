using System;

namespace ImpellerSharp.Interop;

public static class ImpellerInteropOptions
{
    private const string StrictEnvironmentVariable = "IMPELLER_INTEROP_STRICT";
    private const string StrictAppContextSwitch = "ImpellerSharp.Interop.StrictMode";
    private static ImpellerInteropConfiguration _configuration = ImpellerInteropConfiguration.Default;

    public static ImpellerInteropConfiguration Configuration => _configuration;

    static ImpellerInteropOptions()
    {
        if (AppContext.TryGetSwitch(StrictAppContextSwitch, out var strict) && strict)
        {
            EnableStrictMode();
            return;
        }

        var env = Environment.GetEnvironmentVariable(StrictEnvironmentVariable);
        if (!string.IsNullOrEmpty(env) && IsTruthy(env))
        {
            EnableStrictMode();
        }
    }

    public static void Configure(ImpellerInteropConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static void EnableStrictMode()
    {
        _configuration = _configuration with { StrictMode = true };
    }

    private static bool IsTruthy(string value)
    {
        return value.Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }
}

public readonly record struct ImpellerInteropConfiguration(bool StrictMode, bool ValidateThreadAffinity)
{
    public static ImpellerInteropConfiguration Default => new(false, false);
}
