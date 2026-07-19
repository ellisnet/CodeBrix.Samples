using System;
using Pinta.Brix.Engine.Drawing;
using Pinta.Brix.Engine;

//was previously: namespace Pinta.Effects.Tests;
namespace Pinta.Brix.Effects.Tests;

internal sealed class MockLivePreview : ILivePreview
{
	public RectangleI RenderBounds { get; }

	public bool IsEnabled
		=> throw new NotImplementedException ();

	public ImageSurface LivePreviewSurface
		=> throw new NotImplementedException ();

	internal MockLivePreview (RectangleI renderBounds)
	{
		RenderBounds = renderBounds;
	}
}
