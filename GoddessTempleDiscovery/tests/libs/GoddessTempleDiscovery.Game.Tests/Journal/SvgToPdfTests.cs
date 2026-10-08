using System.IO;
using CodeBrix.PdfDocuments.Drawing;
using CodeBrix.PdfDocuments.Pdf;
using GoddessTempleDiscovery.Game.Journal.Pdf;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Game.Tests.Journal;

public class SvgToPdfTests
{
    private const string Fixture =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 100 100\">" +
        "<defs><linearGradient id=\"g\"><stop offset=\"0\" stop-color=\"#D9A441\"/><stop offset=\"1\" stop-color=\"#000\"/></linearGradient></defs>" +
        "<path d=\"M10,10 L30,10 L20,30 Z\" fill=\"#B8322A\"/>" +
        "<rect x=\"40\" y=\"10\" width=\"20\" height=\"10\" fill=\"url(#g)\" stroke=\"#1E1A17\" stroke-width=\"1\"/>" +
        "<circle cx=\"80\" cy=\"15\" r=\"5\" fill=\"#2A4B8D\" opacity=\"0.5\"/>" +
        "<ellipse cx=\"20\" cy=\"50\" rx=\"8\" ry=\"4\" fill=\"#C8955A\"/>" +
        "<polygon points=\"40,40 60,40 50,60\" fill=\"#7A8A3A\"/>" +
        "<polyline points=\"70,40 80,60 90,40\" fill=\"none\" stroke=\"#1E1A17\" stroke-width=\"2\"/>" +
        "<line x1=\"10\" y1=\"80\" x2=\"90\" y2=\"80\" stroke=\"#A8761F\"/>" +
        "<g transform=\"translate(50,90) scale(2) rotate(45) matrix(1 0 0 1 0 0)\" fill=\"#EDE6D6\"><rect x=\"-2\" y=\"-2\" width=\"4\" height=\"4\"/></g>" +
        "<text x=\"0\" y=\"0\">skipped</text>" +
        "</svg>";

    [Fact]
    public void every_supported_element_kind_is_drawn()
    {
        //Arrange
        using var document = new PdfDocument();
        var page = document.AddPage();
        using var gfx = XGraphics.FromPdfPage(page);

        //Act
        var drawn = SvgToPdf.Draw(gfx, Fixture, new XRect(50, 50, 300, 300));

        //Assert - path, rect, circle, ellipse, polygon, polyline, line and the rect inside the group; the text is skipped
        drawn.Should().Be(8);
        using var stream = new MemoryStream();
        document.Save(stream);
        stream.Length.Should().BeGreaterThan(500);
    }

    [Fact]
    public void transforms_compose_left_to_right_as_svg_says()
    {
        //Act
        var matrix = SvgToPdf.ParseTransform("translate(10,20) scale(2)");
        var point = matrix.Transform(new XPoint(1, 1));

        //Assert - scale first, then translate
        point.X.Should().BeApproximately(12, 1e-9);
        point.Y.Should().BeApproximately(22, 1e-9);
    }

    [Fact]
    public void a_rotation_about_a_point_keeps_that_point()
    {
        //Act
        var point = SvgToPdf.ParseTransform("rotate(90 5 5)").Transform(new XPoint(5, 5));
        var turned = SvgToPdf.ParseTransform("rotate(90)").Transform(new XPoint(1, 0));

        //Assert
        point.X.Should().BeApproximately(5, 1e-9);
        point.Y.Should().BeApproximately(5, 1e-9);
        turned.X.Should().BeApproximately(0, 1e-9);
        turned.Y.Should().BeApproximately(1, 1e-9);
    }
}
