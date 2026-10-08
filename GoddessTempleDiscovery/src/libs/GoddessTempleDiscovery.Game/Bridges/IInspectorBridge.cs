namespace GoddessTempleDiscovery.Game.Bridges;

/// <summary>
/// What the host asks of the inspector (the view model implements it): show a card large, or close it. The host
/// calls these on the UI thread through the dispatcher it was given.
/// </summary>
public interface IInspectorBridge
{
    /// <summary>Opens the inspector on a card (a newspaper page for discoveries, seasons, tablets and favors).</summary>
    /// <param name="card">The card.</param>
    void ShowCard(CardView card);

    /// <summary>Closes the inspector.</summary>
    void Hide();
}
