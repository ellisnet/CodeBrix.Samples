// TextLayoutTests.cs
//
// P12.6 from the completion plan: the TextLayout wrapper and the char-index
// base (G3) it rides on. These run headless against the real text-layout
// add-in - the csproj carries CodeBrixRuntimeIdentifier=skia and the native
// HarfBuzz/Skia assets, proven by the P12.0 spike.

using SilverAssertions;
using Xunit;

namespace Pinta.Brix.Engine.Tests;

public class TextLayoutTests
{
	[Fact]
	public void Char_index_round_trips_across_lines ()
	{
		//Arrange
		TextEngine engine = new (["ab", "cd"]);

		//Act + Assert
		//"ab\ncd" - indices 0..5, where 2 is the newline itself.
		engine.PositionToCharIndex (new TextPosition (0, 0)).Should ().Be (0);
		engine.PositionToCharIndex (new TextPosition (0, 2)).Should ().Be (2);
		engine.PositionToCharIndex (new TextPosition (1, 0)).Should ().Be (3);
		engine.PositionToCharIndex (new TextPosition (1, 2)).Should ().Be (5);

		engine.CharIndexToPosition (0).Should ().Be (new TextPosition (0, 0));
		engine.CharIndexToPosition (2).Should ().Be (new TextPosition (0, 2));
		engine.CharIndexToPosition (3).Should ().Be (new TextPosition (1, 0));
		engine.CharIndexToPosition (5).Should ().Be (new TextPosition (1, 2));
	}

	[Fact]
	public void Char_index_round_trips_over_a_surrogate_pair ()
	{
		//Arrange
		//The emoji is one glyph but TWO UTF-16 chars - the exact case where
		//the char-index base and the old UTF-8 base disagree.
		TextEngine engine = new (["a\U0001F600b"]);

		//Act + Assert
		for (int offset = 0; offset <= engine.Lines[0].Length; offset++) {
			int index = engine.PositionToCharIndex (new TextPosition (0, offset));
			engine.CharIndexToPosition (index).Should ().Be (new TextPosition (0, offset));
		}
	}

	[Fact]
	public void Caret_rectangles_advance_along_the_text ()
	{
		//Arrange
		TextEngine engine = new (["Hello"]);
		TextLayout layout = new () { Engine = engine };

		//Act
		PointI start = layout.TextPositionToPoint (new TextPosition (0, 0));
		PointI middle = layout.TextPositionToPoint (new TextPosition (0, 3));
		PointI end = layout.TextPositionToPoint (new TextPosition (0, 5));

		//Assert
		middle.X.Should ().BeGreaterThan (start.X);
		end.X.Should ().BeGreaterThan (middle.X);
		layout.FontHeight.Should ().BeGreaterThan (0);
	}

	[Fact]
	public void Selection_rectangles_span_a_line_break ()
	{
		//Arrange
		TextEngine engine = new (["ab", "cd"]);
		TextLayout layout = new () { Engine = engine };

		//Act
		engine.PerformHome (control: true, shift: false);
		engine.PerformEnd (control: true, shift: true);
		var rects = layout.GetSelectionRectangles ();

		//Assert
		//One rectangle per visual segment: at least one per selected line.
		rects.Length.Should ().BeGreaterThanOrEqualTo (2);
	}

	[Fact]
	public void Right_alignment_pushes_the_shorter_line_right ()
	{
		//Arrange
		TextEngine engine = new (["aa", "aaaaaa"]);
		TextLayout layout = new () { Engine = engine };
		engine.SetFont (FontDescription.New (), TextAlignment.Right, underline: false);

		//Act
		//G1: alignment needs the measure-then-align second pass; with it, the
		//short first line starts further right than the long second line.
		PointI shortLineStart = layout.TextPositionToPoint (new TextPosition (0, 0));
		PointI longLineStart = layout.TextPositionToPoint (new TextPosition (1, 0));

		//Assert
		shortLineStart.X.Should ().BeGreaterThan (longLineStart.X);
	}

	[Fact]
	public void Empty_text_still_reports_caret_and_bounds ()
	{
		//Arrange
		TextEngine engine = new ();
		TextLayout layout = new () { Engine = engine };

		//Act
		RectangleI caret = layout.GetCursorLocation ();
		RectangleI bounds = layout.GetLayoutBounds ();

		//Assert
		caret.Height.Should ().BeGreaterThan (0);
		bounds.Width.Should ().Be (0);
		bounds.Height.Should ().BeGreaterThan (0);
	}
}
