using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CodeBrix.Platform.GameEngine.CardsAndDice.Layout;
using GoddessTempleDiscovery.Game.Bridges;
using GoddessTempleDiscovery.Game.Session;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.PlayTests;

public sealed partial class ApplicationTests
{
    private const int PublishSeed = 7;

    [Fact]
    public async Task Publishing_three_finds_makes_a_report_and_the_score_ribbon_updates()
    {
        // A short game against one computer, three turns a season: the human digs until three finds are in the hand.
        await StartGameAsync(PublishSeed, seats: 2, turns: "3");
        await DigUntilThreeFindsAsync();
        var finds = await OnEngineAsync(() => Human.Hand.Take(3).Select(c => c.Id).ToArray());
        var pointsBefore = await OnEngineAsync(() => Human.PublishedPoints);
        var humanName = await OnEngineAsync(() => Human.Name);

        await _fixture.ClickHudButtonAsync(4);
        await WaitOnEngineAsync(() => _fixture.Host.Controls.Mode, mode => mode == ControlMode.Publish, "the Publish selection mode");
        (await OnEngineAsync(() => _fixture.Host.Frame.Prompt)).Should().Be("Click three or more finds in your hand, then Publish.");
        for (var i = 0; i < finds.Length; i++)
        {
            await ClickHandCardAsync(finds[i]);
            var count = i + 1;
            await WaitOnEngineAsync(() => _fixture.Host.Controls.PublishSelection.Count, n => n == count, $"{count} finds picked");
        }

        (await OnEngineAsync(() => _fixture.Host.Controls.PublishSelection.ToArray())).Should().BeEquivalentTo(finds);
        await _fixture.ClickHudButtonAsync(0);
        var published = await WaitAsync(() => _fixture.Events.OfType<ReportPublished>().FirstOrDefault(), e => e != null, "the ReportPublished event");
        published.TeamName.Should().Be(humanName);
        published.Report.Cards.Select(c => c.Id).Should().BeEquivalentTo(finds);
        (await OnEngineAsync(() => Human.Reports.Count)).Should().Be(1);
        (await OnEngineAsync(() => Human.Hand.Any(c => finds.Contains(c.Id)))).Should().BeFalse();
        var points = await OnEngineAsync(() => Human.PublishedPoints);
        points.Should().BeGreaterThan(pointsBefore);
        (await OnEngineAsync(() => _fixture.Host.Controls.Mode)).Should().Be(ControlMode.Normal);
        // The ribbon (the standings the HUD and the page show) counts the report.
        await WaitAsync(() => Model.Standings.FirstOrDefault(s => s.Label.EndsWith(humanName))?.Value ?? string.Empty,
            value => value.EndsWith("1 reports"), "the ribbon counting the report");
        _fixture.Log.Should().Contain(line => line.Contains($"action: {humanName}") && line.Contains("publish", System.StringComparison.OrdinalIgnoreCase));
        await SnapshotAsync("GoddessTempleDiscovery-published");
    }

    // Rolls and digs on each of the human's turns (closing each newspaper) until three Discoveries are in the hand.
    private async Task DigUntilThreeFindsAsync()
    {
        for (var turn = 0; turn < 12 && await HandDiscoveriesAsync() < 3; turn++)
        {
            if (turn > 0)
            {
                await _fixture.ClickHudButtonAsync(1);
                await WaitForHumanTurnAsync();
            }

            await RollAsync();
            for (var dig = await PlainDigAsync(); dig != null && await HandDiscoveriesAsync() < 3; dig = await PlainDigAsync())
            {
                var finds = await HandDiscoveriesAsync();
                await DigAsync(dig);
                await WaitOnEngineAsync(() => Human.Hand.Count, n => n > finds, "the find in the hand");
                await WaitAsync(() => Model.IsInspectorOpen, open => open, "the newspaper on the find");
                await CloseInspectorAsync();
                // Doubles bring a Favor once both dice are spent; its notice opens too.
                await WaitOnEngineAsync(() => !Session.IsBusy, idle => idle, "the table settled");
                if (await ReadAsync(() => Model.IsInspectorOpen)) await CloseInspectorAsync();
            }
        }

        (await HandDiscoveriesAsync()).Should().BeGreaterThanOrEqualTo(3);
        await WaitOnEngineAsync(() => !Session.IsBusy, idle => idle, "the table settled");
    }

    // Clicks a card of the human's hand where the fan lays it out (left of its centre: a later card may overlap it).
    private async Task ClickHandCardAsync(string id)
    {
        var point = await OnEngineAsync(() =>
        {
            var area = Session.HandArea;
            var cards = area.Pile.Cards;
            var index = cards.ToList().FindIndex(c => TableSession.RulesCard(c) is DiscoveryCard d && d.Id == id);
            var b = area.Bounds;
            var poses = CardLayouts.Arrange(CardLayout.Fan, cards.Count, b.Left, b.Top, b.Width, b.Height, TableLayout.HandCard.Width, TableLayout.HandCard.Height);
            var pose = poses[index];
            return new Vector2(pose.Center.X - (TableLayout.HandCard.Width * 0.25f), pose.Center.Y);
        });
        await _fixture.ClickTableAsync(point);
    }
}
