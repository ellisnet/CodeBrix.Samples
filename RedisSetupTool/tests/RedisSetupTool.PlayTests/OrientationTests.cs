using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace RedisSetupTool.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_container_detail_fits_the_screen()
    {
        await SelectContainerAsync("cache-primary");
        foreach (var action in new[] { "Start", "Stop", "Kill", "Copy id", "Remove", "Advisor" })
        {
            var box = await Button(action).BoundingBoxAsync();
            box.Should().NotBeNull();
            (box.X + box.Width).Should().BeLessThanOrEqualTo(Fixture.Application.Width);
        }
        await SnapshotAsync("RedisSetupTool-portrait-container");
    }
}
