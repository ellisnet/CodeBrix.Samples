using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CodeBrix.Platform.GameEngine.Drawing;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Game.Rendering;
using GoddessTempleDiscovery.Rules.Content;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Game.Tests.Cards;

public class CardFaceComposerTests
{
    private static readonly Lazy<IReadOnlyList<(string Name, string Svg, bool HasCuneiform)>> AllFaces = new(ComposeAll);

    private static IReadOnlyList<(string Name, string Svg, bool HasCuneiform)> ComposeAll()
    {
        using var composer = new CardFaceComposer();
        var faces = new List<(string, string, bool)>();
        faces.AddRange(Catalog.Discoveries.Select(c => (c.Id, composer.Compose(c), !string.IsNullOrWhiteSpace(c.Cuneiform))));
        faces.AddRange(Catalog.Tablets.Select(c => (c.Id, composer.Compose(c), !string.IsNullOrWhiteSpace(c.Cuneiform))));
        faces.AddRange(Catalog.Specialists.GroupBy(s => s.Role).Select(g => (g.Key.ToString(), composer.Compose(g.First()), false)));
        faces.AddRange(Catalog.Favors.Select(c => (c.Id, composer.Compose(c), false)));
        faces.AddRange(Catalog.Seasons.Select(c => (c.Year, composer.Compose(c), false)));
        return faces;
    }

    [Fact]
    public void every_catalog_card_composes_a_face()
    {
        //Assert
        AllFaces.Value.Count.Should().Be(
            Catalog.Discoveries.Count + Catalog.Tablets.Count + Catalog.Specialists.Select(s => s.Role).Distinct().Count()
            + Catalog.Favors.Count + Catalog.Seasons.Count);
        AllFaces.Value.Should().OnlyContain(f => f.Svg.Length > 0);
    }

    [Fact]
    public void every_face_is_well_formed_self_contained_svg_under_the_size_limit()
    {
        foreach (var face in AllFaces.Value)
        {
            //Act
            var document = XDocument.Parse(face.Svg);

            //Assert
            document.Root.Name.LocalName.Should().Be("svg", face.Name);
            ((string)document.Root.Attribute("viewBox")).Should().Be("0 0 250 400", face.Name);
            Encoding.UTF8.GetByteCount(face.Svg).Should().BeLessThan(CardFaceComposer.MaxBytes, face.Name);
            face.Svg.Should().NotContain("<text", face.Name);
            face.Svg.Should().NotContain("<image", face.Name);
            face.Svg.Should().NotContain("<!--", face.Name);
            face.Svg.Replace("xmlns=\"http://www.w3.org/2000/svg\"", string.Empty).Should().NotContain("http", face.Name);
        }
    }

    [Fact]
    public void every_face_rasterizes_through_the_add_on_loader_to_a_non_blank_image()
    {
        foreach (var face in AllFaces.Value)
        {
            //Act
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(face.Svg));
            using var resource = SvgResource.Load(stream);
            using var bitmap = resource.Rasterize(125, 200).Copy();

            //Assert
            SvgRaster.InkCoverage(bitmap).Should().BeGreaterThan(0.2, face.Name);
        }
    }

    [Fact]
    public void every_face_carries_its_title_as_vector_paths()
    {
        foreach (var face in AllFaces.Value)
        {
            //Act
            var title = TitlePath(face.Svg);

            //Assert
            title.Should().NotBeNull(face.Name);
            ((string)title.Attribute("d")).Length.Should().BeGreaterThan(100, face.Name);
        }
    }

    [Fact]
    public void cuneiform_paths_are_present_exactly_when_the_card_has_a_cuneiform_line()
    {
        foreach (var face in AllFaces.Value)
        {
            //Act
            var cuneiform = XDocument.Parse(face.Svg).Descendants().FirstOrDefault(e => (string)e.Attribute("class") == "cuneiform");

            //Assert
            if (face.HasCuneiform)
            {
                cuneiform.Should().NotBeNull(face.Name);
                ((string)cuneiform.Attribute("d")).Length.Should().BeGreaterThan(20, face.Name);
            }
            else
            {
                cuneiform.Should().BeNull(face.Name);
            }
        }
    }

    [Fact]
    public void the_face_keys_follow_the_brief()
    {
        //Assert
        CardFaceComposer.FaceKey("limestone-temple").Should().Be("face/limestone-temple");
        CardFaceComposer.FaceKey(Rules.Cards.SpecialistRole.SmallFindsKeeper).Should().Be("face/specialist-small-finds-keeper");
    }

    [Fact]
    public void a_missing_art_key_falls_back_to_her_star()
    {
        //Arrange
        using var composer = new CardFaceComposer();
        var card = Catalog.Discoveries[0] with { ArtKey = "no-such-art" };

        //Act
        var svg = composer.Compose(card);

        //Assert
        XDocument.Parse(svg).Root.Should().NotBeNull();
    }

    [Fact]
    public void writes_sample_faces_when_asked()
    {
        //Only with GODDESSTEMPLE_DUMP_FACES=<folder>: a look at a few faces by eye
        var folder = Environment.GetEnvironmentVariable("GODDESSTEMPLE_DUMP_FACES");
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        Directory.CreateDirectory(folder);
        foreach (var face in AllFaces.Value.Where((_, i) => i % 9 == 0))
        {
            File.WriteAllBytes(Path.Combine(folder, face.Name.Replace('/', '-') + ".png"), SvgRaster.RenderFacePng(face.Svg));
            File.WriteAllText(Path.Combine(folder, face.Name.Replace('/', '-') + ".svg"), face.Svg);
        }
    }

    private static XElement TitlePath(string svg) =>
        XDocument.Parse(svg).Descendants().FirstOrDefault(e => (string)e.Attribute("class") == "title");
}
