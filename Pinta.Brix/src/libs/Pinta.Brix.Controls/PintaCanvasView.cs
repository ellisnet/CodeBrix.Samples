// PintaCanvasView.cs
//
// The scrollable host for a document's PintaCanvas: a ScrollViewer with the
// canvas centered inside, implementing the engine's ICanvasScrollView so the
// workspace can zoom/scroll without referencing the UI framework. (Zoom is
// applied by resizing the canvas element; ScrollViewer only ever pans.)

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pinta.Brix.Engine;

namespace Pinta.Brix.Controls;

public sealed class PintaCanvasView : Grid, ICanvasScrollView
{
	private readonly ScrollViewer scroller;
	private readonly Grid canvasHost;

	public PintaCanvas Canvas { get; }

	public PintaCanvasView ()
	{
		Canvas = new PintaCanvas {
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};

		canvasHost = new Grid ();
		canvasHost.Children.Add (Canvas);

		scroller = new ScrollViewer {
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			HorizontalScrollMode = ScrollMode.Enabled,
			VerticalScrollMode = ScrollMode.Enabled,
			ZoomMode = ZoomMode.Disabled,
			Content = canvasHost,
		};

		Children.Add (scroller);
	}

	private Document? document;

	public Document? Document {
		get => document;
		set {
			document = value;
			Canvas.Document = value;
			if (value is not null)
				value.Workspace.CanvasWindow = this;
		}
	}

	// ---- ICanvasScrollView -------------------------------------------------

	public Size ViewportSize =>
		new (
			(int) Math.Max (scroller.ViewportWidth > 0 ? scroller.ViewportWidth : scroller.ActualWidth, 0),
			(int) Math.Max (scroller.ViewportHeight > 0 ? scroller.ViewportHeight : scroller.ActualHeight, 0));

	public PointD ScrollOffset {
		get => new (scroller.HorizontalOffset, scroller.VerticalOffset);
		set => scroller.ChangeView (value.X, value.Y, null, disableAnimation: true);
	}

	public new void UpdateLayout ()
	{
		Canvas.UpdateLayout ();
		scroller.UpdateLayout ();
	}

	public bool GrabFocus ()
	{
		Canvas.GrabFocus ();
		return true;
	}
}
