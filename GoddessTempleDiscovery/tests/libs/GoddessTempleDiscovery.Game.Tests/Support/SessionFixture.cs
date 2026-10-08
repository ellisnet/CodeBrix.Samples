using System;
using CodeBrix.Platform.GameEngine.CardsAndDice.Table;
using GoddessTempleDiscovery.Game.Session;

namespace GoddessTempleDiscovery.Game.Tests.Support;

/// <summary>One headless table with every card face registered, shared by the session tests (registering is slow).</summary>
public sealed class SessionFixture : IDisposable
{
    public SessionFixture()
    {
        Table = new CardsAndDiceTable(seed: 7) { ReducedMotion = true };
        Artwork = new TableArtwork(Table);
        CardFaceLibrary.ForCatalog().RegisterAll(Artwork);
    }

    public CardsAndDiceTable Table { get; }

    public TableArtwork Artwork { get; }

    public void Dispose() => Table.Dispose();
}
