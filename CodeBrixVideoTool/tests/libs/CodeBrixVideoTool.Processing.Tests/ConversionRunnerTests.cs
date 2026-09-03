using CodeBrix.VideoPlayback.Authoring;
using CodeBrix.VideoPlayback.Authoring.Encoding;
using CodeBrix.VideoPlayback.Chapters;
using CodeBrix.VideoPlayback.Containers;
using CodeBrix.VideoPlayback.Containers.Cbv;
using CodeBrix.VideoProcessing;
using CodeBrix.VideoProcessing.Enums;
using CodeBrixVideoTool.Processing.Containers;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Operations;
using CodeBrixVideoTool.Processing.Planning;
using CodeBrixVideoTool.Processing.Probing;
using CodeBrixVideoTool.Processing.Resolution;
using CodeBrixVideoTool.Processing.Samples;
using SilverAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Chapter = CodeBrix.VideoPlayback.Chapters.Chapter; //CodeBrix.VideoProcessing has a Chapter of its own

namespace CodeBrixVideoTool.Processing.Tests;

[Collection(SampleMediaCollection.Name)]
public class ConversionRunnerTests
{
    private readonly SampleMediaFixture media;

    public ConversionRunnerTests(SampleMediaFixture media) => this.media = media;

    [Theory]
    [InlineData(MediaFormatKind.Matroska)]
    [InlineData(MediaFormatKind.WebM)]
    [InlineData(MediaFormatKind.CodeBrixMode1)]
    [InlineData(MediaFormatKind.CodeBrixMode2)]
    public async Task an_import_writes_every_one_of_the_four_formats(MediaFormatKind destination)
    {
        //Arrange
        var output = Path.Combine(media.Root, "import-" + destination + MediaFormats.Extension(destination));
        var plan = ConversionPlanner.Create(media.RichMp4Info, destination, output, null);

        //Act
        var outcome = await new ConversionRunner()
            .RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        File.Exists(output).Should().BeTrue();
        outcome.SizeInBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task the_two_cbv_flavours_pass_the_streamable_profile()
    {
        //Act
        var mode2 = StreamableProfile.EvaluateFile(media.Mode2Path);
        var mode1 = StreamableProfile.EvaluateFile(media.Mode1Path);

        //Assert
        mode2.Passes.Should().BeTrue(mode2.ToString());
        mode1.Passes.Should().BeTrue(mode1.ToString());
        await Task.CompletedTask;
    }

    [Fact]
    public void a_mode2_file_carries_vorbis_and_never_opus()
    {
        //Arrange
        using var reader = MediaContainers.Open(media.Mode2Path);

        //Act
        var audio = reader.Tracks.Single(t => t.Kind == MediaTrackKind.Audio);

        //Assert
        reader.Should().BeOfType<CbvReader>();
        audio.CodecId.Should().Be("vorbis");
    }

    [Fact]
    public void a_mode1_file_carries_opus()
    {
        //Arrange
        using var reader = MediaContainers.Open(media.Mode1Path);

        //Act
        var audio = reader.Tracks.Single(t => t.Kind == MediaTrackKind.Audio);

        //Assert
        audio.CodecId.Should().Be("opus");
    }

    [Fact]
    public void an_imported_file_keeps_its_captions_and_chapters()
    {
        //Arrange
        using var reader = MediaContainers.Open(media.Mode2Path);

        //Act
        var captions = reader.CaptionTracks;
        var chapters = reader.Chapters;

        //Assert
        captions.Should().HaveCount(1);
        captions[0].CueCount.Should().Be(3);
        chapters.Should().HaveCount(3);
    }

    [Fact]
    public async Task a_mode2_source_transcodes_to_matroska()
    {
        //Arrange
        var output = Path.Combine(media.Root, "mode2-to.mkv");
        var plan = ConversionPlanner.Create(media.Mode2Info, MediaFormatKind.Matroska, output, null);

        //Act
        var outcome = await new ConversionRunner()
            .RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        var analysis = await FFProbe.AnalyseAsync(output, cancellationToken: TestContext.Current.CancellationToken);
        analysis.PrimaryVideoStream.CodecName.Should().Be("av1");
        analysis.PrimaryAudioStream.CodecName.Should().Be("opus");
    }

    [Fact]
    public async Task a_mode2_source_keeps_its_chapters_and_captions_across_a_transcode()
    {
        //Arrange
        var output = Path.Combine(media.Root, "mode2-to-mode1.cbv");
        var plan = ConversionPlanner.Create(media.Mode2Info, MediaFormatKind.CodeBrixMode1, output, null);

        //Act
        var outcome = await new ConversionRunner()
            .RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        using var reader = MediaContainers.Open(output);
        reader.Chapters.Should().HaveCount(3);
        reader.CaptionTracks.Should().HaveCount(1);
    }

    [Fact]
    public async Task a_mode2_source_exports_to_mp4()
    {
        //Arrange
        var output = Path.Combine(media.Root, "mode2-export.mp4");
        var plan = ConversionPlanner.Create(media.Mode2Info, MediaFormatKind.Mp4, output, null);

        //Act
        var outcome = await new ConversionRunner()
            .RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        var analysis = await FFProbe.AnalyseAsync(output, cancellationToken: TestContext.Current.CancellationToken);
        analysis.PrimaryVideoStream.CodecName.Should().Be("h264");
        analysis.PrimaryAudioStream.CodecName.Should().Be("aac");
        analysis.Chapters.Should().HaveCount(3);
        analysis.SubtitleStreams.Should().HaveCount(1);
        analysis.PrimarySubtitleStream.CodecName.Should().Be("mov_text");
    }

    [Fact]
    public async Task a_mode1_source_exports_to_mp4()
    {
        //Arrange
        var output = Path.Combine(media.Root, "mode1-export.mp4");
        var plan = ConversionPlanner.Create(media.Mode1Info, MediaFormatKind.Mp4, output, null);

        //Act
        var outcome = await new ConversionRunner()
            .RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        var analysis = await FFProbe.AnalyseAsync(output, cancellationToken: TestContext.Current.CancellationToken);
        analysis.PrimaryVideoStream.CodecName.Should().Be("h264");
    }

    [Fact]
    public async Task a_reduced_rung_really_reduces_the_picture()
    {
        //Arrange
        var output = Path.Combine(media.Root, "reduced.webm");
        var rung = ResolutionLadder.Build(media.Width, media.Height)
            .FirstOrDefault(r => !r.IsOriginal)
            ?? ResolutionOption.Reduced("half", 160, 120);
        var plan = ConversionPlanner.Create(media.RichMp4Info, MediaFormatKind.WebM, output, rung);

        //Act
        var outcome = await new ConversionRunner()
            .RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        var analysis = await FFProbe.AnalyseAsync(output, cancellationToken: TestContext.Current.CancellationToken);
        analysis.PrimaryVideoStream.Width.Should().Be(rung.Width);
        analysis.PrimaryVideoStream.Height.Should().Be(rung.Height);
    }

    [Fact]
    public async Task progress_is_reported_all_the_way_to_a_hundred()
    {
        //Arrange
        var output = Path.Combine(media.Root, "progress.mp4");
        var plan = ConversionPlanner.Create(media.Mode1Info, MediaFormatKind.Mp4, output, null);
        var reports = new List<ConversionProgress>();
        var progress = new Progress<ConversionProgress>(reports.Add);

        //Act
        var outcome = await new ConversionRunner()
            .RunAsync(plan, progress, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        await Task.Delay(200, TestContext.Current.CancellationToken);
        reports.Should().NotBeEmpty();
        reports.Should().Contain(r => r.IsIndeterminate);
        reports.Max(r => r.OverallPercent).Should().Be(100d);
    }

    [Fact]
    public async Task cancelling_an_export_leaves_nothing_behind()
    {
        //Arrange
        var output = Path.Combine(media.Root, "cancelled.mp4");
        var plan = ConversionPlanner.Create(media.Mode1Info, MediaFormatKind.Mp4, output, null);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, cancellation.Token);

        //Assert
        outcome.WasCancelled.Should().BeTrue();
        outcome.Succeeded.Should().BeFalse();
        File.Exists(output).Should().BeFalse();
    }

    [Fact]
    public async Task cancelling_an_authoring_pass_stops_it_part_way()
    {
        //Arrange
        var source = Path.Combine(media.Root, "long.mp4");
        await SampleClipFactory.WriteMp4Async(
            source, 640, 480, TimeSpan.FromSeconds(20), 25d, TestContext.Current.CancellationToken);
        var info = await new MediaProbe().ProbeAsync(source, TestContext.Current.CancellationToken);
        var output = Path.Combine(media.Root, "cancelled-part-way.cbv");
        var plan = ConversionPlanner.Create(info, MediaFormatKind.CodeBrixMode2, output, null);
        using var cancellation = new CancellationTokenSource();
        var reports = new List<ConversionProgress>();
        var progress = new ImmediateProgress(report =>
        {
            reports.Add(report);

            //Cancel the moment the encode itself reports that it is under way - off the reporting
            //thread, so the encoder is not asked to stop from inside its own progress callback.
            if (report.StageNumber == 2 && report.StagePercent is > 0 and < 100 && !cancellation.IsCancellationRequested)
            {
                _ = Task.Run(cancellation.Cancel);
            }
        });

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, progress, cancellation.Token);

        //Assert
        cancellation.IsCancellationRequested.Should().BeTrue("the encode never reported being under way, so nothing was cancelled");
        outcome.WasCancelled.Should().BeTrue();
        outcome.Succeeded.Should().BeFalse();
        File.Exists(output).Should().BeFalse();
        reports.Where(r => r.StageNumber == 2 && r.StagePercent.HasValue).Max(r => r.StagePercent.Value)
            .Should().BeLessThan(100d, "an encode that ran to the end was not stopped part-way");
    }

    [Theory]
    [InlineData(22050, 1)]
    [InlineData(96000, 2)]
    public async Task a_source_with_unusual_sound_still_imports_to_mode2_at_its_own_rate(int sampleRate, int channels)
    {
        //Arrange
        var source = Path.Combine(media.Root, $"sound-{sampleRate}-{channels}.mp4");
        await WriteClipWithSoundAsync(source, sampleRate, channels);
        var info = await new MediaProbe().ProbeAsync(source, TestContext.Current.CancellationToken);
        var output = Path.Combine(media.Root, $"sound-{sampleRate}-{channels}.cbv");
        var plan = ConversionPlanner.Create(info, MediaFormatKind.CodeBrixMode2, output, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        info.AudioSampleRateHz.Should().Be(sampleRate);
        info.AudioChannels.Should().Be(channels);
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        using var reader = MediaContainers.Open(output);
        var audio = reader.Tracks.Single(t => t.Kind == MediaTrackKind.Audio);
        audio.CodecId.Should().Be("vorbis");
        audio.SampleRate.Should().Be(sampleRate);
        audio.Channels.Should().Be(channels);
    }

    [Fact]
    public async Task a_surround_source_is_downmixed_to_stereo_for_both_cbv_flavours()
    {
        //Arrange
        var source = Path.Combine(media.Root, "surround.mp4");
        await WriteClipWithSoundAsync(source, 48000, 6);
        var info = await new MediaProbe().ProbeAsync(source, TestContext.Current.CancellationToken);
        var mode1 = Path.Combine(media.Root, "surround-mode1.cbv");
        var mode2 = Path.Combine(media.Root, "surround-mode2.cbv");
        var opusPlan = ConversionPlanner.Create(info, MediaFormatKind.CodeBrixMode1, mode1, null);
        var vorbisPlan = ConversionPlanner.Create(info, MediaFormatKind.CodeBrixMode2, mode2, null);

        //Act
        var opusOutcome = await new ConversionRunner().RunAsync(opusPlan, null, TestContext.Current.CancellationToken);
        var vorbisOutcome = await new ConversionRunner().RunAsync(vorbisPlan, null, TestContext.Current.CancellationToken);

        //Assert
        info.AudioChannels.Should().Be(6);
        opusOutcome.Succeeded.Should().BeTrue(opusOutcome.Failure ?? "");
        opusOutcome.Notes.Any(n => n.Contains("downmixed from 6 channels to stereo", StringComparison.Ordinal)).Should().BeTrue();
        vorbisOutcome.Succeeded.Should().BeTrue(vorbisOutcome.Failure ?? "");
        vorbisOutcome.Notes.Any(n => n.Contains("downmixed from 6 channels to stereo", StringComparison.Ordinal)).Should().BeTrue();

        //The note is the application's own policy, not a codec's limit.
        opusOutcome.Notes.Any(n => n.Contains("mapping family", StringComparison.Ordinal)).Should().BeFalse();
        vorbisOutcome.Notes.Any(n => n.Contains("this application writes mono or stereo audio only", StringComparison.Ordinal)).Should().BeTrue();

        using var opusReader = MediaContainers.Open(mode1);
        opusReader.Tracks.Single(t => t.Kind == MediaTrackKind.Audio).Channels.Should().Be(2);
        using var vorbisReader = MediaContainers.Open(mode2);
        vorbisReader.Tracks.Single(t => t.Kind == MediaTrackKind.Audio).Channels.Should().Be(2);
    }

    [Fact]
    public async Task a_surround_source_exported_to_mp4_keeps_all_six_channels()
    {
        //Arrange
        //The .mp4 export is the one destination this application does not cap, so the fence needs a
        //six-channel SOURCE in one of the four formats. This application never writes one - it caps
        //everything it writes - so the source is a six-channel Matroska file built by FFmpeg directly,
        //which is exactly the kind of foreign file an export exists for.
        var source = Path.Combine(media.Root, "surround-source.mkv");
        await WriteSurroundMatroskaAsync(source);
        var info = await new MediaProbe().ProbeAsync(source, TestContext.Current.CancellationToken);
        var exported = Path.Combine(media.Root, "surround-exported.mp4");
        var plan = ConversionPlanner.Create(info, MediaFormatKind.Mp4, exported, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        info.Format.Should().Be(MediaFormatKind.Matroska);
        info.AudioChannels.Should().Be(6);
        plan.AudioChannels.Should().Be(6);
        plan.DownmixesAudio.Should().BeFalse();
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        outcome.Notes.Any(n => n.Contains("downmixed", StringComparison.Ordinal)).Should().BeFalse();
        var analysis = await FFProbe.AnalyseAsync(exported, cancellationToken: TestContext.Current.CancellationToken);
        analysis.PrimaryAudioStream.CodecName.Should().Be("aac");
        analysis.PrimaryAudioStream.Channels.Should().Be(6);
    }

    [Fact]
    public async Task a_mode2_source_with_two_chapter_title_languages_keeps_only_the_untagged_one()
    {
        //Arrange
        var chapters = WriteChapterFile("two-languages.ffmetadata",
            ["title=Opening", "title-fr=Ouverture"],
            ["title=Middle", "title-fr=Milieu"],
            ["title=Ending", "title-fr=Fin"]);
        var source = await AuthorMode2Async("chapters-two-languages.cbv", chapters);
        var notes = new List<string>();

        //Act
        var collapsed = ReadCollapsedChapters(source, "collapse-two-languages", notes);
        var rewritten = await AuthorMode2Async("chapters-two-languages-out.cbv", collapsed.path);

        //Assert
        //The source really does carry two titles per chapter - otherwise the fence proves nothing.
        collapsed.sourceTitleCounts.Should().AllBeEquivalentTo(2);
        collapsed.chapters.Should().HaveCount(3);
        collapsed.chapters.Select(c => c.Titles.Count).Should().AllBeEquivalentTo(1);
        collapsed.chapters.Select(c => c.Titles.Keys.Single()).Should().AllBeEquivalentTo(string.Empty);
        collapsed.chapters[0].Titles[string.Empty].Should().Be("Opening");
        notes.Should().Contain("1 chapter-title language(s) dropped: this application carries one title per chapter.");

        using var reader = MediaContainers.Open(rewritten);
        reader.Chapters.Should().HaveCount(3);
        reader.Chapters.Select(c => c.Titles.Count).Should().AllBeEquivalentTo(1);
        reader.Chapters[2].Titles[string.Empty].Should().Be("Ending");
    }

    [Fact]
    public async Task chapters_that_carry_only_tagged_titles_keep_the_first_one_listed()
    {
        //Arrange
        var chapters = WriteChapterFile("tagged-only.ffmetadata",
            ["title-de=Anfang", "title-fr=Ouverture"],
            ["title-de=Mitte", "title-fr=Milieu"],
            ["title-de=Ende", "title-fr=Fin"]);
        var source = await AuthorMode2Async("chapters-tagged-only.cbv", chapters);
        var notes = new List<string>();

        //Act
        var collapsed = ReadCollapsedChapters(source, "collapse-tagged-only", notes);

        //Assert
        collapsed.sourceTitleCounts.Should().AllBeEquivalentTo(2);
        collapsed.chapters.Select(c => c.Titles.Count).Should().AllBeEquivalentTo(1);
        collapsed.chapters[0].Titles[string.Empty].Should().Be("Anfang");
        collapsed.chapters[1].Titles[string.Empty].Should().Be("Mitte");
        notes.Should().Contain("1 chapter-title language(s) dropped: this application carries one title per chapter.");
    }

    [Fact]
    public async Task a_multilingual_mode2_source_says_what_it_dropped_when_it_is_transcoded()
    {
        //Arrange
        var chapters = WriteChapterFile("transcoded-languages.ffmetadata",
            ["title=Opening", "title-fr=Ouverture"],
            ["title=Ending", "title-fr=Fin"]);
        var source = await AuthorMode2Async("chapters-transcoded.cbv", chapters);
        var info = await new MediaProbe().ProbeAsync(source, TestContext.Current.CancellationToken);
        var output = Path.Combine(media.Root, "chapters-transcoded.mkv");
        var plan = ConversionPlanner.Create(info, MediaFormatKind.Matroska, output, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        outcome.Notes.Should().Contain("1 chapter-title language(s) dropped: this application carries one title per chapter.");
        outcome.Notes.Should().Contain("Chapters carried across.");
        outcome.Notes.Should().NotContain(n => n.Contains("every language title kept", StringComparison.Ordinal));
    }

    [Fact]
    public async Task an_mp4_import_still_writes_one_untagged_title_per_chapter_and_drops_nothing()
    {
        //Arrange
        var output = Path.Combine(media.Root, "mp4-import-chapters.cbv");
        var plan = ConversionPlanner.Create(media.RichMp4Info, MediaFormatKind.CodeBrixMode2, output, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        outcome.Notes.Should().NotContain(n => n.Contains("dropped", StringComparison.Ordinal));
        outcome.Notes.Should().Contain("Chapters carried across.");
        using var reader = MediaContainers.Open(output);
        reader.Chapters.Should().HaveCount(3);
        reader.Chapters.Select(c => c.Titles.Count).Should().AllBeEquivalentTo(1);
        reader.Chapters[0].Titles[string.Empty].Should().Be("Part 1");
    }

    [Fact]
    public async Task good_quality_writes_the_arguments_this_application_has_always_written()
    {
        //Arrange
        var authored = Path.Combine(media.Root, "quality-good.webm");
        var exported = Path.Combine(media.Root, "quality-good.mp4");
        var authorPlan = ConversionPlanner.Create(media.RichMp4Info, MediaFormatKind.WebM, authored, null);
        var exportPlan = ConversionPlanner.Create(media.Mode1Info, MediaFormatKind.Mp4, exported, null);

        //Act
        var authorOutcome = await new ConversionRunner().RunAsync(authorPlan, null, TestContext.Current.CancellationToken);
        var exportOutcome = await new ConversionRunner().RunAsync(exportPlan, null, TestContext.Current.CancellationToken);

        //Assert
        authorPlan.Quality.Should().Be(QualityLevel.Good);
        var authorCommand = string.Join(" ", authorOutcome.Commands);
        var exportCommand = string.Join(" ", exportOutcome.Commands);
        authorCommand.Should().Contain("-crf 30", authorCommand);
        authorCommand.Should().Contain("-preset 8", authorCommand);
        exportCommand.Should().Contain("-crf 20", exportCommand);
        exportCommand.Should().Contain("-preset medium", exportCommand);
    }

    [Theory]
    [InlineData(QualityLevel.Fair, 42, 27)]
    [InlineData(QualityLevel.Good, 30, 20)]
    [InlineData(QualityLevel.Better, 24, 17)]
    [InlineData(QualityLevel.Best, 18, 14)]
    public async Task every_quality_stop_lands_its_own_rate_factor_in_the_command(
        QualityLevel quality, int av1RateFactor, int h264RateFactor)
    {
        //Arrange
        var authored = Path.Combine(media.Root, $"quality-{quality}.webm");
        var exported = Path.Combine(media.Root, $"quality-{quality}.mp4");
        var authorPlan = ConversionPlanner.Create(media.RichMp4Info, MediaFormatKind.WebM, authored, null, quality);
        var exportPlan = ConversionPlanner.Create(media.Mode1Info, MediaFormatKind.Mp4, exported, null, quality);

        //Act
        var authorOutcome = await new ConversionRunner().RunAsync(authorPlan, null, TestContext.Current.CancellationToken);
        var exportOutcome = await new ConversionRunner().RunAsync(exportPlan, null, TestContext.Current.CancellationToken);

        //Assert
        authorOutcome.Succeeded.Should().BeTrue(authorOutcome.Failure ?? "");
        exportOutcome.Succeeded.Should().BeTrue(exportOutcome.Failure ?? "");
        var authorCommand = string.Join(" ", authorOutcome.Commands);
        var exportCommand = string.Join(" ", exportOutcome.Commands);
        authorCommand.Should().Contain($"-crf {av1RateFactor}", authorCommand);
        exportCommand.Should().Contain($"-crf {h264RateFactor}", exportCommand);

        //The speed presets never move, whichever stop was chosen.
        authorCommand.Should().Contain("-preset 8", authorCommand);
        exportCommand.Should().Contain("-preset medium", exportCommand);
    }

    [Fact]
    public async Task an_ordinary_mkv_records_why_it_does_not_pass_the_streamable_profile()
    {
        //Arrange
        var output = Path.Combine(media.Root, "profile-verdict.mkv");
        var plan = ConversionPlanner.Create(media.RichMp4Info, MediaFormatKind.Matroska, output, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        //A standard MKV writes its cues at the end and is expected to fail; the failure is reported,
        //never treated as an error, and the verdict says WHAT failed rather than only that it did.
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        outcome.PassesProfile.Should().BeFalse();
        outcome.ProfileVerdict.Should().NotBeNullOrWhiteSpace();
        outcome.ProfileVerdict.Should().Be("cues sit before the first cluster");
    }

    [Fact]
    public async Task a_cbv_that_passes_the_profile_says_so_in_the_words_the_tools_print()
    {
        //Arrange
        var output = Path.Combine(media.Root, "profile-verdict.cbv");
        var plan = ConversionPlanner.Create(media.RichMp4Info, MediaFormatKind.CodeBrixMode2, output, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        outcome.PassesProfile.Should().BeTrue();
        outcome.ProfileVerdict.Should().Contain("passes the profile", outcome.ProfileVerdict);
    }

    /// <summary>A chapter metadata file whose chapters carry whatever title lines the test asks for.</summary>
    private string WriteChapterFile(string fileName, params string[][] titleLines)
    {
        var text = new StringBuilder(";FFMETADATA1\n");
        for (var index = 0; index < titleLines.Length; index++)
        {
            text.Append("\n[CHAPTER]\nTIMEBASE=1/1000\n")
                .Append(CultureInfo.InvariantCulture, $"START={index * 500}\n")
                .Append(CultureInfo.InvariantCulture, $"END={(index + 1) * 500}\n");
            foreach (var line in titleLines[index])
            {
                text.Append(line).Append('\n');
            }
        }

        var path = Path.Combine(media.Root, fileName);
        File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));
        return path;
    }

    /// <summary>
    /// Authors a Mode 2 file from the sample clip and a chapter file, straight through the authoring
    /// library - the only way to get a source whose chapters carry more than one title, since this
    /// application never writes one.
    /// </summary>
    private async Task<string> AuthorMode2Async(string fileName, string chaptersPath)
    {
        var output = Path.Combine(media.Root, fileName);
        var request = new VideoAuthoringRequest
        {
            SourcePath = media.Mp4Path,
            OutputPath = output,
            SourceDuration = media.Duration,
            TemporaryFolder = Path.Combine(media.Root, "authoring", Path.GetFileNameWithoutExtension(fileName)),
            ChaptersPath = chaptersPath,
            Flavour = VideoAuthoringFlavour.Bespoke,
            Container = AuthoringContainerFormat.WebM,
            CuesToFront = true,
            ValidateProfile = false,
            CancellationToken = TestContext.Current.CancellationToken,
        };

        request.Video.SpeedPreset = 12;
        request.Audio.Include = true;
        request.Audio.Codec = AuthoringAudioCodec.LibVorbis;
        request.Audio.VorbisQuality = 4d;

        var result = await Task.Run(() => CbvAuthor.Write(request), TestContext.Current.CancellationToken);
        return result.OutputPath;
    }

    /// <summary>
    /// Runs a file's chapters through the extractor exactly as a conversion does, and reads back the
    /// chapter file the authoring pass would be handed.
    /// </summary>
    private (IReadOnlyList<Chapter> chapters, string path, IReadOnlyList<int> sourceTitleCounts) ReadCollapsedChapters(
        string sourcePath, string folderName, List<string> notes)
    {
        var folder = Path.Combine(media.Root, folderName);
        using var reader = MediaContainers.Open(sourcePath);
        var sourceTitleCounts = reader.Chapters.Select(c => c.Titles.Count).ToList();
        var sidecars = SidecarExtractor.ExtractFromReader(reader, folder);
        notes.AddRange(sidecars.Notes);
        return (FfMetadataChapters.ReadFile(sidecars.ChaptersPath), sidecars.ChaptersPath, sourceTitleCounts);
    }

    [Fact]
    public async Task a_source_with_more_channels_than_an_mp4_carries_is_reduced_rather_than_refused()
    {
        //Arrange
        //AAC is written with at most eight channels here, so a source with more has to be told to come
        //down to eight: left to itself FFmpeg's AAC encoder refuses the layout and the export fails.
        var source = Path.Combine(media.Root, "ten-channel-source.mkv");
        await WriteTenChannelMatroskaAsync(source);
        var info = await new MediaProbe().ProbeAsync(source, TestContext.Current.CancellationToken);
        var exported = Path.Combine(media.Root, "ten-channel-exported.mp4");
        var plan = ConversionPlanner.Create(info, MediaFormatKind.Mp4, exported, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        info.AudioChannels.Should().Be(10);
        plan.AudioChannels.Should().Be(8);
        plan.DownmixesAudio.Should().BeTrue();
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        outcome.Notes.Should().Contain("Audio reduced from 10 channels to 8: an exported MP4 carries at most eight.");
        var analysis = await FFProbe.AnalyseAsync(exported, cancellationToken: TestContext.Current.CancellationToken);
        analysis.PrimaryAudioStream.Channels.Should().Be(8);
    }

    /// <summary>A short ten-channel Matroska file, of the kind an .mp4 export cannot carry whole.</summary>
    private static async Task WriteTenChannelMatroskaAsync(string path)
    {
        var errors = new List<string>();
        var succeeded = await FFMpegArguments
            .FromFileInput("testsrc2=size=320x240:rate=25", false, input => input.ForceFormat("lavfi"))
            .AddFileInput("sine=frequency=440:sample_rate=48000", false, input => input.ForceFormat("lavfi"))
            .OutputToFile(path, true, options => options
                .WithCustomArgument(
                    "-filter_complex \"[1:a]pan=10c|c0=c0|c1=c0|c2=c0|c3=c0|c4=c0|c5=c0|c6=c0|c7=c0|c8=c0|c9=c0[a]\"")
                .WithCustomArgument("-map 0:v -map \"[a]\"")
                .WithDuration(TimeSpan.FromSeconds(1))
                .WithVideoCodec("libsvtav1")
                .WithConstantRateFactor(50)
                .WithCustomArgument("-preset 12")
                .ForcePixelFormat("yuv420p")
                .WithAudioCodec("pcm_s16le")
                .ForceFormat("matroska"))
            .NotifyOnError(errors.Add)
            .CancellableThrough(TestContext.Current.CancellationToken)
            .ProcessAsynchronously(false);

        succeeded.Should().BeTrue("the ten-channel sample could not be generated: " + string.Join(" ", errors.TakeLast(5)));
    }

    /// <summary>A short six-channel Matroska file - AV1 and Opus - of the kind this application never writes.</summary>
    private static async Task WriteSurroundMatroskaAsync(string path)
    {
        var errors = new List<string>();
        var succeeded = await FFMpegArguments
            .FromFileInput("testsrc2=size=320x240:rate=25[out0]; sine=frequency=440:sample_rate=48000[out1]",
                false, input => input.ForceFormat("lavfi"))
            .OutputToFile(path, true, options => options
                .WithDuration(TimeSpan.FromSeconds(1))
                .WithVideoCodec("libsvtav1")
                .WithConstantRateFactor(50)
                .WithCustomArgument("-preset 12")
                .ForcePixelFormat("yuv420p")
                .WithAudioCodec("libopus")
                .WithCustomArgument("-ac 6")
                .ForceFormat("matroska"))
            .NotifyOnError(errors.Add)
            .CancellableThrough(TestContext.Current.CancellationToken)
            .ProcessAsynchronously(false);

        succeeded.Should().BeTrue("the six-channel sample could not be generated: " + string.Join(" ", errors.TakeLast(5)));
    }

    /// <summary>A short clip whose sound has a chosen sample rate and channel count.</summary>
    private static async Task WriteClipWithSoundAsync(string path, int sampleRate, int channels)
    {
        var filterGraph = string.Create(CultureInfo.InvariantCulture,
            $"testsrc2=size=320x240:rate=25[out0]; sine=frequency=440:sample_rate={sampleRate}[out1]");

        var errors = new List<string>();
        var succeeded = await FFMpegArguments
            .FromFileInput(filterGraph, false, input => input.ForceFormat("lavfi"))
            .OutputToFile(path, true, options => options
                .WithDuration(TimeSpan.FromSeconds(2))
                .WithVideoCodec("libx264")
                .WithSpeedPreset(Speed.UltraFast)
                .ForcePixelFormat("yuv420p")
                .WithAudioCodec("aac")
                .WithAudioSamplingRate(sampleRate)
                .WithCustomArgument("-ac " + channels.ToString(CultureInfo.InvariantCulture))
                .ForceFormat("mp4"))
            .NotifyOnError(errors.Add)
            .CancellableThrough(TestContext.Current.CancellationToken)
            .ProcessAsynchronously(false);

        succeeded.Should().BeTrue("the sample clip could not be generated: " + string.Join(" ", errors.TakeLast(5)));
    }

    /// <summary>Delivers each report on the reporting thread, so the list is complete when the run returns.</summary>
    private sealed class ImmediateProgress : IProgress<ConversionProgress>
    {
        private readonly Action<ConversionProgress> handler;
        private readonly object gate = new();

        public ImmediateProgress(Action<ConversionProgress> handler) => this.handler = handler;

        public void Report(ConversionProgress value)
        {
            lock (gate)
            {
                handler(value);
            }
        }
    }
}
