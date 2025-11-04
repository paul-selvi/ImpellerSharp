# Developer Documentation Outline (M6 Task 6.3)

## 1. Getting Started
- Prerequisites: native Impeller SDK, .NET 8 or later, platform SDKs (Metal/Vulkan).
- Project setup: referencing `ImpellerSharp.Interop`, platform-specific native binaries.
- First draw: minimal code sample creating context, texture, display list, surface draw.

## 2. Threading Model
- Context creation and thread safety.
- Command buffer encoding (single-thread restriction, multi-buffer patterns).
- Background uploads and synchronization guidelines.

## 3. Memory Management
- SafeHandle semantics, Retain/Release conventions.
- Texture uploads (pinned spans vs. eager copy).
- Resource disposal patterns and strict mode.

## 4. Shader & Pipeline Updates
- Using `ImpellerFragmentProgramNew`.
- Managing shader warm-up and caching.
- Handling runtime shader blobs with `RuntimeStage`.

## 5. Diagnostics & Profiling
- EventSource/ActivitySource integration.
- Benchmarks & profiling workflows (link to Benchmarking Plan).
- Logging/telemetry best practices.

## 6. Troubleshooting
- Version negotiation failures.
- Threading violations.
- Common error codes/null returns.
