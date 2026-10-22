using CodeBrix.VideoPlayback.Containers;
using CodeBrix.VideoPlayback.Containers.Cbv;
using CodeBrix.VideoProcessing;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Operations;
using CodeBrixVideoTool.Processing.Planning;
using CodeBrixVideoTool.Processing.Probing;
using SilverAssertions;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

/// <summary>
/// CodeBrix Mode 3, the master form, end to end: imported from an opaque MP4 and from a source with an
/// alpha channel, transcoded back out through the demultiplexer, and exported to MP4 through the
/// authoring library's exporter.
/// </summary>
[Collection(SampleMediaCollection.Name)]
public class MasterConversionTests
{
    private readonly SampleMediaFixture media;

    public MasterConversionTests(SampleMediaFixture media) => this.media = media;

    [Fact]
    public void an_imported_master_is_a_version_1_cbvf_file_with_flac_sound()
    {
        //Arrange
        using var reader = MediaContainers.Open(media.MasterPath);

        //Act
        var audio = reader.Tracks.Single(t => t.Kind == MediaTrackKind.Audio);

        //Assert
        var cbv = reader.Should().BeOfType<CbvReader>().Subject;
        cbv.Version.Should().Be(CbvFormat.MasterVersion);
        audio.CodecId.Should().Be("flac");
        MediaFormats.Detect(media.MasterPath).Should().Be(MediaFormatKind.CodeBrixMaster);
    }

    [Fact]
    public void both_masters_pass_the_streamable_profile()
    {
        //Act
        var opaque = StreamableProfile.EvaluateFile(media.MasterPath);
        var alpha = StreamableProfile.EvaluateFile(media.AlphaMasterPath);

        //Assert
        opaque.Passes.Should().BeTrue(opaque.ToString());
        alpha.Passes.Should().BeTrue(alpha.ToString());
    }

    [Fact]
    public void an_imported_master_keeps_its_captions_and_chapters()
    {
        //Arrange
        using var reader = MediaContainers.Open(media.MasterPath);

        //Assert
        reader.CaptionTracks.Should().HaveCount(1);
        reader.CaptionTracks[0].CueCount.Should().Be(3);
        reader.Chapters.Should().HaveCount(3);
    }

    [Fact]
    public void an_alpha_source_is_seen_to_have_alpha_and_its_master_keeps_it()
    {
        //Arrange
        using var reader = (CbvReader)MediaContainers.Open(media.AlphaMasterPath);

        //Assert
        media.AlphaMovInfo.HasAlpha.Should().BeTrue();
        media.AlphaMasterInfo.HasAlpha.Should().BeTrue();
        media.AlphaMasterInfo.Format.Should().Be(MediaFormatKind.CodeBrixMaster);
        reader.AlphaPlaneTrack.Should().NotBeNull();
        reader.AlphaPlaneTrack.Width.Should().Be(media.Width);
        reader.AlphaPlaneTrack.Height.Should().Be(media.Height);
    }

    [Fact]
    public void an_opaque_master_is_probed_as_the_picture_without_alpha()
    {
        //Assert
        media.MasterInfo.Format.Should().Be(MediaFormatKind.CodeBrixMaster);
        media.MasterInfo.HasAlpha.Should().BeFalse();
        media.MasterInfo.Width.Should().Be(media.Width);
        media.MasterInfo.AudioCodec.Should().Be("flac");
        media.MasterInfo.IsPlayable.Should().BeTrue();
    }

    [Fact]
    public async Task a_master_transcodes_to_mode2_with_its_captions_and_chapters()
    {
        //Arrange
        var output = Path.Combine(media.Root, "master-to-mode2.cbv");
        var plan = ConversionPlanner.Create(media.MasterInfo, MediaFormatKind.CodeBrixMode2, output, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        outcome.Notes.Should().Contain(n => n.Contains("Mode 3 source demultiplexed") && n.Contains("flac"));
        using var reader = MediaContainers.Open(output);
        reader.Tracks.Single(t => t.Kind == MediaTrackKind.Audio).CodecId.Should().Be("vorbis");
        reader.CaptionTracks.Should().HaveCount(1);
        reader.Chapters.Should().HaveCount(3);
        MediaFormats.Detect(output).Should().Be(MediaFormatKind.CodeBrixMode2);
    }

    [Fact]
    public async Task a_master_with_alpha_transcodes_to_webm_and_says_the_alpha_was_left_behind()
    {
        //Arrange
        var output = Path.Combine(media.Root, "alpha-master-to.webm");
        var plan = ConversionPlanner.Create(media.AlphaMasterInfo, MediaFormatKind.WebM, output, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        plan.Steps.Should().Contain(s => s.Contains("Leave the alpha channel behind"));
        outcome.Notes.Should().Contain(n => n.Contains("alpha plane was left behind"));
        var analysis = await FFProbe.AnalyseAsync(output, cancellationToken: TestContext.Current.CancellationToken);
        analysis.PrimaryVideoStream.CodecName.Should().Be("av1");
        analysis.PrimaryAudioStream.CodecName.Should().Be("opus");
    }

    [Fact]
    public async Task a_mode2_source_imports_to_a_master()
    {
        //Arrange
        var output = Path.Combine(media.Root, "mode2-to.cbvmaster");
        var plan = ConversionPlanner.Create(media.Mode2Info, MediaFormatKind.CodeBrixMaster, output, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        outcome.PassesProfile.Should().BeTrue(outcome.ProfileVerdict ?? "");
        MediaFormats.Detect(output).Should().Be(MediaFormatKind.CodeBrixMaster);
        using var reader = MediaContainers.Open(output);
        reader.Tracks.Single(t => t.Kind == MediaTrackKind.Audio).CodecId.Should().Be("flac");
        reader.Chapters.Should().HaveCount(3);
    }

    [Fact]
    public async Task a_master_exports_to_mp4_with_its_captions_and_chapters()
    {
        //Arrange
        var output = Path.Combine(media.Root, "master-export.mp4");
        var plan = ConversionPlanner.Create(media.MasterInfo, MediaFormatKind.Mp4, output, null, QualityLevel.Better);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        plan.ExportsThroughVideoExporter.Should().BeTrue();
        plan.RequiresMode2Extraction.Should().BeFalse();
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        outcome.Commands.Should().Contain(c => c.Contains("cq 17"));
        outcome.Notes.Should().NotContain(n => n.Contains("an MP4 export does not carry them"));

        var analysis = await FFProbe.AnalyseAsync(output, cancellationToken: TestContext.Current.CancellationToken);
        analysis.PrimaryVideoStream.CodecName.Should().Be("h264");
        analysis.PrimaryAudioStream.CodecName.Should().Be("aac");
        analysis.SubtitleStreams.Should().HaveCount(1);
        analysis.Chapters.Should().HaveCount(3);
    }

    [Fact]
    public async Task a_master_with_alpha_exports_to_mp4_composited()
    {
        //Arrange
        var output = Path.Combine(media.Root, "alpha-master-export.mp4");
        var plan = ConversionPlanner.Create(media.AlphaMasterInfo, MediaFormatKind.Mp4, output, null);

        //Act
        var outcome = await new ConversionRunner().RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        outcome.Notes.Should().Contain(n => n.Contains("composited over 50% grey"));
        var analysis = await FFProbe.AnalyseAsync(output, cancellationToken: TestContext.Current.CancellationToken);
        analysis.PrimaryVideoStream.CodecName.Should().Be("h264");
        analysis.PrimaryVideoStream.Width.Should().Be(media.Width);
        analysis.PrimaryAudioStream.CodecName.Should().Be("aac");
    }
}
