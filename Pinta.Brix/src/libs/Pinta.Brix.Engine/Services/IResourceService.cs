// IResourceService.cs
//
// Pinta.Brix replacement for the upstream resource manager's icon service.
// The UI layer implements icon loading from the application's bundled assets;
// the engine only ever asks for a decoded pixel surface.

using Pinta.Brix.Engine.Drawing;

namespace Pinta.Brix.Engine;

public interface IResourceService
{
	/// <summary>Loads a named icon at the requested pixel size.</summary>
	ImageSurface GetIcon (string name, int size = 16);
}
