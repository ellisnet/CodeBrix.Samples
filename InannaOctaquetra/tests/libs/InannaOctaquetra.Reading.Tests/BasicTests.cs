// BasicTests.cs - proves the test host itself runs, as every test project in the family does.

using SilverAssertions;
using Xunit;

namespace InannaOctaquetra.Reading.Tests;

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
