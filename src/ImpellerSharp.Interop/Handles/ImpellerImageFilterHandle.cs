using System;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerImageFilterHandle : ImpellerSafeHandle
{
    private ImpellerImageFilterHandle(nint native)
    {
        SetHandle(native);
    }

    public static ImpellerImageFilterHandle CreateBlur(float sigmaX, float sigmaY, ImpellerTileMode tileMode)
    {
        var native = ImpellerNative.ImpellerImageFilterCreateBlurNew(sigmaX, sigmaY, tileMode);
        return new ImpellerImageFilterHandle(
            EnsureSuccess(native, "Failed to create Impeller image filter (blur)."));
    }

    public static ImpellerImageFilterHandle CreateDilate(float radiusX, float radiusY)
    {
        var native = ImpellerNative.ImpellerImageFilterCreateDilateNew(radiusX, radiusY);
        return new ImpellerImageFilterHandle(
            EnsureSuccess(native, "Failed to create Impeller image filter (dilate)."));
    }

    public static ImpellerImageFilterHandle CreateErode(float radiusX, float radiusY)
    {
        var native = ImpellerNative.ImpellerImageFilterCreateErodeNew(radiusX, radiusY);
        return new ImpellerImageFilterHandle(
            EnsureSuccess(native, "Failed to create Impeller image filter (erode)."));
    }

    public static ImpellerImageFilterHandle CreateMatrix(in ImpellerMatrix matrix, ImpellerTextureSampling sampling)
    {
        var value = matrix;
        var native = ImpellerNative.ImpellerImageFilterCreateMatrixNew(&value, sampling);
        return new ImpellerImageFilterHandle(
            EnsureSuccess(native, "Failed to create Impeller image filter (matrix)."));
    }

    public static ImpellerImageFilterHandle CreateCompose(
        ImpellerImageFilterHandle outer,
        ImpellerImageFilterHandle inner)
    {
        if (outer is null)
        {
            throw new ArgumentNullException(nameof(outer));
        }

        if (inner is null)
        {
            throw new ArgumentNullException(nameof(inner));
        }

        var native = ImpellerNative.ImpellerImageFilterCreateComposeNew(
            outer.DangerousGetHandle(),
            inner.DangerousGetHandle());

        return new ImpellerImageFilterHandle(
            EnsureSuccess(native, "Failed to create Impeller image filter (compose)."));
    }

    public static ImpellerImageFilterHandle CreateFragmentProgram(
        ImpellerContextHandle context,
        ImpellerFragmentProgramHandle fragmentProgram,
        ReadOnlySpan<ImpellerTextureHandle?> samplers,
        ReadOnlySpan<byte> uniformData)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (fragmentProgram is null)
        {
            throw new ArgumentNullException(nameof(fragmentProgram));
        }

        Span<nint> samplerHandles = samplers.Length <= 16
            ? stackalloc nint[samplers.Length]
            : new nint[samplers.Length];

        for (var i = 0; i < samplers.Length; ++i)
        {
            if (samplers[i] is null)
            {
                throw new ArgumentNullException(nameof(samplers), "Sampler textures must not be null.");
            }

            samplerHandles[i] = samplers[i]!.DangerousGetHandle();
        }

        var addedRef = false;
        nint native = nint.Zero;

        try
        {
            context.DangerousAddRef(ref addedRef);

            fixed (nint* samplerPtr = samplerHandles)
            fixed (byte* dataPtr = uniformData)
            {
                native = ImpellerNative.ImpellerImageFilterCreateFragmentProgramNew(
                    context.DangerousGetHandle(),
                    fragmentProgram.DangerousGetHandle(),
                    samplerPtr,
                    (nuint)samplers.Length,
                    dataPtr,
                    (nuint)uniformData.Length);
            }
        }
        finally
        {
            if (addedRef)
            {
                context.DangerousRelease();
            }
        }

        return new ImpellerImageFilterHandle(
            EnsureSuccess(native, "Failed to create Impeller image filter (fragment program)."));
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerImageFilterRetain(handle);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerImageFilterRelease(handle);
        return true;
    }

    internal new nint DangerousGetHandle()
    {
        ThrowIfInvalid();
        return handle;
    }

    private void ThrowIfInvalid()
    {
        if (IsInvalid)
        {
            throw new ObjectDisposedException(nameof(ImpellerImageFilterHandle));
        }
    }
}
