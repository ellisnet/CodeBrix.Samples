# CodeBrix.Samples Blueprints: Testing

These recipes cover getting a sample application under test when the
interesting code lives behind a running application host, a native library
or a real graphics stack. They start with the organizing constraint that
a SimpleViewModel cannot be constructed outside a running host, so the
rules move into plain classes a test can reach, then work through test
project setup: the family runner conventions, the property that swaps real
CodeBrix.Platform assemblies in for the reference ones, the native packages a
head would otherwise have supplied, and exposing library internals to the test
assembly. From there they show fixtures and test doubles - shared expensive
fixtures, synthetic inputs built rather than committed, stub message handlers,
mocks over rendering and API seams - along with headless graphics testing,
golden-image comparison, opt-in live tests, isolating a process-global store,
and a scripted run that drives the whole application on a real head. Reach
for this file when you are adding a test project to an application, or when
something you need to prove will not run in a bare test host.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Keep view model rules in a plain class so they can be tested](#keep-view-model-rules-in-a-plain-class-so-they-can-be-tested)
- [Set up an xUnit v3 test project for a CodeBrix library](#set-up-an-xunit-v3-test-project-for-a-codebrix-library)
- [Build a test project against real CodeBrix Platform assemblies](#build-a-test-project-against-real-codebrix-platform-assemblies)
- [Add the native assets a head would have supplied](#add-the-native-assets-a-head-would-have-supplied)
- [Expose library internals to its test project](#expose-library-internals-to-its-test-project)
- [Test a service the way the container builds it](#test-a-service-the-way-the-container-builds-it)
- [Route logging from the code under test into test output](#route-logging-from-the-code-under-test-into-test-output)
- [Share one expensive fixture across every test class that needs it](#share-one-expensive-fixture-across-every-test-class-that-needs-it)
- [Build the binary inputs your tests need instead of committing them](#build-the-binary-inputs-your-tests-need-instead-of-committing-them)
- [Generate real media clips from a synthetic source](#generate-real-media-clips-from-a-synthetic-source)
- [Read a committed fixture from beside the test binary](#read-a-committed-fixture-from-beside-the-test-binary)
- [Test a document renderer against the object model it produces](#test-a-document-renderer-against-the-object-model-it-produces)
- [Assert on a generated document without a golden file](#assert-on-a-generated-document-without-a-golden-file)
- [Make live tests opt in and keep them out of the default run](#make-live-tests-opt-in-and-keep-them-out-of-the-default-run)
- [Test an HTTP client offline with a stub handler](#test-an-http-client-offline-with-a-stub-handler)
- [Mock a rendering or API seam with CodeBrix TestMocks](#mock-a-rendering-or-api-seam-with-codebrix-testmocks)
- [Test GL code headlessly with a surfaceless EGL context](#test-gl-code-headlessly-with-a-surfaceless-egl-context)
- [Prove every graphics backend with the same mirrored suite](#prove-every-graphics-backend-with-the-same-mirrored-suite)
- [Pin a fixed bug with a regression test that says why it is shaped that way](#pin-a-fixed-bug-with-a-regression-test-that-says-why-it-is-shaped-that-way)
- [Compare rendered images pixel by pixel](#compare-rendered-images-pixel-by-pixel)
- [Point a process-global store at a throwaway folder in tests](#point-a-process-global-store-at-a-throwaway-folder-in-tests)
- [Drive a scripted end-to-end run of the whole application](#drive-a-scripted-end-to-end-run-of-the-whole-application)
- [Run one boot sequence hosted on a thread and inline for tests](#run-one-boot-sequence-hosted-on-a-thread-and-inline-for-tests)
- [Copy a gold master fixture before a test that writes to it](#copy-a-gold-master-fixture-before-a-test-that-writes-to-it)
- [Check audible-only behavior with an opt-in unattended walkthrough](#check-audible-only-behavior-with-an-opt-in-unattended-walkthrough)
- [Ship a simulator that enforces the real device's limits rather than a stub](#ship-a-simulator-that-enforces-the-real-devices-limits-rather-than-a-stub)
- [Make a byte pump testable by writing to a sink interface instead of a control](#make-a-byte-pump-testable-by-writing-to-a-sink-interface-instead-of-a-control)

## Related blueprints

- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - shows the view model and command shapes these tests are built to reach around
- [BLUEPRINTS-ProjectLayoutAndPackaging.md](BLUEPRINTS-ProjectLayoutAndPackaging.md) - the src/tests/libs layout and native asset references these test projects mirror
- [BLUEPRINTS-GraphicsAndRendering.md](BLUEPRINTS-GraphicsAndRendering.md) - the renderers and backends the headless and golden-image recipes exercise
- [BLUEPRINTS-SettingsAndPersistence.md](BLUEPRINTS-SettingsAndPersistence.md) - the settings store whose real data the throwaway-folder recipe keeps tests away from

---

## Testing

### Keep view model rules in a plain class so they can be tested

**When you want this.** You want your rules covered, and a `SimpleViewModel`
cannot be constructed without a running application host.

**The MVVM shape.** Every decision lives in a static class of plain methods over
plain values; the view model is a thin observable wrapper that calls them and
raises change notifications. The view model keeps the wiring, the collections and
the commands; the class keeps the answers.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/PlaybackSelection.cs
/// <remarks>
/// The rules live here rather than inside the view model because a view model derived from the
/// platform's SimpleViewModel cannot be constructed without a running application host, and rules
/// that cannot be tested are rules that quietly stop being true. The view model is a thin observable
/// wrapper over this.
/// </remarks>
public static class PlaybackSelection
{
    public static bool CanOpen(SourceMediaInfo item) =>
        item is not null && MediaFormats.IsPlayable(item.Format);

    public static string DescribeUnplayable(SourceMediaInfo item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return $"{MediaFormats.DisplayName(item.Format)} is not played in this application - " +
               "import it to one of the four CodeBrix formats first.";
    }

    // ... BuildChapterRows, BuildCaptionRows, ShouldShowChapters, ShouldShowCaptions, DescribeOpened ...
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/ViewModels/PlaybackViewModel.cs
public void Open(SourceMediaInfo item)
{
    Close();

    if (item is null)
    {
        return;
    }

    CurrentItem = item;

    if (!PlaybackSelection.CanOpen(item))
    {
        IsUnplayableFormat = true;
        StatusText = PlaybackSelection.DescribeUnplayable(item);
        return;
    }

    if (surface is null)
    {
        StatusText = "The player is not ready yet.";
        return;
    }

    StatusText = $"Opening {item.FileName}...";
    surface.Open(item.Path);
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/PlaybackSelection.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/ViewModels/PlaybackViewModel.cs`
`CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Playback.Tests/PlaybackSelectionTests.cs`

**Sharp edges.**
- The constraint is stated in three places in that application - the two class
  remarks and the test project file comment - which is a good sign it is the real
  organizing principle.
- The parts that cannot be reduced this way (the panel itself, the player element)
  are covered by a scripted run on a real head instead; see the last blueprint in
  this area.

### Set up an xUnit v3 test project for a CodeBrix library

**When you want this.** You are adding the first test project to an application
and want it to match the family conventions and actually be discovered.

**The MVVM shape.** Not applicable; project setup. A `global.json` at the
application root selects the runner for every project below it.

**Code.**

```xml
<!-- Adapted from CodeBrix.Samples/PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/PalmVisualizer.Rendering.Tests.csproj
     (package ids and versions elided - see the project's csproj) -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <!-- xUnit.net v3 test projects are self-executing binaries and
       must build as Exe; run via Microsoft.Testing.Platform,
       matching the CodeBrix family test convention. -->
  <OutputType>Exe</OutputType>
  <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
  <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="..\..\..\src\libs\PalmVisualizer.Rendering\PalmVisualizer.Rendering.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="(xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk)" />
  <PackageReference Include="(SilverAssertions)" />
</ItemGroup>
```

```text
// From CodeBrix.Samples/PalmVisualizer/global.json
{
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

Test bodies follow the family style - `<Class>Tests.cs`, snake_case method names,
`//Arrange` / `//Act` / `//Assert` comments, and the assertions library's
`Should()`:

```csharp
// From CodeBrix.Samples/PalmVisualizer/tests/libs/PalmVisualizer.Camera.Tests/WebcamCaptureServiceTests.cs
[Fact]
public void TryCopyLatestFrame_returns_false_before_any_frame()
{
    //Arrange
    using var service = new WebcamCaptureService();
    byte[] buffer = null;

    //Act
    bool copied = service.TryCopyLatestFrame(ref buffer, out int width, out int height);

    //Assert
    copied.Should().Be(false);
    width.Should().Be(0);
    height.Should().Be(0);
}
```

**Where to look.**
`PalmVisualizer/global.json` and the three project files under
`PalmVisualizer/tests/libs/`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/PdfSideBySide.PdfRender.Tests.csproj`
`KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Rendering.Tests/KenneyAssetBrowser.Rendering.Tests.csproj`

**Also shown by.**
`NotionDocumentCreator`, `WebcamPainter`, `Pinta.Brix`, `WikipediaPublisher`,
`CodeBrixVideoTool` - these test projects carry the same two properties and
the same comment. (`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests` sets
neither property explicitly.)

**Sharp edges.**
- The output type must be `Exe`. The comment appears in every one of these
  projects: xUnit v3 test projects are self-executing binaries, and a library
  test project will not run.
- Because the runner is the Microsoft Testing Platform, `dotnet test` can report
  that it discovered no tests on some SDK builds; running the built test
  executable directly always works.
- Two applications (`PdfSideBySide` and `CodeBrixVideoTool`) have no `global.json`
  at all, so the runner is selected by the project properties alone. Adding the
  `global.json` shown above matches the rest of the repository.
- Every async test passes `TestContext.Current.CancellationToken` to the method
  under test; it satisfies the analyzer that flags a missing token and makes the
  test cancellable. A test that waits on a background thread passes it to the
  wait as well.
- Enabling nullable annotations in a test project is worth a comment when the
  library under test is annotated, or a ported test file raises a wave of
  warnings about redundant annotations.

### Build a test project against real CodeBrix Platform assemblies

**When you want this.** Your test project references a library that references
CodeBrix.Platform, and calls into platform types.

**The MVVM shape.** Not a view-model concern. One project property.

**Code.**

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/Pinta.Brix.Engine.Tests.csproj -->
<!-- The published CodeBrix.Platform nuget ships REFERENCE assemblies in
     lib/; every method body throws NotSupportedException("Ref assembly").
     Application heads get the real implementations swapped in
     automatically, plain test projects do NOT. This is the lever that
     swaps them in, and without it every text-layout call would compile
     cleanly and then throw on first use. -->
<CodeBrixRuntimeIdentifier>skia</CodeBrixRuntimeIdentifier>
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Playback.Tests/CodeBrixVideoTool.Playback.Tests.csproj -->
<!-- ... Note that even
     with the real assemblies present a SimpleViewModel cannot be constructed here, because its
     dispatcher needs a running application host; the view models are exercised by the
     application's own scripted run instead, and the rules under them live in plain classes
     these tests can reach. -->
<CodeBrixRuntimeIdentifier>skia</CodeBrixRuntimeIdentifier>
```

**Where to look.**
`Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/Pinta.Brix.Engine.Tests.csproj`
`Pinta.Brix/tests/libs/Pinta.Brix.Effects.Tests/Pinta.Brix.Effects.Tests.csproj`
`CodeBrixVideoTool/tests/libs/` (both project files)

**Sharp edges.**
- Without the property, platform calls compile cleanly and then throw at run time.
  The failure looks like a test bug rather than a build-configuration one.
- The property does not lift the view-model construction limit: a `SimpleViewModel`
  still needs a running application host. Put the rules in plain classes (first
  blueprint in this area) and drive the view models from a scripted run (last
  blueprint in this area).

### Add the native assets a head would have supplied

**When you want this.** Your library binds to a native runtime - Skia, text
shaping, computer vision - and the tests exercise it for real.

**The MVVM shape.** Not a view-model concern. The test project references the
native package for the current operating system, with an MSBuild platform
condition.

**Code.**

```xml
<!-- Adapted from CodeBrix.Samples/WebcamPainter/tests/libs/WebcamPainter.Vision.Tests/WebcamPainter.Vision.Tests.csproj
     (package IDs and versions removed - see the project's csproj) -->
<ItemGroup>
  <!-- The tests run real TFLite inference - the native OpenCV library must be present -->
  <PackageReference Include="(OpenCV native, Linux x64)"   Condition="$([MSBuild]::IsOSPlatform('Linux'))" />
  <PackageReference Include="(OpenCV native, Windows x64)" Condition="$([MSBuild]::IsOSPlatform('Windows'))" />
  <PackageReference Include="(OpenCV native, macOS arm64)" Condition="$([MSBuild]::IsOSPlatform('OSX'))" />
  <PackageReference Include="(OpenCV native, macOS x64)"   Condition="$([MSBuild]::IsOSPlatform('OSX'))" />
</ItemGroup>
```

```xml
<!-- Adapted from CodeBrix.Samples/Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/Pinta.Brix.Engine.Tests.csproj -->
<!-- The engine's SkiaSharp reference is managed-only; on Linux the native
     libSkiaSharp must be pulled in explicitly for the tests to run. -->
<!-- Text layout shapes with HarfBuzz, so its native library is needed here
     for the same reason libSkiaSharp is: an application head gets these
     from its runtime package, a bare test project does not. -->
```

**Where to look.**
`WebcamPainter/tests/libs/WebcamPainter.Vision.Tests/` and
`WebcamPainter.Painting.Tests/` project files
`Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/Pinta.Brix.Engine.Tests.csproj`
`KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Rendering.Tests/KenneyAssetBrowser.Rendering.Tests.csproj`
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/PalmVisualizer.Rendering.Tests.csproj`

**Sharp edges.**
- In a running application the head's runtime package supplies these; a bare test
  host does not. The list is exactly what your library touches: the graphics
  native for anything that rasterizes, the shaping native for anything that lays
  out text, the vision native for anything that infers.
- Conditioning on the operating system only brings in the host architecture's
  package; a build machine on another architecture needs its own reference added.
- Shader tests are worth this on their own: compiling and evaluating real shader
  source on raster surfaces needs the native library present, with no engine, no
  window and no GPU.

### Expose library internals to its test project

**When you want this.** A library keeps its implementation types internal and you
want to unit test them without widening the public surface.

**The MVVM shape.** One file per library, named after what it does, with one
attribute naming that library's test project.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/InternalsVisibleTo.cs
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PalmVisualizer.Vision.Tests")]
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/PalmDetector.cs
/// <summary>Exposed for unit tests: the anchor grid's X centers.</summary>
internal static float[] TestAnchorsX => AnchorsX;

/// <summary>Exposed for unit tests: the anchor grid's Y centers.</summary>
internal static float[] TestAnchorsY => AnchorsY;
```

**Where to look.**
`PalmVisualizer/src/libs/*/InternalsVisibleTo.cs`
`WebcamPainter/src/libs/*/InternalsVisibleTo.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/InternalsVisibleTo.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/InternalsVisibleTo.cs`

**Also shown by.**
`NotionDocumentCreator`, `KenneyAssetBrowser`, `CodeBrixVideoTool`,
`PolyHavenBrowser` (where it is also what lets a test reach the client factory's
internal constructor).

**Sharp edges.**
- Every library that has tests carries the file, even one whose tests only touch
  public members; the convention is applied uniformly.
- When a test needs a value that is otherwise private, add a documented internal
  test accessor rather than making the field itself visible.
- Factoring one step of an expensive operation into an internal static method -
  compiling a shader, loading an embedded model - is what lets the test call it
  with nothing else running.

### Test a service the way the container builds it

**When you want this.** Unit tests that resolve the service under test the same
way the application does, with the same constructor dependencies.

**The MVVM shape.** A reusable test fixture base builds a small service
collection, exposes a typed resolve method, and offers one virtual registration
hook. The test project subclasses it once and registers what its tests need; test
classes take the subclass as a class fixture.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/EncryptionTestingFixture.cs
public class EncryptionTestingFixture : SimpleTestFixture
{
    protected override void RegisterCustomServices(
        IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration config,
        Func<IServiceProvider> serviceResolver)
    {
        //Register my custom testing services here
        services.AddSingleton<IEncryptionService>(_ =>
            new EncryptionService(serviceResolver().GetService<ILogger<EncryptionService>>()));
    }
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs
public class EncryptionServiceTests : IClassFixture<EncryptionTestingFixture>
{
    private readonly EncryptionTestingFixture _fixture;
    private readonly ITestOutputHelper _output;

    private IEncryptionService GetService() => _fixture.GetService<IEncryptionService>() as EncryptionService;

    public EncryptionServiceTests(EncryptionTestingFixture fixture,
        ITestOutputHelper output)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _output = new SimpleTestOutputHelper(output);

        fixture.CreateAndRegisterLogger<EncryptionService>(_output);
    }

    [Fact]
    public void can_get_service() => GetService().Should().NotBeNull();

    [Fact]
    public async Task GetDefaultKey_retrieves_key() =>
        (await GetService().GetDefaultKey()).Should().NotBeNullOrEmpty();
}
```

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/JustBetweenUs.Encryption.Tests.csproj -->
<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
  <DefineConstants>$(DefineConstants);SIMPLE_OUTPUT_LOGGING</DefineConstants>
</PropertyGroup>
<ItemGroup>
  <Compile Include="..\..\Shared\Testing\SimpleTestFixture.cs" Link="SimpleTestFixture.cs" />
</ItemGroup>
```

**Where to look.**
`JustBetweenUs/Shared/Testing/SimpleTestFixture.cs`
`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/EncryptionTestingFixture.cs`
`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs`

**Also shown by.**
`WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/` (the same
fixture file, linked in).

**Sharp edges.**
- The fixture is a single file linked into the test project, not a package, and it
  is feature-gated by two compilation constants - one for the test-output logging,
  one for an HTTP client factory stub. Define them, or build in the configuration
  that defines them, because the test classes use the gated types unconditionally.
- The fixture also scans its own assembly for registration classes and calls their
  registration methods after checking that each names this fixture, which gives a
  second hook for tests that want their setup in a separate file.
- It reads optional settings files from the working directory and honors the
  environment name variable, defaulting to Development.
- Resolving a type that was never registered throws rather than returning null, so
  a missing registration fails the test with a readable message.

### Route logging from the code under test into test output

**When you want this.** The service under test logs through a logger abstraction
and you want those lines in the test report.

**The MVVM shape.** The fixture holds a logger factory that wraps the test
framework's output helper; the test class registers a logger for the type it is
testing in its constructor, and the fixture hands that logger out whenever the
container is asked for a logger.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs
_output = new SimpleTestOutputHelper(output);
fixture.CreateAndRegisterLogger<EncryptionService>(_output);
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/Testing/SimpleTestFixture.cs
private void WriteText(string text, bool withEndOfLine = false)
{
    if (text != null)
    {
        // ...
        if (AlwaysWriteToConsole
            || (_wrappedOutput == null)
            || (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))) //Need to write test output to console on Linux
        {
            if (withEndOfLine) { Console.WriteLine(text); }
            else { Console.Write(text); }
        }
        else
        {
            try
            {
                //Note: writing to ITestOutputHelper can fail if the test has already completed
                if (withEndOfLine) { _wrappedOutput.WriteLine(text); }
                else { _wrappedOutput.Write(text); }
            }
            catch (Exception)
            {
                if (withEndOfLine) { Console.WriteLine(text); }
                else { Console.Write(text); }
            }
        }
    }
}
```

**Where to look.**
`JustBetweenUs/Shared/Testing/SimpleTestFixture.cs`
`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs`

**Sharp edges.**
- Two platform notes are baked into the wrapper: the test output helper does not
  reliably reach the console on Linux, so output goes to the console there
  instead; and writing to it after a test has completed throws, so every write is
  wrapped and falls back to the console.
- The logger registration refuses an open generic type, because the logger key is
  built from the type's full name.
- Diagnostic output is also how a probe-style test earns its place: writing
  environment strings through the output helper lets an environment-specific
  failure be pinned before anyone edits platform code.

### Share one expensive fixture across every test class that needs it

**When you want this.** Setup that takes real work - generating media, running
imports, probing the results - and must not run once per test class.

**The MVVM shape.** Not a view-model concern. An async-lifetime fixture plus a
collection definition; each test class takes the fixture in its constructor.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Processing.Tests/SampleMediaFixture.cs
public sealed class SampleMediaFixture : IAsyncLifetime
{
    public string Root { get; } = Path.Combine(
        Path.GetTempPath(), "CodeBrixVideoTool.Tests", Guid.NewGuid().ToString("N"));

    // ... Mp4Path, CaptionsPath, ChaptersPath, RichMp4Path, Mode2Path, Mode1Path ...

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(Root);

        await SampleClipFactory.WriteMp4Async(Mp4Path, Width, Height, Duration).ConfigureAwait(false);
        SampleClipFactory.WriteWebVtt(CaptionsPath, Duration);
        SampleClipFactory.WriteChapterMetadata(ChaptersPath, Duration);

        // ... mux the three into RichMp4Path ...

        var probe = new MediaProbe();
        var runner = new ConversionRunner();

        RichMp4Info = await probe.ProbeAsync(RichMp4Path, CancellationToken.None).ConfigureAwait(false);

        await ImportAsync(probe, runner, MediaFormatKind.CodeBrixMode2, Mode2Path).ConfigureAwait(false);
        await ImportAsync(probe, runner, MediaFormatKind.CodeBrixMode1, Mode1Path).ConfigureAwait(false);

        Mode2Info = await probe.ProbeAsync(Mode2Path, CancellationToken.None).ConfigureAwait(false);
        Mode1Info = await probe.ProbeAsync(Mode1Path, CancellationToken.None).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            if (Directory.Exists(Root)) { Directory.Delete(Root, true); }
        }
        catch (IOException)
        {
            //A temporary folder that will not delete is not worth failing a test run over.
        }

        return ValueTask.CompletedTask;
    }
}

/// <summary>Shares one <see cref="SampleMediaFixture" /> across every test class that needs media.</summary>
[CollectionDefinition(Name)]
public sealed class SampleMediaCollection : ICollectionFixture<SampleMediaFixture>
{
    public const string Name = "sample media";
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Processing.Tests/ConversionRunnerTests.cs
[Collection(SampleMediaCollection.Name)]
public class ConversionRunnerTests
{
    private readonly SampleMediaFixture media;

    public ConversionRunnerTests(SampleMediaFixture media) => this.media = media;

    [Theory]
    [InlineData(MediaFormatKind.Matroska)]
    [InlineData(MediaFormatKind.WebM)]
    [InlineData(MediaFormatKind.CodeBrixMode1)]
    [InlineData(MediaFormatKind.CodeBrixMode2)]
    public async Task an_import_writes_every_one_of_the_four_formats(MediaFormatKind destination)
    {
        //Arrange
        var output = Path.Combine(media.Root, "import-" + destination + MediaFormats.Extension(destination));
        var plan = ConversionPlanner.Create(media.RichMp4Info, destination, output, null);

        //Act
        var outcome = await new ConversionRunner()
            .RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        File.Exists(output).Should().BeTrue();
        outcome.SizeInBytes.Should().BeGreaterThan(0);
    }
}
```

**Where to look.**
`CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Processing.Tests/SampleMediaFixture.cs`
`CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Processing.Tests/ConversionRunnerTests.cs`

**Sharp edges.**
- Keep the collection name as a constant on the definition class, so the attribute
  on each test class cannot be misspelled.
- Let the fixture's own setup use the production code path, so a break in the
  pipeline fails setup loudly rather than one test obscurely.
- The fixture writes everything under one uniquely named temporary folder and
  deletes it on disposal, swallowing the delete failure.

### Build the binary inputs your tests need instead of committing them

**When you want this.** You are testing a reader, a decoder or a renderer, and you
do not want binary fixtures in the repository.

**The MVVM shape.** Not applicable. A small internal builder in the test project
writes exactly the input each test needs, in memory or into a throwaway folder.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Rendering.Tests/TestData/TestAssets.cs
/// <summary>Builders for tiny in-memory test assets (no files on disk, no network).</summary>
internal static class TestAssets
{
    /// <summary>Encodes an SKBitmap-drawn solid-color PNG.</summary>
    public static byte[] BuildPng(int width, int height, SKColor color)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        bitmap.Erase(color);
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }

    /// <summary>
    /// Builds a single-triangle .glb via SharpGLTF.Toolkit: vertices (0,0,0), (1,0,0),
    /// (0,1,0) with a red, double-sided material, optionally translated.
    /// </summary>
    public static byte[] BuildTriangleGlb(Vector3? translation = null) { /* ... */ }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.AssetRead.Tests/TestZipBuilder.cs
internal static class TestZipBuilder
{
    public static void Build(string zipPath, IReadOnlyDictionary<string, byte[]> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);
        using var fileStream = File.Create(zipPath);
        using var zipStream = new ZipOutputStream(fileStream);
        foreach (var (entryPath, bytes) in entries)
        {
            zipStream.PutNextEntry(new ZipEntry(entryPath) { Size = bytes.Length });
            zipStream.Write(bytes, 0, bytes.Length);
            zipStream.CloseEntry();
        }

        zipStream.Finish();
    }

    /// <summary>Encodes text as UTF-8 bytes for an entry.</summary>
    public static byte[] Text(string text) => Encoding.UTF8.GetBytes(text);
}
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs
/// <summary>A fresh, empty temp folder for one test's files.</summary>
public static string CreateTempFolder()
{
    var folder = Path.Combine(Path.GetTempPath(), "PdfSideBySide.PdfRender.Tests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(folder);
    return folder;
}

/// <summary>
/// Writes a PDF with pageCount pages to folder as fileName; every page carries a
/// filled rectangle placed by page number so the pages are not blank.
/// </summary>
public static string WriteSamplePdf(string folder, string fileName, int pageCount)
{
    using var document = new PdfDocument();
    for (var i = 0; i < pageCount; i++)
    {
        var page = document.AddPage();
        using var graphics = XGraphics.FromPdfPage(page);
        graphics.DrawRectangle(XBrushes.Black, new XRect(50, 50 + i * 20, 200, 30));
    }

    var path = Path.Combine(folder, fileName);
    document.Save(path);
    return path;
}
```

**Where to look.**
`KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Rendering.Tests/TestData/TestAssets.cs`
`KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.AssetRead.Tests/TestZipBuilder.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/TestData/TestAssets.cs`

**Also shown by.**
`PolyHavenBrowser_viewer_only` (its test assets hand-encode high-dynamic-range
image bytes as well as building a model file).

**Sharp edges.**
- A synthetic document has to differ page by page, or "different pages render to
  different images" is not testable; draw something placed by the page index.
- A fixture with whole-inch page dimensions lets the renderer tests assert exact
  pixel sizes rather than a tolerance.
- Give every test that writes files its own uniquely named folder, so tests using
  the same file name cannot collide and can run in parallel.
- Writing a deliberately corrupt input is how a warning path gets tested; the
  point of a warning list is that one bad file does not fail the whole load.
- A test project that builds an archive needs the compression library's writing
  side referenced explicitly, even when the library under test only reads.
- A fake image must be a real decodable image when the imaging back-end decodes
  eagerly; a placeholder byte array throws.
- Test classes that write files implement disposal and delete their temporary
  folder on a best-effort basis.
- A synthetic-document writer may reach its document library transitively; if that
  ever stops, the test project needs its own reference.

### Generate real media clips from a synthetic source

**When you want this.** You need real media to test against, and you do not want
binary files in the repository.

**The MVVM shape.** A factory in the production library, not the test project, so
the scripted run can use it too, writing clips from the media tool's own synthetic
sources into a folder the caller names.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Samples/SampleClipFactory.cs
var filterGraph = string.Create(CultureInfo.InvariantCulture,
    $"testsrc2=size={width}x{height}:rate={frameRate}[out0]; sine=frequency=440:sample_rate=48000[out1]");

Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));

var errors = new List<string>();
var succeeded = await FFMpegArguments
    .FromFileInput(filterGraph, false, input => input.ForceFormat("lavfi"))
    .OutputToFile(path, true, options => options
        .WithDuration(length)
        .WithVideoCodec("libx264")
        .WithConstantRateFactor(28)
        .WithSpeedPreset(Speed.UltraFast)
        .ForcePixelFormat("yuv420p")
        .WithAudioCodec("aac")
        .WithAudioBitrate(96)
        .ForceFormat("mp4"))
    .NotifyOnError(errors.Add)
    .CancellableThrough(cancellationToken)
    .ProcessAsynchronously(false)
    .ConfigureAwait(false);
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Samples/SampleClipFactory.cs`
(`WriteMp4Async`, `WriteRichMp4Async`, `WriteWebVtt`, `WriteChapterMetadata`)

**Sharp edges.**
- The synthetic input is passed as a file input with the filter-graph format
  forced and existence checking turned off - a filter graph is not a file.
- The interesting case is built in two passes: a plain clip, then a mux that adds
  a caption track and a chapter metadata file, copying the media rather than
  re-encoding it, with the metadata mapping pointing at the right input.
- The class documentation states the discipline: nothing is copied from anywhere
  and nothing is left behind - every clip is written where the caller asks.

### Read a committed fixture from beside the test binary

**When you want this.** One input really has to be a file a real tool produced -
a document, a photograph, a page of markup - and the tests need to find it.

**The MVVM shape.** Not applicable. Either copy it to the output folder and locate
it from the base directory, or embed it in the test assembly and read it through a
shared helper.

**Code.**

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/PdfSideBySide.PdfRender.Tests.csproj -->
<!-- Real-world PDF the tests open and render (a WikipediaPublisher sample) -->
<ItemGroup>
  <None Include="assets\**" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs
/// <summary>Full path of the assets/Inanna.pdf sample copied beside the test binary.</summary>
public static string InannaPath => Path.Combine(AppContext.BaseDirectory, "assets", "Inanna.pdf");
```

```xml
<!-- From CodeBrix.Samples/WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/WikipediaPublisher.RenderArticle.Tests.csproj -->
<ItemGroup>
  <Compile Include="..\..\Shared\Helpers\EmbeddedResourceHelper.cs" Link="Helpers\EmbeddedResourceHelper.cs" />
</ItemGroup>

<ItemGroup>
  <None Remove="Fixtures\cuneiform.html" />
  <EmbeddedResource Include="Fixtures\cuneiform.html" />
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/Internal/ArticleParserTests.cs
private const string FixtureResource = "WikipediaPublisher.RenderArticle.Tests.Fixtures.cuneiform.html";

private static string _fixtureHtml;

private static async Task<ParsedArticle> ParseFixture()
{
    _fixtureHtml ??= await EmbeddedResourceHelper.GetResourceAsString(
        FixtureResource, typeof(ArticleParserTests).Assembly);
    return new ArticleParser(FixtureUrl).Parse(_fixtureHtml);
}
```

**Where to look.**
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs`
`WikipediaPublisher/Shared/Helpers/EmbeddedResourceHelper.cs`
`WebcamPainter/tests/libs/WebcamPainter.Vision.Tests/HandTrackerTests.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Vision.Tests/`

**Sharp edges.**
- A copied fixture needs both halves: the copy item in the project file and the
  base-directory lookup in the test. Either one alone fails at run time.
- An embedded resource is named by the default namespace with folder separators
  replaced by dots; the shared helper also offers a path-based overload and a
  name-lookup method for when the exact name is uncertain.
- Parse an embedded fixture once into a static field and reuse it across the tests
  in the class.
- The same helper family is what an embedded font resolver mirrors, so fixtures,
  licenses and fonts are all reached the same way.

### Test a document renderer against the object model it produces

**When you want this.** Your library builds a document rather than returning a
value, and you want tests that are fast, offline and specific.

**The MVVM shape.** A test helper builds a themed document, a section and a
renderer over a fresh context, then walks the produced object model (including
into table cells) so each test asserts on styles and text rather than on a
rendered file.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/TestDom.cs
/// <summary>Creates a styled document, one section, and a renderer over a fresh context.</summary>
public static (Document Document, Section Section, RenderContext Context, BlockRenderer Renderer)
    CreateRenderer(Action<RenderContext> configure = null)
{
    var theme = BookTheme.For(PageSizeOption.EightByTen);
    var document = new Document();
    BookStyles.Define(document, theme);
    var section = document.AddSection();
    var context = new RenderContext { Theme = theme };
    configure?.Invoke(context);
    return (document, section, context, new BlockRenderer(context));
}

/// <summary>Every paragraph in the section, including paragraphs inside table cells.</summary>
public static List<Paragraph> AllParagraphs(Section section) =>
    CollectParagraphs(section.Elements).ToList();

/// <summary>The concatenated plain text of one paragraph (line breaks become \n).</summary>
public static string TextOf(Paragraph paragraph) => TextOfElements(paragraph.Elements);
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/BlockRendererTests.cs
private static (Section Section, RenderContext Context) Render(
    Action<RenderContext> configure, params NotionBlockNode[] nodes)
{
    var (_, section, context, renderer) = TestDom.CreateRenderer(configure);
    renderer.RenderPage(section, nodes);
    return (section, context);
}

[Fact]
public void consecutive_paragraphs_get_the_classic_book_indent()
{
    //Act
    var (section, _) = Render(Para("First."), Para("Second."));

    //Assert
    var paragraphs = TestDom.AllParagraphs(section);
    paragraphs[0].Style.Should().Be("BodyOpen");
    paragraphs[1].Style.Should().Be("BodyIndented");
}
```

**Where to look.**
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/TestDom.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/BlockRendererTests.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/PageNumberingTests.cs`

**Sharp edges.**
- Walking the produced object model - including into table cells - is what makes
  the assertions specific: a style name and a string, not a rendered page.
- One helper that builds document, section, context and renderer together keeps
  every test's arrange step to a single line.
- Tests reach the internal renderer types only because the library exposes its
  internals to the test assembly.

### Assert on a generated document without a golden file

**When you want this.** You want the parse-compose-render path covered offline and
you have no golden output to compare against.

**The MVVM shape.** The test drives the internal classes directly, writes the
document to a folder under the test binary, and asserts on the file signature and
a lower bound on its size in pages.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/Services/ArticleRenderServiceTests.cs
[Fact]
public async Task Compose_and_render_fixture_offline_produces_multipage_pdf()
{
    //Arrange - parse the embedded article fixture (no network, no images)
    var html = await EmbeddedResourceHelper.GetResourceAsString(
        FixtureResource, typeof(ArticleRenderServiceTests).Assembly);
    var article = new ArticleParser(CuneiformUrl).Parse(html);
    article.Blocks.Should().NotBeEmpty();

    //Act - compose the book and render it to a PDF
    var composer = new BookComposer(article, BookTheme.For(PageSizeOption.EightByTen), DateTime.Now);
    var document = composer.Compose();
    var renderer = new PdfDocumentRenderer(unicode: true) { Document = document };
    renderer.RenderDocument();

    var outPath = Path.Combine(GetOutDirectory(), "cuneiform-offline.pdf");
    renderer.PdfDocument.Save(outPath);

    //Assert
    File.Exists(outPath).Should().BeTrue();
    VerifyPdfSignature(outPath);
    renderer.PdfDocument.PageCount.Should().BeGreaterThan(5);
    _output.WriteLine($"Rendered {renderer.PdfDocument.PageCount} pages to {outPath}");
}
```

**Where to look.**
`WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/Services/ArticleRenderServiceTests.cs`

**Sharp edges.**
- Asserting on the format signature plus a page count is a cheap, stable way to
  verify a generated document with no golden file to maintain.
- Write the output to a folder under the test binary's base directory, created by
  the test, so a failure leaves something to look at.
- Assertions about content that can change are written as lower bounds rather than
  equalities.

### Make live tests opt in and keep them out of the default run

**When you want this.** A few tests genuinely need the network or a real account,
and they must not fail the suite for anyone who does not have one.

**The MVVM shape.** Either the credentials come from environment variables and the
class skips itself when they are absent, or the live tests carry a category trait
and share one fixture so a filter can exclude the whole set.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/NotionDocumentServiceTests.cs
/// <summary>
/// Integration tests against the live Notion API. Opt-in: they skip unless both
/// NOTION_AUTH_TOKEN and NOTION_TEST_PAGE_ID environment variables are set ...
/// </summary>
public class NotionDocumentServiceTests : IDisposable
{
    public NotionDocumentServiceTests()
    {
        _authToken = Environment.GetEnvironmentVariable("NOTION_AUTH_TOKEN");
        _testPageId = Environment.GetEnvironmentVariable("NOTION_TEST_PAGE_ID");

        Assert.SkipWhen(_authToken == null,
            "NOTION_AUTH_TOKEN environment variable is not set; skipping Notion integration tests.");
        Assert.SkipWhen(_testPageId == null,
            "NOTION_TEST_PAGE_ID environment variable is not set; skipping Notion integration tests.");

        _service = new NotionDocumentService();
    }

    public void Dispose() => _service?.Dispose();
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/Live/LiveApiFixture.cs
/// <summary>
/// Shares one factory and client across all live-API test classes so the whole live suite
/// reuses a single HTTP connection pool. Live tests carry
/// <c>[Trait("Category", "LiveApi")]</c> and can be excluded with
/// <c>dotnet test --filter Category!=LiveApi</c>.
/// </summary>
public sealed class LiveApiFixture : IDisposable
{
    public LiveApiFixture()
    {
        Factory = new DefaultPolyHavenClientFactory(new PolyHavenClientOptions
        {
            UserAgent = "PolyHavenBrowser.PolyHavenApiClient.Tests/1.0",
        });
        Client = Factory.GetClient();
    }
    // ...
}

[CollectionDefinition("LiveApi")]
public sealed class LiveApiCollection : ICollectionFixture<LiveApiFixture>;
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/Services/ArticleRenderServiceTests.cs
//Regression for the "No readable article content" failure: the Uruk article carries more
//  than one .mw-parser-output container (a near-empty template wrapper plus the real body),
//  which used to make the parser walk the empty one and produce zero blocks. Fetches live
//  HTML and parses it WITHOUT downloading images, so it is fast.
[Theory]
[InlineData(UrukUrl, "Uruk")]
[InlineData(CuneiformUrl, "Cuneiform")]
public async Task Fetch_and_parse_finds_readable_content(string url, string expectedTitle)
```

**Where to look.**
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/NotionDocumentServiceTests.cs`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/Live/`
`WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/Services/ArticleRenderServiceTests.cs`

**Also shown by.**
`PolyHavenBrowser_viewer_only` (the same fixture and trait, with the analyzer rule
about cancellation tokens suppressed in the project file and the reason written
down).

**Sharp edges.**
- Skipping in the constructor makes the whole class inert, so nobody has to
  remember an attribute per test.
- A live class takes the shared fixture and carries both the collection attribute
  and the category trait; the trait is what a filter can exclude.
- Split the fast live test (fetch and parse, no downloads) from the slow
  end-to-end one, so a regression can be caught without paying for the rest.
- Assertions against live content are deliberately loose, because the content
  changes; assertions written against one specific account's data are a smoke test
  for that account rather than a portable suite.
- Keep an offline counterpart for the same code path - a stub handler, or a client
  pointed at a closed local port - so a normal run is entirely offline.
- The service under test is disposed by the test class, because the test class
  constructed it.

### Test an HTTP client offline with a stub handler

**When you want this.** Your API client should be almost entirely testable with no
network, including the exact URLs it builds.

**The MVVM shape.** A stub message handler that routes canned responses and
records every request, plus a tiny factory helper that wires it into the real
client.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/TestDoubles/TestClient.cs
internal static class TestClient
{
    public static (IPolyHavenApiClient Client, StubHttpMessageHandler Stub) Create(
        PolyHavenClientOptions options = null)
    {
        var stub = new StubHttpMessageHandler();
        var factory = new DefaultPolyHavenClientFactory(stub, options);
        return (factory.GetClient(), stub);
    }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/TestDoubles/StubHttpMessageHandler.cs
/// <summary>Serves <paramref name="json"/> for requests whose path-and-query matches exactly.</summary>
public void OnPath(string pathAndQuery, string json, HttpStatusCode statusCode = HttpStatusCode.OK) =>
    _routes.Add((
        request => request.RequestUri!.PathAndQuery == pathAndQuery,
        _ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        }));
```

**Where to look.**
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/TestDoubles/`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/Unit/`

**Also shown by.**
`PolyHavenBrowser_viewer_only/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/TestDoubles/`
(with the canned JSON in its own file).

**Sharp edges.**
- Design the library so a test can hand it a handler: an internal constructor
  taking a message handler, reachable through the internals attribute, that never
  disposes what it was given.
- Recording the request URLs on the stub is what makes "did it build the right
  query string?" a one-line assertion.
- Return a not-found response naming the URL for anything unrouted, so a missing
  route reads as a missing route rather than as a client bug.

### Mock a rendering or API seam with CodeBrix TestMocks

**When you want this.** You want the code around an expensive or platform-bound
service covered without touching it.

**The MVVM shape.** This is the payoff for putting interfaces in front of the
concrete loader and renderer: the flow test needs neither a GPU nor a file on
disk.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Mocked/MockedRenderingTests.cs
[Fact]
public void viewer_flow_loads_then_hands_the_model_to_the_renderer()
{
    //Arrange - the typical app flow: load a model, give it to the scene renderer
    var model = TestAssets.BuildTriangleModel();
    var loaderMock = new Mock<IModelLoader>(MockBehavior.Strict);
    loaderMock.Setup(l => l.LoadFile("model.glb")).Returns(model);

    var rendererMock = new Mock<IModelSceneRenderer>(MockBehavior.Strict);
    rendererMock.Setup(r => r.SetModel(model, true));

    //Act
    var loaded = loaderMock.Object.LoadFile("model.glb");
    rendererMock.Object.SetModel(loaded, frameCamera: true);

    //Assert
    loaderMock.VerifyAll();
    rendererMock.VerifyAll();
    rendererMock.VerifyNoOtherCalls();
}
```

**Where to look.**
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Mocked/MockedRenderingTests.cs`
`PolyHavenBrowser_viewer_only/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/Mocked/`

**Sharp edges.**
- The mocks come from the CodeBrix TestMocks library, not a third-party mocking
  package.
- A mocked renderer can still hand out a real camera object, which is how pointer
  input wiring gets covered with no GPU at all.
- The loader interface exists in the production library specifically so the
  loading technology can be swapped or mocked; its own documentation comment says
  so.

### Test GL code headlessly with a surfaceless EGL context

**When you want this.** You want your renderer covered by real tests on a machine
or build agent with no window system.

**The MVVM shape.** A test double that creates the context and hands out the GL
object, plus a helper that skips cleanly when the machine cannot provide one.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/TestDoubles/EglTestContext.cs
private const string LibEgl = "libEGL.so.1";
private const int EGL_PLATFORM_SURFACELESS_MESA = 0x31DD;
// ...

// The core EGL 1.5 entry point: unlike eglGetPlatformDisplayEXT, this is a real
// exported symbol even under GLVND's dispatcher libEGL.
[DllImport(LibEgl)] private static extern IntPtr eglGetPlatformDisplay(int platform, IntPtr nativeDisplay, IntPtr attribs);
// ...

/// <summary>Tries to create a current GL context; returns <see langword="null"/> when the machine can't.</summary>
public static EglTestContext TryCreate()
{
    if (!OperatingSystem.IsLinux()) { return null; }

    try
    {
        var display = eglGetPlatformDisplay(EGL_PLATFORM_SURFACELESS_MESA, IntPtr.Zero, IntPtr.Zero);
        if (display == IntPtr.Zero || !eglInitialize(display, out _, out _)) { return null; }
        // ... eglChooseConfig, eglCreateContext (client version 3), eglCreatePbufferSurface, eglMakeCurrent ...

        var gl = GL.GetApi(name => eglGetProcAddress(name));
        return new EglTestContext(display, context, surface, gl);
    }
    catch (DllNotFoundException) { return null; }
    catch (EntryPointNotFoundException) { return null; }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Gl/GlModelSceneRendererTests.cs
[Trait("Category", "RequiresGL")]
public class GlModelSceneRendererTests
{
    private static EglTestContext RequireGl()
    {
        var context = EglTestContext.TryCreate();
        Assert.SkipWhen(context is null, "No EGL/OpenGL stack available on this machine (install Mesa llvmpipe).");
        return context!;
    }

    [Fact]
    public void renderer_initializes_and_renders_a_triangle_onto_the_background()
    {
        //Arrange
        using var egl = RequireGl();
        var gl = egl.Gl;
        const uint size = 64;
        var (fbo, colorRb, depthRb) = CreateFramebuffer(gl, size, size);
        var renderer = new GlModelSceneRenderer { BackgroundColor = (0f, 0f, 1f, 1f) };
        try
        {
            //Act
            renderer.Initialize(gl);
            renderer.SetModel(TestAssets.BuildTriangleModel());
            renderer.Render(gl, size, size);

            var pixels = new byte[size * size * 4];
            gl.ReadPixels(0, 0, size, size, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());

            //Assert - some pixels show the red triangle, some the blue background
            // ...
        }
        finally
        {
            renderer.Uninitialize(gl);
            gl.DeleteRenderbuffer(colorRb);
            gl.DeleteRenderbuffer(depthRb);
            gl.DeleteFramebuffer(fbo);
        }
    }
}
```

**Where to look.**
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/TestDoubles/EglTestContext.cs`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/TestDoubles/GlDesktopTestContext.cs`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Gl/`

**Also shown by.**
`PolyHavenBrowser_viewer_only/tests/libs/PolyHavenBrowser.Rendering.Tests/TestDoubles/EglTestContext.cs`

**Sharp edges.**
- Bind the core EGL 1.5 platform-display entry point, not the extension one: only
  the former is a real exported symbol under the vendor-neutral dispatcher.
- Two contexts are worth having. The surfaceless one gives OpenGL ES; a second
  binds the desktop GL API and asks for a core profile, which is what the X11,
  Win32, WPF and macOS heads actually hand you. A bug that only appears on desktop
  GL is invisible in an ES-only suite.
- Catch the two native-loading exceptions and return null, so a machine with no
  software GL stack skips instead of failing.
- Trait the class so the whole GPU suite can be excluded by filter, and delete
  every renderbuffer and framebuffer in a finally block.

### Prove every graphics backend with the same mirrored suite

**When you want this.** You ship more than one graphics backend and want the same
behaviors proven for each of them.

**The MVVM shape.** Not applicable; test infrastructure. Each backend gets a
requirement helper that skips with an actionable message, and each suite is
trait-tagged.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/tests/libs/PolyHavenBrowser.Rendering.Tests/Vulkan/VulkanSceneRendererTests.cs
[Trait("Category", "RequiresVulkan")]
public class VulkanSceneRendererTests
{
    private static void RequireVulkan() =>
        Assert.SkipWhen(
            !VulkanSceneRenderer.IsRuntimeAvailable(),
            "No Vulkan stack available on this machine (install a Vulkan driver or Mesa lavapipe).");
```

**Where to look.**
`PolyHavenBrowser_viewer_only/tests/libs/PolyHavenBrowser.Rendering.Tests/Gl/`,
`.../Vulkan/`, `.../Metal/`, `.../TestData/TestAssets.cs`

**Sharp edges.**
- The three suites deliberately mirror each other test for test - draws a triangle
  onto the background, clearing the model renders only the background, resizing
  between frames renders at the new size, a textured material shows its texture
  color, the full path from model file to pixels, and the depth-ordering
  regression - so every backend proves the same behaviors.
- The backend that may hand back its pixels the other way up needs
  orientation-agnostic checks: scan the whole buffer, or assert on a vertically
  symmetric pixel.
- Each requirement helper names what to install, so a skip is actionable rather
  than mysterious.

### Pin a fixed bug with a regression test that says why it is shaped that way

**When you want this.** You fixed something subtle, and you want the test to
survive a later tidy-up that would make it useless.

**The MVVM shape.** Reproduce the cause in the test rather than the environment,
and put the reason in the arrange comment.

**Code.**

```csharp
// Adapted from CodeBrix.Samples/JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs
// (the source file prepends the control character as a literal inside the string;
//  here it stands in as a named constant so it stays visible)
[Theory]
[InlineData("27544076", "This is a test.")]
public async Task AES_decrypt_tolerates_stray_control_chars_from_clipboard(string key, string message)
{
    //Arrange - reproduce the Intel/x64 macOS clipboard glitch where an invisible
    //  U+0001 control character was being prepended to the pasted Base64 text,
    //  which made IsBase64Text() return false and blocked decryption.
    var crypt = GetService();
    var encrypted = await crypt.AES_EncryptToBase64(key, message);
    var corrupted = StrayControlChar + encrypted; //stray SOH char at index 0, as seen in the diagnostic output

    //Act + Assert - the corrupted text must still be recognized as encrypted...
    crypt.IsBase64Text(corrupted).Should().BeTrue();

    //...and must still decrypt back to the original message.
    var decrypted = await crypt.AES_DecryptFromBase64(key, corrupted);
    decrypted.Should().Be(message);
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Gl/GlModelSceneRendererTests.cs
[Fact]
public void nearer_geometry_occludes_farther_geometry_regardless_of_draw_order()
{
    //Arrange - two large overlapping triangles centered on the origin: a near red one
    //(z=+0.5) and a far blue one (z=-0.5). Viewed from a ROTATED (non-axis-aligned)
    //camera, the near red triangle must win the center pixel no matter which is drawn
    //first. A rotated view is essential: a bad model-view-projection transpose collapses
    //the depth axis only for non-axis-aligned cameras (an axis-aligned view hides it).
    using var egl = RequireGl();
    var gl = egl.Gl;
    const uint size = 32;
    var center = (((int)size / 2) * (int)size + ((int)size / 2)) * 4;

    foreach (var nearDrawnFirst in new[] { false, true })
    {
        // ... render, read pixels ...

        //Assert - the near (red) triangle occludes the far (blue) one at the center
        pixels[center].Should().BeGreaterThan((byte)128);
        pixels[center + 2].Should().BeLessThan((byte)128);
    }
}
```

**Where to look.**
`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Gl/GlModelSceneRendererTests.cs`

**Sharp edges.**
- Reproducing the corrupted input inside the test is what frees the test from the
  head the bug appeared on.
- Name both symptoms the fix protects, so a partial regression still fails.
- Assert on a single known pixel rather than an aggregate, and cover both draw
  orders; an aggregate passes with the depth axis flattened, and one order can
  happen to look right.
- Tests that assert an exact string produced from a source literal should
  normalize line endings on both sides, because the literal has whatever endings
  the checkout gave it. Anything with randomness in it is tested by round trip
  instead.

### Compare rendered images pixel by pixel

**When you want this.** Golden-image tests for rendering code - effects, charts,
report layout - that must be exact but tolerate one-bit rounding.

**The MVVM shape.** A test helper that loads the expected image, renders the
actual, compares with a tolerance, and reports the first few differences with
their values. A save hook makes accepting a new golden a one-line change.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/tests/libs/Pinta.Brix.Effects.Tests/Utilities.cs
public static void CompareImages (
	ImageSurface result,
	ImageSurface expected,
	int tolerance = 1)
{
	Assert.Equal (expected.GetSize (), result.GetSize ());

	ReadOnlySpan<ColorBgra> result_pixels = result.GetReadOnlyPixelData ();
	ReadOnlySpan<ColorBgra> expected_pixels = expected.GetReadOnlyPixelData ();

	int diffs = 0;
	StringBuilder details = new ();
	for (int i = 0; i < result_pixels.Length; ++i) {

		if (ColorBgra.ColorsWithinTolerance (result_pixels[i], expected_pixels[i], tolerance))
			continue;

		++diffs;

		// Display info about the first few failures.
		if (diffs <= 10)
			details.AppendLine ($"Difference at pixel {i}, got {result_pixels[i]} vs {expected_pixels[i]}, diff. of {ColorBgra.ColorDifference (result_pixels[i], expected_pixels[i])}");
	}

	if (diffs != 0)
		Assert.Fail ($"{diffs} pixel(s) differ beyond tolerance {tolerance}:{Environment.NewLine}{details}");
}

public static void TestEffect (
	BaseEffect effect,
	string result_image_name,
	string? save_image_name = null,
	string source_image_name = "input.png")
{
	using ImageSurface source = Utilities.LoadImage (source_image_name);
	using ImageSurface result = CairoExtensions.CreateImageSurface (Format.Argb32, source.Width, source.Height);
	using ImageSurface expected = LoadImage (result_image_name);

	effect.Render (source, result, [source.GetBounds ()]);

	// For debugging, optionally save out the result to a file.
	if (save_image_name != null)
		SaveImage (result, save_image_name);

	CompareImages (result, expected);
}
```

**Where to look.**
`Pinta.Brix/tests/libs/Pinta.Brix.Effects.Tests/Utilities.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.Effects.Tests/Mocks/`
`Pinta.Brix/tests/libs/Pinta.Brix.Effects.Tests/EffectsTest.cs`

**Sharp edges.**
- Decode straight into the surface's own pixel format, or the comparison fails on
  conversion rounding rather than on the code under test.
- Report the first few differing pixels with both values and the delta; a bare
  count is not debuggable.
- The effects under test resolve their dependencies from a mock service provider
  built in the same helper, so no real chrome, workspace or palette is needed.
- The optional save hook is what turns "accept the new golden" into changing one
  argument.

### Point a process-global store at a throwaway folder in tests

**When you want this.** Your production code initializes a singleton store on
startup, and your tests must never touch the user's real data.

**The MVVM shape.** A module initializer in the test assembly opens the store at a
temporary path before any test runs, guarded by the store's own initialized flag.
Tests of the store itself take the opposite approach: a fresh directory per test.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/TestSettingsStore.cs
// PintaCore's static constructor builds the palette manager, which reads
// settings, so touching PintaCore at all requires an open settings store.
// SettingsService is a process-global singleton, so it is pointed at a
// throwaway folder once per test assembly - never at the user's real
// ~/.config/Pinta.Brix/settings, which tests must never read or write.

internal static class TestSettingsStore
{
	[ModuleInitializer]
	internal static void Initialize ()
	{
		if (SettingsService.IsInitialized)
			return;

		SettingsService.Initialize (Path.Combine (
			Path.GetTempPath (),
			"PintaBrix.Engine.Tests_" + Guid.NewGuid ().ToString ("N")));
	}
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Settings.Tests/SettingsStoreTests.cs
// These tests exercise the CodeBrix.Platform.AppSettings store that the
// KenneyAssetBrowser.Settings facade wraps. The add-in's store has no public test
// clock, so assertions about timestamped file names match on the naming
// pattern rather than exact names.
public class SettingsStoreTests : IDisposable
{
    public SettingsStoreTests()
    {
        root = Path.Combine(Path.GetTempPath(), "kenney-asset-browser-tests", Path.GetRandomFileName());
        directory = Path.Combine(root, "settings");
        // ...
    }

    public void Dispose()
    {
        try { Directory.Delete(root, recursive: true); } catch { /* best effort */ }
    }

    AppSettingsStore CreateStore() => new AppSettingsStore(SettingsService.AppName, directory);

    // The auto-backup files whose names carry a parseable timestamp,
    // alphabetical (= chronological, the naming scheme's guarantee).
    string[] AutoBackupFiles() =>
        Directory.EnumerateFiles(directory, $"{AppSettingsStore.AutoBackupFilePrefix}*.sqlite")
            .Select(Path.GetFileName)
            .Where(HasParseableTimestamp)
            .OrderBy(name => name)
            .ToArray();
}
```

**Where to look.**
`Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/TestSettingsStore.cs` and the
matching file under `Pinta.Brix.FileFormats.Tests/`
`KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Settings.Tests/SettingsStoreTests.cs`

**Sharp edges.**
- A module initializer is what guarantees the store is open before a static
  constructor in the library under test runs; a fixture would be too late.
- Each test assembly needs its own copy - two of them in one process would
  otherwise race on the guard.
- The store's own constants for file names, backup prefixes and timestamp formats
  are public, so tests assert against the real naming scheme rather than a copy of
  it, and against the guarantee that timestamped names sort chronologically.
- The corruption tests write junk over the store's file and assert that it is
  quarantined and restored from the newest backup; that path exists only because
  the add-in provides it.

### Drive a scripted end-to-end run of the whole application

**When you want this.** The parts a unit test cannot reach - a real head, a real
player element, a real visual tree - still need proving.

**The MVVM shape.** The page reads options from the environment in its constructor
and, when they are present, hooks its loaded event to run a script that drives the
view model's own commands and properties, then prints machine-readable lines and
exits with a status. Nothing about the run changes what the application does when
the variables are not set.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
//Optional scripted run: import, play and report without anyone touching the window.
if (SmokeOptions.FromEnvironment() is { } smoke)
{
    Loaded += (_, _) => RunSmoke(smoke);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
private static void Fact(string name, object value) =>
    Console.WriteLine($"CBVT-SMOKE: {name}={value?.ToString() ?? "(null)"}");

private static void Finish(int failures)
{
    Console.WriteLine($"CBVT-SMOKE: RESULT {(failures == 0 ? "PASS" : $"FAIL ({failures})")}");
    Console.Out.Flush();
    Environment.Exit(failures == 0 ? 0 : 1);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
var outputPath = Path.Combine(
    options.WorkFolder, "smoke" + MediaFormats.Extension(options.Destination));
viewModel.Conversion.PickOutputPathAsync = (_, _) => Task.FromResult(outputPath);

var finished = new TaskCompletionSource<Processing.Operations.ConversionOutcome>();
void OnFinished(object _, Processing.Operations.ConversionOutcome result) => finished.TrySetResult(result);
viewModel.Conversion.ConversionFinished += OnFinished;
viewModel.Conversion.RunCommand.Execute(null);
var outcome = await finished.Task;
viewModel.Conversion.ConversionFinished -= OnFinished;
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs` (the smoke-mode
region: `SmokeOptions`, `RunSmoke`, `RunMp4ExportAsync`, `CheckLastRunNotes`,
`ShownRowOpacity`, `FindLibraryRow`)

**Sharp edges.**
- The bridge delegates are what make the script possible: replacing the save-path
  delegate with one that returns a fixed path removes the only dialog in the way.
- An event plus a completion source is how the script awaits a fire-and-forget
  command.
- Anything that happens off an event rather than in the command needs a bounded
  retry loop before the script asserts on it, rather than an assumption that it
  has landed.
- To prove a visual rule is real rather than only configured, the script forces a
  layout pass, gets the item's container, walks the visual tree for the named
  element and compares it against a control case.
- Where a case is expected to fail a profile check, assert the expectation rather
  than success.

### Run one boot sequence hosted on a thread and inline for tests

**When you want this.** An application whose real work happens inside a long
startup sequence - create an engine, register extensions, source scripts, hand it
a surface - and you want a test suite to exercise that exact sequence rather than
a test-only approximation of it.

**The MVVM shape.** Not a view-model concern. One internal runtime class exposes
two entry points that differ only in how work reaches the engine: a hosted start
that the page calls, and a direct start that the tests call. Both funnel into one
private boot method, and the single thing that differs between them is captured as
a `dispatch` delegate, so the body below it is written once.

**Code.**

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs
public void Start(TkHostView host)
{
    // ...
    Task.Run(() =>
    {
        try
        {
            if (!Boot(tree, hosted: true, code => Environment.Exit(code), new TkHostFileDialogs(), assets))
            {
                return;
            }
            RunHostedDiagnostics();
        }
        // ...
    });
}

/// <summary>
/// Starts DRAKON in DIRECT mode against a headless root window, inline on the
/// calling thread — the way tests run it. Runs the SAME registration and
/// sourcing sequence as <see cref="Start"/>, so the Tcl open path is
/// identical; only the marshalling differs (inline vs. a dedicated thread).
/// A no-op quit action is used so a stray <c>exit</c> cannot end the test
/// host. After this returns, evaluate Tcl through <see cref="EvaluateScriptForTest"/>.
/// </summary>
/// <param name="assetsDirectory">
/// The directory holding <c>bootstrap.tcl</c> and <c>drakon/drakon_editor.tcl</c>
/// (the application's <c>Assets</c> folder, in source or output).
/// </param>
internal void StartDirect(string assetsDirectory)
{
    if (assetsDirectory == null) { throw new ArgumentNullException(nameof(assetsDirectory)); }
    if (_started) { return; }
    _started = true;

    _directRoot = TkWindow.CreateRoot();
    _directRoot.SetForcedSize(1024, 768);

    Boot(_directRoot.Tree, hosted: false, code => { }, null, assetsDirectory);
}
```

The boot method is worth learning as an order, and each step that can fail is
reported rather than thrown, because by the time the later steps run there is no
caller left to catch anything:

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs
private bool Boot(
    WindowTree tree, bool hosted, Action<int> onQuit,
    TkHostFileDialogs fileDialogs, string assetsDirectory)
{
    Result createResult = null;
    _interpreter = Interpreter.Create(ref createResult);
    if (_interpreter == null)
    {
        Report("interpreter creation failed: " + createResult);
        return false;
    }

    //DRAKON re-draws diagrams by re-running the same Tcl procedure bodies
    //thousands of times per file open; cache their parsed form so each
    //body is tokenized once per interpreter instead of on every execution.
    _interpreter.CacheParsedScripts = true;

    //ProductionMode skips optional per-command engine work (readiness
    //checks, previous-result tracking, usage counters, ...) for another
    //large speedup; results are byte-identical. Trade-off: [interp cancel]
    //cannot interrupt a running script promptly — DRAKON never uses script
    //cancellation, so that is acceptable here. Remove this line if prompt
    //cancellation ever becomes necessary.
    _interpreter.ProductionMode = true;

    Result error = null;
    if (TkBootstrap.Register(_interpreter, ref error) != ReturnCode.Ok)
    {
        Report("TkBootstrap failed: " + error);
        return false;
    }

    error = null;
    if (TclTkExtras.RegisterAll(_interpreter, ref error) != ReturnCode.Ok)
    {
        Report("Extras registration failed: " + error);
        return false;
    }

    _bridge = hosted
        ? TkTclBridge.RegisterHosted(_interpreter, tree)
        : TkTclBridge.Register(_interpreter, tree);
    if (fileDialogs != null) { _bridge.FileDialogs = fileDialogs; }
    _bridge.BackgroundError += message => Report("bgerror: " + message);

    Action<Action<Interpreter>> dispatch;
    if (hosted) { dispatch = action => _bridge.Post(action); }
    else { dispatch = action => action(_interpreter); }
    // ...
}
```

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs
    string bootstrap = Path.Combine(assetsDirectory, "bootstrap.tcl");
    string editor = Path.Combine(assetsDirectory, "drakon", "drakon_editor.tcl");

    dispatch(interp =>
    {
        Result result = null;
        if (interp.EvaluateScript("source {" + bootstrap + "}", ref result) != ReturnCode.Ok)
        {
            Report("bootstrap.tcl failed: " + result);
            return;
        }

        result = null;
        if (interp.EvaluateScript("source {" + editor + "}", ref result) != ReturnCode.Ok)
        {
            Report("drakon_editor.tcl failed: " + result);
            return;
        }

        Report("DRAKON Editor is up.");
    });
```

Hosted mode also hands the bridge a file-dialog adapter, so the guest's own open
and save commands raise the platform's native dialogs; direct mode leaves it unset,
because a headless run has nobody to show a dialog to. That one argument, the quit
action and the dispatch delegate are the entire difference between the two modes.

**Where to look.**
`DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs`
`DRAKON.Brix/tests/libs/DRAKON.Brix.TclBridge.Tests/DrnFileOpenTests.cs` and
`tests/libs/DRAKON.Brix.TclBridge.Tests/DrakonRuntimeTests.cs`

**Sharp edges.**
- The order is load-bearing: engine first, then the toolkit, then the command
  shims, then the bridge, then the application's own commands, and only then the
  scripts. Sourcing is itself two steps, and the glue script has to have finished
  before the guest's first line runs.
- The direct mode creates its own root window with a forced size. A headless boot
  against a surface with no size fails inside the guest's own layout code, a long
  way from the cause.
- Report and return rather than throw. The hosted path runs the whole sequence
  inside a background task where an exception has no caller, so a failure that is
  not reported is a window that stays empty with no explanation.
- Make the direct entry point internal and reach it from the test project through
  an `InternalsVisibleTo.cs` that names only that assembly, rather than widening
  the library's public surface for testing.
- A performance switch that trades away a capability needs the trade written down
  in place: which capability, why this program does not need it, and what would
  make the trade invalid.

### Copy a gold master fixture before a test that writes to it

**When you want this.** The test's input is a committed real-world document, and
the code under test writes to it - a schema upgrade, an in-place migration, a
journal file left beside it. This is the write-back angle on
[Read a committed fixture from beside the test binary](#read-a-committed-fixture-from-beside-the-test-binary):
the fixture is not copied to the output at all, it is read from the source tree
and copied per test case, because a fixture that the test mutates cannot be shared.

**The MVVM shape.** Not a view-model concern. A small support class finds the
application root by walking up from the test assembly's base directory until it
sees the solution file, and derives every path from there. Each test case copies
its file into a temp directory it owns, runs the real code path against the copy,
and deletes the directory in a finally block.

**Code.**

```csharp
// From CodeBrix.Samples/DRAKON.Brix/tests/libs/DRAKON.Brix.TclBridge.Tests/Support/SampleLocations.cs
/// <summary>
/// Resolves paths inside the DRAKON.Brix sample by walking up from the test
/// assembly to the sample root (identified by <c>DRAKON.Brix.slnx</c>). The
/// gold-master example <c>.drn</c> files and the DRAKON <c>Assets</c> tree are
/// read from source — never copied into the test project — so there is a single
/// source of truth and no build-copy of the whole vendored Tcl tree.
/// </summary>
internal static class SampleLocations
{
    /// <summary>The DRAKON.Brix sample root directory.</summary>
    public static string SampleRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "DRAKON.Brix.slnx"))) { return dir.FullName; }
            dir = dir.Parent;
        }
        throw new InvalidOperationException(
            "Could not locate the DRAKON.Brix sample root (DRAKON.Brix.slnx) above " +
            AppContext.BaseDirectory);
    }

    /// <summary>The read-only gold-master example <c>.drn</c> directory.</summary>
    public static string ExamplesDir(string root) => Path.Combine(root, "examples");

    /// <summary>The DRAKON <c>Assets</c> directory (bootstrap.tcl + drakon/ tree).</summary>
    public static string AssetsDir(string root) =>
        Path.Combine(root, "src", "DRAKON.Brix.Core", "Assets");
}
```

```csharp
// From CodeBrix.Samples/DRAKON.Brix/tests/libs/DRAKON.Brix.TclBridge.Tests/DrnFileOpenTests.cs
[Theory]
[InlineData("01.Insertion.drn", "")]
// ... one case per committed example document ...
[InlineData("verilog_example.drn", "Verilog")]
public void A_real_drn_file_opens_through_the_app_boot_path(string fileName, string relativePath)
{
    //Arrange — locate the gold master and copy it into a test-owned temp dir.
    //relativePath is the directory (relative to the examples folder) holding
    //the file, forward-slash separated; it is empty for a file that lives
    //directly in the examples folder. fileName is the leaf name.
    string root = SampleLocations.SampleRoot();
    string examples = SampleLocations.ExamplesDir(root);
    string goldMaster = String.IsNullOrWhiteSpace(relativePath)
        ? Path.Combine(examples, fileName)
        : Path.Combine(examples, relativePath.Replace('/', Path.DirectorySeparatorChar), fileName);
    File.Exists(goldMaster).Should().BeTrue();

    string tempDir = Path.Combine(Path.GetTempPath(), "drnopen_" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(tempDir);
    try
    {
        string copy = Path.Combine(tempDir, fileName);
        File.Copy(goldMaster, copy);

        var diagnostics = new List<string>();
        using var runtime = new DrakonRuntime();
        runtime.Diagnostic += diagnostics.Add;

        //Act — boot DIRECT (headless), then open through the exact app path.
        // ...
        runtime.StartDirect(SampleLocations.AssetsDir(root));
        string opened = runtime.EvaluateScriptForTest("ds::openfile {" + copy + "}");

        //Assert — the editor came up and the file opened (openfile returns 1
        //only after mod::open + gate + upgrade + reload all succeed).
        diagnostics.Contains("DRAKON Editor is up.").Should().BeTrue();
        opened.Should().Be("1");
    }
    finally
    {
        Directory.Delete(tempDir, true);
    }
}
```

A suite like this also has to say how it may be run, because the thing under test
keeps process-global state:

```xml
<!-- From CodeBrix.Samples/DRAKON.Brix/tests/libs/DRAKON.Brix.TclBridge.Tests/DRAKON.Brix.TclBridge.Tests.csproj -->
  <ItemGroup>
    <!-- The bridge hosts CodeBrix.Platform.TclTk interpreters (which keep heavy
         process-global state); run tests strictly sequentially so concurrent
         interpreter creation cannot race/crash. -->
    <None Update="xunit.runner.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
```

**Where to look.**
`DRAKON.Brix/tests/libs/DRAKON.Brix.TclBridge.Tests/DrnFileOpenTests.cs`
`DRAKON.Brix/tests/libs/DRAKON.Brix.TclBridge.Tests/Support/SampleLocations.cs` and
`tests/libs/DRAKON.Brix.TclBridge.Tests/xunit.runner.json`

**Sharp edges.**
- Never open a gold master in place. An in-place upgrade rewrites the committed
  file and can leave a journal beside it, so the first run passes, the repository
  is dirty afterwards, and the second run tests something else.
- Delete the temp directory in a finally block, and give each case its own
  directory, so a failing case cannot leave state for the next one.
- Walk up to a file that identifies the root - the solution file will do - rather
  than counting parent segments from the test binary. A chain of parent hops
  breaks the moment the configuration, target framework or output layout changes.
- Reading fixtures from the source tree rather than copying them to the output is
  the right call when the tree is large and the suite touches a few files per run;
  it also keeps one source of truth.
- Turn parallelism off in the runner configuration when the code under test owns
  process-global state, and copy that configuration file to the output or it does
  nothing.
- Assert on both halves: that the subject came up, and that the operation returned
  success. Either one alone can pass while the other silently did not happen.

### Check audible-only behavior with an opt-in unattended walkthrough

**When you want this.** The behavior you most need to check is the behavior nobody
can assert on: whether a file really loaded through the instrument format you think
it did, what it said about its own tempo, whether a transition landed where a tempo
map said it should. This differs from
[Drive a scripted end-to-end run of the whole application](BLUEPRINTS-Testing.md#drive-a-scripted-end-to-end-run-of-the-whole-application),
which drives a page's commands through the view model and exits the process with a
status: this one lives inside the shipped library as an internal type, is started
by the code under test once it is running, drives a real engine from a worker
thread, and reports only to the log, because there is no window to look at and
nothing to exit.

**The MVVM shape.** The walkthrough is a maintainer tool in the library, not a
feature. Start-up asks it whether the environment wants it and hands it the running
object; the view model and the page know nothing about it.

**Code.**

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoWalkthrough.cs
/// <para>
/// MAINTAINER TOOL, NOT A FEATURE OF THE SAMPLE. It runs only when the environment variable
/// <c>GAMEENGINEMUSICDEMO_SELFTEST</c> is set to <c>1</c>, so a person running the demo never sees it. It
/// exists because these are audible-only behaviours: on a machine with no sound card, and in a
/// terminal, the log is the only evidence that the music system did what it says.
/// </para>
// ...
internal static bool IsRequested =>
    string.Equals(Environment.GetEnvironmentVariable("GAMEENGINEMUSICDEMO_SELFTEST"), "1", StringComparison.Ordinal);

/// <summary>Starts the walkthrough on a worker thread and returns immediately.</summary>
/// <param name="demo">The running demo.</param>
internal static void Start(GameEngineMusicDemoGame demo) => Task.Run(() => Run(demo));
```

Each check compares a claim against what the engine reports, and says both numbers
rather than only its verdict:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoWalkthrough.cs
var timeline = sampler?.Timeline;
var wait = timeline?.TimeToNextBoundary(position, MusicTransitionQuantize.Bar) ?? TimeSpan.Zero;
var atOpeningTempo = timeline is null
    ? 0
    : timeline.SecondsPerBar - (position.TotalSeconds % timeline.SecondsPerBar);

Check(ref passed, ref total, "grid-follows-the-tempo-map",
    timeline is not null && Math.Abs(wait.TotalSeconds - atOpeningTempo) > 0.1,
    $"the map says {wait.TotalSeconds:0.000}s to the next bar; a grid fixed at the opening "
    + $"tempo would have said {atOpeningTempo:0.000}s");

demo.CrossfadeToTrackA(MusicTransitionQuantize.Bar);

Check(ref passed, ref total, "transition-queued",
    MusicManager.Instance.HasPendingTransition,
    $"waiting {wait.TotalSeconds:0.000}s for the next bar");
```

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoWalkthrough.cs
Engine.Logger.LogInformation("{Prefix}: RESULT {Result} ({Passed}/{Total} checks)",
    Prefix, passed == total && total > 0 ? "PASS" : "FAIL", passed, total);
```

The one thing about it a unit test can hold is the guarantee that it stays off, and
the test project reaches the internal type to do it:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/tests/libs/GameEngineMusicDemo.Game.Tests/GameEngineMusicDemoWalkthroughTests.cs
[Fact]
public void IsRequested_is_true_only_for_the_exact_opt_in_value()
{
    //Arrange
    Environment.SetEnvironmentVariable(VariableName, "1");

    //Act
    var optedIn = GameEngineMusicDemoWalkthrough.IsRequested;
    Environment.SetEnvironmentVariable(VariableName, "true");
    var somethingElse = GameEngineMusicDemoWalkthrough.IsRequested;
    Environment.SetEnvironmentVariable(VariableName, null);

    //Assert
    optedIn.Should().Be(true);
    somethingElse.Should().Be(false);
}
```

**Where to look.**
`GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/GameEngineMusicDemoWalkthrough.cs`
`GameEngineMusicDemo/tests/libs/GameEngineMusicDemo.Game.Tests/GameEngineMusicDemoWalkthroughTests.cs`
and `src/libs/GameEngineMusicDemo.Game/InternalsVisibleTo.cs`

**Sharp edges.**
- Opt in on one exact value, and pin that with a test. An environment variable
  matched loosely is a maintainer tool that one day runs for a user.
- Drive the same methods the controls drive. A walkthrough with its own private
  path through the code checks a path nobody else takes.
- Let the first frames and the first audio blocks go by before asking anything
  about them, and wait out a queued transition before asserting that it landed.
- Wrap the run so a throw still produces a single machine-readable result line, and
  count the failure rather than losing it: a walkthrough whose output stops partway
  is indistinguishable from one that never started.

### Ship a simulator that enforces the real device's limits rather than a stub

**When you want this.** The application drives hardware, and it has to be
runnable, testable and demonstrable without it - but you do not want code that
works against the simulator and then fails against the instrument. This is the
hardware counterpart to
[Test an HTTP client offline with a stub handler](BLUEPRINTS-Testing.md#test-an-http-client-offline-with-a-stub-handler)
and
[Mock a rendering or API seam with CodeBrix TestMocks](BLUEPRINTS-Testing.md#mock-a-rendering-or-api-seam-with-codebrix-testmocks),
except that the fake ships in the product, not only in the tests.

**The MVVM shape.** Not a view-model concern. The capability set of the real
instrument is a plain data object in the contract library. The real
implementation rebuilds that object by interrogating whatever is attached; the
simulator loads the same object as a constant and validates every call against
it, so the two reject exactly the same things.

**Code.**

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData/Model/ScopeCapabilities.cs
public static ScopeCapabilities PicoScope2204A { get; } = new ScopeCapabilities
{
    Variant = "2204A",
    SupportedChannels = new[] { ChannelId.ChannelA, ChannelId.ChannelB },
    SupportedRanges = new[]
    {
        VoltageRange.Range50mV, VoltageRange.Range100mV, VoltageRange.Range200mV,
        VoltageRange.Range500mV, VoltageRange.Range1V, VoltageRange.Range2V,
        VoltageRange.Range5V, VoltageRange.Range10V, VoltageRange.Range20V
    },
    // ...
    MinTimebaseSingleChannel = 0,
    MinTimebaseMultiChannel = 1,
    MaxTimebase = 23,
    MaxSamplesSingleChannel = 8064,
    MaxSamplesMultiChannel = 3968,
    MaxOversample = 4,
    // ...
};
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData/Simulation/SimulatedScopeDataDevice.cs
public void SetChannel(ChannelId channel, ChannelSettings settings)
{
    EnsureOpen("set a channel");

    if (!Capabilities.Supports(channel))
    {
        throw new ScopeCapabilityException(
            $"Channel {channel} is not present on a {Capabilities.Variant}.");
    }
    if (!Capabilities.Supports(settings.Range))
    {
        throw new ScopeCapabilityException(
            $"Range {settings.Range.ToDisplayString()} is not supported by a {Capabilities.Variant}.");
    }

    _channels[channel] = settings;
}
```

Because the limits are enforced, they can be asserted, and the assertions are
about the instrument rather than about the fake:

```csharp
// From CodeBrix.Samples/PicoScope.Brix/tests/libs/PicoScope.Brix.ScopeData.Tests/SimulatedScopeDataDeviceTests.cs
[Fact]
public void SetChannel_rejects_a_range_the_device_does_not_have()
{
    using var scope = Open();
    Assert.Throws<ScopeCapabilityException>(
        () => scope.SetChannel(ChannelId.ChannelA, new ChannelSettings(true, Coupling.Dc, VoltageRange.Range10mV)));
    Assert.Throws<ScopeCapabilityException>(
        () => scope.SetChannel(ChannelId.ChannelA, new ChannelSettings(true, Coupling.Dc, VoltageRange.Range50V)));
}

// ...

[Fact]
public async Task RunBlockAsync_rejects_more_samples_than_the_buffer_holds()
{
    using var scope = Open();
    await Assert.ThrowsAsync<PicoScopeException>(() => scope.RunBlockAsync(4000, 7, cancellationToken: TestContext.Current.CancellationToken));
}
```

**Where to look.**
`PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData/Simulation/SimulatedScopeDataDevice.cs`
`PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData/Model/ScopeCapabilities.cs`
`PicoScope.Brix/tests/libs/PicoScope.Brix.ScopeData.Tests/SimulatedScopeDataDeviceTests.cs`
and `TestScopeDataDevice.cs`

**Sharp edges.**
- A fake that accepts what the hardware rejects is worse than no fake at all: it
  moves the failure from your test run to the one machine that has the device.
- Put the limits in one data object that both implementations read. The moment
  the simulator carries its own copy of the numbers, the two drift.
- The simulator enforces the lifecycle too - every call before the device is
  opened throws the same not-open exception the real one does.
- It is not a substitute for a scripted test double. The simulator synthesizes a
  noisy signal on purpose, so tests that need exact values use a separate
  scripted implementation of the same interface that plays back the samples it
  is handed and records what was asked of it.
- Making the fake wait roughly as long as the real operation would, capped so a
  slow setting does not make it unusable, keeps command enablement and busy
  flags honest.

### Make a byte pump testable by writing to a sink interface instead of a control

**When you want this.** Something in your application moves bytes between a
remote stream and a control - a terminal, a log tail, a serial monitor - and the
interesting behavior is all in the moving: chunk boundaries, ordering, resizes,
end of stream, failure banners. Talking straight to the control makes every one of
those tests need a window, a render backend and native libraries.

**The MVVM shape.** Put the pump in a library with no UI reference at all and give
it a small sink interface to write into. One adapter class is the only type in the
application that names the control. Tests then drive the pump with a fake session
on one side and a recording sink on the other, and assert on what came out, byte
for byte.

**Code.**

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/libs/RedisSetupTool.TerminalView/ITerminalSink.cs
/// <summary>
/// Where the pump writes. Bytes go through untouched: decoding a partial read is exactly the bug this
/// seam avoids. Having it as an interface is also what lets the pump be tested with no control, no
/// Skia natives and no window.
/// </summary>
public interface ITerminalSink
{
    /// <summary>Writes bytes straight to the terminal.</summary>
    void Feed(byte[] data, int length);

    /// <summary>Writes text the session generated itself, such as an exit banner.</summary>
    void Feed(string text);

    /// <summary>Resets the terminal.</summary>
    void Reset();
}
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/libs/RedisSetupTool.TerminalView/ExecTerminalSession.cs
private async Task PumpAsync()
{
    var buffer = new byte[Math.Max(256, _options.ReadBufferSize)];

    try
    {
        while (!_cancellation.IsCancellationRequested)
        {
            var read = await _session.ReadAsync(buffer, _cancellation.Token)
                .ConfigureAwait(false);
            if (read.EndOfStream)
            {
                break;
            }

            _sink.Feed(buffer, read.Count);
        }

        ExitCode = await _session.WaitForExitAsync(_cancellation.Token).ConfigureAwait(false);

        if (_options.ExitBanner)
        {
            _sink.Feed("\r\n\x1b[2m[process exited with code "
                + ExitCode.ToString(CultureInfo.InvariantCulture) + "]\x1b[0m\r\n");
        }

        SetState(TerminalSessionState.Exited);
    }
    catch (OperationCanceledException)
    {
        //Disposal cancels the pump; that is not a failure.
    }
    catch (Exception exception)
    {
        Fail(exception);
    }
}
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/libs/RedisSetupTool.TerminalView/TerminalControlSink.cs
/// <summary>
/// The only type in the application that names <c>TerminalControl</c>. Everything else drives the
/// console through <see cref="ITerminalSink"/>.
/// </summary>
public sealed class TerminalControlSink : ITerminalSink
{
    private readonly TerminalControl _control;

    // ...

    /// <inheritdoc />
    /// <remarks>The control's Feed is thread safe: it copies the buffer and enqueues on the
    /// dispatcher, so the pump can call it from a worker thread.</remarks>
    public void Feed(byte[] data, int length) => _control.Feed(data, length);

    /// <inheritdoc />
    public void Feed(string text) => _control.Feed(text);

    /// <inheritdoc />
    public void Reset() => _control.Reset();
}
```

The bug the seam exists to prevent then has a test of its own, and it needs
nothing but the SDK:

```csharp
// From CodeBrix.Samples/RedisSetupTool/tests/libs/RedisSetupTool.TerminalView.Tests/ExecTerminalSessionTests.cs
/// <summary>A read that splits a multi-byte sequence still produces the right text.</summary>
[Fact]
public async Task Pump_WhenAReadSplitsAUtf8Sequence_NeverDecodesAPartialRead()
{
    //Arrange
    var text = "café über";
    var bytes = Encoding.UTF8.GetBytes(text);
    var session = new FakeExecSession();
    var sink = new RecordingTerminalSink();
    await using var pump = new ExecTerminalSession(session, sink);

    //The split lands in the middle of the two-byte sequence for the accented letter.
    session.Emit(bytes[..4]);
    session.Emit(bytes[4..]);
    session.EndOfStream();

    //Act
    pump.Start();
    await WaitForAsync(() => pump.State == TerminalSessionState.Exited);

    //Assert
    sink.Chunks.Count.Should().Be(2);
    sink.DecodedText.Should().Be(text);
}
```

**Where to look.**
`RedisSetupTool/src/libs/RedisSetupTool.TerminalView/` (the sink, the pump, the
adapter and the factory that joins them)
`RedisSetupTool/tests/libs/RedisSetupTool.TerminalView.Tests/Fakes/` and
`tests/libs/RedisSetupTool.TerminalView.Tests/ExecTerminalSessionTests.cs`

**Sharp edges.**
- The sink takes a buffer and a length, not an array to keep. The pump reuses one
  buffer, so a recording fake that stores the reference records the same bytes
  over and over; copy the used prefix on the way in.
- Never decode inside the pump. A read can split a multi-byte sequence, and the
  test above is the whole reason the interface takes bytes rather than a string.
- Where the two sides disagree about argument order - here a control reporting
  columns then rows and a daemon taking rows then columns - do the swap in one
  place and have the fake record the far side's order, so the test asserts the
  swap rather than repeating it.
- The pump raises its state and size events on a worker thread, and the one
  adapter class is the compliance point for all of this: if a second type starts
  naming the control, or a dispatcher gets added to the library, the suite quietly
  stops covering the path the application actually runs.

