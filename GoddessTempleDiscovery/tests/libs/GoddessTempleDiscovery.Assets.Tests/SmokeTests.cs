using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Assets.Tests;

public class SmokeTests
{
    [Fact]
    public void Test_project_builds_and_runs() => (1 + 1).Should().Be(2);
}
