using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using Xunit;

namespace MediaPlayerDemo.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_keeps_address_row_and_status_visible()
    {
        Fixture.Application.Width.Should().Be(1080);
        Fixture.Application.Height.Should().Be(1920);
        await Expect(AddressBox).ToBeVisibleAsync();
        await Expect(Button("Load")).ToBeVisibleAsync();
        await Expect(StretchPicker).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("Player")).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("Loaded: " + AppFixture.DefaultAddress);
        var status = await Status.BoundingBoxAsync();
        (status.Y + status.Height).Should().BeLessThanOrEqualTo(1920);
        await AddressBox.FillAsync(Address);
        await Button("Load").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Loaded: " + Address);
        await SnapshotAsync("MediaPlayerDemo-portrait");
    }

    [Fact]
    public async Task Rotation_preserves_address_and_selected_stretch()
    {
        var app = Fixture.Application;
        var opposite = app.PreferredOrientation == ScreenOrientation.Landscape
            ? ScreenOrientation.Portrait : ScreenOrientation.Landscape;
        await AddressBox.FillAsync(Address);
        await Button("Load").ClickAsync();
        await StretchPicker.ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "Fill", Exact = true }).ClickAsync();
        await app.WaitForAsync(() => Player().Stretch, stretch => stretch == Stretch.Fill,
            description: "the player element's stretch");
        try
        {
            await app.SetOrientationAsync(opposite);
            app.Orientation.Should().Be(opposite);
            await Expect(AddressBox).ToHaveValueAsync(Address);
            await Expect(Status).ToHaveTextAsync("Loaded: " + Address);
            (await Page.EvaluateAsync(() => Player().Stretch)).Should().Be(Stretch.Fill);
            (await Page.EvaluateAsync(() => Fixture.Model.SelectedStretch)).Should().Be(Stretch.Fill);
            await Expect(Page.GetByTestId("Player")).ToBeVisibleAsync();
        }
        finally
        {
            await app.SetOrientationAsync();
        }
        await Expect(AddressBox).ToHaveValueAsync(Address);
    }
}
