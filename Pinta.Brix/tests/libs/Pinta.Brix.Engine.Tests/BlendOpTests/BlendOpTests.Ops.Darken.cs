namespace Pinta.Brix.Engine.Tests;

partial class BlendOpTests
{
	private static TheoryData<UserBlendOp, ColorBgra, ColorBgra, ColorBgra> CreateDarkenIOCases (UserBlendOps.DarkenBlendOp darkenOp)
	{
		return new () {

			// Semi-transparent over opaque
			{
				darkenOp,
				ColorBgra.FromBgra (0, 0, 100, 255), // Opaque red
				ColorBgra.FromBgra (0, 100, 0, 128), // 50% transparent green
				ColorBgra.FromBgra (0, 0, 50, 255)
			},

			// Semi-transparent over semi-transparent
			{
				darkenOp,
				ColorBgra.FromBgra (0, 0, 100, 128), // semi-transparent red
				ColorBgra.FromBgra (0, 100, 0, 128), // semi-transparent green
				ColorBgra.FromBgra (0, 50, 50, 192)
			},

			// Opaque gray over opaque lighter gray
			{
				darkenOp,
				ColorBgra.FromBgra (192, 192, 192, 255), // Opaque light gray
				ColorBgra.FromBgra (128, 128, 128, 255), // Opaque gray
				ColorBgra.FromBgra (128, 128, 128, 255)
			},

			// --- Special Cases

			// Blending with opaque white should leave the color unchanged (but make it opaque)
			{
				darkenOp,
				ColorBgra.FromBgra (50, 150, 200, 180), // Some semi-transparent color
				ColorBgra.White, // Opaque white
				ColorBgra.FromBgra (125, 225, 255, 255)
			},

			// Blending with opaque black should result in black
			{
				darkenOp,
				ColorBgra.FromBgra (50, 150, 200, 180), // Some semi-transparent color
				ColorBgra.Black, // Opaque black
				ColorBgra.Black
			},

			// Transparent layer on top (should be identity)
			{
				darkenOp,
				ColorBgra.FromBgra (50, 150, 200, 180),
				ColorBgra.Transparent,
				ColorBgra.FromBgra (50, 150, 200, 180)
			},

			// Transparent layer on bottom (should be identity)
			{
				darkenOp,
				ColorBgra.Transparent,
				ColorBgra.FromBgra (50, 150, 200, 180),
				ColorBgra.FromBgra (50, 150, 200, 180)
			},
		};
	}
}
