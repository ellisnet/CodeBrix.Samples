using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using InannaRosette.Reading.Data;
using InannaRosette.Reading.Models;
using InannaRosette.Reading.Services;
using InannaRosette.ViewModels;
using InannaRosette.Views;
using Microsoft.UI.Xaml;
using SilverAssertions;
using Xunit;

namespace InannaRosette.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    // The real PDF report is composed in-process: give it longer than a UI update.
    private const float ReportTimeout = 60000;
    private Locator Dialog => Page.GetByRole(AriaRole.Dialog);
    private Locator Status => Page.GetByTestId("StatusText");
    private Locator Stations => Page.GetByTestId("StationsCount");
    private Locator DeckCount => Page.GetByTestId("DeckCount");
    private Locator TrayCaption => Page.GetByTestId("TrayCaption");
    private Locator DialogButton(string name) => Dialog.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
    private Locator CardNamed(string name) => Page.GetByLabel(name, new() { Exact = true });

    private async Task AutoLayAsync()
    {
        await Button("Auto-lay").ClickAsync();
        await Expect(Stations).ToHaveTextAsync("9 of 9 stations filled");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsBusy, busy => !busy);
    }
    private async Task InterpretAsync()
    {
        await Button("Interpret").ClickAsync();
        await Expect(Button("Back to deck")).ToBeVisibleAsync();
    }
    private Task<ReadingCard> CardAtAsync(int station) => Page.EvaluateAsync(() => Fixture.Model.CardAt(station));
    private async Task<(float X, float Y)> AltarCornerAsync()
    {
        var altar = await Page.EvaluateAsync(() => ((FrameworkElement)Fixture.View.FindName("AltarBorder"))
            .TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0)));
        // Well inside the altar's top-left corner, far from every station of the rosette.
        return ((float)altar.X + 70, (float)altar.Y + 90);
    }
    private string DataFile(string name) => Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N") + "-" + name);

    [Fact]
    public async Task Startup_shows_full_deck_and_gates_commands()
    {
        await Expect(DeckCount).ToHaveTextAsync("40 cards remain");
        await Expect(TrayCaption).ToHaveTextAsync("No cards drawn yet");
        await Expect(Stations).ToHaveTextAsync("0 of 9 stations filled");
        await Expect(Status).ToHaveTextAsync("Click the deck (or press Draw) to take a card, then drag it onto a petal.");
        foreach (var name in new[] { "Shuffle", "Draw", "Auto-lay", "Open reading…" })
            await Expect(Button(name)).ToBeEnabledAsync();
        foreach (var name in new[] { "Clear", "Interpret", "Create PDF", "Save reading…" })
            await Expect(Button(name)).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("StaleBadge")).ToBeHiddenAsync();
        await SnapshotAsync("InannaRosette-startup");
    }

    [Fact]
    public async Task Draw_moves_a_card_to_the_tray()
    {
        await Button("Draw").ClickAsync();
        await Expect(TrayCaption).ToHaveTextAsync("1 card waiting");
        await Expect(DeckCount).ToHaveTextAsync("39 cards remain");
        await Expect(Button("Clear")).ToBeEnabledAsync();
        await Expect(Button("Interpret")).ToBeDisabledAsync();
        var drawn = await Page.EvaluateAsync(() => Fixture.Model.Cards.Single());
        drawn.InTray.Should().BeTrue();
        await Expect(CardNamed(drawn.Card.Name)).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Clicking_the_deck_stack_draws_a_card()
    {
        await Page.GetByTestId("DeckStack").ClickAsync();
        await Expect(TrayCaption).ToHaveTextAsync("1 card waiting");
        await Expect(DeckCount).ToHaveTextAsync("39 cards remain");
        await Expect(Status).ToHaveTextAsync(
            "Drag a card from the tray onto a petal of the rosette. Double-click a laid card to reverse it.");
    }

    [Fact]
    public async Task Auto_lay_fills_all_nine_stations()
    {
        await AutoLayAsync();
        await Expect(Button("Auto-lay")).ToBeDisabledAsync();
        await Expect(Button("Interpret")).ToBeEnabledAsync();
        await Expect(Button("Save reading…")).ToBeEnabledAsync();
        await Expect(DeckCount).ToHaveTextAsync("31 cards remain");
        await Expect(Status).ToHaveTextAsync("All nine stations are filled. Press Interpret to read the rosette.");
        for (var station = 0; station < MainViewModel.StationCount; station++)
            await Expect(CardNamed((await CardAtAsync(station)).Card.Name)).ToBeVisibleAsync();
        await SnapshotAsync("InannaRosette-laid");
    }

    [Fact]
    public async Task Shuffle_returns_tray_cards_to_the_deck()
    {
        await Button("Draw").ClickAsync();
        await Expect(TrayCaption).ToHaveTextAsync("1 card waiting");
        await Button("Draw").ClickAsync();
        await Expect(TrayCaption).ToHaveTextAsync("2 cards waiting");
        await Button("Shuffle").ClickAsync();
        await Expect(TrayCaption).ToHaveTextAsync("No cards drawn yet");
        await Expect(DeckCount).ToHaveTextAsync("40 cards remain");
        await Expect(Status).ToHaveTextAsync("The tray returned to the deck and it is shuffled; 40 cards remain.");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsBusy, busy => !busy);
        await Expect(Button("Clear")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Clear_cancel_keeps_the_table()
    {
        await AutoLayAsync();
        await Button("Clear").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Clear the rosette?");
        await DialogButton("Cancel").ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
        await Expect(Stations).ToHaveTextAsync("9 of 9 stations filled");
        await Expect(DeckCount).ToHaveTextAsync("31 cards remain");
    }

    [Fact]
    public async Task Clear_confirm_empties_the_table()
    {
        await AutoLayAsync();
        await Button("Clear").ClickAsync();
        await DialogButton("Clear").ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
        await Expect(Stations).ToHaveTextAsync("0 of 9 stations filled");
        await Expect(DeckCount).ToHaveTextAsync("40 cards remain");
        await Expect(Status).ToHaveTextAsync("The rosette is clear and every card is back under the deck; 40 cards remain.");
        await Expect(Button("Clear")).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Cards.Count)).Should().Be(0);
    }

    [Fact]
    public async Task Interpret_shows_panel_and_back_to_deck_restores_rail()
    {
        await AutoLayAsync();
        await InterpretAsync();
        await Expect(Button("Create PDF Report")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("DeckStack")).ToBeHiddenAsync();
        var title = await Page.EvaluateAsync(() => Fixture.Model.Interpretation.Title);
        title.Should().NotBeNullOrWhiteSpace();
        await Expect(Page.GetByTestId("InterpretationTitle")).ToHaveTextAsync(title);
        await Expect(Status).ToHaveTextAsync("Read the interpretation, then create a PDF report to keep or share it.");
        await Expect(Button("Create PDF")).ToBeEnabledAsync();
        await SnapshotAsync("InannaRosette-interpretation");
        await Button("Back to deck").ClickAsync();
        await Expect(Button("Back to deck")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("DeckStack")).ToBeVisibleAsync();
        await Expect(DeckCount).ToHaveTextAsync("31 cards remain");
    }

    [Fact]
    public async Task Interpret_subtitle_names_the_querent()
    {
        await Page.GetByTestId("Querent").FillAsync("Ninshubur");
        await AutoLayAsync();
        await InterpretAsync();
        // The reading is dated when it is built, so only the shape of the date is fixed.
        await Expect(Page.GetByTestId("InterpretationSubtitle"))
            .ToHaveTextAsync(new Regex(@"^A reading for Ninshubur · \d{1,2} [A-Z][a-z]+ \d{4}$"));
    }

    [Fact]
    public async Task Changing_the_table_marks_reading_stale()
    {
        await AutoLayAsync();
        await InterpretAsync();
        await Expect(Page.GetByTestId("StaleBadge")).ToBeHiddenAsync();
        var heart = await CardAtAsync(0);
        await CardNamed(heart.Card.Name).ClickAsync(new() { ClickCount = 2 });
        await Expect(Page.GetByTestId("StaleBadge")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.IsInterpretationStale)).Should().BeTrue();
    }

    [Fact]
    public async Task Save_reading_writes_json_with_suggested_name()
    {
        await Page.GetByTestId("Querent").FillAsync("Ninshubur");
        await AutoLayAsync();
        var path = DataFile("saved.json");
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Button("Save reading…").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Reading saved to " + path);
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be("Ninshubur-rosette.json");
        var saved = new ReadingSerializer().FromJson(File.ReadAllText(path));
        saved.Querent.Should().Be("Ninshubur");
        saved.Placements.Should().HaveCount(9);
        foreach (var placement in saved.Placements)
        {
            var laid = await CardAtAsync(placement.Position.Index);
            placement.Card.Id.Should().Be(laid.Card.Id);
            placement.IsReversed.Should().Be(laid.IsReversed);
        }
    }

    [Fact]
    public async Task Save_reading_cancel_writes_nothing()
    {
        await AutoLayAsync();
        Fixture.Application.FilePickers.EnqueueSaveFile(null);
        await Button("Save reading…").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.SaveFileRequestCount, count => count == 1);
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be("Reading-rosette.json");
        await Expect(Status).ToHaveTextAsync("All nine stations are filled. Press Interpret to read the rosette.");
        Directory.EnumerateFiles(Fixture.DataDirectory, "*Reading-rosette.json").Should().BeEmpty();
    }

    [Fact]
    public async Task Open_reading_lays_saved_cards_and_orientations()
    {
        var reading = new RosetteReading { Querent = "Enheduanna", Question = "What does the Goddess ask of me?" };
        reading.Placements.Add(new PlacedCard(RosetteSpread.Positions[0], DeckData.Cards[0], false));
        reading.Placements.Add(new PlacedCard(RosetteSpread.Positions[4], DeckData.Cards[5], true));
        reading.Placements.Add(new PlacedCard(RosetteSpread.Positions[8], DeckData.Cards[12], false));
        var path = DataFile("opened.json");
        File.WriteAllText(path, new ReadingSerializer().ToJson(reading));
        Fixture.Application.FilePickers.EnqueueOpenFile(path);
        await Button("Open reading…").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Opened a reading with 3 cards laid; 37 cards remain in the deck.");
        await Expect(Stations).ToHaveTextAsync("3 of 9 stations filled");
        await Expect(Page.GetByTestId("Querent")).ToHaveValueAsync("Enheduanna");
        await Expect(Page.GetByTestId("Question")).ToHaveValueAsync("What does the Goddess ask of me?");
        foreach (var placement in reading.Placements)
        {
            var laid = await CardAtAsync(placement.Position.Index);
            laid.Card.Id.Should().Be(placement.Card.Id);
            laid.IsReversed.Should().Be(placement.IsReversed);
            await Expect(CardNamed(placement.Card.Name)).ToBeVisibleAsync();
        }
        // Holy Inanna's card was laid from the file, not drawn, so She gives no blessing.
        await Expect(Page.GetByText(MainViewModel.InannaBlessing, new() { Exact = true })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Open_malformed_reading_shows_error_dialog()
    {
        var path = DataFile("malformed.json");
        File.WriteAllText(path, "{ this is not a rosette reading");
        Fixture.Application.FilePickers.EnqueueOpenFile(path);
        await Button("Open reading…").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("That reading could not be opened");
        await DialogButton("Close").ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
        await Expect(Stations).ToHaveTextAsync("0 of 9 stations filled");
        await Expect(DeckCount).ToHaveTextAsync("40 cards remain");
    }

    [Fact]
    public async Task Create_pdf_writes_a_pdf_and_shows_saved_dialog()
    {
        await Page.GetByTestId("Querent").FillAsync("Ninshubur");
        await AutoLayAsync();
        await InterpretAsync();
        var path = DataFile("report.pdf");
        Fixture.Application.FilePickers.EnqueueSaveFile(path);
        await Button("Create PDF").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Report saved", new() { Timeout = ReportTimeout });
        await Expect(Dialog).ToContainTextAsync(path);
        await Expect(Status).ToHaveTextAsync("Report saved to " + path);
        var interpretation = await Page.EvaluateAsync(() => Fixture.Model.Interpretation);
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be(new PdfReportBuilder().SuggestedFileName(interpretation));
        Encoding.ASCII.GetString(File.ReadAllBytes(path), 0, 5).Should().Be("%PDF-");
        await SnapshotAsync("InannaRosette-report-saved");
        // "Done" only: the dialog's "Open" button hands the file to a real PDF viewer.
        await DialogButton("Done").ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
        await Expect(Button("Create PDF")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Create_pdf_cancel_reports_not_saved()
    {
        await AutoLayAsync();
        await InterpretAsync();
        Fixture.Application.FilePickers.EnqueueSaveFile(null);
        await Button("Create PDF Report").ClickAsync();
        await Expect(Status).ToHaveTextAsync("The report was not saved.", new() { Timeout = ReportTimeout });
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(1);
        await Expect(Dialog).ToHaveCountAsync(0);
        await Expect(Button("Create PDF Report")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Drag_tray_card_onto_a_petal_snaps_to_station()
    {
        await Button("Draw").ClickAsync();
        await Expect(TrayCaption).ToHaveTextAsync("1 card waiting");
        var drawn = await Page.EvaluateAsync(() => Fixture.Model.Cards.Single());
        var card = CardNamed(drawn.Card.Name);
        await Expect(card).ToBeVisibleAsync();
        var from = await card.BoundingBoxAsync();
        var to = await Page.GetByTestId("Station2").BoundingBoxAsync();
        await card.DragByAsync(to.X + to.Width / 2 - (from.X + from.Width / 2), to.Y + to.Height / 2 - (from.Y + from.Height / 2));
        await Expect(Stations).ToHaveTextAsync("1 of 9 stations filled");
        await Expect(TrayCaption).ToHaveTextAsync("No cards drawn yet");
        await Expect(Status).ToHaveTextAsync($"{drawn.Card.Name} laid at {RosetteSpread.Positions[2].Title}.");
        (await CardAtAsync(2)).Should().BeSameAs(drawn);
    }

    [Fact]
    public async Task Drag_to_empty_space_returns_card_to_tray()
    {
        await AutoLayAsync();
        var heart = await CardAtAsync(0);
        var card = CardNamed(heart.Card.Name);
        var from = await card.BoundingBoxAsync();
        var to = await AltarCornerAsync();
        await card.DragByAsync(to.X - (from.X + from.Width / 2), to.Y - (from.Y + from.Height / 2));
        await Expect(Stations).ToHaveTextAsync("8 of 9 stations filled");
        await Expect(TrayCaption).ToHaveTextAsync("1 card waiting");
        await Expect(Status).ToHaveTextAsync("Drag a drawn card onto a petal, or press Interpret to read what is already laid.");
        (await CardAtAsync(0)).Should().BeNull();
        heart.InTray.Should().BeTrue();
    }

    [Fact]
    public async Task Double_click_reverses_a_laid_card()
    {
        await AutoLayAsync();
        var heart = await CardAtAsync(0);
        var reversed = heart.IsReversed;
        await CardNamed(heart.Card.Name).ClickAsync(new() { ClickCount = 2 });
        await Fixture.Application.WaitForAsync(() => heart.IsReversed, value => value != reversed);
        await Expect(Status).ToHaveTextAsync($"{heart.Card.Name} is now {(reversed ? "upright" : "reversed")}.");
        await Expect(Page.GetByTestId("CardDetail")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Single_click_opens_lore_panel_and_scene_click_closes_it()
    {
        await AutoLayAsync();
        var heart = await CardAtAsync(0);
        await CardNamed(heart.Card.Name).ClickAsync();
        var detail = Page.GetByTestId("CardDetail");
        await Expect(detail).ToBeVisibleAsync();
        await Expect(detail).ToContainTextAsync(heart.Card.Epithet);
        await SnapshotAsync("InannaRosette-lore");
        var corner = await AltarCornerAsync();
        await Page.Mouse.ClickAsync(corner.X, corner.Y);
        await Expect(detail).ToHaveCountAsync(0);
        await Expect(Stations).ToHaveTextAsync("9 of 9 stations filled");
    }

    [Fact]
    public async Task Hover_close_badge_returns_card_to_tray()
    {
        await AutoLayAsync();
        var heart = await CardAtAsync(0);
        var card = CardNamed(heart.Card.Name);
        var badge = card.GetByTestId("ReturnToTray");
        await Expect(badge).ToBeHiddenAsync();
        await card.HoverAsync();
        await Expect(badge).ToBeVisibleAsync();
        await badge.ClickAsync();
        await Expect(Stations).ToHaveTextAsync("8 of 9 stations filled");
        await Expect(TrayCaption).ToHaveTextAsync("1 card waiting");
        await Expect(Status).ToHaveTextAsync($"{heart.Card.Name} returned to the tray.");
    }

    [Fact]
    public async Task Drawing_the_card_of_Holy_Inanna_shows_Her_blessing()
    {
        Fixture.Decks.Seed = DeckFixture.SeedWithInannaOnTop();
        await Fixture.ResetAsync();
        var blessing = Page.GetByText(MainViewModel.InannaBlessing, new() { Exact = true });
        await Expect(blessing).ToHaveCountAsync(0);
        await Button("Draw").ClickAsync();
        await Expect(blessing).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Cards.Single().Card.Id)).Should().Be(1);
        await Expect(TrayCaption).ToHaveTextAsync("1 card waiting");
        // The blessing plays for about two seconds, then leaves the altar empty again.
        await Expect(blessing).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Interpreter_failure_shows_dialog_and_clears_busy()
    {
        Fixture.Interpreter.Fail = true;
        await AutoLayAsync();
        await Button("Interpret").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("The reading could not be composed");
        await Expect(Dialog).ToContainTextAsync("Fixture interpreter unavailable");
        await DialogButton("Close").ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
        (await Page.EvaluateAsync(() => Fixture.Model.IsBusy)).Should().BeFalse();
        await Expect(Button("Interpret")).ToBeEnabledAsync();
        await Expect(Button("Back to deck")).ToBeHiddenAsync();
        Fixture.Interpreter.Fail = false;
        await InterpretAsync();
    }
}
