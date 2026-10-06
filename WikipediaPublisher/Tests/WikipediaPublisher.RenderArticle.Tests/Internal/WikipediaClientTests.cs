using System;
using System.Collections.Generic;
using SilverAssertions;
using WikipediaPublisher.RenderArticle.Internal;
using Xunit;

namespace WikipediaPublisher.RenderArticle.Tests.Internal;

public class WikipediaClientTests
{
    private static Dictionary<string, IReadOnlyDictionary<string, string>> NewResult() =>
        new(StringComparer.OrdinalIgnoreCase);

    [Fact]
    public void ParseImageMetadata_reads_the_value_of_each_field()
    {
        //Arrange
        const string json = """
            {"batchcomplete":true,"query":{"pages":[
              {"pageid":1,"ns":6,"title":"File:Tablet.jpg","imagerepository":"shared","imageinfo":[
                {"extmetadata":{
                  "Artist":{"value":"Jane Doe","source":"commons-desc-page"},
                  "LicenseShortName":{"value":"CC BY-SA 4.0","source":"commons-desc-page","hidden":""}}}]}
            ]}}
            """;
        var result = NewResult();

        //Act
        WikipediaClient.ParseImageMetadata(json, result);

        //Assert
        result.Count.Should().Be(1);
        result["File:Tablet.jpg"]["Artist"].Should().Be("Jane Doe");
        result["File:Tablet.jpg"]["LicenseShortName"].Should().Be("CC BY-SA 4.0");
    }

    //Regression: MediaWiki renders an empty extmetadata map as [] rather than {}, which used to
    //throw out of the parser and abort the whole render.
    [Fact]
    public void ParseImageMetadata_skips_an_image_whose_extmetadata_is_an_empty_array()
    {
        //Arrange
        const string json = """
            {"batchcomplete":true,"query":{"pages":[
              {"pageid":1,"ns":6,"title":"File:No metadata.jpg","imagerepository":"shared","imageinfo":[
                {"extmetadata":[]}]},
              {"pageid":2,"ns":6,"title":"File:With metadata.jpg","imagerepository":"shared","imageinfo":[
                {"extmetadata":{"Artist":{"value":"Jane Doe","source":"commons-desc-page"}}}]}
            ]}}
            """;
        var result = NewResult();

        //Act
        WikipediaClient.ParseImageMetadata(json, result);

        //Assert
        result.Count.Should().Be(1);
        result.ContainsKey("File:No metadata.jpg").Should().BeFalse();
        result["File:With metadata.jpg"]["Artist"].Should().Be("Jane Doe");
    }

    [Fact]
    public void ParseImageMetadata_skips_a_field_that_is_not_an_object()
    {
        //Arrange
        const string json = """
            {"batchcomplete":true,"query":{"pages":[
              {"pageid":1,"ns":6,"title":"File:Odd.jpg","imagerepository":"shared","imageinfo":[
                {"extmetadata":{
                  "Artist":[],
                  "Credit":"plain text",
                  "LicenseShortName":{"value":"Public domain","source":"commons-desc-page"}}}]}
            ]}}
            """;
        var result = NewResult();

        //Act
        WikipediaClient.ParseImageMetadata(json, result);

        //Assert
        result["File:Odd.jpg"].Count.Should().Be(1);
        result["File:Odd.jpg"]["LicenseShortName"].Should().Be("Public domain");
    }
}
