using CodeBrix.NotionApi;
using NotionDocumentCreator.CreateDocument.Internal;
using SilverAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NText = CodeBrix.NotionApi.Text;

namespace NotionDocumentCreator.CreateDocument.Tests;

/// <summary>
/// The look-ahead that pairs an image/video with the separate credit/rights
/// paragraph that follows it in Jeremy's page layouts.
/// </summary>
public class CreditPairingTests
{
    private static NotionBlockNode Image() =>
        new() { Block = new ImageBlock { Id = "img", Image = new UploadedFile() } };

    private static NotionBlockNode Paragraph(string text) =>
        new()
        {
            Block = new ParagraphBlock
            {
                Paragraph = new ParagraphBlock.Info
                {
                    RichText = new List<RichTextBase>
                    {
                        new RichTextText { PlainText = text, Text = new NText { Content = text } }
                    }
                }
            }
        };

    [Theory]
    [InlineData("Credit: NASA archives")]
    [InlineData("Image credit: the Parondzhanov family")]
    [InlineData("Photo by V. Ivanov")]
    [InlineData("Source: Buran program records")]
    [InlineData("© 1988 TASS")]
    [InlineData("Public domain image")]
    [InlineData("CC BY-SA 4.0, Wikimedia Commons")]
    public void credit_shaped_paragraph_is_consumed(string creditText)
    {
        //Arrange
        var blocks = new List<NotionBlockNode> { Image(), Paragraph(creditText) };

        //Act
        var result = BlockRenderer.FindCreditParagraph(blocks, 0);

        //Assert
        result.Should().Be(1);
    }

    [Fact]
    public void ordinary_paragraph_is_left_alone()
    {
        //Arrange
        var blocks = new List<NotionBlockNode> { Image(), Paragraph("The story continues apace.") };

        //Act
        var result = BlockRenderer.FindCreditParagraph(blocks, 0);

        //Assert
        result.Should().Be(0);
    }

    [Fact]
    public void image_at_the_end_of_the_page_has_no_credit()
    {
        //Arrange
        var blocks = new List<NotionBlockNode> { Image() };

        //Act
        var result = BlockRenderer.FindCreditParagraph(blocks, 0);

        //Assert
        result.Should().Be(0);
    }

    [Fact]
    public void non_paragraph_sibling_is_not_consumed()
    {
        //Arrange
        var blocks = new List<NotionBlockNode>
        {
            Image(),
            new() { Block = new DividerBlock() }
        };

        //Act
        var result = BlockRenderer.FindCreditParagraph(blocks, 0);

        //Assert
        result.Should().Be(0);
    }
}
