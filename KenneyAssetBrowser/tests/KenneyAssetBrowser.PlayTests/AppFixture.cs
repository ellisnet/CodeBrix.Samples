using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using KenneyAssetBrowser.Settings;
using KenneyAssetBrowser.ViewModels;
using KenneyAssetBrowser.Views;
using Microsoft.UI.Xaml;
using SkiaSharp;

namespace KenneyAssetBrowser.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    public string AssetsDirectory => Path.Combine(DataDirectory, "Fixture Assets");
    public MainViewModel Model => (MainViewModel)View.DataContext;
    protected override Application CreateApplication() => new App(null, Path.Combine(DataDirectory, "settings"));
    protected override void Prepare()
    {
        Directory.CreateDirectory(AssetsDirectory);
        using var zip = ZipFile.Open(Path.Combine(AssetsDirectory, "kenney_playtest-fixtures.zip"), ZipArchiveMode.Create);
        Add(zip, "License.txt", Encoding.UTF8.GetBytes("PlayTest synthetic assets\nCC0 test fixtures; generated locally."));
        Add(zip, "Docs/Readme.txt", Encoding.UTF8.GetBytes("A deterministic document displayed by the real asset viewer."));
        Add(zip, "Images/blue_tile.png", Png(SKColors.CornflowerBlue));
        Add(zip, "Images/red_tile.png", Png(SKColors.OrangeRed));
    }
    private static void Add(ZipArchive zip, string name, byte[] bytes)
    {
        using var stream = zip.CreateEntry(name).Open();
        stream.Write(bytes);
    }
    private static byte[] Png(SKColor color)
    {
        using var bitmap = new SKBitmap(96, 96);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color);
        using var paint = new SKPaint { Color = SKColors.White };
        canvas.DrawCircle(48, 48, 22, paint);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
    protected override Task BeforeResetAsync()
    {
        SettingsService.Set(MainViewModel.AssetsFolderKey, null);
        SettingsService.Set(MainViewModel.LastBundleKey, null);
        return Task.CompletedTask;
    }
    protected override void Cleanup() => SettingsService.Shutdown();
}
