using System;

namespace ImpellerSharp.Interop;

[Flags]
public enum ImpellerTextDecorationType
{
    None = 0,
    Underline = 1 << 0,
    Overline = 1 << 1,
    LineThrough = 1 << 2,
}
