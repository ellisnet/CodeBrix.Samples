using System;
using System.Collections.Generic;
using System.Linq;

namespace Pinta.Brix.Engine.Tests;

public sealed class TextEngineTest
{
	// The string below contains combining characters, so there are fewer text elements than chars.
	private readonly IReadOnlyList<string> test_snippet = [
		"a\u0304\u0308bc\u0327",
		"c\u0327ba\u0304\u0308",
		"bc\u0327a\u0304\u0308"
	];

	private static string LinesToString (string[] lines) => string.Join (Environment.NewLine, lines);

	//was previously: [OneTimeSetUp] calling Pango.Module.Initialize (); the
	//Pinta.Brix TextEngine is engine-pure and needs no toolkit initialization.

	[Fact]
	public void PerformEnter ()
	{
		TextEngine engine = new (["foo", "bar"]);
		engine.SetCursorPosition (new TextPosition (1, 1), true);
		engine.PerformEnter ();

		Assert.Equal (3, engine.LineCount);
		Assert.Equal (LinesToString (["foo", "b", "ar"]), engine.ToString ());
		Assert.Equal (new TextPosition (2, 0), engine.CurrentPosition);
	}

	[Fact]
	public void DeleteMultiLineSelection ()
	{
		TextEngine engine = new (["line 1", "line 2", "line 3"]);
		engine.SetCursorPosition (new TextPosition (0, 2), true);
		engine.PerformDown (true);
		engine.PerformDown (true);
		engine.PerformDelete ();

		Assert.Equal (1, engine.LineCount);
		Assert.Equal (LinesToString (["line 3"]), engine.ToString ());
	}

	[Fact]
	public void DeleteSelection ()
	{
		TextEngine engine = new (["это тест", "это еще один тест"]);
		engine.SetCursorPosition (new TextPosition (0, 2), true);
		engine.PerformDown (true);
		engine.PerformDelete ();

		Assert.Equal (1, engine.LineCount);
		Assert.Equal (LinesToString (["это еще один тест"]), engine.ToString ());
		Assert.Equal (new TextPosition (0, 2), engine.CurrentPosition);
	}

	[Fact]
	public void BackspaceJoinLines ()
	{
		TextEngine engine = new (["foo", "bar"]);
		engine.SetCursorPosition (new TextPosition (1, 0), true);
		engine.PerformBackspace (false);

		Assert.Equal (1, engine.LineCount);
		Assert.Equal ("foobar", engine.ToString ());
		Assert.Equal (new TextPosition (0, 3), engine.CurrentPosition);
	}

	[Fact]
	public void Backspace ()
	{
		TextEngine engine = new (test_snippet);

		// End of a line.
		engine.SetCursorPosition (new TextPosition (0, 6), true);
		engine.PerformBackspace (false);

		Assert.Equal ("a\u0304\u0308b", engine.Lines[0]);
		Assert.Equal (new TextPosition (0, 4), engine.CurrentPosition);

		// First character of a line.
		engine.SetCursorPosition (new TextPosition (1, 2), true);
		engine.PerformBackspace (false);

		Assert.Equal ("ba\u0304\u0308", engine.Lines[1]);
		Assert.Equal (new TextPosition (1, 0), engine.CurrentPosition);

		// Middle of a line.
		engine.SetCursorPosition (new TextPosition (2, 3), true);
		engine.PerformBackspace (false);

		Assert.Equal ("ba\u0304\u0308", engine.Lines[2]);
		Assert.Equal (new TextPosition (2, 1), engine.CurrentPosition);
	}

	[Fact]
	public void ControlBackspace ()
	{
		TextEngine engine = new ([string.Join ("  ", test_snippet)]);

		engine.SetCursorPosition (new TextPosition (0, 19), true);
		engine.PerformBackspace (true);

		Assert.Equal ("a\u0304\u0308bc\u0327  c\u0327ba\u0304\u0308  a\u0304\u0308", engine.Lines[0]);
		Assert.Equal (new TextPosition (0, 16), engine.CurrentPosition);

		engine.PerformBackspace (true);

		Assert.Equal ("a\u0304\u0308bc\u0327  a\u0304\u0308", engine.Lines[0]);
		Assert.Equal (new TextPosition (0, 8), engine.CurrentPosition);

		engine.PerformBackspace (true);

		Assert.Equal ("a\u0304\u0308", engine.Lines[0]);
		Assert.Equal (new TextPosition (0, 0), engine.CurrentPosition);

		engine.PerformBackspace (true);

		Assert.Equal ("a\u0304\u0308", engine.Lines[0]);
		Assert.Equal (new TextPosition (0, 0), engine.CurrentPosition);
	}

	[Fact]
	public void DeleteJoinLines ()
	{
		TextEngine engine = new (["foo", "bar"]);
		engine.SetCursorPosition (new TextPosition (0, 3), true);
		engine.PerformDelete ();

		Assert.Equal (1, engine.LineCount);
		Assert.Equal ("foobar", engine.ToString ());
		Assert.Equal (new TextPosition (0, 3), engine.CurrentPosition);

		// Nothing happens when deleting at the end of the last line.
		engine.SetCursorPosition (new TextPosition (0, 6), true);
		engine.PerformDelete ();

		Assert.Equal (1, engine.LineCount);
		Assert.Equal ("foobar", engine.ToString ());
		Assert.Equal (new TextPosition (0, 6), engine.CurrentPosition);
	}

	[Fact]
	public void Delete ()
	{
		TextEngine engine = new (test_snippet);

		// Beginning of a line.
		engine.SetCursorPosition (new TextPosition (0, 0), true);
		engine.PerformDelete ();

		Assert.Equal ("bc\u0327", engine.Lines[0]);
		Assert.Equal (new TextPosition (0, 0), engine.CurrentPosition);

		// Middle of a line.
		engine.SetCursorPosition (new TextPosition (2, 1), true);
		engine.PerformDelete ();

		Assert.Equal ("ba\u0304\u0308", engine.Lines[2]);
		Assert.Equal (new TextPosition (2, 1), engine.CurrentPosition);

		// End of a line.
		engine.SetCursorPosition (new TextPosition (1, 3), true);
		engine.PerformDelete ();

		Assert.Equal ("c\u0327b", engine.Lines[1]);
		Assert.Equal (new TextPosition (1, 3), engine.CurrentPosition);
	}

	[Fact]
	public void PerformLeftRight ()
	{
		TextEngine engine = new (test_snippet.Append ("a longer line"));

		engine.SetCursorPosition (new TextPosition (0, 3), true);
		engine.PerformRight (false, false);

		Assert.Equal (new TextPosition (0, 4), engine.CurrentPosition);

		engine.PerformRight (false, false);

		Assert.Equal (new TextPosition (0, 6), engine.CurrentPosition);

		engine.PerformRight (false, false);
		engine.PerformRight (false, false);

		Assert.Equal (new TextPosition (1, 2), engine.CurrentPosition);

		engine.PerformLeft (false, false);

		Assert.Equal (new TextPosition (1, 0), engine.CurrentPosition);

		engine.PerformLeft (false, false);

		Assert.Equal (new TextPosition (0, 6), engine.CurrentPosition);

		// Test bug #1824, when going from a longer line up to a shorter line
		engine.SetCursorPosition (new TextPosition (3, 0), true);
		engine.PerformLeft (false, false);
		Assert.Equal (new TextPosition (2, 6), engine.CurrentPosition);

		// Should stay at the beginning / end when attempting to advance further.
		engine.SetCursorPosition (new TextPosition (0, 0), true);
		engine.PerformLeft (false, false);

		Assert.Equal (new TextPosition (0, 0), engine.CurrentPosition);

		TextPosition endPosition = new (engine.LineCount - 1, engine.Lines.Last ().Length);
		engine.SetCursorPosition (endPosition, true);
		engine.PerformRight (false, false);

		Assert.Equal (endPosition, engine.CurrentPosition);
	}

	[Fact]
	public void PerformControlLeftRight ()
	{
		TextEngine engine = new ([string.Join ("  ", test_snippet)]);

		engine.SetCursorPosition (new TextPosition (0, 0), true);
		engine.PerformRight (true, false);

		Assert.Equal (new TextPosition (0, 8), engine.CurrentPosition);

		engine.SetCursorPosition (new TextPosition (0, 7), true);
		engine.PerformRight (true, false);

		Assert.Equal (new TextPosition (0, 8), engine.CurrentPosition);

		engine.PerformRight (true, false);
		engine.PerformRight (true, false);

		Assert.Equal (new TextPosition (0, 22), engine.CurrentPosition);

		engine.PerformLeft (true, false);
		engine.PerformLeft (true, false);

		Assert.Equal (new TextPosition (0, 8), engine.CurrentPosition);

		engine.PerformLeft (true, false);

		Assert.Equal (new TextPosition (0, 0), engine.CurrentPosition);
	}

	[Fact]
	public void PerformUpDown ()
	{
		TextEngine engine = new (test_snippet);

		engine.SetCursorPosition (new TextPosition (1, 2), true);
		engine.PerformUp (false);
		Assert.Equal (new TextPosition (0, 3), engine.CurrentPosition);

		engine.PerformDown (false);
		Assert.Equal (new TextPosition (1, 2), engine.CurrentPosition);
	}
}
