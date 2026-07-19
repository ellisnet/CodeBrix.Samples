using Pinta.Brix.Engine.Drawing;

namespace Pinta.Brix.Engine.Tests;

public sealed class DashPatternTest
{
	[Theory]
	[InlineData (LineCap.Butt, "", new double[] { }, 0.0)]
	[InlineData (LineCap.Butt, "-", new double[] { }, 0.0)]
	[InlineData (LineCap.Butt, " ", new double[] { }, 0.0)]
	[InlineData (LineCap.Butt, " -", new[] { 3.0, 3.0 }, 3.0)]
	[InlineData (LineCap.Butt, "- -", new[] { 3.0, 3.0, 3.0, 0.0 }, 0.0)]
	[InlineData (LineCap.Butt, "-- ", new[] { 6.0, 3.0 }, 0.0)]
	[InlineData (LineCap.Butt, " --", new[] { 6.0, 3.0 }, 6.0)]
	[InlineData (LineCap.Butt, "  -", new[] { 3.0, 6.0 }, 3.0)]
	[InlineData (LineCap.Butt, "$ !-", new[] { 3.0, 9.0 }, 3.0)]
	[InlineData (LineCap.Butt, " - --", new[] { 3.0, 3.0, 6.0, 3.0 }, 12.0)]
	[InlineData (LineCap.Butt, " - - --------", new[] { 3.0, 3.0, 3.0, 3.0, 24.0, 3.0 }, 36.0)]

	[InlineData (LineCap.Square, "", new double[] { }, 0.0)]
	[InlineData (LineCap.Square, "-", new double[] { }, 0.0)]
	[InlineData (LineCap.Square, " ", new double[] { }, 0.0)]
	[InlineData (LineCap.Square, " -", new[] { 1.0, 6.0 }, 2.5)]
	[InlineData (LineCap.Square, "- -", new[] { 1.0, 6.0, 1.0, 3.0 }, 0.0)]
	[InlineData (LineCap.Square, "-- ", new[] { 3.0, 6.0 }, 0.0)]
	[InlineData (LineCap.Square, " --", new[] { 3.0, 6.0 }, 4.5)]
	[InlineData (LineCap.Square, "  -", new[] { 1.0, 9.0 }, 2.5)]
	[InlineData (LineCap.Square, "$ !-", new[] { 1.0, 12.0 }, 2.5)]
	[InlineData (LineCap.Square, " - --", new[] { 1.0, 6.0, 3.0, 6.0 }, 11.5)]
	[InlineData (LineCap.Square, " - - --------", new[] { 1.0, 6.0, 1.0, 6.0, 21.0, 6.0 }, 36.5)]
	public void CreateDashPattern (LineCap line_cap, string pattern, double[] expected_dashes, double expected_offset)
	{
		CairoExtensions.CreateDashPattern (pattern, 3.0, line_cap, out var dashes, out var offset);
		Assert.Equal (expected_dashes, dashes);
		Assert.Equal (expected_offset, offset);
	}
}
