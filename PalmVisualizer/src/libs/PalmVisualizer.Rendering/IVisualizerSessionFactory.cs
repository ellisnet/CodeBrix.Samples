namespace PalmVisualizer.Rendering;

/// <summary>
/// Builds a Visualize Mode scene for a page. Registered with the dependency-injection
/// container at startup and resolved by the view model, which then owns the session it is
/// handed without knowing how one is made.
/// </summary>
public interface IVisualizerSessionFactory
{
    /// <summary>
    /// Creates a session for the chosen visualizer that will show its view in the host's
    /// panel; the session is not started.
    /// </summary>
    /// <param name="kind">The visualizer to show.</param>
    /// <param name="host">The page that owns the panel the visualizer is shown in.</param>
    /// <returns>The session, ready to be started.</returns>
    IVisualizerSession CreateSession(VisualizerKind kind, IVisualizerCanvasHost host);
}
