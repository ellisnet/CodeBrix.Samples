using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>
/// Verifies <see cref="MetalPlatformSupport"/>'s hardcoded allow-list - the mirror image of the
/// Vulkan gate: Metal is okayed on macOS only, and excluded on every other head (just as Vulkan
/// is excluded on macOS). The loaded-assembly head scan itself can only be observed inside a real
/// head process.
/// </summary>
public class MetalPlatformSupportTests
{
    [Theory]
    [InlineData(PlatformHead.LinuxX11)]
    [InlineData(PlatformHead.LinuxWayland)]
    [InlineData(PlatformHead.Win32Skia)]
    [InlineData(PlatformHead.WinWpfSkia)]
    [InlineData(PlatformHead.LinuxFrameBuffer)]
    [InlineData(PlatformHead.Unknown)]
    public void metal_is_not_okayed_on_any_non_macos_head(PlatformHead head) =>
        MetalPlatformSupport.IsSupported(head).Should().BeFalse();

    [Fact]
    public void metal_is_okayed_on_macos_for_supported_process_architectures()
    {
        //Metal is macOS-only, but also only on the process architectures the raw objc_msgSend
        //interop supports (arm64 / x64 - every realistic test host). Asserting against
        //IsSupportedProcessArchitecture keeps this deterministic on any host.
        MetalPlatformSupport.IsSupported(PlatformHead.MacOS)
            .Should().Be(MetalPlatformSupport.IsSupportedProcessArchitecture);
    }

    [Fact]
    public void a_unit_test_host_detects_no_head_and_is_unsupported()
    {
        //No CodeBrix.Platform head runtime is loaded in the test process
        MetalPlatformSupport.CurrentHead.Should().Be(PlatformHead.Unknown);
        MetalPlatformSupport.IsCurrentPlatformSupported.Should().BeFalse();
    }
}
