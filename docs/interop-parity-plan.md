# ImpellerSharp Interop Parity Plan

1. [x] Build an automated coverage report that parses `impeller.h`, compares native exports to managed P/Invoke signatures, and refreshes `docs/api-summary.md` whenever drift is detected.
2. [x] Add missing interop signatures for Vulkan swapchain, display-list advanced draws, path utilities, paint miter setter, and paragraph metrics in the `ImpellerNative.*.cs` files.
3. [x] Introduce managed wrappers for the new interop points (for example `ImpellerVulkanSwapchainHandle` plus extended builder/paint/paragraph helpers) so higher-level code can access the added functionality.
4. [x] Update samples and tests to exercise the new APIs (rounded-rect drawing, paragraph metric queries, Vulkan swapchain acquisition) and ensure they run cleanly across supported platforms.
5. [ ] Verify native packaging includes the `ImpellerSurfaceCreateWrappedMetalTextureNew` shim or replace it with an upstream-supported alternative to avoid missing exports at runtime.
6. [ ] Re-run the coverage script, commit the refreshed `docs/api-summary.md`, and wire the script into CI to block regressions when the upstream Impeller header changes.

> Run `python3 build/scripts/generate_api_summary.py` to regenerate the coverage report after modifying bindings.
