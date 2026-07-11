using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>
/// Verifies <see cref="VulkanPlatformSupport"/>'s hardcoded allow-list and its head-runtime
/// assembly-name classification (the pure parts; the loaded-assembly scan itself can only be
/// observed inside a real head process).
/// </summary>
public class VulkanPlatformSupportTests
{
    [Theory]
    [InlineData(PlatformHead.LinuxX11, true)]
    [InlineData(PlatformHead.LinuxWayland, true)]
    [InlineData(PlatformHead.Win32Skia, true)]
    [InlineData(PlatformHead.WinWpfSkia, true)]
    [InlineData(PlatformHead.MacOS, false)]
    [InlineData(PlatformHead.LinuxFrameBuffer, false)]
    [InlineData(PlatformHead.Unknown, false)]
    public void hardcoded_allow_list_okays_exactly_the_four_approved_heads(PlatformHead head, bool expected) =>
        VulkanPlatformSupport.IsSupported(head).Should().Be(expected);

    [Theory]
    [InlineData("CodeBrix.Platform.UI.Runtime.Skia.X11", PlatformHead.LinuxX11)]
    [InlineData("CodeBrix.Platform.UI.Runtime.Skia.Wayland", PlatformHead.LinuxWayland)]
    [InlineData("CodeBrix.Platform.UI.Runtime.Skia.Linux.FrameBuffer", PlatformHead.LinuxFrameBuffer)]
    [InlineData("CodeBrix.Platform.UI.Runtime.Skia.MacOS", PlatformHead.MacOS)]
    [InlineData("CodeBrix.Platform.UI.Runtime.Skia.Win32", PlatformHead.Win32Skia)]
    [InlineData("CodeBrix.Platform.UI.Runtime.Skia.Win32.Support", PlatformHead.Win32Skia)]
    [InlineData("CodeBrix.Platform.UI.Runtime.Skia.Wpf", PlatformHead.WinWpfSkia)]
    public void head_runtime_assembly_names_classify_to_their_heads(string assemblyName, PlatformHead expected) =>
        VulkanPlatformSupport.ClassifyAssemblyName(assemblyName).Should().Be(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("PolyHavenBrowser.Rendering")]
    [InlineData("CodeBrix.Platform.UI")]
    [InlineData("CodeBrix.Platform.UI.Runtime.Skia.SomethingElse")]
    public void non_head_assembly_names_classify_as_unknown(string? assemblyName) =>
        VulkanPlatformSupport.ClassifyAssemblyName(assemblyName).Should().Be(PlatformHead.Unknown);

    [Fact]
    public void a_unit_test_host_detects_no_head_and_is_unsupported()
    {
        //No CodeBrix.Platform head runtime is loaded in the test process
        VulkanPlatformSupport.CurrentHead.Should().Be(PlatformHead.Unknown);
        VulkanPlatformSupport.IsCurrentPlatformSupported.Should().BeFalse();
    }
}
