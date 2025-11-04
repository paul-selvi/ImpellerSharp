using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

public static unsafe class TextureFactory
{
    private static readonly delegate* unmanaged[Cdecl]<nint, void> ReleaseCallbackPointer = &ReleasePinnedBuffer;

    public static ImpellerTextureHandle CreateTexture(
        this ImpellerContextHandle context,
        in ImpellerTextureDescriptor descriptor,
        ReadOnlySpan<byte> pixelData)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (pixelData.IsEmpty)
        {
            throw new ArgumentException("Texture data must not be empty.", nameof(pixelData));
        }

        var upload = new PinnedTextureUpload(pixelData);
        var userDataHandle = GCHandle.Alloc(upload, GCHandleType.Normal);
        var handoffToNative = false;
        var addedRef = false;

        using var activity = ImpellerDiagnostics.ActivitySource.StartActivity("ImpellerTexture.Upload");

        try
        {
            context.DangerousAddRef(ref addedRef);

            var mapping = new ImpellerMapping
            {
                Data = upload.Pointer,
                Length = (ulong)upload.Length,
                OnRelease = ReleaseCallbackPointer,
            };

            var texturePtr = ImpellerNative.ImpellerTextureCreateWithContentsNew(
                context.DangerousGetHandle(),
                descriptor,
                mapping,
                GCHandle.ToIntPtr(userDataHandle));

            if (texturePtr == nint.Zero)
            {
                throw new ImpellerInteropException("ImpellerTextureCreateWithContentsNew returned null.");
            }

            handoffToNative = true;
            ImpellerDiagnostics.TextureCreated(descriptor);
            return ImpellerTextureHandle.FromOwned(texturePtr);
        }
        finally
        {
            if (addedRef)
            {
                context.DangerousRelease();
            }

            if (!handoffToNative)
            {
                upload.Dispose();
                if (userDataHandle.IsAllocated)
                {
                    userDataHandle.Free();
                }
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void ReleasePinnedBuffer(nint userData)
    {
        if (userData == nint.Zero)
        {
            return;
        }

        var handle = GCHandle.FromIntPtr(userData);
        if (handle.Target is PinnedTextureUpload upload)
        {
            upload.Dispose();
        }

        if (handle.IsAllocated)
        {
            handle.Free();
        }
    }

    private sealed class PinnedTextureUpload : IDisposable
    {
        private readonly byte[] _buffer;
        private readonly GCHandle _pinned;
        private bool _disposed;

        internal PinnedTextureUpload(ReadOnlySpan<byte> source)
        {
            Length = source.Length;
            _buffer = ArrayPool<byte>.Shared.Rent(Length);
            source.CopyTo(_buffer.AsSpan(0, Length));
            _pinned = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        }

        internal int Length { get; }

        internal byte* Pointer => (byte*)_pinned.AddrOfPinnedObject();

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_pinned.IsAllocated)
            {
                _pinned.Free();
            }

            ArrayPool<byte>.Shared.Return(_buffer, clearArray: false);
        }
    }
}
