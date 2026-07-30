using NotionDocumentCreator.CreateDocument;
using SilverAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NotionDocumentCreator.CreateDocument.Tests;

/// <summary>
/// The selection rules: tree order is depth-first top-to-bottom, and selection
/// is fully independent per node — a checked grandchild under an unchecked
/// child still appears, and an unchecked parent contributes nothing.
/// </summary>
public class SelectionFlatteningTests
{
    private sealed class FakeNode
    {
        public string Id { get; init; } = "";
        public bool IsChecked { get; init; }
        public List<FakeNode> Children { get; init; } = [];
    }

    private static FakeNode Node(string id, bool isChecked, params FakeNode[] children) =>
        new() { Id = id, IsChecked = isChecked, Children = [.. children] };

    private static List<string> CheckedIds(params FakeNode[] roots) =>
        SelectionFlattening.FlattenDepthFirst(roots, n => n.Children)
            .Where(n => n.IsChecked)
            .Select(n => n.Id)
            .ToList();

    [Fact]
    public void checked_grandchild_under_an_unchecked_child_still_appears()
    {
        //Arrange
        var tree = Node("root", true,
            Node("child", false,
                Node("grandchild", true)));

        //Act
        var ids = CheckedIds(tree);

        //Assert
        ids.Should().HaveCount(2);
        ids[0].Should().Be("root");
        ids[1].Should().Be("grandchild");
    }

    [Fact]
    public void unchecked_parent_contributes_nothing()
    {
        //Arrange
        var tree = Node("root", false,
            Node("child-a", true),
            Node("child-b", false));

        //Act
        var ids = CheckedIds(tree);

        //Assert
        ids.Should().HaveCount(1);
        ids[0].Should().Be("child-a");
    }

    [Fact]
    public void order_is_depth_first_tree_order()
    {
        //Arrange - the order shown in the treeview, top to bottom
        var tree = Node("1", true,
            Node("1.1", true,
                Node("1.1.1", true)),
            Node("1.2", true));

        //Act
        var ids = CheckedIds(tree);

        //Assert
        string.Join(",", ids).Should().Be("1,1.1,1.1.1,1.2");
    }

    [Fact]
    public void multiple_roots_flatten_in_sequence()
    {
        //Act
        var ids = CheckedIds(Node("a", true), Node("b", true));

        //Assert
        string.Join(",", ids).Should().Be("a,b");
    }
}
