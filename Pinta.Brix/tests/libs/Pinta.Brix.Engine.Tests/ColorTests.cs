using System;
using Color = Pinta.Brix.Engine.Drawing.Color;

namespace Pinta.Brix.Engine.Tests;

public sealed class ColorTests
{
	[Theory]
	[InlineData (1, 0, 0, 0, 1, 1)]
	[InlineData (0, 1, 0, 120, 1, 1)]
	[InlineData (0, 0, 1, 240, 1, 1)]
	[InlineData (0, 0.5, 1, 210, 1, 1)]
	[InlineData (0.2, 0.5, 0.25, 130, 0.6, 0.5)]
	public void ColorToHsv (double r, double g, double b, double h, double s, double v)
	{
		Color c = new (r, g, b);
		HsvColor hsv = c.ToHsv ();
		Assert.Equal (new HsvColor (h, s, v), hsv);
		Color c2 = hsv.ToColor ();
		// assert reversibility; color > hsv > color retains same info
		// floating point rounding
		c2 = new (Math.Round (c2.R, 4), Math.Round (c2.G, 4), Math.Round (c2.B, 4));
		Assert.Equal (c, c2);
	}

	[Theory]
	[InlineData ("FFFFFF", 1, 1, 1, 1)]
	[InlineData ("FFFF", 1, 1, 1, 1)]
	[InlineData ("FFF", 1, 1, 1, 1)]
	[InlineData ("#FFFFFF", 1, 1, 1, 1)]
	[InlineData ("#FFF", 1, 1, 1, 1)]
	[InlineData ("CC33AA99", 0.8, 0.2, 0.6667, 0.6)]
	[InlineData ("#CC33AA99", 0.8, 0.2, 0.6667, 0.6)]
	[InlineData ("C3A9", 0.8, 0.2, 0.6667, 0.6)]
	[InlineData ("C3A", 0.8, 0.2, 0.6667, 1)]
	public void FromHex (string hex, double r, double g, double b, double a)
	{
		Color hc = Color.FromHex (hex)!.Value;
		hc = new (Math.Round (hc.R, 4), Math.Round (hc.G, 4), Math.Round (hc.B, 4), Math.Round (hc.A, 4));
		Color expectedColor = new (r, g, b, a);
		Assert.Equal (expectedColor, hc);
	}

	[Theory]
	[InlineData (0.6, 0, 0.3, 1.0, true, "99004CFF")]
	[InlineData (0.6, 0, 0.3, 1.0, false, "99004C")]
	public void ToHex (double r, double g, double b, double a, bool alpha, string expected)
	{
		Color c = new (r, g, b, a);
		Assert.Equal (expected, c.ToHex (alpha));
	}

	[Theory]
	[InlineData ("CC33AA99", 0.8, 0.2, 0.6667, 0.6)]
	public void FromBgraHexString (string bgraHex, double a, double r, double g, double b)
	{
#pragma warning disable CS0618 // Type or member is obsolete
		Color hc = Color.ParseBgraHexString (bgraHex)!.Value;
#pragma warning restore CS0618
		hc = new (Math.Round (hc.R, 4), Math.Round (hc.G, 4), Math.Round (hc.B, 4), Math.Round (hc.A, 4));
		Color expectedColor = new (r, g, b, a);
		Assert.Equal (expectedColor, hc);
	}

	[Theory]
	[InlineData (0.6, 0, 0.3, 1.0, "99004CFF")]
	public void ToBgraHexString (double a, double r, double g, double b, string expected)
	{
		Color c = new (r, g, b, a);
#pragma warning disable CS0618 // Type or member is obsolete
		string result = Color.ToBgraHexString (c);
#pragma warning restore CS0618
		Assert.Equal (expected, result);
	}
}
