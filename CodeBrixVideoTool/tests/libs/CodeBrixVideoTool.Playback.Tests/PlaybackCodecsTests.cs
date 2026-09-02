using CodeBrix.Audio.Opus;
using CodeBrix.VideoPlayback.Dav1d;
using CodeBrixVideoTool.Playback.Services;
using SilverAssertions;
using Xunit;

namespace CodeBrixVideoTool.Playback.Tests;

public class PlaybackCodecsTests
{
    [Fact]
    public void registering_turns_on_av1_and_opus()
    {
        //Act
        PlaybackCodecs.RegisterOnce();

        //Assert
        PlaybackCodecs.IsRegistered.Should().BeTrue();
        CodeBrixVideoPlaybackDav1d.IsRegistered.Should().BeTrue();
        CodeBrixAudioOpus.IsRegistered.Should().BeTrue();
    }

    [Fact]
    public void registering_twice_is_harmless()
    {
        //Act
        PlaybackCodecs.RegisterOnce();
        PlaybackCodecs.RegisterOnce();

        //Assert
        PlaybackCodecs.IsRegistered.Should().BeTrue();
    }

    [Fact]
    public void the_native_av1_decoder_really_loaded()
    {
        //Arrange
        PlaybackCodecs.RegisterOnce();

        //Act
        var version = CodeBrixVideoPlaybackDav1d.NativeVersion;

        //Assert
        version.Should().NotBeNullOrEmpty();
    }
}
