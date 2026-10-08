using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GoddessTempleDiscovery.Assets;
using GoddessTempleDiscovery.Game.Rendering;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Gallery_opens_from_the_title_filters_a_category_and_shows_a_picture_large()
    {
        await Id("Gallery").ClickAsync();
        await Expect(Id("GalleryPane")).ToBeVisibleAsync();
        (await ReadAsync(() => Model.GalleryItems.Count)).Should().Be(System.Math.Min(ArtCatalog.Keys.Count, 24));
        await Expect(Id("GalleryPageText")).ToContainTextAsync(ArtCatalog.Keys.Count + " pictures");

        //A category: only its pictures, every one of them over the pages
        await ReadAsync(() => Model.GalleryCategory = "deco");
        var deco = ArtCatalog.Keys.Count(k => ArtInfo.CategoryOf(k) == "deco");
        await Expect(Id("GalleryPageText")).ToContainTextAsync(deco + " pictures");
        (await ReadAsync(() => Model.GalleryItems.All(i => i.Category == "deco"))).Should().BeTrue();
        await WaitAsync(() => Model.GalleryItems.All(i => i.Image != null), done => done, "the thumbnails rendered");

        //One picture large, with its header's title, subject and sources and the license
        var first = await ReadAsync(() => Model.GalleryItems[0].Info);
        await Id("GalleryItem").First.ClickAsync();
        await Expect(Id("GalleryLarge")).ToBeVisibleAsync();
        await Expect(Id("GalleryLargeTitle")).ToHaveTextAsync(first.Title);
        await Expect(Id("GalleryLargeSubject")).ToHaveTextAsync(first.Subject);
        first.Sources.Should().NotBeEmpty();
        await Expect(Id("GalleryLargeSources")).ToHaveTextAsync(first.Sources);
        await Expect(Id("GalleryLicense")).ToHaveTextAsync(ArtCatalog.LicenseText);
        await Id("CloseGalleryLarge").ClickAsync();
        await Expect(Id("GalleryLarge")).ToBeHiddenAsync();

        await Id("CloseGallery").ClickAsync();
        await Expect(Id("GalleryPane")).ToBeHiddenAsync();
        await Expect(Id("TitlePane")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Gallery_opens_from_the_table_blocks_table_clicks_and_pages_with_the_arrows()
    {
        await ShowTableAsync();
        if (await ReadAsync(() => Model.IsInspectorOpen)) await CloseInspectorAsync();
        var button = await OnEngineAsync(() => Session.Layout.HeaderButton(2));
        await _fixture.ClickTableAsync(new Vector2(button.MidX, button.MidY));
        await Expect(Id("GalleryPane")).ToBeVisibleAsync();
        (await ReadAsync(() => _fixture.Host.PanesOpen)).Should().BeTrue();

        //The arrows page; Escape closes
        await ReadAsync(() => Model.GalleryCategory = "everything");
        await Expect(Id("GalleryPageText")).ToContainTextAsync("Page 1 of");
        await ReadAsync(() => Model.HandleKey(Windows.System.VirtualKey.Right));
        await Expect(Id("GalleryPageText")).ToContainTextAsync("Page 2 of");
        await ReadAsync(() => Model.HandleKey(Windows.System.VirtualKey.Left));
        await Expect(Id("GalleryPageText")).ToContainTextAsync("Page 1 of");
        await ReadAsync(() => Model.HandleKey(Windows.System.VirtualKey.Escape));
        await Expect(Id("GalleryPane")).ToBeHiddenAsync();
        await Expect(Id("PlayMenu")).ToBeVisibleAsync();
        (await ReadAsync(() => _fixture.Host.PanesOpen)).Should().BeFalse();
    }
}
