using System;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using InannaRosette.Reading.Models;
using InannaRosette.Reading.Services;
using InannaRosette.Services;
using InannaRosette.ViewModels;
using InannaRosette.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace InannaRosette.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    public DeckFixture Decks { get; } = new();
    public InterpreterFixture Interpreter { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    protected override Application CreateApplication() => new App(services =>
    {
        services.AddSingleton<IDeckFactory>(Decks);
        services.AddSingleton<IReadingInterpreter>(Interpreter);
    });

    protected override Task BeforeResetAsync()
    {
        Interpreter.Fail = false;
        return Task.CompletedTask;
    }
    // The new page's view model has taken its deck; a seed a test chose applies to that page only.
    protected override Task AfterResetAsync()
    {
        Decks.Seed = DeckFixture.DefaultSeed;
        return Task.CompletedTask;
    }
}

// Every page gets a deck dealt in a known order, so draws, orientations and card names repeat.
public sealed class DeckFixture : IDeckFactory
{
    public const int DefaultSeed = 7;
    public int Seed { get; set; } = DefaultSeed;
    public Deck Create() => new(Seed);

    // The first seed whose top card is the Goddess Inanna's own (card 1), searched in a fixed order.
    public static int SeedWithInannaOnTop()
    {
        for (var seed = 0; ; seed++)
            if (new Deck(seed).Peek().Id == 1) return seed;
    }
}

// The library's real interpreter, with a switch that makes it fail like a broken service.
public sealed class InterpreterFixture : IReadingInterpreter
{
    private readonly ReadingInterpreter _real = new();
    public bool Fail { get; set; }
    public ReadingInterpretation Interpret(RosetteReading reading) => Fail
        ? throw new InvalidOperationException("Fixture interpreter unavailable")
        : _real.Interpret(reading);
}
