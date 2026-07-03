using System.Collections.Generic;
using SilverAssertions;
using WikipediaPublisher.RenderArticle.Internal;
using Xunit;

namespace WikipediaPublisher.RenderArticle.Tests.Internal;

public class AttributionFormatterTests
{
    private static Dictionary<string, string> Meta(params (string Key, string Value)[] pairs)
    {
        var dict = new Dictionary<string, string>();
        foreach (var (key, value) in pairs) { dict[key] = value; }
        return dict;
    }

    [Fact]
    public void Format_combines_author_and_licence()
    {
        //Arrange — Artist is HTML with a link, as Wikimedia returns it
        var metadata = Meta(
            ("Artist", "<a href=\"//commons.wikimedia.org/wiki/User:JD\" title=\"User:JD\">Jane Doe</a>"),
            ("LicenseShortName", "CC BY-SA 4.0"));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert
        line.Should().Be("Jane Doe · CC BY-SA 4.0");
    }

    [Fact]
    public void Format_decodes_html_entities_in_the_author_name()
    {
        //Arrange
        var metadata = Meta(
            ("Artist", "Gerard &amp; Sons"),
            ("LicenseShortName", "Public domain"));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert
        line.Should().Be("Gerard & Sons · Public domain");
    }

    [Fact]
    public void Format_returns_licence_only_when_no_author()
    {
        //Arrange
        var metadata = Meta(("LicenseShortName", "Public domain"));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert
        line.Should().Be("Public domain");
    }

    [Fact]
    public void Format_falls_back_to_the_attribution_field_for_the_name()
    {
        //Arrange — no Artist, but an explicit preferred Attribution credit
        var metadata = Meta(
            ("Attribution", "Photo by A. Photographer"),
            ("LicenseShortName", "CC BY 3.0"));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert
        line.Should().Be("Photo by A. Photographer · CC BY 3.0");
    }

    [Fact]
    public void Format_treats_placeholder_authors_as_no_author()
    {
        //Arrange
        var metadata = Meta(
            ("Artist", "Unknown author"),
            ("LicenseShortName", "CC BY-SA 3.0"));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert — the placeholder name is dropped, licence remains
        line.Should().Be("CC BY-SA 3.0");
    }

    [Theory]
    [InlineData("CC0")]
    [InlineData("CC0 1.0")]
    [InlineData("CCO")] //the "letter O" look-alike Wikimedia/fonts sometimes produce
    public void Format_rewrites_meaningless_cc0_as_public_domain(string licenseValue)
    {
        //Arrange — a bare "CC0" code means nothing to a reader, but it IS a public-domain dedication
        var metadata = Meta(("LicenseShortName", licenseValue));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert
        line.Should().Be("Public domain");
    }

    [Fact]
    public void Format_shows_the_author_with_public_domain_for_a_cc0_license()
    {
        //Arrange
        var metadata = Meta(
            ("Artist", "Jane Doe"),
            ("LicenseShortName", "CC0"));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert — CC0 is rewritten to the plain-language "Public domain"
        line.Should().Be("Jane Doe · Public domain");
    }

    [Fact]
    public void Format_collapses_a_doubled_placeholder_author()
    {
        //Arrange — Wikimedia emits "Unknown artist Unknown artist" for some public-domain files
        var metadata = Meta(
            ("Artist", "Unknown artist Unknown artist"),
            ("LicenseShortName", "Public domain"));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert — the doubled placeholder collapses and is dropped, leaving just the licence
        line.Should().Be("Public domain");
    }

    [Fact]
    public void Format_collapses_a_doubled_real_author()
    {
        //Arrange
        var metadata = Meta(
            ("Artist", "Jane Doe Jane Doe"),
            ("LicenseShortName", "CC BY-SA 4.0"));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert
        line.Should().Be("Jane Doe · CC BY-SA 4.0");
    }

    [Fact]
    public void Format_does_not_repeat_an_author_that_equals_the_license()
    {
        //Arrange
        var metadata = Meta(
            ("Attribution", "Public domain"),
            ("LicenseShortName", "Public domain"));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert — "Public domain · Public domain" collapses to one
        line.Should().Be("Public domain");
    }

    [Fact]
    public void Format_returns_empty_when_nothing_usable_is_present()
    {
        //Arrange
        var metadata = Meta(("Credit", "Own work"));

        //Act
        var line = AttributionFormatter.Format(metadata);

        //Assert
        line.Should().BeEmpty();
    }

    [Fact]
    public void Format_returns_empty_for_empty_metadata()
    {
        AttributionFormatter.Format(new Dictionary<string, string>()).Should().BeEmpty();
    }
}
