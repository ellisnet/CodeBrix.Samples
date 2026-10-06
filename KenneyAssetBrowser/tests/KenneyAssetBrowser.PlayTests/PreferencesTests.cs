using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using KenneyAssetBrowser.Settings;
using KenneyAssetBrowser.ViewModels;
using Microsoft.UI.Xaml;
using SilverAssertions;
using Xunit;

namespace KenneyAssetBrowser.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Chosen_folder_and_last_bundle_are_restored_on_new_page()
    {
        await OpenBundleFolderAsync(Fixture.LibraryDirectory);
        await Page.GetByRole(AriaRole.Button).Filter(new() { HasText = "Puzzle Pack" }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ResultCountText, text => text.EndsWith("· Puzzle Pack"), description: "the Puzzle Pack cells");
        SettingsService.Get<string>(MainViewModel.AssetsFolderKey).Should().Be(Fixture.LibraryDirectory);
        SettingsService.Get<string>(MainViewModel.LastBundleKey).Should().Be("kenney_puzzle-pack-1.zip");

        // A fresh page reads the remembered folder and bundle, as a restarted application would.
        Fixture.KeepSettingsOnNextReset = true;
        await Fixture.ResetAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ResultCountText, text => text.EndsWith("· Puzzle Pack"), description: "the restored bundle");
        (await Page.EvaluateAsync(() => Fixture.Model.AssetsFolderLabel)).Should().Be(Fixture.LibraryDirectory);
        (await Page.EvaluateAsync(() => Fixture.Model.FolderPromptVisibility)).Should().Be(Visibility.Collapsed);
        Fixture.Application.FilePickers.FolderRequestCount.Should().Be(0);
    }
}
