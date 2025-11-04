using System.Runtime.InteropServices;

namespace ImpellerSharp.Interop;

internal static partial class ImpellerNative
{
    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerTypographyContextNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerTypographyContextNew();

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerTypographyContextRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerTypographyContextRetain(nint context);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerTypographyContextRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerTypographyContextRelease(nint context);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerTypographyContextRegisterFont")]
    [SuppressGCTransition]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static unsafe partial bool ImpellerTypographyContextRegisterFont(
        nint context,
        in ImpellerMapping mapping,
        nint userData,
        byte* familyAlias);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerParagraphStyleNew();

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleRetain(nint paragraphStyle);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleRelease(nint paragraphStyle);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetForeground")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleSetForeground(nint paragraphStyle, nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetBackground")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleSetBackground(nint paragraphStyle, nint paint);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetFontWeight")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleSetFontWeight(nint paragraphStyle, ImpellerFontWeight weight);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetFontStyle")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleSetFontStyle(nint paragraphStyle, ImpellerFontStyle style);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetFontFamily")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerParagraphStyleSetFontFamily(nint paragraphStyle, byte* familyName);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetFontSize")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleSetFontSize(nint paragraphStyle, float size);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetHeight")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleSetHeight(nint paragraphStyle, float height);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetTextAlignment")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleSetTextAlignment(nint paragraphStyle, ImpellerTextAlignment alignment);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetTextDirection")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleSetTextDirection(nint paragraphStyle, ImpellerTextDirection direction);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetTextDecoration")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerParagraphStyleSetTextDecoration(nint paragraphStyle, ImpellerTextDecoration* decoration);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetMaxLines")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphStyleSetMaxLines(nint paragraphStyle, uint maxLines);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetLocale")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerParagraphStyleSetLocale(nint paragraphStyle, byte* locale);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphStyleSetEllipsis")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerParagraphStyleSetEllipsis(nint paragraphStyle, byte* ellipsis);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphBuilderNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerParagraphBuilderNew(nint typographyContext);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphBuilderRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphBuilderRetain(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphBuilderRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphBuilderRelease(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphBuilderPushStyle")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphBuilderPushStyle(nint builder, nint paragraphStyle);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphBuilderPopStyle")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphBuilderPopStyle(nint builder);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphBuilderAddText")]
    [SuppressGCTransition]
    internal static unsafe partial void ImpellerParagraphBuilderAddText(nint builder, byte* data, uint length);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphBuilderBuildParagraphNew")]
    [SuppressGCTransition]
    internal static partial nint ImpellerParagraphBuilderBuildParagraphNew(nint builder, float width);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphRetain")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphRetain(nint paragraph);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphRelease")]
    [SuppressGCTransition]
    internal static partial void ImpellerParagraphRelease(nint paragraph);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphGetMaxWidth")]
    [SuppressGCTransition]
    internal static partial float ImpellerParagraphGetMaxWidth(nint paragraph);

    [LibraryImport(ImpellerLibrary.Name, EntryPoint = "ImpellerParagraphGetHeight")]
    [SuppressGCTransition]
    internal static partial float ImpellerParagraphGetHeight(nint paragraph);
}
