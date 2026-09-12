using System;
using System.Threading.Tasks;
using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class TestScopeDataDeviceTests
{
    [Fact]
    public async Task RunBlockAsync_plays_the_script_back_and_repeats_it()
    {
        //Arrange
        using var scope = new TestScopeDataDevice();
        scope.OpenScope();
        scope.Script(ChannelId.ChannelA, 1, 2, 3);

        //Act
        CaptureBlock block = await scope.RunBlockAsync(5, 1, cancellationToken: TestContext.Current.CancellationToken);

        //Assert
        block.SampleCount.Should().Be(5);
        block.Channels[ChannelId.ChannelA].Samples.Should().Equal(new short[] { 1, 2, 3, 1, 2 });
        block.Channels[ChannelId.ChannelB].Samples.Should().Equal(new short[] { 0, 0, 0, 0, 0 });
        block.IntervalNanoseconds.Should().Be(20);
    }

    [Fact]
    public void EmitBatch_raises_SamplesAvailable_and_accumulates_the_total()
    {
        //Arrange
        using var scope = new TestScopeDataDevice();
        scope.OpenScope();
        scope.Script(ChannelId.ChannelB, 7);
        StreamingSamplesEventArgs received = null;
        scope.SamplesAvailable += (_, e) => received = e;
        scope.StartStreaming(new StreamingSettings { SampleInterval = 50, IntervalUnits = TimeUnits.Microseconds });

        //Act
        scope.EmitBatch(4);
        StreamingSamplesEventArgs second = scope.EmitBatch(6);

        //Assert
        received.Should().NotBeNull();
        received.TotalSampleCount.Should().Be(10);
        second.SampleCount.Should().Be(6);
        second.IntervalNanoseconds.Should().Be(50_000);
        second.Channels[ChannelId.ChannelB].Samples[5].Should().Be((short)7);
    }

    [Fact]
    public void EmitBatch_throws_when_not_streaming()
    {
        //Arrange
        using var scope = new TestScopeDataDevice();
        scope.OpenScope();

        //Act + Assert
        Assert.Throws<InvalidOperationException>(() => scope.EmitBatch(1));
    }

    [Fact]
    public void OpenScope_honours_CanOpen_and_OpenException()
    {
        //Arrange
        using var closed = new TestScopeDataDevice { CanOpen = false };
        using var broken = new TestScopeDataDevice { OpenException = new PicoScopeException("no") };

        //Act + Assert
        closed.OpenScope().Should().BeFalse();
        closed.IsOpen.Should().BeFalse();
        Assert.Throws<PicoScopeException>(() => broken.OpenScope());
        closed.OpenCount.Should().Be(1);
        broken.OpenCount.Should().Be(1);
    }

    [Fact]
    public void every_device_call_requires_an_open_scope()
    {
        //Arrange
        using var scope = new TestScopeDataDevice();

        //Act + Assert
        Assert.Throws<ScopeNotOpenException>(() => scope.SetChannel(ChannelId.ChannelA, ChannelSettings.Default));
        Assert.Throws<ScopeNotOpenException>(() => scope.FlashLed());
        Assert.Throws<ScopeNotOpenException>(() => scope.StartStreaming(new StreamingSettings()));
    }

    [Fact]
    public void Dispose_closes_the_scope_and_rejects_reopening()
    {
        //Arrange
        var scope = new TestScopeDataDevice();
        scope.OpenScope();

        //Act
        scope.Dispose();

        //Assert
        scope.IsOpen.Should().BeFalse();
        scope.CloseCount.Should().Be(1);
        Assert.Throws<ObjectDisposedException>(() => scope.OpenScope());
    }
}
