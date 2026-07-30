using NotionDocumentCreator.CreateDocument.Internal;
using SilverAssertions;
using Xunit;

namespace NotionDocumentCreator.CreateDocument.Tests;

public class NotionConvertTests
{
    [Fact]
    public void normalize_id_keeps_a_hyphenated_id()
    {
        //Arrange
        var input = "1a2b3c4d-5e6f-4a8b-9c0d-1e2f3a4b5c6d";

        //Act
        var result = NotionConvert.NormalizeId(input);

        //Assert
        result.Should().Be("1a2b3c4d-5e6f-4a8b-9c0d-1e2f3a4b5c6d");
    }

    [Fact]
    public void normalize_id_hyphenates_a_bare_id()
    {
        //Arrange
        var input = "1a2b3c4d5e6f4a8b9c0d1e2f3a4b5c6d";

        //Act
        var result = NotionConvert.NormalizeId(input);

        //Assert
        result.Should().Be("1a2b3c4d-5e6f-4a8b-9c0d-1e2f3a4b5c6d");
    }

    [Fact]
    public void normalize_id_extracts_the_id_from_a_notion_url()
    {
        //Arrange
        var input = "https://www.notion.so/The-Father-of-DRAKON-1a2b3c4d5e6f4a8b9c0d1e2f3a4b5c6d";

        //Act
        var result = NotionConvert.NormalizeId(input);

        //Assert
        result.Should().Be("1a2b3c4d-5e6f-4a8b-9c0d-1e2f3a4b5c6d");
    }

    [Fact]
    public void normalize_id_ignores_other_ids_in_the_query_string()
    {
        //Arrange - the ?v= view ID must not win over the ID in the URL path
        var input = "https://www.notion.so/1a2b3c4d5e6f4a8b9c0d1e2f3a4b5c6d?v=aaaabbbbccccddddeeeeffff00001111";

        //Act
        var result = NotionConvert.NormalizeId(input);

        //Assert
        result.Should().Be("1a2b3c4d-5e6f-4a8b-9c0d-1e2f3a4b5c6d");
    }

    [Fact]
    public void normalize_id_returns_unmatchable_input_trimmed_and_unchanged()
    {
        //Arrange
        var input = "  not-an-id  ";

        //Act
        var result = NotionConvert.NormalizeId(input);

        //Assert
        result.Should().Be("not-an-id");
    }
}
