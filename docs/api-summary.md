# Impeller Interop API Coverage

_Last updated: 2025-11-05_

This document tracks how the managed layer (`ImpellerSharp.Interop`) maps onto
the Impeller toolkit C ABI exposed from
`impeller/toolkit/interop/impeller.h`. Use it to identify finished bindings,
partial coverage, and the remaining native entry points that still need
wrappers.

## Coverage Snapshot

- Upstream entry points (from header): 176
- Managed P/Invokes declared today: 158
- Remaining gap: 19

> The managed layer also declares
> `ImpellerSurfaceCreateWrappedMetalTextureNew`, a helper shipped in our native
> shim that is not currently part of the upstream header.

## Legend

- ✅ Implemented in `ImpellerSharp.Interop`
- ⚠️ Partially implemented (some entry points still missing)
- ⏳ Not yet surfaced in managed code

## Core Runtime

| Native API | Status | Notes |
| --- | --- | --- |
| `ImpellerGetVersion` | ✅ | Exposed via `ImpellerNative.Core`. |
| Context creation (`ImpellerContextCreateMetal/OpenGLES/VulkanNew`) | ✅ | Factory helpers on `ImpellerContextHandle`. |
| `ImpellerContextRetain/Release` | ✅ | Managed by `ImpellerSafeHandle`. |
| `ImpellerContextGetVulkanInfo` | ✅ | Available through `TryGetVulkanInfo`. |

## Vulkan Swapchain

| Native API | Status | Notes |
| --- | --- | --- |
| `ImpellerVulkanSwapchainCreateNew` | ⏳ | Swapchain helpers are not yet bound. |
| `ImpellerVulkanSwapchainRetain/Release` | ⏳ | Add `ImpellerVulkanSwapchainHandle`. |
| `ImpellerVulkanSwapchainAcquireNextSurfaceNew` | ⏳ | Required for full Vulkan WSI. |

## Surfaces & Textures

| Native API | Status | Notes |
| --- | --- | --- |
| Surface lifecycle (`Retain/Release`) | ✅ | Covered by `ImpellerSurfaceHandle`. |
| `ImpellerSurfaceCreateWrappedMetalDrawableNew` | ✅ | Wraps CAMetalDrawable. |
| `ImpellerSurfaceCreateWrappedFBONew` | ✅ | OpenGL FBO binding via `WrapFramebuffer`. |
| `ImpellerSurfaceDrawDisplayList` / `Present` | ✅ | Draw + present exposed on the handle. |
| `ImpellerSurfaceCreateWrappedMetalTextureNew` | ✅ | Custom shim helper (not in upstream header). |
| Texture lifecycle (`CreateWithContentsNew`, `CreateWithOpenGLTextureHandleNew`, retain/release/get handle) | ✅ | `ImpellerTextureHandle` maps creation helpers and GL interop. |

## Display Lists

| Native API | Status | Notes |
| --- | --- | --- |
| Display list retain/release | ✅ | `ImpellerDisplayListHandle`. |
| Builder lifecycle (`New`, retain/release, `CreateDisplayListNew`) | ✅ | `ImpellerDisplayListBuilderHandle.Create/Build`. |
| State stack (`Save`, `SaveLayer`, `Restore`) | ✅ | Full coverage including optional paint/backdrop filters. |
| Transform helpers (`Translate`, `Scale`, `Rotate`, `Transform`, `Set/Get/ResetTransform`, `GetSaveCount`, `RestoreToCount`) | ✅ | All matrix helpers wired up. |
| Clipping (`ClipRect`, `ClipOval`, `ClipRoundedRect`, `ClipPath`) | ✅ | Uses `ImpellerRoundingRadii` and `ImpellerClipOperation`. |
| Primitive draws (`DrawPaint`, `DrawLine`, `DrawDashedLine`, `DrawRect`, `DrawPath`, `DrawTexture`, `DrawTextureRect`, `DrawParagraph`) | ✅ | Exposed via strongly-typed helpers. |
| Advanced draws (`DrawOval`, `DrawRoundedRect`, `DrawRoundedRectDifference`, `DrawDisplayList`, `DrawShadow`) | ⏳ | Still need managed wrappers. |

## Paths

| Native API | Status | Notes |
| --- | --- | --- |
| Path lifecycle (`Retain/Release`) | ✅ | Implemented by `ImpellerPathHandle`. |
| `ImpellerPathGetBounds` | ⏳ | No managed accessor yet. |
| Path builder basic verbs (`MoveTo`, `LineTo`, `QuadraticCurveTo`, `CubicCurveTo`, `TakePath`) | ✅ | Available on `ImpellerPathBuilderHandle`. |
| Path builder convenience helpers (`AddRect`, `AddArc`, `AddOval`, `AddRoundedRect`, `Close`, `CopyPathNew`) | ⏳ | Needed for parity with native API. |

## Paint & Rendering State

| Native API | Status | Notes |
| --- | --- | --- |
| Paint lifecycle (`New`, retain/release) | ✅ | `ImpellerPaintHandle`. |
| Core paint state (`SetColor`, `SetBlendMode`, `SetDrawStyle`, `SetStrokeCap`, `SetStrokeJoin`, `SetStrokeWidth`) | ✅ | All mapped to setters. |
| `ImpellerPaintSetStrokeMiter` | ⏳ | Add setter to expose miter limit. |
| `ImpellerPaintSetColorSource/ColorFilter/ImageFilter/MaskFilter` | ✅ | Bound via handle accessors. |
| Color sources (`CreateLinear/Radial/Conical/SweepGradientNew`, `CreateImageNew`, `CreateFragmentProgramNew`) | ✅ | All gradients, image, and fragment-program sources supported. |
| Color filters (`CreateBlendNew`, `CreateColorMatrixNew`) | ✅ | `ImpellerColorFilterHandle`. |
| Mask filters (`CreateBlurNew`) | ✅ | `ImpellerMaskFilterHandle`. |
| Image filters (`CreateBlur/Dilate/Erode/Matrix/FragmentProgram/Compose`) | ✅ | All variants mapped. |
| Fragment programs (`New`, retain/release) | ✅ | `ImpellerFragmentProgramHandle`. |

## Typography

| Native API | Status | Notes |
| --- | --- | --- |
| Typography context (`New`, retain/release, `RegisterFont`) | ✅ | Lifetime helpers plus span-friendly font registration. |
| Paragraph style (`New`, retain/release, setters for foreground/background/font weight & style/family/size/height/alignment/direction/decoration/max lines/locale/ellipsis) | ✅ | All documented setters available. |
| Paragraph builder (`New`, retain/release, `PushStyle`, `PopStyle`, `AddText`, `BuildParagraphNew`) | ✅ | Text addition marshals via `Utf8String`. |
| Paragraph metrics (`GetMaxWidth`, `GetHeight`, `GetMaxIntrinsicWidth`, `GetIdeographicBaseline`, `GetAlphabeticBaseline`, `GetLineCount`, `GetWordBoundary`, `GetLineMetrics`, glyph queries) | ✅ | Includes glyph info helpers. |
| `ImpellerParagraphGetMinIntrinsicWidth`, `ImpellerParagraphGetLongestLineWidth` | ⏳ | Pending managed exposure. |
| Line metrics accessors (`GetUnscaledAscent`, `GetAscent`, `GetDescent`, `GetBaseline`, `IsHardbreak`, `GetWidth`, `GetHeight`, `GetLeft`, code-unit index helpers) | ✅ | Wrapped by `ImpellerLineMetricsHandle`. |
| Glyph info accessors (range, bounds, ellipsis flag, text direction) | ✅ | `ImpellerGlyphInfoHandle`. |

## Outstanding Gaps / Next Steps

- **Display lists:** add wrappers for `ImpellerDisplayListBuilderDrawOval`, `DrawRoundedRect`, `DrawRoundedRectDifference`, `DrawDisplayList`, and `DrawShadow`.
- **Path builder:** implement `ImpellerPathBuilderAddRect`, `AddArc`, `AddOval`, `AddRoundedRect`, `Close`, and `CopyPathNew`.
- **Path utilities:** surface `ImpellerPathGetBounds` for callers that need geometry introspection.
- **Paint:** expose `ImpellerPaintSetStrokeMiter`.
- **Paragraph metrics:** add `ImpellerParagraphGetMinIntrinsicWidth` and `GetLongestLineWidth`.
- **Vulkan swapchain:** introduce a managed `ImpellerVulkanSwapchainHandle` covering `CreateNew`, `Retain`, `Release`, and `AcquireNextSurfaceNew`.

Keep this document updated alongside changes in either the upstream Impeller header
or the bindings under `src/ImpellerSharp.Interop`. When new native entry points
arrive, record them here with their coverage status.
