namespace GoddessTempleDiscovery.Game.Bridges;

/// <summary>One team as the XAML side lists it.</summary>
/// <param name="Name">The team's name.</param>
/// <param name="Colour">The team's colour, a CSS hex value.</param>
/// <param name="Points">The points as they stand.</param>
/// <param name="Workers">The Workers held.</param>
/// <param name="Specialists">The Specialists recruited.</param>
/// <param name="Reports">The reports published.</param>
/// <param name="IsComputer">True for a computer team.</param>
/// <param name="IsCurrent">True when it is this team's turn.</param>
public sealed record TeamView(string Name, string Colour, int Points, int Workers, int Specialists, int Reports, bool IsComputer, bool IsCurrent);
