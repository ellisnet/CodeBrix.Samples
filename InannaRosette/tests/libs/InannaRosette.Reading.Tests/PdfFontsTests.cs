// PdfFontsTests.cs - the embedded Merriweather faces: that they travel inside the library
// assembly, that the licence travels with them, and that registering them is idempotent.

using System;
using System.IO;
using System.Linq;
using InannaRosette.Reading.Services.Pdf;
using SilverAssertions;
using Xunit;

namespace InannaRosette.Reading.Tests;

public class PdfFontsTests
{
    private const string Prefix = "InannaRosette.Reading.Fonts.";

    private static readonly System.Reflection.Assembly Library = typeof(PdfFonts).Assembly;

    public static TheoryData<string> FontResources() => new(
        "Merriweather-Regular.ttf",
        "Merriweather-Bold.ttf",
        "Merriweather-Italic.ttf",
        "Merriweather-BoldItalic.ttf",
        "Merriweather-Light.ttf",
        "Merriweather-LightItalic.ttf",
        "Merriweather-Medium.ttf",
        "Merriweather-MediumItalic.ttf");

    [Theory]
    [MemberData(nameof(FontResources))]
    public void every_face_is_an_embedded_resource_of_the_library(string fileName)
    {
        //Act
        var names = Library.GetManifestResourceNames();

        //Assert
        names.Should().Contain(Prefix + fileName);
    }

    [Theory]
    [MemberData(nameof(FontResources))]
    public void every_face_is_a_real_truetype_file(string fileName)
    {
        //Act
        using var stream = Library.GetManifestResourceStream(Prefix + fileName);

        //Assert
        stream.Should().NotBeNull();
        var header = new byte[4];
        stream.ReadExactly(header);
        // 0x00010000 is the sfnt version of a TrueType outline font.
        header.Should().Equal([0x00, 0x01, 0x00, 0x00]);
        stream.Length.Should().BeGreaterThan(20_000);
    }

    [Fact]
    public void exactly_eight_faces_are_embedded()
    {
        //Act
        var fonts = Library.GetManifestResourceNames()
            .Where(n => n.StartsWith(Prefix) && n.EndsWith(".ttf")).ToList();

        //Assert
        fonts.Should().HaveCount(8);
    }

    [Fact]
    public void the_open_font_licence_travels_with_the_faces()
    {
        //Act
        var names = Library.GetManifestResourceNames();

        //Assert
        names.Should().Contain(Prefix + "OFL-Merriweather.txt");
    }

    [Fact]
    public void the_licence_text_names_the_open_font_licence()
    {
        //Act
        using var stream = Library.GetManifestResourceStream(Prefix + "OFL-Merriweather.txt");
        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();

        //Assert
        text.Should().Contain("SIL OPEN FONT LICENSE");
        text.Should().Contain("Merriweather");
    }

    [Fact]
    public void EnsureRegistered_does_not_throw()
    {
        //Act
        Action act = PdfFonts.EnsureRegistered;

        //Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureRegistered_is_idempotent()
    {
        //Arrange - it is called from the report builder's constructor as well
        PdfFonts.EnsureRegistered();

        //Act
        Action act = () =>
        {
            for (var i = 0; i < 25; i++) PdfFonts.EnsureRegistered();
        };

        //Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void the_two_family_aliases_are_distinct_and_stable()
    {
        //Assert
        PdfFonts.Serif.Should().Be("InannaSerif");
        PdfFonts.SerifLight.Should().Be("InannaSerifLight");
        PdfFonts.SerifLight.Should().NotBe(PdfFonts.Serif);
    }

    [Fact]
    public void the_faces_the_resolver_names_are_the_files_that_are_embedded()
    {
        //Arrange - the resolver builds resource names as prefix + file name
        var names = Library.GetManifestResourceNames();

        //Act & Assert
        foreach (var file in new[]
                 {
                     "Merriweather-Regular.ttf", "Merriweather-Bold.ttf", "Merriweather-Italic.ttf",
                     "Merriweather-BoldItalic.ttf", "Merriweather-Light.ttf", "Merriweather-Medium.ttf",
                     "Merriweather-LightItalic.ttf", "Merriweather-MediumItalic.ttf",
                 })
        {
            names.Should().Contain(Prefix + file);
        }
    }
}
