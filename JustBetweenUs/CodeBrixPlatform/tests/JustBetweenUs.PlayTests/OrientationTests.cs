using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Windows.Graphics.Display;
using Xunit;

namespace JustBetweenUs.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    public async Task Method_can_require_landscape()
    {
        await AssertOrientationAsync(ScreenOrientation.Landscape);
        await RoundtripInCurrentOrientationAsync(Aes, "A landscape-only test.");
    }

    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Method_can_require_portrait()
    {
        await AssertOrientationAsync(ScreenOrientation.Portrait);
        await RoundtripInCurrentOrientationAsync(Aes, "A portrait-only test.");
    }

    [Theory]
    [PlayTestOrientation(ScreenOrientation.Landscape)]
    [InlineData(Aes, ScreenOrientation.Landscape, Traits = new[] { "PlayTestOrientation", "Landscape" })]
    [InlineData(Twofish, ScreenOrientation.Portrait, Traits = new[] { "PlayTestOrientation", "Portrait" })]
    [InlineData(TripleDes, ScreenOrientation.Landscape)]
    public async Task Case_orientation_overrides_landscape_method(string algorithm, ScreenOrientation expected)
    {
        await AssertOrientationAsync(expected);
        await RoundtripInCurrentOrientationAsync(algorithm, $"{algorithm} in {expected}.");
    }

    [Theory(DisableDiscoveryEnumeration = true)]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    [InlineData(Twofish, ScreenOrientation.Landscape, Traits = new[] { "PlayTestOrientation", "Landscape" })]
    [InlineData(Aes, ScreenOrientation.Portrait, Traits = new[] { "PlayTestOrientation", "Portrait" })]
    [InlineData(TripleDes, ScreenOrientation.Portrait)]
    public async Task Case_orientation_overrides_portrait_method(string algorithm, ScreenOrientation expected)
    {
        await AssertOrientationAsync(expected);
        await RoundtripInCurrentOrientationAsync(algorithm, $"{algorithm} in {expected}.");
    }

    [Fact]
    public async Task Unspecified_orientation_uses_fixture_preference()
    {
        await AssertOrientationAsync(_fixture.Application.PreferredOrientation);
    }

    [Fact]
    public async Task Page_reset_restores_preference_after_repeated_orientation_changes()
    {
        // Deliberately exercise both directions in one process, independent of xUnit execution order.
        for (var i = 0; i < 2; i++)
        {
            await _fixture.ResetAsync(ScreenOrientation.Portrait);
            await AssertOrientationAsync(ScreenOrientation.Portrait);
            await RoundtripInCurrentOrientationAsync(Aes, "Portrait after a resize.");
            await _fixture.ResetAsync(ScreenOrientation.Landscape);
            await AssertOrientationAsync(ScreenOrientation.Landscape);
            await RoundtripInCurrentOrientationAsync(Aes, "Landscape after a resize.");
        }
        await _fixture.ResetAsync();
        await AssertOrientationAsync(_fixture.Application.PreferredOrientation);
        await Expect(Input).ToHaveValueAsync("");
        await Expect(Output).ToHaveValueAsync("");
    }

    [Fact]
    public async Task Orientation_changes_notify_the_application_and_preserve_existing_page_state()
    {
        var app = _fixture.Application;
        var preferred = app.PreferredOrientation;
        var opposite = preferred == ScreenOrientation.Landscape ? ScreenOrientation.Portrait : ScreenOrientation.Landscape;
        var notifications = new List<DisplayOrientations>();
        Windows.Foundation.TypedEventHandler<DisplayInformation, object> handler = (display, _) =>
        {
            notifications.Add(display.CurrentOrientation);
            display.ScreenWidthInRawPixels.Should().Be((uint)app.Width);
            display.ScreenHeightInRawPixels.Should().Be((uint)app.Height);
        };
        await Input.FillAsync("This page survives orientation changes.");
        await app.EvaluateAsync(() => DisplayInformation.GetForCurrentView().OrientationChanged += handler);
        try
        {
            await app.SetOrientationAsync(opposite);
            app.Orientation.Should().Be(opposite);
            await app.SetOrientationAsync(opposite); // No duplicate event for an unchanged orientation.
            await Expect(Input).ToHaveValueAsync("This page survives orientation changes.");
            await Encrypt.ClickAsync();
            await Expect(Output).Not.ToHaveValueAsync("");
            var encrypted = await Output.InputValueAsync();
            await app.SetOrientationAsync();
            app.Orientation.Should().Be(preferred);
            await Input.FillAsync(encrypted);
            await Decrypt.ClickAsync();
            await Expect(Output).ToHaveValueAsync("This page survives orientation changes.");
            notifications.Should().Equal(opposite == ScreenOrientation.Portrait ? DisplayOrientations.Portrait : DisplayOrientations.Landscape,
                preferred == ScreenOrientation.Portrait ? DisplayOrientations.Portrait : DisplayOrientations.Landscape);
        }
        finally
        {
            await app.EvaluateAsync(() => DisplayInformation.GetForCurrentView().OrientationChanged -= handler);
            await app.SetOrientationAsync();
        }
    }

    private async Task AssertOrientationAsync(ScreenOrientation expected)
    {
        var width = expected == ScreenOrientation.Landscape ? 1920 : 1080;
        var height = expected == ScreenOrientation.Landscape ? 1080 : 1920;
        _fixture.Application.Orientation.Should().Be(expected);
        _fixture.PageCreationOrientation.Should().Be(expected);
        _fixture.Application.Width.Should().Be(width);
        _fixture.Application.Height.Should().Be(height);
        await Page.EvaluateAsync(() =>
        {
            _fixture.View.ActualWidth.Should().Be(width);
            _fixture.View.ActualHeight.Should().Be(height);
            var display = DisplayInformation.GetForCurrentView();
            display.CurrentOrientation.Should().Be(expected == ScreenOrientation.Landscape
                ? DisplayOrientations.Landscape : DisplayOrientations.Portrait);
            display.ScreenWidthInRawPixels.Should().Be((uint)width);
            display.ScreenHeightInRawPixels.Should().Be((uint)height);
        });
        var png = await Page.ScreenshotAsync();
        BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)).Should().Be(width);
        BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)).Should().Be(height);
        await Expect(Encrypt).ToBeVisibleAsync();
        await Expect(Copy).ToBeVisibleAsync();
    }

    private async Task RoundtripInCurrentOrientationAsync(string algorithm, string message)
    {
        await SelectModeAsync(algorithm);
        var encrypted = await EncryptAsync(message);
        await Input.FillAsync(encrypted);
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync(message);
    }
}
