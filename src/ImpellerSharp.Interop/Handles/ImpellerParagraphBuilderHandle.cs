using System;
using System.Text;

namespace ImpellerSharp.Interop;

public sealed unsafe class ImpellerParagraphBuilderHandle : ImpellerSafeHandle
{
    private ImpellerParagraphBuilderHandle(nint native)
    {
        SetHandle(native);
    }

    public static ImpellerParagraphBuilderHandle Create(ImpellerTypographyContextHandle context)
    {
        if (context is null || context.IsInvalid)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var addedRef = false;

        try
        {
            context.DangerousAddRef(ref addedRef);
            var native = ImpellerNative.ImpellerParagraphBuilderNew(context.DangerousGetHandle());
            return new ImpellerParagraphBuilderHandle(
                EnsureSuccess(native, "Failed to create Impeller paragraph builder."));
        }
        finally
        {
            if (addedRef)
            {
                context.DangerousRelease();
            }
        }
    }

    internal void Retain()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphBuilderRetain(handle);
    }

    public void PushStyle(ImpellerParagraphStyleHandle style)
    {
        ThrowIfInvalid();
        if (style is null || style.IsInvalid)
        {
            throw new ArgumentNullException(nameof(style));
        }

        ImpellerNative.ImpellerParagraphBuilderPushStyle(handle, style.DangerousGetHandle());
    }

    public void PopStyle()
    {
        ThrowIfInvalid();
        ImpellerNative.ImpellerParagraphBuilderPopStyle(handle);
    }

    public void AddText(string text)
    {
        ThrowIfInvalid();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var length = Encoding.UTF8.GetByteCount(text);
        using var utf8 = new Utf8String(text);
        ImpellerNative.ImpellerParagraphBuilderAddText(handle, (byte*)utf8.Pointer, (uint)length);
    }

    public ImpellerParagraphHandle Build(float width)
    {
        ThrowIfInvalid();
        var native = ImpellerNative.ImpellerParagraphBuilderBuildParagraphNew(handle, width);
        return ImpellerParagraphHandle.FromOwned(native);
    }

    protected override bool ReleaseHandle()
    {
        ImpellerNative.ImpellerParagraphBuilderRelease(handle);
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
            throw new ObjectDisposedException(nameof(ImpellerParagraphBuilderHandle));
        }
    }
}
