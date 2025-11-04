# Compatibility Matrix (M6 Task 6.4)

| Platform | Backend | Native Binary | Managed Runtime | Status | Notes |
| --- | --- | --- | --- | --- | --- |
| macOS 14 (arm64) | Metal | libimpeller.dylib | .NET 8 (CoreCLR) | Planned | Primary dev environment |
| macOS 14 (arm64) | Vulkan (MoltenVK) | libimpeller_vulkan.dylib | .NET 8 | Planned | Requires MoltenVK install |
| Windows 11 (x64) | Vulkan | impeller.dll | .NET 8 | Planned | Needs GN Windows build |
| Linux (Ubuntu 22.04) | Vulkan | libimpeller.so | .NET 8 | Planned | CI target |
| Windows 11 (x64) | OpenGL ES (ANGLE) | impeller.dll | .NET 8 | Stretch | Evaluate demand |
| macOS 14 (arm64) | Metal | libimpeller.dylib | .NET NativeAOT | Planned | Packaging considerations |
| Windows/Linux | Vulkan | impeller.* | .NET NativeAOT | Planned | Validate P/Invoke compatibility |
| macOS/Windows | Metal/Vulkan | -- | Unity (Mono) | Stretch | Requires IL2CPP bindings |

Legend: Planned = roadmap target; Stretch = optional future support.
