using SilverAssertions;
using Xunit;

namespace DRAKON.Brix.TclBridge.Tests;

public class BasicTests
{
    [Fact]
    public void can_run_tests()
    {
        //Arrange
        var isRunning = true;

        //Assert
        isRunning.Should().Be(true);
    }
}
