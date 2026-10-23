using Microsoft.UI.Xaml.Controls;

namespace PalmVisualizer.Rendering;

/// <summary>
/// The page that shows the visualizer, seen as the one thing this library needs from it: the
/// panel a visualizer session puts its view into. The hosting page implements it and hands
/// itself to the view model; the view model passes it to
/// <see cref="IVisualizerSessionFactory.CreateSession"/> without ever naming a view type.
/// </summary>
public interface IVisualizerCanvasHost
{
    /// <summary>
    /// The panel a session adds its view to when it starts and removes it from when it stops.
    /// Only one session's view is in it at a time.
    /// </summary>
    Panel Container { get; }
}
