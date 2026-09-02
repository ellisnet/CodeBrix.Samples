using CodeBrix.VideoPlayback.Containers;
using CodeBrix.VideoPlayback.Containers.Cbv;
using CodeBrix.VideoProcessing;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Operations;
using CodeBrixVideoTool.Processing.Planning;
using CodeBrixVideoTool.Processing.Probing;
using CodeBrixVideoTool.Processing.Resolution;
using SilverAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

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
}
