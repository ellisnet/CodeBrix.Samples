using DRAKON.Brix.Drakon;
using SilverAssertions;
using Xunit;

namespace DRAKON.Brix.TclBridge.Tests;

/// <summary>
/// Cheap guard on the runtime's own lifecycle: constructing it must acquire
/// nothing, so disposing one that was never started has to be harmless. The
/// coverage of the full boot and file-open path lives in
/// <see cref="DrnFileOpenTests"/>.
/// </summary>
public class DrakonRuntimeTests
{
    [Fact]
    public void Construct_and_dispose_without_starting_is_safe()
    {
        //Arrange
        var runtime = new DrakonRuntime();

        //Act
        runtime.Dispose();

        //Assert
        runtime.Should().NotBeNull();
    }
}
