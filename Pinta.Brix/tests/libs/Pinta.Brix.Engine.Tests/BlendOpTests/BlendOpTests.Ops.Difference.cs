namespace Pinta.Brix.Engine.Tests;

partial class BlendOpTests
{
	private static TheoryData<UserBlendOp, ColorBgra, ColorBgra, ColorBgra> CreateDifferenceIOCases (UserBlendOps.DifferenceBlendOp differenceOp)
	{
		return new () {

			// Semi-transparent over opaque
			{
				differenceOp,
				ColorBgra.FromBgra (0, 0, 100, 255), // Opaque red
				ColorBgra.FromBgra (0, 100, 0, 128), // 50% transparent green
				ColorBgra.FromBgra (0, 100, 100, 255)
			},

			// Semi-transparent over semi-transparent
			{
				differenceOp,
				ColorBgra.FromBgra (0, 0, 100, 128), // semi-transparent red
				ColorBgra.FromBgra (0, 100, 0, 128), // semi-transparent green
				ColorBgra.FromBgra (0, 100, 100, 192)
			},

			// Opaque gray over opaque lighter gray
			{
				differenceOp,
				ColorBgra.FromBgra (192, 192, 192, 255), // Opaque light gray
				ColorBgra.FromBgra (128, 128, 128, 255), // Opaque gray
				ColorBgra.FromBgra (64, 64, 64, 255)
			},

			// Blending with opaque white should, in technical terms, invert the backdrop
			{
				differenceOp,
				ColorBgra.FromBgra (50, 150, 200, 255), // Some solid color
				ColorBgra.White, // Opaque white
				ColorBgra.FromBgra (205, 105, 55, 255)
			},

			// Blending with opaque black should result in the other color
			{
				differenceOp,
				ColorBgra.FromBgra (50, 150, 200, 255), // Some solid color
				ColorBgra.Black, // Opaque black
				ColorBgra.FromBgra (50, 150, 200, 255)
			},

			// Transparent layer on top (should be identity)
			{
				differenceOp,
				ColorBgra.Cyan,
				ColorBgra.Transparent,
				ColorBgra.Cyan
			},

			// Transparent layer on bottom (should be identity)
			{
				differenceOp,
				ColorBgra.Transparent,
				ColorBgra.Cyan,
				ColorBgra.Cyan
			},

			// --- Special Cases

			// Blending a semi-transparent color with opaque white
			{
				differenceOp,
				ColorBgra.FromBgra (50, 150, 200, 180), // Some semi-transparent color
				ColorBgra.White, // Opaque white
				ColorBgra.FromBgra (205, 105, 95, 255)
			},

			// Blending a semi-transparent color with opaque black
			{
				differenceOp,
				ColorBgra.FromBgra (50, 150, 200, 180), // Some semi-transparent color
				ColorBgra.Black, // Opaque black
				ColorBgra.FromBgra (50, 150, 200, 255)
			},

			// Transparent layer on top (should be identity)
			{
				differenceOp,
				ColorBgra.FromBgra (50, 150, 200, 180),
				ColorBgra.Transparent,
				ColorBgra.FromBgra (50, 150, 200, 180)
			},

			// Transparent layer on bottom (should be identity)
			{
				differenceOp,
				ColorBgra.Transparent,
				ColorBgra.FromBgra (50, 150, 200, 180),
				ColorBgra.FromBgra (50, 150, 200, 180)
			},
		};
	}
}
