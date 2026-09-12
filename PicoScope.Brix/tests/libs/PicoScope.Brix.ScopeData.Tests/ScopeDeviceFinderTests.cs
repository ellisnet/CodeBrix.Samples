using System;
using PicoScope.Brix.ScopeData.Simulation;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

//ScopeDeviceFinder is a process-wide registry, so every test starts and ends
//  with it empty. xUnit runs the tests of one class sequentially, and no other
//  class touches the finder.
public class ScopeDeviceFinderTests : IDisposable
{
    public ScopeDeviceFinderTests()
    {
        ScopeDeviceFinder.Reset();
    }

    public void Dispose()
    {
        ScopeDeviceFinder.Reset();
    }

    [Fact]
    public void Register_throws_on_null()
    {
        Assert.Throws<ArgumentNullException>(() => ScopeDeviceFinder.Register(null));
    }

    [Fact]
    public void Register_ignores_the_same_instance_twice()
    {
        //Arrange
        var scope = new TestScopeDataDevice();

        //Act
        ScopeDeviceFinder.Register(scope);
        ScopeDeviceFinder.Register(scope);

        //Assert
        ScopeDeviceFinder.GetRegistered().Count.Should().Be(1);
    }

    [Fact]
    public void GetRegistered_preserves_registration_order()
    {
        //Arrange
        var first = new TestScopeDataDevice("first");
        var second = new TestScopeDataDevice("second", isSimulated: true);

        //Act
        ScopeDeviceFinder.Register(first);
        ScopeDeviceFinder.Register(second);

        //Assert
        ScopeDeviceFinder.GetRegistered()[0].Name.Should().Be("first");
        ScopeDeviceFinder.GetRegistered()[1].Name.Should().Be("second");
    }

    [Fact]
    public void Unregister_reports_whether_the_scope_was_registered()
    {
        //Arrange
        var scope = new TestScopeDataDevice();
        ScopeDeviceFinder.Register(scope);

        //Act + Assert
        ScopeDeviceFinder.Unregister(scope).Should().BeTrue();
        ScopeDeviceFinder.Unregister(scope).Should().BeFalse();
        ScopeDeviceFinder.Unregister(null).Should().BeFalse();
        ScopeDeviceFinder.GetRegistered().Count.Should().Be(0);
    }

    [Fact]
    public void HasRealScopeImplementation_is_false_with_only_simulators()
    {
        //Arrange
        ScopeDeviceFinder.Register(new SimulatedScopeDataDevice());
        ScopeDeviceFinder.Register(new TestScopeDataDevice(isSimulated: true));

        //Assert
        ScopeDeviceFinder.HasRealScopeImplementation.Should().BeFalse();
    }

    [Fact]
    public void HasRealScopeImplementation_is_true_once_a_real_device_is_registered()
    {
        //Arrange
        ScopeDeviceFinder.Register(new SimulatedScopeDataDevice());
        ScopeDeviceFinder.Register(new TestScopeDataDevice());

        //Assert
        ScopeDeviceFinder.HasRealScopeImplementation.Should().BeTrue();
    }

    [Fact]
    public void FindBest_prefers_a_real_device_that_opens_whatever_the_registration_order()
    {
        //Arrange
        var simulator = new SimulatedScopeDataDevice();
        var real = new TestScopeDataDevice("real");
        ScopeDeviceFinder.Register(simulator);
        ScopeDeviceFinder.Register(real);

        //Act
        IScopeDataDevice best = ScopeDeviceFinder.FindBest();

        //Assert
        ReferenceEquals(best, real).Should().BeTrue();
        real.IsOpen.Should().BeTrue();
        simulator.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void FindBest_falls_back_to_the_simulator_when_no_real_device_opens()
    {
        //Arrange
        var real = new TestScopeDataDevice("real") { CanOpen = false };
        var simulator = new SimulatedScopeDataDevice();
        ScopeDeviceFinder.Register(real);
        ScopeDeviceFinder.Register(simulator);

        //Act
        IScopeDataDevice best = ScopeDeviceFinder.FindBest();

        //Assert
        ReferenceEquals(best, simulator).Should().BeTrue();
        best.IsOpen.Should().BeTrue();
        real.OpenCount.Should().Be(1);
    }

    [Fact]
    public void FindBest_skips_a_real_device_that_throws_a_pico_exception()
    {
        //Arrange
        var broken = new TestScopeDataDevice("broken") { OpenException = new PicoScopeException("driver missing") };
        var working = new TestScopeDataDevice("working");
        ScopeDeviceFinder.Register(broken);
        ScopeDeviceFinder.Register(working);

        //Act
        IScopeDataDevice best = ScopeDeviceFinder.FindBest();

        //Assert
        ReferenceEquals(best, working).Should().BeTrue();
    }

    [Fact]
    public void FindBest_returns_null_when_fallback_is_disallowed_and_no_real_device_opens()
    {
        //Arrange
        ScopeDeviceFinder.Register(new TestScopeDataDevice { CanOpen = false });
        ScopeDeviceFinder.Register(new SimulatedScopeDataDevice());

        //Act
        IScopeDataDevice best = ScopeDeviceFinder.FindBest(allowSimulatedFallback: false);

        //Assert
        best.Should().BeNull();
    }

    [Fact]
    public void FindBest_returns_null_when_nothing_is_registered()
    {
        ScopeDeviceFinder.FindBest().Should().BeNull();
    }

    [Fact]
    public void FindBest_returns_an_already_open_device_without_reopening_it()
    {
        //Arrange
        var real = new TestScopeDataDevice();
        real.OpenScope();
        ScopeDeviceFinder.Register(real);

        //Act
        IScopeDataDevice best = ScopeDeviceFinder.FindBest();

        //Assert
        ReferenceEquals(best, real).Should().BeTrue();
        real.OpenCount.Should().Be(1);
    }

    [Fact]
    public void FindBest_opens_the_simulator_when_it_is_the_only_option()
    {
        //Arrange
        var simulator = new SimulatedScopeDataDevice();
        ScopeDeviceFinder.Register(simulator);

        //Act
        IScopeDataDevice best = ScopeDeviceFinder.FindBest();

        //Assert
        ReferenceEquals(best, simulator).Should().BeTrue();
        best.IsOpen.Should().BeTrue();
        best.IsSimulated.Should().BeTrue();
    }

    [Fact]
    public void Reset_disposes_and_forgets_every_registration()
    {
        //Arrange
        var real = new TestScopeDataDevice();
        var simulator = new SimulatedScopeDataDevice();
        ScopeDeviceFinder.Register(real);
        ScopeDeviceFinder.Register(simulator);
        ScopeDeviceFinder.FindBest();

        //Act
        ScopeDeviceFinder.Reset();

        //Assert
        ScopeDeviceFinder.GetRegistered().Count.Should().Be(0);
        real.IsDisposed.Should().BeTrue();
        real.IsOpen.Should().BeFalse();
        simulator.IsOpen.Should().BeFalse();
    }
}
