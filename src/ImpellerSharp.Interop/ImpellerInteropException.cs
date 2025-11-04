using System;

namespace ImpellerSharp.Interop;

public sealed class ImpellerInteropException : Exception
{
    public ImpellerInteropException(string message)
        : base(message)
    {
    }
}
