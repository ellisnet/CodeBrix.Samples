// TextLayoutSpikeTests.cs
//
// P12.0 from the completion plan: prove that CodeBrix.Platform.TextLayout can
// be called from a plain test project - no application, no visual tree - before
// any of the text tool is ported onto it. If these pass, the rest of P12 is
// ordinary work; if they fail, nothing downstream is worth starting.
//
// The ref-assembly trap is the thing being tested as much as the layout: the
// csproj sets CodeBrixRuntimeIdentifier=skia, and without it every call here
// would throw NotSupportedException("Ref assembly").

using CodeBrix.Platform.UI.TextLayout;
using SilverAssertions;
using Xunit;

namespace Pinta.Brix.Engine.Tests;

public class TextLayoutSpikeTests
{
	[Fact]
	public void Layout_measures_non_empty_text ()
	{
		//Arrange
		const string text = "Hello";

		//Act
		using TextLayoutResult layout = TextLayoutEngine.Layout (text, "sans-serif", 24f);

		//Assert
		layout.Text.Should ().Be (text);
		layout.Size.Width.Should ().BeGreaterThan (0f);
		layout.Size.Height.Should ().BeGreaterThan (0f);
		layout.LineCount.Should ().Be (1);
	}

	[Fact]
	public void Layout_reports_a_caret_rectangle_for_every_index ()
	{
		//Arrange
		const string text = "Hello";

		//Act
		using TextLayoutResult layout = TextLayoutEngine.Layout (text, "sans-serif", 24f);

		//Assert
		//Index 0 through Length inclusive - the caret can sit after the last
		//character, which is what an editor needs.
		for (int i = 0; i <= text.Length; i++) {
			var caret = layout.GetCaretRect (i);
			caret.Height.Should ().BeGreaterThan (0f);
		}
	}

	[Fact]
	public void Layout_hit_tests_back_to_the_index_it_came_from ()
	{
		//Arrange
		using TextLayoutResult layout = TextLayoutEngine.Layout ("Hello", "sans-serif", 24f);
		var caret = layout.GetCaretRect (3);

		//Act
		//Probe OFF-CENTRE: a probe at a cluster's exact horizontal centre
		//resolves to the cluster END, so a centred probe is a coin toss.
		int index = layout.GetNearestIndexAt (new SkiaSharp.SKPoint (caret.Left + 1f, caret.MidY));

		//Assert
		index.Should ().Be (3);
	}

	[Fact]
	public void Layout_returns_selection_rectangles_for_a_range ()
	{
		//Arrange
		using TextLayoutResult layout = TextLayoutEngine.Layout ("Hello world", "sans-serif", 24f);

		//Act
		var rects = layout.GetSelectionRects (0, 5);

		//Assert
		rects.Should ().NotBeEmpty ();
		rects[0].Width.Should ().BeGreaterThan (0f);
	}

	[Fact]
	public void Layout_yields_an_outline_path_for_stroked_text ()
	{
		//Arrange
		using TextLayoutResult layout = TextLayoutEngine.Layout ("Hello", "sans-serif", 24f);

		//Act
		//This is the path Pinta needs: upstream strokes the text outline via
		//PangoCairo.LayoutPath, which has no other equivalent here.
		using SkiaSharp.SKPath path = layout.GetOutlinePath ();

		//Assert
		path.PointCount.Should ().BeGreaterThan (0);
		path.Bounds.Width.Should ().BeGreaterThan (0f);
	}

	[Fact]
	public void Layout_breaks_lines_where_the_text_does ()
	{
		//Arrange
		const string text = "one\ntwo\nthree";

		//Act
		using TextLayoutResult layout = TextLayoutEngine.Layout (text, "sans-serif", 16f);

		//Assert
		//MaxWidth is null, so lines exist only where the text itself breaks
		//them - which is exactly what a text editor holding its own line list
		//wants from the layout engine.
		layout.LineCount.Should ().Be (3);
	}
}
