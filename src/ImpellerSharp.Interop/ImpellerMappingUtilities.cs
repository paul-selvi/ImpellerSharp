using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static unsafe class ImpellerMappingUtilities
{
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void ReleaseAllocatedMapping(nint userData)
    {
        if (userData != 0)
        {
            NativeMemory.Free((void*)userData);
        }
    }

    public static ImpellerMapping Create(ReadOnlySpan<byte> data, out nint userData)
    {
        if (data.IsEmpty)
        {
            userData = nint.Zero;
            return new ImpellerMapping
            {
                Data = null,
                Length = 0,
                OnRelease = null,
            };
        }

        var buffer = (byte*)NativeMemory.Alloc((nuint)data.Length);
        data.CopyTo(new Span<byte>(buffer, data.Length));

        userData = (nint)buffer;

        return new ImpellerMapping
        {
            Data = buffer,
            Length = (ulong)data.Length,
            OnRelease = &ReleaseAllocatedMapping,
        };
    }

    public static void Free(nint userData)
    {
        if (userData != 0)
        {
            NativeMemory.Free((void*)userData);
        }
    }
}
