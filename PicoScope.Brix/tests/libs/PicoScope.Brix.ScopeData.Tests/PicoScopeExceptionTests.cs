using System;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class PicoScopeExceptionTests
{
    [Fact]
    public void message_names_the_driver_call_and_the_device_error_code()
    {
        //Act
        var ex = new PicoScopeException("Capture failed", "ps2000_run_block", "3");

        //Assert
        ex.Message.Should().Be("Capture failed (driver call: ps2000_run_block) (device error code: 3)");
        ex.DriverFunction.Should().Be("ps2000_run_block");
        ex.DeviceErrorCode.Should().Be("3");
    }

    [Fact]
    public void a_zero_error_code_is_left_out_of_the_message()
    {
        new PicoScopeException("Capture failed", "ps2000_run_block", "0").Message
            .Should().Be("Capture failed (driver call: ps2000_run_block)");
    }

    [Fact]
    public void plain_constructors_keep_the_message_and_inner_exception()
    {
        //Arrange
        var inner = new InvalidOperationException("inner");

        //Act
        var plain = new PicoScopeException("plain");
        var wrapped = new PicoScopeException("wrapped", inner);

        //Assert
        plain.Message.Should().Be("plain");
        plain.DriverFunction.Should().BeNull();
        ReferenceEquals(wrapped.InnerException, inner).Should().BeTrue();
    }

    [Fact]
    public void ScopeNotOpenException_says_what_was_attempted()
    {
        var ex = new ScopeNotOpenException("run a block capture");
        ex.Message.Should().Contain("run a block capture");
        (ex is PicoScopeException).Should().BeTrue();
    }

    [Fact]
    public void ScopeCapabilityException_is_a_PicoScopeException()
    {
        (new ScopeCapabilityException("no") is PicoScopeException).Should().BeTrue();
    }
}
