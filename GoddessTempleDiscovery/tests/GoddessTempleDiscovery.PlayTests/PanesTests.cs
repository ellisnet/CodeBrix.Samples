using System.Linq;
using System.Threading.Tasks;
using GoddessTempleDiscovery.Game.Credits;
using GoddessTempleDiscovery.Rules.Content;
using Microsoft.UI.Xaml.Controls;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task How_to_play_opens_with_every_section_and_closes_to_the_title()
    {
        await Id("HowToPlay").ClickAsync();
        await Expect(Id("HowToPlayPane")).ToBeVisibleAsync();
        await Expect(Id("TitlePane")).ToBeHiddenAsync();
        var headings = await ReadAsync(() => Model.HowToPlayItems.Select(i => i.Heading).ToArray());
        headings.Should().Contain(new[] { "The premise", "A turn: roll", "Dig", "Study", "Survey", "Publish", "Scoring at the end" });
        await Expect(Page.GetByText("The premise", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Id("HowToPlayItems").GetByType<TextBlock>()).ToHaveCountAsync(await ReadAsync(() =>
            Model.HowToPlayItems.Sum(i => i.Subheading.Length > 0 ? 3 : 2)));
        await Id("HowToPlayBack").ClickAsync();
        await Expect(Id("HowToPlayPane")).ToBeHiddenAsync();
        await Expect(Id("TitlePane")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task History_shows_the_people_and_the_timeline_tabs_and_closes()
    {
        await Id("History").ClickAsync();
        await Expect(Id("HistoryPane")).ToBeVisibleAsync();
        await Expect(Id("People")).ToBeVisibleAsync();
        await Expect(Id("Timeline")).ToBeHiddenAsync();
        (await ReadAsync(() => Model.PeopleItems.Count)).Should().Be(Catalog.People.Count);
        var person = Catalog.People.First();
        await Expect(Id("People").GetByText(person.Name, new() { Exact = true })).ToBeVisibleAsync();

        await Id("TimelineTab").ClickAsync();
        await Expect(Id("Timeline")).ToBeVisibleAsync();
        await Expect(Id("People")).ToBeHiddenAsync();
        (await ReadAsync(() => Model.TimelineItems.Count)).Should().Be(Catalog.Periods.Count);
        await Expect(Id("Timeline").GetByText(Catalog.Periods.First().Name, new() { Exact = true })).ToBeVisibleAsync();

        await Id("PeopleTab").ClickAsync();
        await Expect(Id("People")).ToBeVisibleAsync();
        await Id("HistoryBack").ClickAsync();
        await Expect(Id("HistoryPane")).ToBeHiddenAsync();
        await Expect(Id("TitlePane")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Credits_print_the_heritage_note_verbatim_and_close()
    {
        await Id("Credits").ClickAsync();
        await Expect(Id("CreditsPane")).ToBeVisibleAsync();
        await Expect(Id("HeritageTitle")).ToHaveTextAsync("Note on Cultural Heritage and History");
        await Expect(Id("HeritageFirst")).ToHaveTextAsync(HeritageNote.FirstParagraph);
        await Expect(Id("HeritageFirst")).ToContainTextAsync(
            "I hope to not be another agent of cultural appropriation; but instead to honor the legacy of the people who have lived in the Tigris and Euphrates valleys for millennia;");
        await Expect(Id("HeritageSecond")).ToHaveTextAsync(HeritageNote.SecondParagraph);
        await Expect(Id("HeritageSecond")).ToContainTextAsync("No part of this creative work is intended to celebrate individuals who espoused or were aligned with Nazi ideology");
        (await ReadAsync(() => Model.CreditItems.Select(i => i.Heading).ToArray())).Should().Equal("The art", "The fonts", "The table", "The sources");
        await Expect(Id("CreditItems").GetByText("The fonts", new() { Exact = true })).ToBeVisibleAsync();
        await Id("CreditsBack").ClickAsync();
        await Expect(Id("CreditsPane")).ToBeHiddenAsync();
        await Expect(Id("TitlePane")).ToBeVisibleAsync();
    }
}
