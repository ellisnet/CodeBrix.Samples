namespace Pinta.Brix.Engine.Tests;

partial class BlendOpTests
{
	private static TheoryData<UserBlendOp, ColorBgra, ColorBgra, ColorBgra> CreateMultiplyIOCases (UserBlendOps.MultiplyBlendOp multiplyOp)
	{
		return new () {

			// Semi-transparent over opaque
			{
				multiplyOp,
				ColorBgra.FromBgra (0, 0, 100, 255), // Opaque red
				ColorBgra.FromBgra (0, 100, 0, 128), // 50% transparent green
				ColorBgra.FromBgra (0, 0, 50, 255)
			},

			// Semi-transparent over semi-transparent
			{
				multiplyOp,
				ColorBgra.FromBgra (0, 0, 100, 128), // semi-transparent red
				ColorBgra.FromBgra (0, 100, 0, 128), // semi-transparent green
				ColorBgra.FromBgra (0, 50, 50, 192)
			},

			// Opaque gray over opaque gray
			{
				multiplyOp,
				ColorBgra.FromBgra (128, 128, 128, 255), // Opaque gray
				ColorBgra.FromBgra (128, 128, 128, 255), // Opaque gray
				ColorBgra.FromBgra (64, 64, 64, 255)
			},

			// --- Cases including invalid colors

			// Multiplying with opaque white
			{
				multiplyOp,
				ColorBgra.FromBgra (50, 150, 200, 180), // Some semi-transparent color
				ColorBgra.White, // Opaque white
				ColorBgra.FromBgra (125, 225, 255, 255)
			},

			// Multiplying with opaque black
			{
				multiplyOp,
				ColorBgra.FromBgra (50, 150, 200, 180), // Some semi-transparent color
				ColorBgra.Black, // Opaque black
				ColorBgra.Black
			},

			// Transparent layer on top (should be identity)
			{
				multiplyOp,
				ColorBgra.FromBgra (50, 150, 200, 180),
				ColorBgra.Transparent,
				ColorBgra.FromBgra (50, 150, 200, 180)
			},

			// Transparent layer on bottom (should be identity)
			{
				multiplyOp,
				ColorBgra.Transparent,
				ColorBgra.FromBgra (50, 150, 200, 180),
				ColorBgra.FromBgra (50, 150, 200, 180)
			},
		};
	}
}
