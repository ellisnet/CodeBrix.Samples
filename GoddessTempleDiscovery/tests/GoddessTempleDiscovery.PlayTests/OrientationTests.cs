using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using GoddessTempleDiscovery.Game.Session;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_letterboxes_the_table_and_its_clicks_still_land()
    {
        _fixture.Application.Orientation.Should().Be(ScreenOrientation.Portrait);
        await ShowTableAsync();
        if (await ReadAsync(() => Model.IsInspectorOpen)) await CloseInspectorAsync();
        var canvas = await _fixture.Canvas.BoundingBoxAsync();
        (canvas.Width, canvas.Height).Should().Be((1080f, 1920f));
        // Re-laying out the canvas must not start a second game host, and the table keeps its 1280 x 800 shape.
        (await ReadAsync(() => ReferenceEquals(_fixture.Host, _fixture.FirstHost))).Should().BeTrue();
        await WaitOnEngineAsync(() => Session.Layout.Width, width => width == TableLayout.BaseWidth, "the table at its base width");
        var height = canvas.Width * TableLayout.Height / TableLayout.BaseWidth;
        var top = (int)(canvas.Y + ((canvas.Height - height) / 2));
        // The engine presents the re-fitted table on its own clock: take screenshots until the bands show.
        var screen = await WaitForScreenAsync(shot =>
            shot.Region(0, 80, 1080, top - 90).DistinctColorCount <= 2 &&
            shot.Region(0, top + (int)height + 10, 1080, 1920 - top - (int)height - 10).DistinctColorCount <= 2);
        // Plain bands above (below the MENU button) and below the table; the table itself in between.
        screen.Region(0, 80, 1080, top - 90).DistinctColorCount.Should().BeLessThanOrEqualTo(2);
        screen.Region(0, top + (int)height + 10, 1080, 1920 - top - (int)height - 10).DistinctColorCount.Should().BeLessThanOrEqualTo(2);
        screen.Region(0, top + 10, 1080, (int)height - 20).DistinctColorCount.Should().BeGreaterThan(50);
        await SnapshotAsync("GoddessTempleDiscovery-portrait-table");

        // The pointer, scaled into the letterboxed table, still finds the trench under it.
        var (x, y) = await _fixture.ToScreenAsync(await OnEngineAsync(() => Session.Layout.SiteSlotCentre(3)));
        y.Should().BeInRange(top, top + (float)height);
        await Page.Mouse.MoveAsync(x, y);
        await WaitOnEngineAsync(() => _fixture.Host.Controls.HoverSlot, slot => slot == 3, "the hovered trench");
        await Page.Mouse.MoveAsync(x, top - 200);
        await WaitOnEngineAsync(() => _fixture.Host.Controls.HoverSlot, slot => slot == -1, "no trench under the band");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Landscape_widens_the_table_to_fill_the_canvas()
    {
        _fixture.Application.Orientation.Should().Be(ScreenOrientation.Landscape);
        await ShowTableAsync();
        if (await ReadAsync(() => Model.IsInspectorOpen)) await CloseInspectorAsync();
        var canvas = await _fixture.Canvas.BoundingBoxAsync();
        (canvas.Width, canvas.Height).Should().Be((1920f, 1080f));
        // 1920 x 1080 widens the 800-high table to 1422: the Site Row and the buttons spread out, nothing is letterboxed.
        await WaitOnEngineAsync(() => Session.Layout.Width, width => width > 1400, "the widened table");
        var screen = await WaitForScreenAsync(shot => shot.Region(200, 0, 1720, 60).DistinctColorCount > 20);
        screen.Region(200, 0, 1720, 60).DistinctColorCount.Should().BeGreaterThan(20);
        screen.Region(0, 1000, 1920, 80).DistinctColorCount.Should().BeGreaterThan(20);
        screen.Region(1860, 100, 60, 900).DistinctColorCount.Should().BeGreaterThan(5);
    }

    [Theory]
    [InlineData("landscape", Traits = new[] { "PlayTestOrientation", "Landscape" })]
    [InlineData("portrait", Traits = new[] { "PlayTestOrientation", "Portrait" })]
    public async Task The_panes_lay_out_on_screen(string orientation)
    {
        _fixture.Application.Orientation.ToString().ToLowerInvariant().Should().Be(orientation);
        await ExpectOnScreenAsync("TitleWordmark", "TitleEpigraph", "NewGame", "Continue", "Settings", "HowToPlay", "History", "Credits");
        await OpenSetupAsync();
        await SetSeatCountAsync(4);
        await ExpectOnScreenAsync("AddSeat", "TurnsPerSeason", "Difficulty", "Seed", "SetupBack", "Start");
        for (var seat = 0; seat < 4; seat++)
        {
            await ExpectOnScreenAsync(Id("SeatTeam").Nth(seat), Id("SeatComputer").Nth(seat), Id("SeatRemove").Nth(seat));
        }

        await SnapshotAsync($"GoddessTempleDiscovery-setup-{orientation}");
        await Id("SetupBack").ClickAsync();
        await Id("Settings").ClickAsync();
        await ExpectOnScreenAsync("SoundEnabled", "ReducedMotion", "RevealComputerDiscoveries", "AnimationSpeed", "CloseSettings");
        await Id("CloseSettings").ClickAsync();
        await Id("Credits").ClickAsync();
        await ExpectOnScreenAsync("HeritageTitle", "CreditsBack");
        await Id("CreditsBack").ClickAsync();
        await Id("History").ClickAsync();
        await ExpectOnScreenAsync("PeopleTab", "TimelineTab", "HistoryBack");
    }

    private async Task<PixelStats> WaitForScreenAsync(System.Func<PixelStats, bool> ready)
    {
        PixelStats shot = null;
        for (var attempt = 0; attempt < 50; attempt++)
        {
            shot = PixelStats.FromPng(await Page.ScreenshotAsync());
            if (ready(shot)) return shot;
            await _fixture.WaitStepsAsync(3);
        }

        return shot;
    }

    private Task ExpectOnScreenAsync(params string[] automationIds)
    {
        var locators = new List<Locator>();
        foreach (var id in automationIds) locators.Add(Id(id));
        return ExpectOnScreenAsync(locators.ToArray());
    }

    private async Task ExpectOnScreenAsync(params Locator[] locators)
    {
        foreach (var locator in locators)
        {
            await Expect(locator).ToBeVisibleAsync();
            var box = await locator.BoundingBoxAsync();
            box.X.Should().BeGreaterThanOrEqualTo(0);
            box.Y.Should().BeGreaterThanOrEqualTo(0);
            (box.X + box.Width).Should().BeLessThanOrEqualTo(_fixture.Application.Width);
            (box.Y + box.Height).Should().BeLessThanOrEqualTo(_fixture.Application.Height);
        }
    }
}
