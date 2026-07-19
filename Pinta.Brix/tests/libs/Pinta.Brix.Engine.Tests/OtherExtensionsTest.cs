namespace Pinta.Brix.Engine.Tests;

public sealed class OtherExtensionsTest
{
	[Theory]
	[MemberData (nameof (create_polygon_set_arguments_for_empty))]
	public void EmptyStencilReturnsEmptyPolygonSet (RectangleD bounds, PointI translateOffset)
	{
		BitMask bitmask = new (0, 0);
		var polygonSet = bitmask.CreatePolygonSet (bounds, translateOffset);
		Assert.Empty (polygonSet);
	}

	public static readonly TheoryData<RectangleD, PointI> create_polygon_set_arguments_for_empty = new () {
		{ new RectangleD (0, 0, 1, 1), new PointI (1, 1) },
	};
}
