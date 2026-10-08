using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
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
    public string PuzzleDirectory => Path.Combine(DataDirectory, "Puzzle Pack");
    public string SoundsDirectory => Path.Combine(DataDirectory, "Sci-Fi Sounds");
    public string CharactersDirectory => Path.Combine(DataDirectory, "Blocky Characters");
    public string LibraryDirectory => Path.Combine(DataDirectory, "Library");
    public string EdgeCasesDirectory => Path.Combine(DataDirectory, "Edge Cases");
    public string EmptyDirectory => Path.Combine(DataDirectory, "Empty Folder");
    public MainViewModel Model => (MainViewModel)View.DataContext;
    // The next reset keeps the remembered folder and bundle, like a restart of the application.
    public bool KeepSettingsOnNextReset { get; set; }
    protected override Application CreateApplication() => new App(null, Path.Combine(DataDirectory, "settings"));

    // The application initializes OpenGL elements. These tests cover the no-OpenGL fallback, so the launch opts out;
    // to give the elements real contexts instead, call CodeBrixPlayTestOpenGL.Register() in Prepare() and drop this override.
    protected override void Configure(PlayTestOptions options) => options.OpenGL = PlayTestOpenGL.Unavailable;
    protected override void Prepare()
    {
        Directory.CreateDirectory(AssetsDirectory);
        var fixtureZip = Path.Combine(AssetsDirectory, "kenney_playtest-fixtures.zip");
        using (var zip = ZipFile.Open(fixtureZip, ZipArchiveMode.Create))
        {
            Add(zip, "License.txt", Encoding.UTF8.GetBytes("PlayTest synthetic assets\nCC0 test fixtures; generated locally."));
            Add(zip, "Docs/Readme.txt", Encoding.UTF8.GetBytes("A deterministic document displayed by the real asset viewer."));
            Add(zip, "Images/blue_tile.png", Png(SKColors.CornflowerBlue));
            Add(zip, "Images/red_tile.png", Png(SKColors.OrangeRed));
        }

        // The CC0 Kenney bundles that ship with the sample, copied so the application reads its own data.
        var bundles = FindBundleDirectory();
        CopyBundle(bundles, "kenney_puzzle-pack-1.zip", PuzzleDirectory);
        CopyBundle(bundles, "kenney_sci-fi-sounds.zip", SoundsDirectory);
        CopyBundle(bundles, "kenney_blocky-characters_20.zip", CharactersDirectory);
        CopyBundle(bundles, "kenney_puzzle-pack-1.zip", LibraryDirectory);
        File.Copy(fixtureZip, Path.Combine(LibraryDirectory, "kenney_playtest-fixtures.zip"));
        File.WriteAllBytes(Path.Combine(LibraryDirectory, "kenney_broken.zip"), Encoding.UTF8.GetBytes("This is not a zip archive."));

        Directory.CreateDirectory(EdgeCasesDirectory);
        using (var zip = ZipFile.Open(Path.Combine(EdgeCasesDirectory, "kenney_edge-cases.zip"), ZipArchiveMode.Create))
        {
            Add(zip, "License.txt", Encoding.UTF8.GetBytes("PlayTest edge cases\nCC0 test fixtures; generated locally."));
            Add(zip, "Images/broken.png", Encoding.UTF8.GetBytes("These bytes are not an image."));
            Add(zip, "Maps/tiles.png", Tileset());
            Add(zip, "Maps/level.tmx", Encoding.UTF8.GetBytes(TiledMap));
            // The application's own font package supplies a real TrueType file (SIL Open Font License).
            Add(zip, "Fonts/Merriweather.ttf", File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,
                "CodeBrix.Platform.Fonts.Merriweather", "Fonts", "Merriweather.ttf")));
        }

        Directory.CreateDirectory(EmptyDirectory);
    }
    private static string FindBundleDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "sample_asset_bundles");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("The sample's sample_asset_bundles folder was not found above " + AppContext.BaseDirectory);
    }
    private static void CopyBundle(string bundles, string fileName, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);
        File.Copy(Path.Combine(bundles, fileName), Path.Combine(targetDirectory, fileName));
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
        return Encode(bitmap);
    }
    // Two 16 x 16 tiles side by side: green grass and grey stone.
    private static byte[] Tileset()
    {
        using var bitmap = new SKBitmap(32, 16);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint { Color = SKColors.ForestGreen };
        canvas.DrawRect(0, 0, 16, 16, paint);
        paint.Color = SKColors.SlateGray;
        canvas.DrawRect(16, 0, 16, 16, paint);
        return Encode(bitmap);
    }
    private static byte[] Encode(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
    private const string TiledMap = """
        <?xml version="1.0" encoding="UTF-8"?>
        <map version="1.10" orientation="orthogonal" renderorder="right-down" width="4" height="3" tilewidth="16" tileheight="16">
         <tileset firstgid="1" name="tiles" tilewidth="16" tileheight="16" tilecount="2" columns="2">
          <image source="tiles.png" width="32" height="16"/>
         </tileset>
         <layer id="1" name="Ground" width="4" height="3">
          <data encoding="csv">
        1,1,1,1,
        1,2,2,1,
        1,1,1,1
        </data>
         </layer>
        </map>
        """;
    protected override Task BeforeResetAsync()
    {
        if (KeepSettingsOnNextReset)
        {
            KeepSettingsOnNextReset = false;
            return Task.CompletedTask;
        }
        SettingsService.Set(MainViewModel.AssetsFolderKey, null);
        SettingsService.Set(MainViewModel.LastBundleKey, null);
        return Task.CompletedTask;
    }
    protected override void Cleanup() => SettingsService.Shutdown();
}

// Stands in for the page's AudioPlayer element at the view model's audio bridge, so the transport
// rules are checked without an audio device.
public sealed class FakeAudioBridge
{
    public List<string> Calls { get; } = new();
    public long LoadedLength { get; private set; }
    public bool Looping { get; private set; }
    public bool Playing { get; set; }
    public TimeSpan Position { get; set; }
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);
    public void Attach(MainViewModel model)
    {
        model.LoadAudioSource = stream =>
        {
            LoadedLength = stream.Length;
            stream.Dispose();
            Calls.Add("Load");
        };
        model.PlayAudio = () =>
        {
            Playing = true;
            Calls.Add("Play");
        };
        model.PauseAudio = () =>
        {
            Playing = false;
            Calls.Add("Pause");
        };
        model.StopAudio = () =>
        {
            Playing = false;
            Position = TimeSpan.Zero;
            Calls.Add("Stop");
        };
        model.SetAudioLooping = looping => Looping = looping;
        model.IsAudioPlaying = () => Playing;
        model.AudioPosition = () => Position;
        model.AudioDuration = () => Duration;
        model.SeekAudio = position =>
        {
            Position = position;
            Calls.Add("Seek " + position.TotalSeconds);
        };
    }
}
