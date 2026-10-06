using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace DRAKON.Brix.PlayTests;

public sealed partial class ApplicationTests
{
    [Theory]
    [InlineData("landscape", Traits = new[] { "PlayTestOrientation", "Landscape" })]
    [InlineData("portrait", Traits = new[] { "PlayTestOrientation", "Portrait" })]
    public async Task Tk_root_matches_the_virtual_screen(string orientation)
    {
        var portrait = orientation == "portrait";
        Fixture.Application.Orientation.Should().Be(portrait ? ScreenOrientation.Portrait : ScreenOrientation.Landscape);
        (await Fixture.TclAsync("list [winfo width .] [winfo height .]")).Should().Be(portrait ? "1080 1920" : "1920 1080");
        (await Page.EvaluateAsync(() => Host.Root.Width)).Should().Be(Fixture.Application.Width);
        int.Parse(await Fixture.TclAsync("expr {[winfo rootx .intro] + [winfo width .intro]}")).Should().BeLessThanOrEqualTo(Fixture.Application.Width);
        await SnapshotAsync($"DRAKON.Brix-{orientation}");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Tk_root_relayouts_after_switching_to_portrait()
    {
        await OpenThroughIntroAsync("01.Insertion.drn");
        (await Fixture.TclAsync("list [winfo width .] [winfo height .]")).Should().Be("1920 1080");
        await Fixture.Application.SetOrientationAsync(ScreenOrientation.Portrait);
        await Fixture.WaitForTclAsync("list [winfo width .] [winfo height .]", "1080 1920");
        await Fixture.WaitForTclAsync("expr {[winfo rootx $mw::canvas] + [winfo width $mw::canvas]}", right => int.Parse(right) <= 1080);
        int.Parse(await Fixture.TclAsync("winfo height .root")).Should().BeGreaterThan(1080);
        await SnapshotAsync("DRAKON.Brix-relayout-portrait");
    }
}
