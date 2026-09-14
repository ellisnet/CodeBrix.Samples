// SvgPathToPdfTests.cs - the SVG path mini-language parser that turns the deck's emblem art into
// PDF geometry: the commands it accepts, the number-scanning quirks it tolerates, and the
// malformed input it refuses.

using System;
using System.Linq;
using CodeBrix.PdfDocuments.Drawing;
using InannaRosette.Reading.Data;
using InannaRosette.Reading.Models;
using InannaRosette.Reading.Services.Pdf;
using SilverAssertions;
using Xunit;

namespace InannaRosette.Reading.Tests;

public class SvgPathToPdfTests
{
    [Theory]
    // absolute commands
    [InlineData("M10,10 L90,10 L90,90 L10,90 Z")]
    [InlineData("M10,10 H90 V90 H10 Z")]
    [InlineData("M10,50 C10,20 90,20 90,50 Z")]
    [InlineData("M10,50 C10,20 40,20 50,50 S90,80 90,50")]
    [InlineData("M10,50 Q50,10 90,50 T90,90")]
    [InlineData("M20,50 A30,30 0 1 0 80,50 A30,30 0 1 0 20,50 Z")]
    // relative commands
    [InlineData("m10,10 l80,0 l0,80 l-80,0 z")]
    [InlineData("m10,10 h80 v80 h-80 z")]
    [InlineData("m10,50 c0,-30 80,-30 80,0 z")]
    [InlineData("m10,50 c0,-30 30,-30 40,0 s40,30 40,0")]
    [InlineData("m10,50 q40,-40 80,0 t0,40")]
    [InlineData("m20,50 a30,30 0 1 0 60,0 a30,30 0 1 0 -60,0 z")]
    // several sub-paths, and implicit repeated commands
    [InlineData("M10,10 L20,20 30,30 40,40 Z M60,60 L70,70 Z")]
    public void a_representative_path_parses_without_throwing(string pathData)
    {
        //Act
        Func<int> act = () => SvgPathToPdf.CountSegments(pathData);

        //Assert
        act.Should().NotThrow();
        SvgPathToPdf.CountSegments(pathData).Should().BeGreaterThan(0);
    }

    [Fact]
    public void a_square_drawn_with_lines_produces_four_lines_and_a_close()
    {
        //Act
        var segments = SvgPathToPdf.CountSegments("M10,10 L90,10 L90,90 L10,90 Z");

        //Assert
        segments.Should().Be(4);
    }

    [Fact]
    public void an_implicit_line_to_follows_a_move_to()
    {
        //Arrange - the second and third coordinate pairs are implicit L commands
        var explicitForm = SvgPathToPdf.CountSegments("M10,10 L20,20 L30,30");

        //Act
        var implicitForm = SvgPathToPdf.CountSegments("M10,10 20,20 30,30");

        //Assert
        implicitForm.Should().Be(explicitForm);
    }

    [Fact]
    public void a_repeated_command_letter_may_be_left_out()
    {
        //Act
        var segments = SvgPathToPdf.CountSegments("M0,0 C10,0 20,10 20,20 30,20 40,30 40,40");

        //Assert - two cubic segments from one C command letter
        segments.Should().Be(2);
    }

    [Theory]
    [InlineData("M0,0L10-5")]      // a sign begins the next number
    [InlineData("M0,0L.5.5")]      // a second decimal point begins the next number
    [InlineData("M0,0L1e-3 1e-3")] // exponents
    [InlineData("M0,0 L 10 , 20")] // generous whitespace and commas
    [InlineData("M0,0L+10+20")]
    public void numbers_may_be_written_without_separators(string pathData)
    {
        //Act
        var segments = SvgPathToPdf.CountSegments(pathData);

        //Assert
        segments.Should().Be(1);
    }

    [Fact]
    public void arc_flags_may_be_written_without_separators()
    {
        //Arrange - "1150" is largeArc=1, sweep=1, then x=50
        var pathData = "M0,0 a5,5 0 1150 20";

        //Act
        Func<int> act = () => SvgPathToPdf.CountSegments(pathData);

        //Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void an_arc_with_coincident_endpoints_is_omitted()
    {
        //Act
        var segments = SvgPathToPdf.CountSegments("M10,10 A5,5 0 0 1 10,10");

        //Assert
        segments.Should().Be(0);
    }

    [Fact]
    public void an_arc_with_a_zero_radius_degenerates_to_a_line()
    {
        //Act
        var segments = SvgPathToPdf.CountSegments("M10,10 A0,0 0 0 1 50,50");

        //Assert - one straight segment, not a curve
        segments.Should().Be(1);
    }

    [Fact]
    public void a_half_circle_arc_is_emitted_as_two_quarter_turns()
    {
        //Act
        var segments = SvgPathToPdf.CountSegments("M20,50 A30,30 0 0 1 80,50");

        //Assert - pieces of at most ninety degrees
        segments.Should().Be(2);
    }

    [Fact]
    public void an_empty_path_produces_nothing()
    {
        //Assert
        SvgPathToPdf.CountSegments("").Should().Be(0);
        SvgPathToPdf.CountSegments("   ").Should().Be(0);
    }

    [Theory]
    [InlineData("!!!", "must begin with a command")]
    [InlineData("hello", "must begin with a move-to")]
    [InlineData("L10,10", "must begin with a move-to")]
    [InlineData("M10,10 Z 5", "Unexpected number after a close-path")]
    [InlineData("M10,10 L", "ended while a number was expected")]
    [InlineData("M10,10 L,,", "ended while a number was expected")]
    [InlineData("M10,10 Lx", "Expected a number")]
    [InlineData("M0,0 A5,5 0 2 0 10,10", "Arc flag must be 0 or 1")]
    [InlineData("M0,0 A5,5 0 1", "ended while an arc flag was expected")]
    public void malformed_path_data_is_rejected_with_a_format_exception(string pathData, string messageFragment)
    {
        //Act
        var thrown = Record.Exception(() => SvgPathToPdf.CountSegments(pathData));

        //Assert
        thrown.Should().BeOfType<FormatException>();
        thrown.Message.Should().Contain(messageFragment);
    }

    [Fact]
    public void every_layer_of_every_emblem_parses()
    {
        //Act & Assert
        foreach (var emblem in Enum.GetValues<Emblem>())
        {
            foreach (var layer in EmblemArt.Layers(emblem))
            {
                var segments = SvgPathToPdf.CountSegments(layer.PathData);
                segments.Should().BeGreaterThan(0);
            }
        }
    }

    [Fact]
    public void every_layer_of_the_shared_ornaments_parses()
    {
        //Arrange
        var layers = EmblemArt.VenusStar
            .Concat(EmblemArt.RosetteMotif)
            .Concat(EmblemArt.CornerFlourish)
            .ToList();

        //Act & Assert
        foreach (var layer in layers)
        {
            SvgPathToPdf.CountSegments(layer.PathData).Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void Build_turns_a_path_into_pdf_geometry()
    {
        //Act
        var path = SvgPathToPdf.Build("M10,10 L90,10 L90,90 Z", new XRect(0, 0, 200, 200));

        //Assert
        path.Should().NotBeNull();
    }

    [Theory]
    // square, wide, tall, tiny, and a box offset well away from the origin
    [InlineData(0, 0, 100, 100)]
    [InlineData(0, 0, 200, 100)]
    [InlineData(0, 0, 100, 200)]
    [InlineData(20, 40, 44, 70.4)]
    [InlineData(300, 500, 1, 1)]
    public void Build_maps_the_design_box_onto_any_destination(double x, double y, double w, double h)
    {
        //Act
        Func<XGraphicsPath> act = () =>
            SvgPathToPdf.Build("M0,0 L100,0 L100,100 L0,100 Z", new XRect(x, y, w, h));

        //Assert
        act.Should().NotThrow();
        act().Should().NotBeNull();
    }

    [Fact]
    public void Build_takes_an_explicit_scale_and_offset()
    {
        //Act
        Func<XGraphicsPath> act = () => SvgPathToPdf.Build("M0,0 L10,20 Z", sx: 2, sy: 3, dx: 5, dy: 7);

        //Assert
        act.Should().NotThrow();
        act().Should().NotBeNull();
    }

    [Fact]
    public void Build_draws_every_layer_of_every_emblem_into_a_card_sized_box()
    {
        //Arrange - the thumbnail box the report draws a card emblem in
        var box = new XRect(0, 0, 44, 70.4);

        //Act & Assert
        foreach (var emblem in Enum.GetValues<Emblem>())
        {
            foreach (var layer in EmblemArt.Layers(emblem))
            {
                SvgPathToPdf.Build(layer.PathData, box).Should().NotBeNull();
            }
        }
    }

    [Fact]
    public void Build_rejects_malformed_path_data_too()
    {
        //Act
        Action act = () => SvgPathToPdf.Build("not a path", new XRect(0, 0, 100, 100));

        //Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void the_design_box_the_emblems_are_drawn_in_is_one_hundred_units_square()
    {
        //Assert
        SvgPathToPdf.DefaultDesignBox.Should().Be(100.0);
    }
}
