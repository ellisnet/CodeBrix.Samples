using CodeBrixVideoTool.Processing.Tools;
using SilverAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

/// <summary>
/// The order the external tools are checked in and what is said about each. None of these tests runs FFmpeg:
/// the three questions are answered by the test, so they pass the same on every machine and every OS.
/// </summary>
public class ExternalToolCheckTests
{
    [Fact]
    public void missing_ffmpeg_is_reported_and_nothing_after_it_is_asked()
    {
        //Arrange
        var asked = 0;
        var check = new ExternalToolCheck(
            () => false,
            () => { asked++; return false; },
            _ => { asked++; return false; });

        //Act
        var problem = check.FindProblem(CancellationToken.None);

        //Assert
        problem.Should().Be(ExternalToolCheck.FFmpegMissingMessage);
        problem.Should().StartWith("FFmpeg is required for this application, but cannot be found on this machine.");
        asked.Should().Be(0);
    }

    [Fact]
    public void missing_ffprobe_is_reported_when_ffmpeg_is_there_and_the_encoder_is_not_asked_about()
    {
        //Arrange
        var askedAboutEncoder = false;
        var check = new ExternalToolCheck(
            () => true,
            () => false,
            _ => { askedAboutEncoder = true; return false; });

        //Act
        var problem = check.FindProblem(CancellationToken.None);

        //Assert
        problem.Should().Be(ExternalToolCheck.FFprobeMissingMessage);
        problem.Should().StartWith("FFprobe is required for this application, but cannot be found on this machine.");
        askedAboutEncoder.Should().BeFalse();
    }

    [Fact]
    public void missing_svt_av1_is_reported_when_both_tools_are_there()
    {
        //Arrange
        string askedFor = null;
        var check = new ExternalToolCheck(() => true, () => true, name => { askedFor = name; return false; });

        //Act
        var problem = check.FindProblem(CancellationToken.None);

        //Assert
        problem.Should().Be(ExternalToolCheck.SvtAv1MissingMessage);
        askedFor.Should().Be("libsvtav1");
    }

    [Fact]
    public void nothing_is_reported_when_everything_is_there()
    {
        //Arrange
        var check = new ExternalToolCheck(() => true, () => true, _ => true);

        //Act
        var problem = check.FindProblem(CancellationToken.None);

        //Assert
        problem.Should().BeNull();
    }

    [Fact]
    public void an_encoder_list_that_could_not_be_read_is_not_reported_as_a_missing_encoder()
    {
        //Arrange
        var check = new ExternalToolCheck(() => true, () => true, _ => null);

        //Act
        var problem = check.FindProblem(CancellationToken.None);

        //Assert
        problem.Should().BeNull();
    }

    [Fact]
    public async Task the_real_check_answers_without_throwing_whatever_this_machine_has()
    {
        //Arrange
        var check = new ExternalToolCheck();

        //Act
        var problem = await check.FindProblemAsync(TestContext.Current.CancellationToken);

        //Assert - any of the four answers is right for SOME machine; the point is that asking never throws.
        new[]
        {
            null,
            ExternalToolCheck.FFmpegMissingMessage,
            ExternalToolCheck.FFprobeMissingMessage,
            ExternalToolCheck.SvtAv1MissingMessage
        }.Should().Contain(problem);
    }
}
