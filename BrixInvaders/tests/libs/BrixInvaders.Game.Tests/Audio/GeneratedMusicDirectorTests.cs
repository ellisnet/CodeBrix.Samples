using System;
using System.Collections.Generic;
using System.Linq;
using BrixInvaders.Assets;
using BrixInvaders.Game.Audio;
using BrixInvaders.Game.Credits;
using BrixInvaders.Game.Hosting;
using BrixInvaders.Game.Tests.Support;
using BrixInvaders.Music;
using CodeBrix.Platform.GameEngine.Audio;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Audio;

//The music wiring against a fake engine: which options start a session, when a moment is a follow-up and when it is
//  a fresh session, the ducks, and the log lines. No model loads and no audio device opens.
public class GeneratedMusicDirectorTests
{
    private static MusicSettings Choices(string generator = MusicChoices.SkyTNT, string library = MusicChoices.ModestSynthGm) =>
        new MusicSettings { GeneratorName = generator, InstrumentLibraryName = library };

    private static (GeneratedMusicDirector Director, FakeMusicEngine Engine) Started(MusicSettings settings = null, double volume = 0.8)
    {
        var engine = new FakeMusicEngine();
        var director = engine.CreateDirector();
        director.Start(settings ?? Choices(), volume);
        return (director, engine);
    }

    private static List<string> CaptureLog(Action action)
    {
        var lines = new List<string>();
        GameLog.Sink = lines.Add;
        try
        {
            action();
        }
        finally
        {
            GameLog.Sink = null;
        }

        return lines;
    }

    [Fact]
    public void Start_registers_everything_then_starts_the_title_music_with_the_players_choices()
    {
        //Arrange
        var engine = new FakeMusicEngine();
        var director = engine.CreateDirector();

        //Act
        director.Start(Choices(), 0.6);

        //Assert
        engine.RegisterCount.Should().Be(1);
        engine.Started.Should().HaveCount(1);
        var options = engine.Started[0];
        options.Generator.Should().Be(MusicChoices.SkyTNT);
        options.InstrumentLibrary.Should().Be(MusicChoices.ModestSynthGm);
        options.Preset.Should().Be(SectorMusic.Title.PresetFor(MusicChoices.SkyTNT));
        options.BeatsPerMinute.Should().Be(SectorMusic.Title.BeatsPerMinute);
        options.TrackKey.Should().Be(MusicSetup.TrackKey);
        options.StartImmediately.Should().BeTrue();
        director.Sector.Should().Be(SectorMusic.TitleSector);
    }

    [Fact]
    public void Start_puts_the_music_slider_on_the_bus_and_leaves_the_session_level_at_one()
    {
        //Arrange
        var (_, engine) = Started(volume: 0.6);

        //Act
        var sessionLevel = engine.Started[0].MasterVolume;

        //Assert
        engine.Volumes.Should().Equal(0.6f);
        sessionLevel.Should().Be(1f);
    }

    [Fact]
    public void Start_with_MuPT_through_FluidR3Gm_plays_the_MuPT_title_tune_through_FluidR3Gm()
    {
        //Arrange
        var settings = Choices("mupt", "fluidr3gm");

        //Act
        var (_, engine) = Started(settings);

        //Assert
        engine.Started[0].Generator.Should().Be(MusicChoices.MuPT);
        engine.Started[0].InstrumentLibrary.Should().Be(MusicChoices.FluidR3Gm);
        engine.Started[0].Preset.Should().Be(SectorMusic.Title.PresetFor(MusicChoices.MuPT));
    }

    [Fact]
    public void Start_with_unknown_stored_names_falls_back_to_the_defaults()
    {
        //Arrange
        var settings = Choices("NoSuchModel", "NoSuchLibrary");

        //Act
        var (_, engine) = Started(settings);

        //Assert
        engine.Started[0].Generator.Should().Be(MusicChoices.DefaultGenerator);
        engine.Started[0].InstrumentLibrary.Should().Be(MusicChoices.DefaultInstrumentLibrary);
    }

    [Fact]
    public void Start_that_fails_leaves_the_game_running_in_silence()
    {
        //Arrange
        var engine = new FakeMusicEngine { StartFailure = new InvalidOperationException("no engine") };
        var director = engine.CreateDirector();

        //Act
        director.Start(Choices(), 1.0);
        director.OnSector(1);
        director.OnBoss(1);
        director.Stop();

        //Assert
        director.ActiveSource.Should().BeEmpty();
        engine.Streams.Should().BeEmpty();
    }

    [Theory]
    [InlineData(MusicChoices.SkyTNT, 1)]
    [InlineData(MusicChoices.SkyTNT, 4)]
    [InlineData(MusicChoices.MuPT, 2)]
    [InlineData(MusicChoices.MuPT, 5)]
    public void OnSector_follows_up_with_the_sector_preset_and_never_restarts(string generator, int sector)
    {
        //Arrange
        var (director, engine) = Started(Choices(generator));

        //Act
        director.OnSector(sector);

        //Assert
        engine.Started.Should().HaveCount(1, "a sector is a follow-up on the running session, never a restart");
        engine.Current.FollowUps.Should().Equal(SectorMusic.For(sector).PresetFor(generator));
        director.Sector.Should().Be(sector);
        director.Boss.Should().BeFalse();
    }

    [Fact]
    public void OnSector_past_the_fifth_repeats_the_designs()
    {
        //Arrange
        var (director, engine) = Started();

        //Act
        director.OnSector(1);
        director.OnSector(6);

        //Assert
        engine.Current.FollowUps[1].Should().Be(engine.Current.FollowUps[0]);
        engine.Started.Should().HaveCount(1);
    }

    [Fact]
    public void OnBoss_plays_the_warning_stinger_on_the_effects_bus_ducks_the_music_and_follows_up_with_the_boss_preset()
    {
        //Arrange
        var (director, engine) = Started();
        director.OnSector(3);

        //Act
        director.OnBoss(3);

        //Assert
        engine.TimedDucks.Should().Equal(GeneratedMusicDirector.BossDuckDepth);
        engine.Stingers.Should().Equal($"{AssetKeys.Sfx.BossWarning} on {AudioBus.Sfx}");
        engine.Current.FollowUps.Last().Should().Be(SectorMusic.For(3).PresetFor(MusicChoices.SkyTNT, boss: true));
        engine.Started.Should().HaveCount(1);
        director.Boss.Should().BeTrue();
    }

    [Fact]
    public void OnGameOver_plays_the_stinger_on_the_effects_bus_fades_the_music_down_and_keeps_the_session_alive()
    {
        //Arrange
        var (director, engine) = Started();
        director.OnSector(1);

        //Act
        director.OnGameOver();

        //Assert
        engine.PushedDucks.Should().HaveCount(1);
        engine.PushedDucks[0].Depth.Should().Be(GeneratedMusicDirector.GameOverDuckDepth);
        engine.PushedDucks[0].IsReleased.Should().BeFalse();
        engine.Stingers.Should().Equal($"{AssetKeys.Sfx.GameOver} on {AudioBus.Sfx}");
        engine.Started.Should().HaveCount(1, "the session plays on for the title");
        engine.Current.FollowUps.Should().HaveCount(1);
        director.IsGameOverFaded.Should().BeTrue();
    }

    [Fact]
    public void OnTitle_after_a_game_over_restores_the_level_and_follows_up_with_the_title_music()
    {
        //Arrange
        var (director, engine) = Started();
        director.OnSector(2);
        director.OnGameOver();

        //Act
        director.OnTitle();

        //Assert
        engine.PushedDucks.Single().IsReleased.Should().BeTrue();
        engine.Current.FollowUps.Last().Should().Be(SectorMusic.Title.PresetFor(MusicChoices.SkyTNT));
        engine.Started.Should().HaveCount(1);
        director.Sector.Should().Be(SectorMusic.TitleSector);
        director.IsGameOverFaded.Should().BeFalse();
    }

    [Fact]
    public void OnPause_ducks_the_music_and_OnResume_brings_it_back()
    {
        //Arrange
        var (director, engine) = Started();
        director.OnSector(1);

        //Act
        director.OnPause();
        var duckedWhilePaused = director.IsPauseDucked;
        director.OnResume();

        //Assert
        duckedWhilePaused.Should().BeTrue();
        engine.PushedDucks.Single().Depth.Should().Be(GeneratedMusicDirector.PauseDuckDepth);
        engine.PushedDucks.Single().IsReleased.Should().BeTrue();
        director.IsPauseDucked.Should().BeFalse();
        engine.Current.FollowUps.Should().HaveCount(1, "pausing never changes the music");
    }

    [Fact]
    public void quitting_to_the_title_from_the_pause_menu_releases_the_pause_duck()
    {
        //Arrange
        var (director, engine) = Started();
        director.OnSector(1);
        director.OnPause();

        //Act
        director.OnTitle();

        //Assert
        engine.PushedDucks.Single().IsReleased.Should().BeTrue();
        director.IsPauseDucked.Should().BeFalse();
    }

    [Fact]
    public void SetVolumes_drives_only_the_music_bus_with_the_music_level()
    {
        //Arrange
        var (director, engine) = Started(volume: 1.0);

        //Act
        director.SetVolumes(0.5, 0.25, 0.75);

        //Assert
        engine.Volumes.Should().Equal(1f, 0.25f);
        engine.Started.Should().HaveCount(1);
    }

    [Fact]
    public void ApplySettings_with_a_new_model_starts_a_fresh_session_suited_to_the_current_sector()
    {
        //Arrange
        var (director, engine) = Started();
        director.OnSector(2);
        var first = engine.Current;

        //Act
        director.ApplySettings(Choices(MusicChoices.MuPT, MusicChoices.FluidR3Gm), 2, false);

        //Assert
        engine.Started.Should().HaveCount(2);
        engine.Started[1].Generator.Should().Be(MusicChoices.MuPT);
        engine.Started[1].InstrumentLibrary.Should().Be(MusicChoices.FluidR3Gm);
        engine.Started[1].Preset.Should().Be(SectorMusic.For(2).PresetFor(MusicChoices.MuPT));
        engine.Started[1].BeatsPerMinute.Should().Be(SectorMusic.For(2).BeatsPerMinute);
        first.SubscriberCount.Should().Be(0, "the replaced session is no longer listened to");
        director.Settings.GeneratorName.Should().Be(MusicChoices.MuPT);
    }

    [Fact]
    public void ApplySettings_with_a_new_library_on_the_title_restarts_with_the_title_music()
    {
        //Arrange
        var (director, engine) = Started();

        //Act
        director.ApplySettings(Choices(MusicChoices.SkyTNT, MusicChoices.FluidR3Gm), 0, false);

        //Assert
        engine.Started.Should().HaveCount(2);
        engine.Started[1].InstrumentLibrary.Should().Be(MusicChoices.FluidR3Gm);
        engine.Started[1].Preset.Should().Be(SectorMusic.Title.PresetFor(MusicChoices.SkyTNT));
    }

    [Fact]
    public void ApplySettings_during_a_boss_starts_the_boss_music()
    {
        //Arrange
        var (director, engine) = Started();

        //Act
        director.ApplySettings(Choices(MusicChoices.MuPT), 4, true);

        //Assert
        engine.Started[1].Preset.Should().Be(SectorMusic.For(4).PresetFor(MusicChoices.MuPT, boss: true));
        engine.Started[1].BeatsPerMinute.Should().Be(SectorMusic.For(4).BeatsPerMinuteFor(boss: true));
    }

    [Fact]
    public void ApplySettings_with_the_same_choices_keeps_the_session()
    {
        //Arrange
        var (director, engine) = Started();

        //Act
        director.ApplySettings(Choices("skytnt", "modestsynthgm"), 0, false);

        //Assert
        engine.Started.Should().HaveCount(1);
    }

    [Fact]
    public void every_state_change_is_logged_and_the_source_once_music_plays()
    {
        //Arrange
        var engine = new FakeMusicEngine();
        var director = engine.CreateDirector();

        //Act
        var lines = CaptureLog(() =>
        {
            director.Start(Choices(), 1.0);
            engine.Current.PlayModel("SkyTNT", "ModestSynthGm");
            engine.Current.MoveTo(StreamingMusicState.Starved);
            engine.Current.MoveTo(StreamingMusicState.Playing);
        });

        //Assert
        lines.Should().Contain("[BrixInvaders] music: state Stopped -> Starting");
        lines.Should().Contain("[BrixInvaders] music: state Starting -> Playing");
        lines.Should().Contain("[BrixInvaders] music: SkyTNT (SkyTNT) through ModestSynthGm, voiced by test");
        lines.Should().Contain("[BrixInvaders] music: state Playing -> Starved (starvation 1)");
        lines.Should().Contain("[BrixInvaders] music: state Starved -> Playing");
        lines.Count(line => line.Contains("through ModestSynthGm, voiced by")).Should().Be(1);
        director.StarvedCount.Should().Be(1);
        director.ActiveSource.Should().Be("SkyTNT (SkyTNT) through ModestSynthGm, voiced by test");
    }

    [Fact]
    public void state_changes_from_other_threads_are_handled_on_the_engine_thread()
    {
        //Arrange
        var (director, engine) = Started();
        engine.HoldPosts = true;

        //Act
        var before = CaptureLog(() => engine.Current.PlayModel("SkyTNT", "ModestSynthGm"));
        var after = CaptureLog(engine.RunHeldPosts);

        //Assert
        before.Should().BeEmpty();
        after.Should().Contain("[BrixInvaders] music: state Starting -> Playing");
        director.StarvedCount.Should().Be(0);
    }

    [Fact]
    public void a_replaced_session_is_no_longer_heard_from()
    {
        //Arrange
        var (director, engine) = Started();
        var first = engine.Current;
        director.ApplySettings(Choices(MusicChoices.MuPT), 0, false);

        //Act
        var lines = CaptureLog(() => first.MoveTo(StreamingMusicState.Starved));

        //Assert
        lines.Should().BeEmpty();
        director.StarvedCount.Should().Be(0);
    }

    [Fact]
    public void a_faulted_session_is_logged_with_its_reason()
    {
        //Arrange
        var (_, engine) = Started();
        engine.Current.Fault = new InvalidOperationException("no instrument library");

        //Act
        var lines = CaptureLog(() => engine.Current.MoveTo(StreamingMusicState.Faulted));

        //Assert
        lines.Should().Contain("[BrixInvaders] music: state Starting -> Faulted: no instrument library");
    }

    [Fact]
    public void a_refused_follow_up_is_logged_and_the_game_carries_on()
    {
        //Arrange
        var (director, engine) = Started();
        engine.Current.FollowUpFailure = new InvalidOperationException("the music has faulted");

        //Act
        var lines = CaptureLog(() => director.OnSector(1));

        //Assert
        lines.Should().Contain(line => line.Contains("follow-up") && line.Contains("refused") && line.Contains("the music has faulted"));
        engine.Started.Should().HaveCount(1);
    }

    [Fact]
    public void Stop_logs_the_starvation_count_and_stops_listening()
    {
        //Arrange
        var (director, engine) = Started();
        engine.Current.PlayModel("SkyTNT", "ModestSynthGm");
        engine.Current.MoveTo(StreamingMusicState.Starved);
        engine.Current.MoveTo(StreamingMusicState.Playing);
        engine.Current.StarvationGapCount = 3;

        //Act
        var lines = CaptureLog(director.Stop);

        //Assert
        lines.Should().Contain("[BrixInvaders] music: stopping - state Playing, 1 starvation(s) seen this run, 3 starvation gap(s) in the diagnostics");
        engine.Current.SubscriberCount.Should().Be(0);
    }

    [Fact]
    public void Stop_twice_logs_once() =>
        CaptureLog(() =>
        {
            var (director, _) = Started();
            director.Stop();
            director.Stop();
        }).Count(line => line.Contains("music: stopping")).Should().Be(1);

    [Fact]
    public void many_starvations_are_logged_in_full_only_at_first()
    {
        //Arrange
        var (director, engine) = Started();
        engine.Current.PlayModel("SkyTNT", "ModestSynthGm");

        //Act
        var lines = CaptureLog(() =>
        {
            for (var i = 0; i < 30; i++)
            {
                engine.Current.MoveTo(StreamingMusicState.Starved);
                engine.Current.MoveTo(StreamingMusicState.Playing);
            }
        });

        //Assert
        director.StarvedCount.Should().Be(30);
        lines.Count(line => line.Contains("-> Starved")).Should().Be(GeneratedMusicDirector.StarvedLinesLoggedInFull + 2);
    }

    [Fact]
    public void CreditLines_name_the_model_and_library_that_really_play()
    {
        //Arrange
        var (director, engine) = Started(Choices(MusicChoices.MuPT, MusicChoices.FluidR3Gm));

        //Act
        var warming = director.CreditLines()[0].Text;
        engine.Current.PlayModel("MuPT", "FluidR3Gm");
        var playing = director.CreditLines()[0].Text;

        //Assert
        warming.Should().StartWith(MusicCreditsCard.WrittenLiveLine(MusicChoices.MuPT, MusicChoices.FluidR3Gm));
        playing.Should().Be("Music written live by MuPT through FluidR3Gm");
    }

    [Fact]
    public void ActiveSource_is_empty_before_the_music_has_started() =>
        new FakeMusicEngine().CreateDirector().ActiveSource.Should().BeEmpty();
}
