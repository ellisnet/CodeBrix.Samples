namespace Pinta.Brix.Engine.Tests;

partial class BlendOpTests
{
	private static TheoryData<UserBlendOp, ColorBgra, ColorBgra, ColorBgra> CreateScreenIOCases (UserBlendOps.ScreenBlendOp screenOp)
	{
		return new () {

			// Semi-transparent over opaque
			{
				screenOp,
				ColorBgra.FromBgra (0, 0, 100, 255), // Opaque red
				ColorBgra.FromBgra (0, 100, 0, 128), // 50% transparent green
				ColorBgra.FromBgra (0, 100, 100, 255)
			},

			// Semi-transparent over semi-transparent
			{
				screenOp,
				ColorBgra.FromBgra (0, 0, 100, 128), // semi-transparent red
				ColorBgra.FromBgra (0, 100, 0, 128), // semi-transparent green
				ColorBgra.FromBgra (0, 100, 100, 192)
			},

			// Opaque gray over opaque gray
			{
				screenOp,
				ColorBgra.FromBgra (128, 128, 128, 255), // Opaque gray
				ColorBgra.FromBgra (128, 128, 128, 255), // Opaque gray
				ColorBgra.FromBgra (192, 192, 192, 255)
			},

			// --- Cases including invalid colors

			// Screening with opaque white should result in white
			{
				screenOp,
				ColorBgra.FromBgra (50, 150, 200, 180), // Some semi-transparent color
				ColorBgra.White, // Opaque white
				ColorBgra.White
			},

			// Screening with opaque black should leave the color unchanged (but make it opaque)
			{
				screenOp,
				ColorBgra.FromBgra (50, 150, 200, 180), // Some semi-transparent color
				ColorBgra.Black, // Opaque black
				ColorBgra.FromBgra (50, 150, 200, 255)
			},

			// Transparent layer on top (should be identity)
			{
				screenOp,
				ColorBgra.FromBgra (50, 150, 200, 180),
				ColorBgra.Transparent,
				ColorBgra.FromBgra (50, 150, 200, 180)
			},

			// Transparent layer on bottom (should be identity)
			{
				screenOp,
				ColorBgra.Transparent,
				ColorBgra.FromBgra (50, 150, 200, 180),
				ColorBgra.FromBgra (50, 150, 200, 180)
			},
		};
	}
}
