using System;
using PicoScope.Brix.ScopeData.Model;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class SignalGeneratorSettingsTests
{
    [Fact]
    public void Tone_is_a_fixed_frequency_with_the_amplitude_in_microvolts()
    {
        //Act
        SignalGeneratorSettings tone = SignalGeneratorSettings.Tone(WaveType.Square, 1234.5, 2.0);

        //Assert
        tone.WaveType.Should().Be(WaveType.Square);
        tone.PeakToPeakMicrovolts.Should().Be(2_000_000u);
        tone.OffsetMicrovolts.Should().Be(0);
        tone.StartFrequencyHz.Should().Be(1234.5);
        tone.StopFrequencyHz.Should().Be(1234.5);
        tone.IncrementHz.Should().Be(0.0);
        tone.SweepType.Should().Be(SweepType.Up);
        tone.SweepCount.Should().Be(0u);
    }

    [Fact]
    public void Off_is_zero_volts_dc()
    {
        SignalGeneratorSettings.Off.WaveType.Should().Be(WaveType.DcVoltage);
        SignalGeneratorSettings.Off.PeakToPeakMicrovolts.Should().Be(0u);
        SignalGeneratorSettings.Off.OffsetMicrovolts.Should().Be(0);
    }

    [Fact]
    public void FrequencyToDeltaPhase_follows_the_48_MHz_DDS()
    {
        //1 kHz * 2^32 / 48 MHz = 89478.485..., truncated
        ArbitraryWaveformSettings.FrequencyToDeltaPhase(1000, 4096).Should().Be(89478u);
        ArbitraryWaveformSettings.FrequencyToDeltaPhase(0, 4096).Should().Be(0u);
        ArbitraryWaveformSettings.FrequencyToDeltaPhase(-5, 4096).Should().Be(0u);
    }

    [Fact]
    public void FrequencyToDeltaPhase_rejects_an_empty_waveform()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ArbitraryWaveformSettings.FrequencyToDeltaPhase(1000, 0));
    }

    [Fact]
    public void Fixed_uses_one_delta_phase_for_start_and_stop()
    {
        //Act
        ArbitraryWaveformSettings awg = ArbitraryWaveformSettings.Fixed(new byte[1024], 1000, 1.5);

        //Assert
        awg.Waveform.Length.Should().Be(1024);
        awg.PeakToPeakMicrovolts.Should().Be(1_500_000u);
        awg.StartDeltaPhase.Should().Be(89478u);
        awg.StopDeltaPhase.Should().Be(89478u);
        awg.DeltaPhaseIncrement.Should().Be(0u);
        awg.SweepCount.Should().Be(0u);
    }

    [Fact]
    public void Fixed_rejects_a_null_waveform()
    {
        Assert.Throws<ArgumentNullException>(() => ArbitraryWaveformSettings.Fixed(null, 1000, 1.0));
    }
}
