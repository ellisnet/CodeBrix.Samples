# CodeBrix.Samples Blueprints: Documents, data and web APIs

These recipes cover the work an application does with content rather than with
pixels: reading archives and embedded resources, encrypting and validating
text, calling REST and HTTP services, parsing messy markup into a typed model,
and generating, reading or saving PDF and image documents. The largest group
typesets one: a theme derived from the chosen page size, book sections with
running heads and folios, a contents page whose numbers are right after layout,
numbered figures with credit lines, ruled tables, a raised initial and rich
text runs. The shape they share is a UI-free library behind a service interface
that the view model resolves and awaits, so the work stays testable and none
of the heads know how it is done. Reach for this file when your application
has to fetch, parse, produce or save a document or a body of data, and you
want the network, file and layout code out of the view model. Along the way
they cover the care that work needs in practice: pacing outbound calls,
reporting true progress across a multi-file download, caching, embedding
fonts, and registering the formats you can import and export - including
letting that registry, rather than a page, say what a file dialog offers. A
last group is about a document that is drawn rather than composed, and about
the data behind it: vector art described once and rendered by both a page
control and the report, path data parsed into the document library's own path
type, a page cursor that gives hand placement margins and page breaks, tracked
capitals and justified copy typeset by hand, gradient stops blended to opaque
because their alpha is ignored, a saved file that stores identity and looks the
text up again, prose made deterministic by a seed the data itself produces, and
a static table built lazily because field initializers run in order.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Put the real work in a UI free library behind a service interface](#put-the-real-work-in-a-ui-free-library-behind-a-service-interface)
- [Encrypt text with the CodeBrix Cryptography library](#encrypt-text-with-the-codebrix-cryptography-library)
- [Read an embedded default value at run time](#read-an-embedded-default-value-at-run-time)
- [Guard Base64 input against invisible clipboard characters](#guard-base64-input-against-invisible-clipboard-characters)
- [Read a zip archive without extracting it with the CodeBrix Compression library](#read-a-zip-archive-without-extracting-it-with-the-codebrix-compression-library)
- [Resolve a file that another archive entry references by relative path](#resolve-a-file-that-another-archive-entry-references-by-relative-path)
- [Classify and group the contents of a container for browsing](#classify-and-group-the-contents-of-a-container-for-browsing)
- [Build a typed REST client with source generated JSON and its own exceptions](#build-a-typed-rest-client-with-source-generated-json-and-its-own-exceptions)
- [Call a REST API behind a service interface the view model resolves](#call-a-rest-api-behind-a-service-interface-the-view-model-resolves)
- [Pace outbound API calls with a rate gate](#pace-outbound-api-calls-with-a-rate-gate)
- [Throttle from the rate limit headers an API sends back](#throttle-from-the-rate-limit-headers-an-api-sends-back)
- [Be a polite HTTP client to a public API](#be-a-polite-http-client-to-a-public-api)
- [Fall back to one search per repository when a search API caps its results](#fall-back-to-one-search-per-repository-when-a-search-api-caps-its-results)
- [Normalize a user entered ID or URL before calling an API](#normalize-a-user-entered-id-or-url-before-calling-an-api)
- [Resolve an ID that may be one of several object kinds](#resolve-an-id-that-may-be-one-of-several-object-kinds)
- [Read a nested tree from an API with a cycle guard](#read-a-nested-tree-from-an-api-with-a-cycle-guard)
- [Batch a metadata API and treat the result as best effort](#batch-a-metadata-api-and-treat-the-result-as-best-effort)
- [Fetch a whole remote catalog once and cache images behind a concurrency gate](#fetch-a-whole-remote-catalog-once-and-cache-images-behind-a-concurrency-gate)
- [Report true byte progress across a multi file download with side car files](#report-true-byte-progress-across-a-multi-file-download-with-side-car-files)
- [Cache downloaded assets with a key you can invalidate](#cache-downloaded-assets-with-a-key-you-can-invalidate)
- [Parse messy HTML into structured blocks with the CodeBrix MarkupParse library](#parse-messy-html-into-structured-blocks-with-the-codebrix-markupparse-library)
- [Strip web only chrome while walking the DOM](#strip-web-only-chrome-while-walking-the-dom)
- [Upgrade thumbnail URLs to print resolution](#upgrade-thumbnail-urls-to-print-resolution)
- [Run a multi stage pipeline behind one service method](#run-a-multi-stage-pipeline-behind-one-service-method)
- [Register embedded OFL fonts with the PDF font system](#register-embedded-ofl-fonts-with-the-pdf-font-system)
- [Drop characters your embedded fonts cannot render](#drop-characters-your-embedded-fonts-cannot-render)
- [Derive a whole document theme from one page size choice](#derive-a-whole-document-theme-from-one-page-size-choice)
- [Compose a book with sections styles running heads and folios](#compose-a-book-with-sections-styles-running-heads-and-folios)
- [Build a table of contents with real page numbers and dot leaders](#build-a-table-of-contents-with-real-page-numbers-and-dot-leaders)
- [Place numbered framed figures with credit lines](#place-numbered-framed-figures-with-credit-lines)
- [Pair a figure with the credit paragraph that follows it](#pair-a-figure-with-the-credit-paragraph-that-follows-it)
- [Render booktabs style tables from parsed rows](#render-booktabs-style-tables-from-parsed-rows)
- [Open a document with a raised initial](#open-a-document-with-a-raised-initial)
- [Write rich text runs into a paragraph or a hyperlink](#write-rich-text-runs-into-a-paragraph-or-a-hyperlink)
- [Render into either a section or a table cell](#render-into-either-a-section-or-a-table-cell)
- [Keep unsupported content visible instead of failing the document](#keep-unsupported-content-visible-instead-of-failing-the-document)
- [Compose a fixed layout poster with the CodeBrix PdfDocuments library](#compose-a-fixed-layout-poster-with-the-codebrix-pdfdocuments-library)
- [Open a PDF and read its page count with the CodeBrix PdfRasterizer library](#open-a-pdf-and-read-its-page-count-with-the-codebrix-pdfrasterizer-library)
- [Rasterize a PDF page to PNG off the UI thread](#rasterize-a-pdf-page-to-png-off-the-ui-thread)
- [Keep two documents in step while letting the user offset one](#keep-two-documents-in-step-while-letting-the-user-offset-one)
- [Treat two spellings of one path as the same file](#treat-two-spellings-of-one-path-as-the-same-file)
- [Register import and export formats at startup through one entry point](#register-import-and-export-formats-at-startup-through-one-entry-point)
- [Add codec coverage beyond SkiaSharp with the CodeBrix Imaging library](#add-codec-coverage-beyond-skiasharp-with-the-codebrix-imaging-library)
- [Save a document through a native picker with format filters](#save-a-document-through-a-native-picker-with-format-filters)
- [Raise a UI hook from a codec through a static event](#raise-a-ui-hook-from-a-codec-through-a-static-event)
- [Offer a data folder by rule rather than by a hard-coded list](#offer-a-data-folder-by-rule-rather-than-by-a-hard-coded-list)
- [Build a file dialog's filter list in the format registry](#build-a-file-dialogs-filter-list-in-the-format-registry)
- [Describe vector art once and render it through two renderers](#describe-vector-art-once-and-render-it-through-two-renderers)
- [Convert SVG path data into a PDF graphics path](#convert-svg-path-data-into-a-pdf-graphics-path)
- [Give a hand-placed document a page cursor with margins and breaks](#give-a-hand-placed-document-a-page-cursor-with-margins-and-breaks)
- [Typeset tracked capitals justified copy and fitted lines by hand](#typeset-tracked-capitals-justified-copy-and-fitted-lines-by-hand)
- [Blend a tint to opaque because gradient stops ignore alpha](#blend-a-tint-to-opaque-because-gradient-stops-ignore-alpha)
- [Save only identity and rehydrate the text from the data tables](#save-only-identity-and-rehydrate-the-text-from-the-data-tables)
- [Make generated prose deterministic from a seed the data makes](#make-generated-prose-deterministic-from-a-seed-the-data-makes)
- [Build a static data table lazily because field initializers run in order](#build-a-static-data-table-lazily-because-field-initializers-run-in-order)

## Related blueprints

- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - the view-model side of these services: async commands, progress properties and cancellation
- [BLUEPRINTS-ProjectLayoutAndPackaging.md](BLUEPRINTS-ProjectLayoutAndPackaging.md) - how the UI-free library project and its embedded resources are set up
- [BLUEPRINTS-PlatformServices.md](BLUEPRINTS-PlatformServices.md) - reaching the file pickers and other platform services these save and open paths depend on
- [BLUEPRINTS-GraphicsAndRendering.md](BLUEPRINTS-GraphicsAndRendering.md) - showing a decoded image or a rasterized page once the service hands one back

---

## Documents, data and web APIs

### Put the real work in a UI free library behind a service interface

**When you want this.** The application's actual functionality should be testable,
reusable and independent of any of the heads.

**The MVVM shape.** The library targets plain `net10.0`, references no UI
framework, exposes one interface, takes its dependencies through its constructor,
and registers itself through its own `IServiceCollection` extension. The view
model resolves the interface and calls it; nothing else in the application knows
the implementation exists.

**Code.**

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/JustBetweenUs.Encryption.csproj -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <Compile Include="..\Shared\Helpers\EmbeddedResourceHelper.cs" Link="Helpers\EmbeddedResourceHelper.cs" />
</ItemGroup>
<ItemGroup>
  <EmbeddedResource Include="Embedded\DefaultKey.txt" />
</ItemGroup>
<ItemGroup>
  <!-- ... the CodeBrix Cryptography library plus the Microsoft.Extensions -->
  <!-- ... hosting and logging abstractions; no CodeBrix.Platform package ... -->
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/Services/IEncryptionService.cs
public interface IEncryptionService
{
    bool IsBase64Text(string text);
    Task<string> GetDefaultKey();
    Task<string> OriginalTripleDES_EncryptToBase64(string key, string toEncrypt);
    Task<string> OriginalTripleDES_DecryptFromBase64(string key, string toDecrypt);
    Task<string> AES_EncryptToBase64(string key, string toEncrypt);
    Task<string> AES_DecryptFromBase64(string key, string toDecrypt);
    Task<string> Twofish_EncryptToBase64(string key, string toEncrypt);
    Task<string> Twofish_DecryptFromBase64(string key, string toDecrypt);
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
_encryptSvc = GetService<IEncryptionService>();
// ...
ProcessedText = await _encryptSvc.AES_EncryptToBase64(EncryptionKey.Trim(), EnteredText);
```

**Where to look.**
`JustBetweenUs/JustBetweenUs.Encryption/JustBetweenUs.Encryption.csproj`
`JustBetweenUs/JustBetweenUs.Encryption/Services/IEncryptionService.cs`
`JustBetweenUs/JustBetweenUs.Encryption/RegisterServices.cs`

**Also shown by.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/`,
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/`,
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/`,
`PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/`,
`InannaRosette/src/libs/InannaRosette.Reading/`
(the deck, the spread, the prose, the JSON and the printable report behind
`IReadingInterpreter`, `IReadingSerializer` and `IPdfReportBuilder`, in a library
with no CodeBrix.Platform reference and no XAML anywhere in it)

**Sharp edges.**
- The library is referenced by the shared platform library and, separately, by each
  native head. Nothing in it can reference back up.
- Every method returns a task even where the work is synchronous, and the
  implementation wraps the CPU-bound calls in `Task.Run` so the work does not run
  on the UI thread. That is the shape that keeps the view model's commands
  genuinely asynchronous.
- An `[Obsolete]` attribute on a concrete method is invisible to a caller going
  through the interface. If you want callers warned, put the attribute on the
  interface member too.

### Encrypt text with the CodeBrix Cryptography library

**When you want this.** Symmetric encryption inside a service, using algorithms
the base class library does not provide, with the randomness carried alongside the
ciphertext so a single encoded string is all the user has to copy.

**The MVVM shape.** All of it is inside the service implementation. The view model
selects an algorithm from its picker and calls the matching method; it never sees
a cipher, a key derivation or a byte array.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs
using CodeBrix.Cryptography.Crypto;
using CodeBrix.Cryptography.Crypto.Digests;
using CodeBrix.Cryptography.Crypto.Engines;
using CodeBrix.Cryptography.Crypto.Generators;
using CodeBrix.Cryptography.Crypto.Paddings;
using CodeBrix.Cryptography.Crypto.Parameters;

// ...

private const int TwofishSaltLength = 16;

private IBufferedCipher GetTwofishCipher(bool forEncryption, byte[] keyBytes, byte[] saltBytes)
{
    var paramGen = new Pkcs5S2ParametersGenerator(new Sha3Digest());
    paramGen.Init(keyBytes, saltBytes, 1000);

    var engine = new TwofishEngine();
    var cipher = new PaddedBufferedBlockCipher(engine, new Pkcs7Padding());
    cipher.Init(forEncryption,
        parameters: (KeyParameter)paramGen.GenerateDerivedParameters(engine.AlgorithmName, 256));

    return cipher;
}

public async Task<string> Twofish_EncryptToBase64(string key, string toEncrypt)
{
    // ... argument checks that log and return an empty string ...
    var keyBytes = await GetKeyBytes(key);
    var encryptBytes = await Task.Run(() => Encoding.UTF8.GetBytes(toEncrypt));

    var saltBytes = RandomNumberGenerator.GetBytes(TwofishSaltLength);
    var cipher = GetTwofishCipher(true, keyBytes, saltBytes);
    var encrypted = await Task.Run(() => cipher.DoFinal(encryptBytes));

    //Attach our salt bytes to the end of our encrypted byte array - see notes in AES_EncryptToBase64
    encrypted = encrypted.Concat(saltBytes).ToArray();

    result = await Task.Run(() => Convert.ToBase64String(encrypted));
    // ...
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs
//IMPORTANT INFORMATION ABOUT THE INITIALIZATION VECTOR (IV):
//  The IV bytes must be RANDOM, however they don't need to be SECRET;
//  BUT the same bytes also must be used as the IV on the decryption end.
//  So, we generate a random set of bytes; use it as our IV; and
//  then tack it onto the end of our encrypted message; so we can
//  retrieve it when it is time to decrypt the message.

var ivBytes = RandomNumberGenerator.GetBytes(aes.IV.Length);

aes.Key = keyBytes;
aes.IV = ivBytes;

using var encryptor = aes.CreateEncryptor();
var encrypted = await Task.Run(() =>
    encryptor.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length));

encrypted = encrypted.Concat(ivBytes).ToArray();
```

**Where to look.**
`JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs`

**Sharp edges.**
- Both random-material schemes append their bytes to the end of the ciphertext
  before encoding, and both decryption paths first check that the incoming array
  is longer than that material before splitting it. Skipping that check turns a
  malformed paste into an unhelpful exception.
- The Triple DES path has no randomness at all, which is why its output is
  reproducible and why the tests can assert an exact string for it. It is in the
  sample as a deliberately obsolete example.
- The text key is turned into key bytes with a plain hash in the two older paths.
  That is a sample-grade key derivation, not a recommendation; the Twofish path
  shows the better shape with a salted derivation over a modern digest.

### Read an embedded default value at run time

**When you want this.** A default that must ship inside the assembly rather than
as a file on disk, read once and cached.

**The MVVM shape.** The service owns the reading and the caching; the view model
just awaits a task. A helper resolves the manifest resource name from a folder
path and a root namespace so callers do not have to spell the name out.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs
public async Task<string> GetDefaultKey()
{
    var result = _defaultKey;

    if (string.IsNullOrWhiteSpace(result))
    {
        try
        {
            var filename = $"Embedded/{DefaultKeyFilename}";
            _logger.LogInformation("Attempting to read the file: {FilePath}", filename);
            var key = (await EmbeddedResourceHelper.GetResourceAsString(filename,
                typeof(RegisterServices).Namespace)).Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new DataException($"The embedded file {DefaultKeyFilename} appears to be empty.");
            }

            result = _defaultKey = key.Trim();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error while trying to {nameof(GetDefaultKey)}.");
        }
    }

    return result;
}
```

**Where to look.**
`JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs`
`JustBetweenUs/Shared/Helpers/EmbeddedResourceHelper.cs`
`JustBetweenUs/JustBetweenUs.Encryption/JustBetweenUs.Encryption.csproj`

**Sharp edges.**
- The root namespace is derived from a type in the assembly rather than
  hard-coded, so renaming the project's default namespace does not break the
  lookup.
- The helper converts both slash characters in the path into dots, so the same
  call works whichever separator you write.
- The project file needs both a `None Remove` and an `EmbeddedResource Include`
  for the file; without the removal it is also treated as content.
- The helper offers a line-ending option so a text resource can be normalized,
  which matters for anything compared against a literal in a test.

### Guard Base64 input against invisible clipboard characters

**When you want this.** Users paste your encoded output back into your application
and decoding sometimes fails for no visible reason.

**The MVVM shape.** The guard belongs in the service, applied at every point where
text becomes bytes, so no caller can forget it. The view model asks the service
whether the text is valid and gets a truthful answer.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs
//Strips any character that is not part of the standard Base64 alphabet
//  (A-Z, a-z, 0-9, '+', '/', '='). This guards against invisible junk that
//  can ride along on a copy/paste round-trip - e.g. a stray U+0001 control
//  character that was observed being prepended on the clipboard->TextBox path
//  on Intel/x64 macOS. Such characters are never part of valid Base64, so it
//  is always safe to remove them before decoding.
private static string CleanBase64(string text) =>
    string.IsNullOrEmpty(text)
        ? text ?? string.Empty
        : new string(text.Where(c =>
            (c >= 'A' && c <= 'Z') ||
            (c >= 'a' && c <= 'z') ||
            (c >= '0' && c <= '9') ||
            c == '+' || c == '/' || c == '=').ToArray());

public bool IsBase64Text(string text)
{
    var result = false;

    if (!string.IsNullOrWhiteSpace(text))
    {
        try
        {
            var converted = Convert.FromBase64String(CleanBase64(text));
            result = converted is { Length: > 0 };
        }
        catch (Exception)
        {
            result = false;
        }
    }

    return result;
}
```

**Where to look.**
`JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs`
`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs`

**Sharp edges.**
- The cleaner is applied in all three decrypt paths as well as in the validity
  check, so the check and the decode always agree.
- The bug it guards against was head-specific and invisible: a control character
  prepended on the clipboard-to-text-box path on one platform. Anything that
  survives a system clipboard round trip in a multi-head application deserves this
  kind of guard, and a regression test.

### Read a zip archive without extracting it with the CodeBrix Compression library

**When you want this.** Your application browses a container file and should read
individual members on demand rather than unpacking everything.

**The MVVM shape.** A plain class in a UI-free library owns the open archive and
serves byte reads. A service wraps every call in `Task.Run` so the view model
never blocks the UI thread, and the view model owns the archive's lifetime.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/BundleArchive.cs
public class BundleArchive : IDisposable
{
    private readonly ZipFile _zipFile;
    private readonly Dictionary<string, long> _indexesByEntryPath;

    //ZipFile entry streams share the underlying FileStream, so reads are serialized
    private readonly object _gate = new();

    public BundleArchive(string zipPath)
    {
        ZipPath = zipPath ?? throw new ArgumentNullException(nameof(zipPath));
        _zipFile = new ZipFile(zipPath);

        //ZipFile.GetEntry is an O(n) scan per lookup; build a name index once instead
        _indexesByEntryPath = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var entries = new List<AssetEntry>();
        foreach (ZipEntry entry in _zipFile)
        {
            if (entry.IsDirectory) { continue; }
            _indexesByEntryPath[entry.Name] = entry.ZipFileIndex;
            entries.Add(new AssetEntry(entry.Name, Math.Max(0, entry.Size), AssetClassifier.Classify(entry.Name)));
        }

        Entries = entries.OrderBy(e => e.EntryPath, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public byte[] ReadEntryBytes(string entryPath)
    {
        if (entryPath == null || !_indexesByEntryPath.TryGetValue(entryPath, out var index))
        {
            return null;
        }

        lock (_gate)
        {
            using var input = _zipFile.GetInputStream(index);
            using var buffer = new MemoryStream();
            input.CopyTo(buffer);
            return buffer.ToArray();
        }
    }

    /// <summary>Closes the underlying zip file.</summary>
    public void Dispose() => _zipFile.Close();
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/BundleArchive.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/AssetFolderCatalog.cs`

**Sharp edges.**
- Looking an entry up by name is a linear scan; build a case-insensitive
  name-to-index dictionary once in the constructor and read by index afterwards.
- Entry streams share the archive's underlying file stream, so every read is taken
  under a lock. Reads can still be issued from worker threads; they simply
  serialize.
- Skip directory entries, and clamp a reported size, because an archive may report
  an unknown one.
- The whole-folder load catches per-file exceptions and turns them into warnings,
  so one corrupt archive does not fail the catalog.

### Resolve a file that another archive entry references by relative path

**When you want this.** A file inside your container refers to a sibling - a model
to its texture, a map to its tileset, a tileset to its image - and you have to
turn that reference into a real member of the archive.

**The MVVM shape.** A pure lookup method on the archive class. Because it returns
a resolved path as well as bytes, a resolved path can anchor the next reference in
a chain.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/BundleArchive.cs
public string ResolveDependencyPath(string baseEntryPath, string relativeUri)
{
    if (baseEntryPath == null || string.IsNullOrWhiteSpace(relativeUri)) { return null; }

    var relative = relativeUri.Replace('\\', '/').TrimStart('/');
    var lastSlash = baseEntryPath.Replace('\\', '/').LastIndexOf('/');
    var folder = lastSlash < 0 ? string.Empty : baseEntryPath.Substring(0, lastSlash);

    //Try the reference against the entry's own folder, then each parent up to the root
    while (true)
    {
        var candidate = NormalizePath(folder.Length == 0 ? relative : folder + "/" + relative);
        if (HasEntry(candidate)) { return candidate; }
        if (folder.Length == 0) { break; }

        var parentSlash = folder.LastIndexOf('/');
        folder = parentSlash < 0 ? string.Empty : folder.Substring(0, parentSlash);
    }

    //Last resort: match the bare file name anywhere in the archive
    var fileNameSlash = relative.LastIndexOf('/');
    var fileName = fileNameSlash < 0 ? relative : relative.Substring(fileNameSlash + 1);
    var match = Entries.FirstOrDefault(e =>
        e.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    return match?.EntryPath;
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
foreach (var tilesetRef in map.Tilesets)
{
    var info = tilesetRef.Inline;
    var basePath = entry.EntryPath;
    if (tilesetRef.Source != null)
    {
        basePath = archive?.ResolveDependencyPath(entry.EntryPath, tilesetRef.Source);
        if (basePath == null) { continue; }
        TiledMapParser.TryParseTileset(archive.ReadEntryText(basePath), out info);
    }
    if (info == null) { continue; }

    var imagePath = archive?.ResolveDependencyPath(basePath, info.ImagePath);
    var imageBytes = imagePath == null ? null : archive.ReadEntryBytes(imagePath);
    if (imageBytes == null) { continue; }

    resolved.Add((tilesetRef.FirstGid, info, LdrImageDecoder.Decode(imageBytes)));
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/BundleArchive.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Normalize separators and collapse relative segments before any lookup, because
  references inside asset files are written by many different tools.
- The bare-file-name fallback is deliberate and last; without it, packs whose
  references are written relative to an export folder that does not exist in the
  archive would fail.
- Dispose intermediate decoded images in a `finally` after compositing; only the
  composited result survives.

### Classify and group the contents of a container for browsing

**When you want this.** A container holds many files that should be presented as
fewer, more meaningful items - one entry per logical asset rather than one per
file.

**The MVVM shape.** The classification and grouping live in the UI-free library
and produce plain model objects. The view model turns them into cells and decides
which raw entries to hide because a grouped card already represents them.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/KenneyBundleReader.cs
//Groups 3D model entries that share a file-name stem into one ModelAsset per model
private static List<ModelAsset> GroupModelAssets(IReadOnlyList<AssetEntry> entries)
{
    var modelEntries = entries.Where(e => e.Kind == AssetKind.Model3D).ToList();
    if (modelEntries.Count == 0) { return []; }

    var previewsByName = entries
        .Where(e => e.Kind == AssetKind.Image &&
            e.EntryPath.StartsWith("Previews/", StringComparison.OrdinalIgnoreCase))
        .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(g => g.Key, g => g.First().EntryPath, StringComparer.OrdinalIgnoreCase);

    // ... materialsByName the same way ...

    return modelEntries
        .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
        .Select(group => new ModelAsset(
            group.Key,
            group.OrderBy(e => FormatRank(e.Extension)).ToList(),
            previewsByName.TryGetValue(group.Key, out var preview) ? preview : null,
            materialsByName.TryGetValue(group.Key, out var material) ? material : null))
        .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
//Sheet images and their XML files are represented by the Spritesheet cards
var atlasPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
foreach (var atlas in bundle.Atlases)
{
    atlasPaths.Add(atlas.ImageEntryPath);
    atlasPaths.Add(atlas.XmlEntryPath);
}

foreach (var entry in bundle.Entries)
{
    //Model files (and their materials) are represented by the grouped Model cards
    if (entry.Kind is AssetKind.Model3D or AssetKind.Material) { continue; }
    // ... hide atlas members and per-model preview renders when browsing "All categories" ...
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/KenneyBundleReader.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/Parsing/AssetClassifier.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Classification is a single extension-to-kind dictionary with an unknown
  fallback; adding a format is a one-line change and the test is a theory over the
  mapping.
- The grouped-card rule must be applied twice - once when building cells, once
  when building the category filter - or a folder of hidden files shows up as an
  empty category.
- Identity comes from the pack's own license file first, falling back to a
  prettified file name; the title parser rejects a first line that is too long or
  contains a URL, because those are license prose rather than a title.
- Some packs declare a stale image path; the reader falls back to the same-stem
  sibling beside it rather than dropping the asset.

### Build a typed REST client with source generated JSON and its own exceptions

**When you want this.** You want an API library with no UI dependency, trimmable
JSON, a sensible timeout policy, error types callers can catch meaningfully, and
DI-friendly registration with correct client lifetime.

**The MVVM shape.** A standalone library project consumed only through its
interface, with an interface for the client, an interface for a factory, an
options class, and one `IServiceCollection` extension. It has no platform
reference at all, which is what lets its whole test suite run offline. View models
never construct an HTTP client.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.PolyHavenApiClient/PolyHavenServiceCollectionExtensions.cs
public static IServiceCollection AddPolyHavenApiClient(
    this IServiceCollection services,
    Action<PolyHavenClientOptions>? configureOptions = null)
{
    ArgumentNullException.ThrowIfNull(services);

    var options = new PolyHavenClientOptions();
    configureOptions?.Invoke(options);

    services.AddHttpClient(DefaultPolyHavenClientFactory.HttpClientName)
        .ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            AutomaticDecompression = DecompressionMethods.All,
        });

    services.TryAddSingleton<IPolyHavenApiClientFactory>(serviceProvider =>
        new DefaultPolyHavenClientFactory(
            serviceProvider.GetRequiredService<IHttpClientFactory>(), options));

    return services;
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/Serialization/PolyHavenJsonContext.cs
/// <summary>
/// Source-generated JSON serialization context for Poly Haven API response types.
/// The API uses snake_case property names (e.g. <c>date_published</c>).
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(Dictionary<string, PolyHavenAsset>))]
[JsonSerializable(typeof(PolyHavenAsset))]
[JsonSerializable(typeof(PolyHavenAuthor))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Dictionary<string, int>))]
internal sealed partial class PolyHavenJsonContext : JsonSerializerContext;
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/DefaultPolyHavenClientFactory.cs
// Metadata calls enforce their own timeout; downloads are governed by the caller's
// CancellationToken, so the HttpClient-level timeout must not cut long downloads short.
httpClient.Timeout = Timeout.InfiniteTimeSpan;
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/RestPolyHavenApiClient.cs
private async Task<string> GetStringAsync(
    string relativeUrl, string resourceDescription, CancellationToken cancellationToken)
{
    ThrowIfDisposed();

    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    if (_options.MetadataRequestTimeout > TimeSpan.Zero)
    {
        timeoutCts.CancelAfter(_options.MetadataRequestTimeout);
    }

    using var response = await _httpClient.GetAsync(
        new Uri(_baseUri, relativeUrl), HttpCompletionOption.ResponseContentRead, timeoutCts.Token)
        .ConfigureAwait(false);
    await EnsureSuccessAsync(response, resourceDescription, timeoutCts.Token).ConfigureAwait(false);
    return await response.Content.ReadAsStringAsync(timeoutCts.Token).ConfigureAwait(false);
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/IPolyHavenApiClient.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/RestPolyHavenApiClient.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.PolyHavenApiClient/PolyHavenServiceCollectionExtensions.cs`

**Sharp edges.**
- A single client-wide timeout cannot serve both metadata calls and large
  downloads. Disable it and apply a per-call linked-token timeout to metadata
  requests only.
- The registration is a factory singleton, not a client singleton. Clients are
  cheap and short-lived, and disposing one never tears down the shared connection
  pool. The factory also works standalone with its own pooled handler - which is
  how the tests use it - and in that case it constructs the client without
  disposing the handler.
- Downloads stream with a headers-only completion option and report progress per
  buffer; checksum verification folds into the same read loop.
- A not-found response maps to a distinct exception type so callers can tell
  "missing" from "failed"; unparseable JSON becomes an API exception naming the
  resource; a failed download deletes the partially written file.
- The interface's documentation names the endpoint each method calls, which makes
  the library readable without a separate document.

### Call a REST API behind a service interface the view model resolves

**When you want this.** Your application talks to a REST API with the user's own
credential, and you want the whole conversation behind one interface the view
model can hold.

**The MVVM shape.** The library owns the interface; the implementation creates and
holds the client, and every method the view model needs is one call returning
plain data objects. Reconnecting disposes the previous client. Every method takes
an optional cancellation token.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/INotionDocumentService.cs
public interface INotionDocumentService
{
    /// <summary>Validates the token and returns the bot user's name, or throws.</summary>
    Task<string> ConnectAsync(string integrationToken, CancellationToken cancellationToken = default);

    /// <summary>Loads the root node(s) for a page ID or a database/data-source ID.</summary>
    Task<IList<NotionPageNode>> LoadRootsAsync(string pageOrDatabaseId,
        CancellationToken cancellationToken = default);

    /// <summary>Loads the immediate child pages of a node (called on expand).</summary>
    Task<IList<NotionPageNode>> LoadChildrenAsync(string pageId,
        CancellationToken cancellationToken = default);

    /// <summary>A short, non-scrolling preview for the right-hand pane.</summary>
    Task<NotionPagePreview> LoadPreviewAsync(string pageId,
        CancellationToken cancellationToken = default);

    /// <summary>Renders the selected pages, in the given order, into one book PDF.</summary>
    Task<CreatedDocument> CreateDocumentAsync(CreateRequest request,
        IProgress<CreateProgress> progress = null, CancellationToken cancellationToken = default);
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs
public async Task<string> ConnectAsync(
    string integrationToken, CancellationToken cancellationToken = default)
{
    CheckIsDisposed();
    if (string.IsNullOrWhiteSpace(integrationToken))
    {
        throw new ArgumentException("An integration token is required.", nameof(integrationToken));
    }

    //Reconnecting replaces any previous client (the user may paste a new token)
    _client?.Dispose();
    _client = null;
    _treeReader = null;
    _pageReader = null;

    var client = NotionClientFactory.Instance.Create(new ClientOptions
    {
        AuthToken = integrationToken.Trim(),
        //Retries transient 429/5xx responses; the NotionRateGate does the proactive pacing
        RetryPolicy = new DefaultRetryPolicy()
    });

    try
    {
        var user = await _gate.RunAsync(
            () => client.Users.MeAsync(cancellationToken), cancellationToken);

        _client = client;
        _treeReader = new NotionTreeReader(client, _gate);
        _pageReader = new NotionPageReader(client, _gate);

        var botName = string.IsNullOrWhiteSpace(user.Name) ? "Notion integration" : user.Name;
        _logger.LogInformation("Connected to Notion as bot \"{Bot}\".", botName);
        return botName;
    }
    catch
    {
        client.Dispose();
        throw;
    }
}

private NotionTreeReader CheckConnected()
{
    if (_client is null || _treeReader is null || _pageReader is null)
    {
        throw new InvalidOperationException(
            "Not connected to Notion — call ConnectAsync with an integration token first.");
    }
    return _treeReader;
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/INotionDocumentService.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs`

**Sharp edges.**
- The connect call is a real round trip, so a bad credential fails immediately
  with a message the view model can show, instead of failing later inside a long
  build.
- If the validation call throws, the half-built client is disposed before the
  exception propagates, and the service stays in its previous state.
- The service is disposable and disposes both the client and its rate gate;
  `ObjectDisposedException.ThrowIf` guards every public method.
- Retries for transient failures come from the client's own retry policy; the
  application layers proactive pacing on top rather than replacing it.

### Pace outbound API calls with a rate gate

**When you want this.** An API publishes a request-rate limit and your application
can easily exceed it while walking a tree.

**The MVVM shape.** A tiny internal gate object owned by the service; every call
site wraps its API call in it. The view model knows nothing about it.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionRateGate.cs
/// <summary>
/// Serialises Notion API calls and enforces a minimum delay between them, keeping
/// the app inside Notion's published rate limit of roughly three requests per
/// second average. The NotionApi client's resilience layer already retries
/// transient 429s; this gate keeps us from provoking them in the first place.
/// </summary>
internal sealed class NotionRateGate : IDisposable
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromMilliseconds(350);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private DateTimeOffset _lastCallCompleted = DateTimeOffset.MinValue;
    private bool _isDisposed;

    public async Task<T> RunAsync<T>(Func<Task<T>> apiCall, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(apiCall);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var sinceLast = DateTimeOffset.UtcNow - _lastCallCompleted;
            if (sinceLast < MinimumInterval)
            {
                await Task.Delay(MinimumInterval - sinceLast, cancellationToken).ConfigureAwait(false);
            }

            return await apiCall().ConfigureAwait(false);
        }
        finally
        {
            _lastCallCompleted = DateTimeOffset.UtcNow;
            _gate.Release();
        }
    }
    // ...
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionRateGate.cs`

**Sharp edges.**
- The semaphore serializes calls as well as spacing them, so concurrent
  expansions cannot burst.
- `ConfigureAwait(false)` throughout: this is library code and must not capture
  the UI context.
- The interval is measured from call completion, not call start.

### Throttle from the rate limit headers an API sends back

**When you want this.** A public API publishes a rate limit, reports what is left of it
in every response, and refuses you outright when you overrun it. You want to stay inside
the limit without guessing, and you want to test the waiting without waiting.

**The MVVM shape.** One throttle object per quota pool, owned by the service and
invisible to the view model. It trusts the response headers first, because they account
for every other caller sharing the address, and falls back to a sliding count of its own
calls before the first response has arrived. It takes a `TimeProvider`, so a test can run
its waits on a fake clock.

**Code.**

The gate: the headers decide, then the local count, and only then is the call counted and
made.

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/RateLimitThrottle.cs
//Waits until a call may be made, then counts it. Both waits report themselves once a
//second through reportWait, which may be null when nobody is listening.
internal async Task AcquireAsync(Action<TimeSpan, DateTimeOffset> reportWait,
    CancellationToken cancellationToken)
{
    await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
        //The headers win: GitHub knows about every other caller on this address.
        var snapshot = Reported;
        if (snapshot != null && snapshot.Remaining <= 1
            && _timeProvider.GetUtcNow() < snapshot.ResetAt)
        {
            await DelayUntilAsync(snapshot.ResetAt + ResetGrace, reportWait, cancellationToken)
                .ConfigureAwait(false);
        }

        //Then the local count, which is all there is to go on before the first response.
        while (true)
        {
            DateTimeOffset oldest;
            lock (_issued)
            {
                Trim(_timeProvider.GetUtcNow());
                if (_issued.Count < Ceiling) { break; }
                oldest = _issued.Peek();
            }

            await DelayUntilAsync(oldest + Window, reportWait, cancellationToken).ConfigureAwait(false);
        }

        RecordIssued();
    }
    finally
    {
        _gate.Release();
    }
}
```

Every response refreshes what the pool reports, and a response with no rate-limit headers
leaves the last reading in place rather than pretending the pool is unknown:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/RateLimitThrottle.cs
internal void UpdateFrom(HttpResponseHeaders headers)
{
    if (headers == null) { return; }
    if (!TryReadLong(headers, LimitHeaderName, out var limit)) { return; }
    if (!TryReadLong(headers, RemainingHeaderName, out var remaining)) { return; }
    if (!TryReadLong(headers, ResetHeaderName, out var reset)) { return; }

    Reported = new RateLimitSnapshot((int)limit, (int)remaining, Ceiling,
        DateTimeOffset.FromUnixTimeSeconds(reset));
}
```

What the application *displays* is not the raw header value. It is worked out on every
read, so a budget pill reaches zero exactly when the throttle starts waiting and climbs
back on its own:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/RateLimitThrottle.cs
internal RateLimitSnapshot Snapshot
{
    get
    {
        var reported = Reported;
        if (reported == null) { return null; }

        //Past the reset moment GitHub's pool has refilled, so the number the last response
        //carried is stale and the pool is full again.
        var reportedLeft = _timeProvider.GetUtcNow() >= reported.ResetAt
            ? reported.Limit
            : reported.Remaining;

        var left = Ceiling - IssuedInWindow;
        if (left < 0) { left = 0; }
        if (reportedLeft < left) { left = reportedLeft; }
        if (left < 0) { left = 0; }

        return new RateLimitSnapshot(reported.Limit, left, Ceiling, reported.ResetAt);
    }
}
```

A refusal is still handled, because another caller on the same address can empty the pool
between the last response and this call. The retry waits once, counts itself in, and sends
again; a second refusal becomes a typed exception naming the reset time.

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/GitHubIssueSearchService.cs
//One call, with everything the rate limits ask of it: wait for a slot, send, take the
//new pool figures from the headers, and give a refusal exactly one second chance.
private async Task<string> GetAsync(string relativeUrl, RateLimitThrottle throttle, string owner,
    SearchState state, IProgress<SearchProgress> progress, CancellationToken cancellationToken)
{
    ObjectDisposedException.ThrowIf(IsDisposed, this);

    var reportWait = BuildWaitReporter(progress, state);
    var absoluteUrl = new Uri(_httpClient.BaseAddress, relativeUrl).AbsoluteUri;

    await throttle.AcquireAsync(reportWait, cancellationToken).ConfigureAwait(false);
    var attempt = await SendOnceAsync(relativeUrl, absoluteUrl, throttle, cancellationToken)
        .ConfigureAwait(false);
    if (attempt.IsSuccess) { return attempt.Body; }

    if (attempt.IsRateLimitRefusal)
    {
        var until = attempt.RetryAfter.HasValue
            ? TimeProvider.GetUtcNow() + attempt.RetryAfter.Value
            : ResetMoment(throttle);

        await throttle.DelayUntilAsync(until, reportWait, cancellationToken).ConfigureAwait(false);

        //The wait replaced the gate, so the retry only has to be counted.
        throttle.RecordIssued();
        attempt = await SendOnceAsync(relativeUrl, absoluteUrl, throttle, cancellationToken)
            .ConfigureAwait(false);
        if (attempt.IsSuccess) { return attempt.Body; }
    }

    throw BuildException(attempt, absoluteUrl, throttle, owner);
}
```

**The test technique.** The clock seam is a `TimeProvider` on the internal constructor.
The test double is a real `TimeProvider` subclass that treats being asked for a timer as
being asked to wait: it records the wait, jumps its own clock to the due moment, and hands
the callback to the thread pool. A test of an hour-long wait finishes in a millisecond,
against the shipping code unchanged, and can then assert on how long the code *believed* it
waited.

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/libs/GitHubIssueFinder.GitHub.Tests/RateLimitThrottleTests.cs
[Fact]
public async Task the_snapshot_climbs_back_as_the_window_slides()
{
    //Arrange
    var clock = new FakeTimeProvider(Start);
    var throttle = new RateLimitThrottle("search", 3, Minute, clock);
    await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
    throttle.UpdateFrom(Headers(10, 9, Start.AddSeconds(60)));
    throttle.Snapshot.Remaining.Should().Be(2);

    //Act - the call ages out of the window
    clock.Advance(TimeSpan.FromSeconds(61));

    //Assert
    throttle.Snapshot.Remaining.Should().Be(3);
}
```

One case still proves the real clock, so the fake cannot hide a mistake in the waiting
itself:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/tests/libs/GitHubIssueFinder.GitHub.Tests/RateLimitThrottleTests.cs
[Fact]
public async Task a_run_at_the_ceiling_on_the_real_clock_really_waits()
{
    //Arrange - a one second window, so the wall clock proves the point in a moment
    var throttle = new RateLimitThrottle("search", 1, TimeSpan.FromSeconds(1), TimeProvider.System);
    await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
    var watch = Stopwatch.StartNew();

    //Act
    await throttle.AcquireAsync(null, TestContext.Current.CancellationToken);
    watch.Stop();

    //Assert - the second call waited out the window instead of going straight through
    watch.Elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(900));
}
```

**Where to look.**
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/RateLimitThrottle.cs`
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/GitHubIssueSearchService.cs`
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Models/RateLimitSnapshot.cs`
`GitHubIssueFinder/tests/libs/GitHubIssueFinder.GitHub.Tests/TestDoubles/FakeTimeProvider.cs`

**Related.**
[Pace outbound API calls with a rate gate](#pace-outbound-api-calls-with-a-rate-gate) is
the simpler shape for an API that publishes a rate but does not report what is left.

**Sharp edges.**
- Hold yourself one call below the published ceiling. The last call in a pool is the one
  every other process sharing your address is also about to take.
- Keep two numbers apart: what the server reported, and what your own budget allows. The
  wait decisions want the first; the display wants the smaller of the two, or the pill
  says one call is available at the moment you start waiting.
- Give the reset moment a second of grace. Your clock and the server's are never exactly
  the same, and coming back a moment early is a refusal.
- One throttle per pool, not one per client. Two pools with different windows - a
  per-minute search allowance and a per-hour general one - are two objects, and the work
  that spends one must not be able to starve the other.
- `Task.Delay(slice, timeProvider, token)` takes the provider, so the fake clock reaches
  the delay as well as the arithmetic. A `Task.Delay` without it is a real wait no test
  can skip.

### Be a polite HTTP client to a public API

**When you want this.** You are downloading many files from someone else's servers
and do not want to be blocked.

**The MVVM shape.** One disposable internal client owning one HTTP client, with an
identifying user agent, a timeout, and a semaphore-guarded minimum gap between
downloads.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/WikipediaClient.cs
//Wikimedia's user-agent policy asks for an identifying UA string with contact info
private const string UserAgent =
    "WikipediaPublisher/1.0 (https://github.com/ellisnet; jeremy@ellisnet.com) CodeBrix.MarkupParse";

private const int MediaDownloadDelayMs = 250;

private readonly HttpClient _httpClient;
private DateTime _lastMediaDownloadUtc = DateTime.MinValue;
private readonly SemaphoreSlim _mediaThrottle = new(1, 1);

public WikipediaClient()
{
    _httpClient = new HttpClient();
    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
    _httpClient.Timeout = TimeSpan.FromSeconds(60);
}

/// <summary>
/// Downloads a media file (rate-limited to be polite to Wikimedia servers).
/// Returns null when the download fails — callers treat missing images as non-fatal.
/// </summary>
public async Task<byte[]> TryDownloadMediaAsync(string url, CancellationToken cancellationToken = default)
{
    // ...
    await _mediaThrottle.WaitAsync(cancellationToken);
    try
    {
        var sinceLast = DateTime.UtcNow - _lastMediaDownloadUtc;
        var wait = TimeSpan.FromMilliseconds(MediaDownloadDelayMs) - sinceLast;
        if (wait > TimeSpan.Zero)
        {
            await Task.Delay(wait, cancellationToken);
        }

        _lastMediaDownloadUtc = DateTime.UtcNow;
        return await _httpClient.GetByteArrayAsync(url, cancellationToken);
    }
    catch (HttpRequestException)
    {
        return null;
    }
    catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        //Request timeout (not user cancellation)
        return null;
    }
    finally
    {
        _mediaThrottle.Release();
    }
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/WikipediaClient.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/RegisterServices.cs` (an identifying
user agent set once through the client library's options, because the service asks
consumers to identify themselves)

**Sharp edges.**
- The exception filter is what separates a request timeout from a real user
  cancellation; without it, cancelling would look like a failed download.
- The throttle is enforced with a semaphore plus a timestamp, so concurrent
  callers cannot bypass it.
- The owning service disposes the client, and the client disposes both the HTTP
  client and the semaphore.

### Fall back to one search per repository when a search API caps its results

**When you want this.** A search endpoint tells you how many matches there are but will
only ever serve the first N of them, and your user's query is bigger than N. Paging
harder does not help: past the cap the endpoint refuses the page outright.

**The MVVM shape.** The service reads the total from page one and picks a plan. Under
the cap it walks the pages. Over the cap it throws that page away, enumerates the
smaller units the query can be split into - here, the owner's repositories - and runs the
same query against each of them in turn. Both plans yield the same page type on the same
stream, so nothing above the service knows which plan ran.

**Code.**

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/GitHubIssueSearchService.cs
private async IAsyncEnumerable<IssueSearchPage> SearchInternalAsync(IssueSearchRequest request,
    IProgress<SearchProgress> progress, [EnumeratorCancellation] CancellationToken cancellationToken)
{
    var state = new SearchState();
    Report(progress, SearchPhase.Starting, state);

    var firstUrl = IssueSearchQueryBuilder.BuildSearchUrl(request, 1);
    var first = await GetSearchAsync(firstUrl, request.Owner, state, progress, cancellationToken)
        .ConfigureAwait(false);
    state.Total = first.TotalCount;

    if (first.TotalCount > SearchResultCap)
    {
        //Past the cap the whole-owner search can never reach the end, so this first page
        //is thrown away and the work starts again one repository at a time. The total
        //stays as it was: it is still the number of matches the user is waiting for.
        await foreach (var page in SearchByRepositoryAsync(request, state, progress, cancellationToken)
            .ConfigureAwait(false))
        {
            yield return page;
        }
    }
    else
    {
        await foreach (var page in WalkAsync(request, null, first, state, progress, cancellationToken)
            .ConfigureAwait(false))
        {
            yield return page;
        }
    }

    Report(progress, SearchPhase.Completed, state);
}
```

The second plan lists the units, drops the ones that cannot hold a match, and runs the
same walk against each. Listing is a different API with a different quota pool, so it
goes through a different throttle:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/GitHubIssueSearchService.cs
//The plan for an owner with more matches than one search can return: list the owner's
//repositories, drop the ones that cannot hold a match, and search what is left one at a
//time. The core pool pays for the listing, the search pool for the searches.
private async IAsyncEnumerable<IssueSearchPage> SearchByRepositoryAsync(IssueSearchRequest request,
    SearchState state, IProgress<SearchProgress> progress,
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    var repositories = await ListRepositoriesAsync(request, state, progress, cancellationToken)
        .ConfigureAwait(false);

    foreach (var repository in repositories)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await foreach (var page in WalkAsync(request, repository, null, state, progress, cancellationToken)
            .ConfigureAwait(false))
        {
            yield return page;
        }
    }
}
```

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/GitHubIssueSearchService.cs
foreach (var repository in repositories)
{
    if (repository == null || string.IsNullOrWhiteSpace(repository.FullName)) { continue; }
    if (repository.Archived || !repository.HasIssues) { continue; }

    //A repository with nothing open cannot answer an open-items search; it can
    //still answer one that includes closed items.
    if (repository.OpenIssuesCount <= 0 && !request.IncludeClosed) { continue; }

    kept.Add(repository);
}
```

The cap still applies to each unit, so the page walk stops at the last servable page
rather than asking for one the endpoint will refuse:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/GitHubIssueSearchService.cs
if (pageNumber == 1)
{
    //However many matches there are, GitHub serves only the first thousand of
    //them, so the walk stops at the tenth page even when the total is larger.
    //A whole-owner search never gets here with a larger total, but one
    //repository inside the per-repository plan can.
    total = response.TotalCount > SearchResultCap ? SearchResultCap : response.TotalCount;
}
```

One query builder writes both queries, so the two plans cannot drift apart. The only
difference between them is the scope term at the front:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/IssueSearchQueryBuilder.cs
public static string BuildQuery(IssueSearchRequest request, string repositoryFullName = null)
{
    var owner = RequireOwner(request);

    var repository = Clean(repositoryFullName);
    var scope = repository == null ? "user:" + owner : "repo:" + repository;

    var assignee = Clean(request.Assignee);
    var assignment = assignee == null ? "no:assignee" : "assignee:" + assignee;

    //Leaving is:open out is what widens the search to closed items as well.
    return request.IncludeClosed
        ? scope + " " + assignment
        : scope + " is:open " + assignment;
}
```

**Where to look.**
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/GitHubIssueSearchService.cs`
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/IssueSearchQueryBuilder.cs`
`GitHubIssueFinder/tests/libs/GitHubIssueFinder.GitHub.Tests/GitHubIssueSearchServiceTests.cs`

**Sharp edges.**
- The fallback costs a page you cannot use. Reading page one is how you learn the total,
  and past the cap that page is thrown away because the per-unit plan will return the same
  items in a different order.
- Keep reporting the original total. The plan changed; what the user is waiting for did
  not, and a progress bar that restarts at a different denominator looks broken.
- The cap applies per query, so it still applies to each unit. A single unit bigger than
  the cap is truncated, and that is worth saying in the application's documentation rather
  than hiding.
- The two APIs usually have different quota pools. Give each its own throttle, or the
  listing spends the budget the searches need.
- Filter the units before searching them. Every unit you can rule out from the listing you
  already paid for is a search call you do not spend.

### Normalize a user entered ID or URL before calling an API

**When you want this.** Users paste whatever they have: a bare identifier, a
formatted one, or a full URL with query parameters that contain other identifiers.

**The MVVM shape.** A pure static function in the library, unit-tested without a
network, called at the boundary of every service method.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionConvert.cs
/// <summary>
/// Normalises user input into a canonical hyphenated Notion ID. Accepts a bare
/// 32-hex ID, a hyphenated ID, or a full Notion URL (the query string is dropped
/// first, because a URL's ?v=…/?p=… parameters carry other object IDs). Returns
/// the trimmed input unchanged when no ID can be found, letting the API reject it.
/// </summary>
public static string NormalizeId(string input)
{
    if (string.IsNullOrWhiteSpace(input)) { return ""; }

    var value = input.Trim();
    var queryStart = value.IndexOf('?');
    if (queryStart >= 0) { value = value[..queryStart]; }

    var compact = value.Replace("-", "");
    var matches = Regex.Matches(compact, "[0-9a-fA-F]{32}");
    if (matches.Count == 0) { return input.Trim(); }

    var hex = matches[^1].Value.ToLowerInvariant();
    return $"{hex[..8]}-{hex[8..12]}-{hex[12..16]}-{hex[16..20]}-{hex[20..]}";
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionConvert.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/NotionConvertTests.cs`

**Sharp edges.**
- The query string is stripped first, or a view parameter's identifier would win.
- The last match in the path is taken, because a slug precedes the identifier.
- Unrecognizable input is passed through so the API produces the error message,
  rather than the application inventing one.

### Resolve an ID that may be one of several object kinds

**When you want this.** One input box has to accept several kinds of object, and
the API answers "not found" for a retrieve of the wrong kind.

**The MVVM shape.** The reader tries each shape in turn, filtering the exception
by its API error code, and remembers the shape per identifier so that later
"load children" calls use the right retrieval. The final failure is rethrown as a
message the user can act on.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionTreeReader.cs
public async Task<IList<NotionPageNode>> LoadRootsAsync(
    string pageOrDatabaseId, CancellationToken cancellationToken = default)
{
    var id = NotionConvert.NormalizeId(pageOrDatabaseId);

    try
    {
        var page = await _gate.RunAsync(() => _client.Pages.RetrieveAsync(id, cancellationToken), cancellationToken);
        return [BuildPageNode(page, depth: 0, parentId: null)];
    }
    catch (NotionApiException ex) when (IsWrongKind(ex)) { }

    try
    {
        var database = await _gate.RunAsync(() => _client.Databases.RetrieveAsync(id, cancellationToken), cancellationToken);
        return [BuildDatabaseNode(database, depth: 0, parentId: null)];
    }
    catch (NotionApiException ex) when (IsWrongKind(ex)) { }

    try
    {
        var dataSource = await _gate.RunAsync(() => _client.DataSources.RetrieveAsync(
            new RetrieveDataSourceRequest { DataSourceId = id }, cancellationToken), cancellationToken);
        return [BuildDataSourceNode(dataSource, depth: 0, parentId: null)];
    }
    catch (NotionApiException ex) when (IsWrongKind(ex))
    {
        throw new InvalidOperationException(
            "No page or database with that ID is visible to this integration. " +
            "Check the ID, and make sure the page is shared with the integration in Notion.", ex);
    }
}

//Notion answers a retrieve of the wrong kind (or of an unshared/absent object)
//  with object_not_found, and a malformed ID with validation_error
private static bool IsWrongKind(NotionApiException ex) =>
    ex.NotionAPIErrorCode == NotionAPIErrorCode.ObjectNotFound
    || ex.NotionAPIErrorCode == NotionAPIErrorCode.ValidationError;
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionTreeReader.cs
//A database's children come from querying its data sources rather than from
//  block children, so the reader tracks which retrieval shape each known ID needs.
private enum SourceShape { Page, Database, DataSource }

private sealed record NodeMeta(SourceShape Shape, int Depth, string Title, string ParentId);

public async Task<IList<NotionPageNode>> LoadChildrenAsync(
    string id, CancellationToken cancellationToken = default)
{
    var normalized = NotionConvert.NormalizeId(id);
    var meta = _metaById.TryGetValue(normalized, out var known)
        ? known
        : new NodeMeta(SourceShape.Page, 0, "", null);
    var childDepth = meta.Depth + 1;

    return meta.Shape switch
    {
        SourceShape.Database => await LoadDatabaseChildrenAsync(normalized, childDepth, cancellationToken),
        SourceShape.DataSource => await LoadDataSourceChildrenAsync(normalized, childDepth, cancellationToken),
        _ => await LoadPageChildrenAsync(normalized, childDepth, cancellationToken)
    };
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionTreeReader.cs`

**Sharp edges.**
- The exception filter is on the API's error code, not on the message.
- Failing to retrieve a child's full record is not fatal: the reader falls back to
  the title carried on the reference itself and loses only decoration.
- Paging uses the API's own has-more and cursor fields in a do-while, and the
  metadata map is a concurrent dictionary with a case-insensitive comparer.

### Read a nested tree from an API with a cycle guard

**When you want this.** An API returns children one level per request, some node
types must not be recursed into, and some can point back at each other.

**The MVVM shape.** A recursive reader in the library that returns a plain node
tree; a set of visited identifiers guards against loops.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionPageReader.cs
private async Task<IReadOnlyList<NotionBlockNode>> ReadChildrenAsync(
    string blockId, HashSet<string> visited, CancellationToken cancellationToken)
{
    if (!visited.Add(blockId)) { return []; } //Cycle guard (synced blocks can loop)

    var blocks = await _gate.RunAsync(
        () => _client.RetrieveAllChildrenAsync(blockId, cancellationToken), cancellationToken);

    var nodes = new List<NotionBlockNode>(blocks.Count);
    foreach (var block in blocks)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<NotionBlockNode> children = [];
        if (ShouldRecurse(block))
        {
            children = await ReadChildrenAsync(block.Id, visited, cancellationToken);
        }
        else if (block is SyncedBlockBlock synced
            && !string.IsNullOrEmpty(synced.SyncedBlock?.SyncedFrom?.BlockId))
        {
            //A duplicate synced block mirrors its source — fetch the source's
            //  children (the visited set guards against reference cycles)
            children = await ReadChildrenAsync(
                synced.SyncedBlock.SyncedFrom.BlockId, visited, cancellationToken);
        }
        nodes.Add(new NotionBlockNode { Block = block, Children = children });
    }
    return nodes;
}

private static bool ShouldRecurse(IBlock block) =>
    block.HasChildren && block is not ChildPageBlock && block is not ChildDatabaseBlock;
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionPageReader.cs`

**Sharp edges.**
- Node kinds that are separate documents in their own right are never recursed
  into; they are references, not inline content.
- A preview that must stay cheap reads one batch of children only, and its comment
  records the consequence: for a longer page the child count reads as "at least".

### Batch a metadata API and treat the result as best effort

**When you want this.** You need per-item metadata for many items and one failure
must not fail the job.

**The MVVM shape.** An internal resolver asks only for items that will actually be
used, de-duplicates, batches, and writes results back onto the model. Failures
leave the field empty.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/WikipediaClient.cs
/// <summary>
/// Looks up Wikimedia "extmetadata" (author, credit, license, …) for the given "File:" page
/// titles via the MediaWiki imageinfo API, batching up to 50 titles per request. The local
/// wiki's API transparently resolves files hosted on Wikimedia Commons. Titles that cannot be
/// resolved are simply absent from the result; a failed request yields an empty dictionary
/// (attribution is best-effort and never fails the render).
/// </summary>
public async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>>
    GetImageMetadataAsync(
        IReadOnlyCollection<string> fileTitles,
        string wikiHost = DefaultWikiHost,
        CancellationToken cancellationToken = default)
{
    // ...
    for (var offset = 0; offset < uniqueTitles.Count; offset += MaxTitlesPerMetadataQuery)
    {
        // ... build apiUrl with the batch's titles ...
        try
        {
            var json = await _httpClient.GetStringAsync(apiUrl, cancellationToken);
            ParseImageMetadata(json, result);
        }
        catch (HttpRequestException) { /* best-effort; skip this batch */ }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested) { }
    }

    return result;
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/AttributionResolver.cs
//Only fetch for images that will actually appear (downloaded) and that we can identify
var placed = images
    .Where(i => i.ProcessedBytes is { Length: > 0 }
                && !string.IsNullOrWhiteSpace(i.MediaPageTitle))
    .ToList();
if (placed.Count == 0) { return; }
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/WikipediaClient.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/AttributionResolver.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/AttributionFormatter.cs`

**Sharp edges.**
- The result dictionary is keyed case-insensitively, because the API normalizes
  titles.
- The formatter has to defend against real-world metadata: markup inside field
  values, entity encoding, placeholder names, a phrase duplicated by
  machine-plus-human templates, and codes that mean nothing to a reader. Each of
  those is a named private method with a comment.

### Fetch a whole remote catalog once and cache images behind a concurrency gate

**When you want this.** One endpoint returns everything, images are per item, and
you want to be polite to the server without starving the scroll.

**The MVVM shape.** A service registered in the container and resolved by the view
model. Filtering and sorting are a static, pure method on the service, which makes
them trivially testable and keeps the view model free of query expressions.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/Services/ModelCatalogService.cs
public sealed class ModelCatalogService
{
    //Cell thumbnails are requested at exactly the size the catalog cell displays them.
    private const int ThumbnailWidth = 512;
    private const int ThumbnailHeight = 288;

    private readonly SemaphoreSlim _catalogGate = new(1, 1);

    //At most a handful of thumbnail requests in flight at once - polite to the CDN, and
    //plenty to keep up with scrolling.
    private readonly SemaphoreSlim _thumbnailGate = new(4, 4);
    private readonly ConcurrentDictionary<string, byte[]> _thumbnailCache = new();

    public async Task<IReadOnlyList<PolyHavenAsset>> GetModelsAsync(CancellationToken cancellationToken)
    {
        if (_models != null) { return _models; }

        await _catalogGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_models != null) { return _models; }

            using var client = _factory.GetClient();
            var assets = await client.GetAssetsAsync(PolyHavenAssetType.Model, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            _models = assets.Values.ToList();
            return _models;
        }
        finally
        {
            _catalogGate.Release();
        }
    }

    public static IReadOnlyList<PolyHavenAsset> SortAndFilter(
        IReadOnlyList<PolyHavenAsset> models, CatalogSortOrder sortOrder, string searchText)
    { /* ... Where(Matches) then an OrderBy per CatalogSortOrder ... */ }
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Services/ModelCatalogService.cs`

**Sharp edges.**
- Double-checked locking around the one-shot fetch: check before waiting on the
  semaphore and again after acquiring it, so a burst of startup callers makes one
  request.
- Every service await uses `ConfigureAwait(false)`; only the view-model layer
  needs the dispatcher context back.
- Thumbnails are requested at exactly the cell's display size, so the server does
  the downscale.

### Report true byte progress across a multi file download with side car files

**When you want this.** One logical download is several files - a main file and
the companions it references by relative path - and a per-file bar would jump back
to zero repeatedly.

**The MVVM shape.** The service takes an `IProgress<double>` and computes the
fraction across all files from sizes the API advertises up front. The view model
converts the fraction into a bound percentage inside a `Progress<double>`
callback, which marshals back to the UI context for you.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/Services/ModelDownloadService.cs
//Every file advertises its size, so the bar can show true byte progress across the
//main .gltf and all its sidecars (buffer .bin + texture images).
var totalBytes = Math.Max(1L, gltf.Size + (gltf.Include?.Values.Sum(f => f.Size) ?? 0L));
var completedBytes = 0L;

Directory.CreateDirectory(modelFolder);

var gltfPath = Path.Combine(modelFolder, FileNameFromUrl(gltf.Url));
await DownloadOneAsync(client, gltf, gltfPath, totalBytes, completedBytes, progress, cancellationToken)
    .ConfigureAwait(false);
completedBytes += gltf.Size;

//A glTF references its sidecar files by relative path; the Include dictionary is
//keyed by exactly those relative paths, so the files land where the glTF expects them.
if (gltf.Include != null)
{
    foreach (var (relativePath, sidecar) in gltf.Include)
    {
        var sidecarPath = Path.Combine(modelFolder, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(sidecarPath)!);
        await DownloadOneAsync(client, sidecar, sidecarPath, totalBytes, completedBytes, progress, cancellationToken)
            .ConfigureAwait(false);
        completedBytes += sidecar.Size;
    }
}

private static async Task DownloadOneAsync(
    IPolyHavenApiClient client, PolyHavenFileRef file, string destinationPath,
    long totalBytes, long completedBytes, IProgress<double> progress, CancellationToken cancellationToken)
{
    var fileProgress = progress == null
        ? null
        : new Progress<PolyHavenDownloadProgress>(p =>
            progress.Report(Math.Min(1d, (completedBytes + p.BytesReceived) / (double)totalBytes)));

    await client.DownloadFileAsync(file, destinationPath, fileProgress, cancellationToken: cancellationToken)
        .ConfigureAwait(false);
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
var progress = new Progress<double>(fraction => DownloadProgress = fraction * 100d);
var downloaded = await _downloads.EnsureDownloadedAsync(
    cell.Asset, _downloadFolder, progress, CancellationToken.None);
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Services/ModelDownloadService.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Services/SampleAssetService.cs`

**Sharp edges.**
- Side-car keys are relative paths using forward slashes; translate them to the
  platform separator and create the intermediate directory, or the file lands
  somewhere the main file cannot find it.
- The service checks for an existing download first and reports completion
  immediately, so re-opening an asset costs no network traffic and the bar does
  not flicker.
- The file selection uses layered fallbacks, so an asset with an unexpected tree
  still resolves or fails with a clear message.
- A `double` bound property may need hand-written compare-and-notify; see the
  view-model area.

### Cache downloaded assets with a key you can invalidate

**When you want this.** Your application fetches large files it should download
once, and you want a cache that invalidates correctly when you change which file
you fetch.

**The MVVM shape.** A singleton service registered in the container, taking the
API client factory in its constructor and exposing one async method that reports
progress and honors a cancellation token. The view model resolves it and awaits
it.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Services/SampleAssetService.cs
public SampleAssetService(IPolyHavenApiClientFactory factory)
{
    _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    _cacheRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PolyHavenBrowser", "cache");
}

public async Task<SampleAsset> EnsureSampleAsync(
    SampleAssetKind kind, IProgress<string> status, CancellationToken cancellationToken)
{
    await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
        var kindDir = Path.Combine(_cacheRoot, kind.ToString().ToLowerInvariant());
        var markerPath = Path.Combine(kindDir, "sample.json");
        var slug = SlugFor(kind);

        //Reuse the cached asset only when it is the SAME curated slug. The marker is keyed by
        //kind, so changing the curated slug (SlugFor) must invalidate a stale marker and
        //re-download rather than keep serving the previously-cached asset.
        var cached = TryReadMarker(markerPath);
        if (cached != null && string.Equals(cached.Slug, slug, StringComparison.Ordinal)
            && File.Exists(cached.PrimaryFilePath))
        {
            status?.Report($"{Describe(kind)}: {cached.Name} (cached)");
            return cached;
        }

        using var client = _factory.GetClient();
        // ... download, then:
        var result = new SampleAsset { Slug = slug, Name = name, PrimaryFilePath = primaryPath };
        WriteMarker(markerPath, result);
        status?.Report($"{Describe(kind)}: {name}");
        return result;
    }
    finally
    {
        _gate.Release();
    }
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Services/SampleAssetService.cs`

**Sharp edges.**
- The cache marker records which asset it holds, and that is compared before
  reuse. The marker file is keyed by kind, so without the comparison, changing
  which asset the application curates would keep serving the old one forever.
- The marker also verifies the file still exists before trusting it, and reading a
  marker swallows any exception and returns null, so a corrupt marker just
  re-downloads.
- A semaphore serializes the whole method, so two rapid button presses cannot race
  on the same folder.
- Every await uses `ConfigureAwait(false)` - this is a service, not view-model
  code.

### Parse messy HTML into structured blocks with the CodeBrix MarkupParse library

**When you want this.** You have real-world HTML and need typed content, not a
string.

**The MVVM shape.** Parsing lives in an internal class in the pipeline library,
reached only through the service interface. The result is a plain model with an
ordered block list and no UI or document types in it.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs
public ParsedArticle Parse(string html)
{
    if (string.IsNullOrWhiteSpace(html))
    {
        throw new ArgumentException("Value cannot be null or blank.", nameof(html));
    }

    var parser = new HtmlParser();
    var document = parser.ParseDocument(html);

    _strippedGlyphCount = 0;
    var article = new ParsedArticle { SourceUrl = SourceUrl };

    article.Title = GetTitle(document);
    article.ShortDescription = document.QuerySelector("div.shortdescription")?.TextContent?.Trim() ?? "";
    article.LeadImage = GetLeadImage(document);

    //Some articles contain more than one .mw-parser-output element - e.g. a small wrapper
    //  emitted by a transcluded template (coordinates, short description, hatnotes) plus the
    //  real article body. QuerySelector would return whichever comes first in document order,
    //  which can be the near-empty wrapper. Pick the container that actually holds the prose:
    //  the one with the most paragraph descendants.
    var contentRoots = document.QuerySelectorAll(".mw-parser-output").ToList();
    var contentRoot = contentRoots
        .OrderByDescending(e => e.QuerySelectorAll("p").Count())
        .ThenByDescending(e => e.Children.Count())
        .FirstOrDefault();
    // ...
    var state = new WalkState();
    WalkChildren(contentRoot, article, state);
    // ...
    ReportSkips(article, state);
    return article;
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Models/ArticleContent.cs
public enum ArticleBlockType
{
    Heading,
    Paragraph,
    BulletList,
    NumberedList,
    BlockQuote,
    Image,
    Table,
    DefinitionList
}

/// <summary>
/// A single run of text with uniform character formatting.
/// </summary>
public sealed record TextRun(
    string Text,
    bool Bold = false,
    bool Italic = false,
    bool Superscript = false,
    bool Subscript = false);
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Models/ArticleContent.cs`

**Sharp edges.**
- Picking "the first matching container" is a trap on real pages; this parser
  ranks candidates by paragraph count and records a warning when there was more
  than one. There is a regression test for exactly this.
- Warnings are collected on the model rather than thrown, and surface in the
  render result.
- The service treats an empty block list as a hard error with a message the user
  can act on.

### Strip web only chrome while walking the DOM

**When you want this.** Printed output must not carry citation markers, edit
links, navigation boxes or reference sections.

**The MVVM shape.** A recursive walk with a small state object, a class deny-list
for block elements, an inline skip predicate, and an in-place removal helper.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs
private static readonly HashSet<string> StopSections =
    new(StringComparer.OrdinalIgnoreCase)
    {
        "References", "See also", "External links", "Notes", "Further reading",
        "Bibliography", "Sources", "Citations", "Footnotes", "Works cited",
        "Explanatory notes", "General references"
    };

//Divs with any of these classes are web chrome and never book content
private static readonly string[] SkippedDivClasses =
[
    "hatnote", "navbox", "vertical-navbox", "toc", "reflist", "refbegin",
    "sistersitebox", "side-box", "ambox", "mbox-small", "asbox", "metadata",
    "printfooter", "mw-empty-elt", "noprint", "mw-authority-control", "portalbox",
    "portal-bar", "navigation-not-searchable", "spoken-wikipedia", "sister-bar"
];
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs
/// <summary>Removes reference markers, edit links and other non-content from an element (in place).</summary>
private static void RemoveNonContent(IElement element)
{
    foreach (var node in element
                 .QuerySelectorAll("sup.reference, sup.plainlinks, .mw-editsection, style, script, .noprint, sup[typeof*='mw:Extension/ref']")
                 .ToList())
    {
        node.Remove();
    }
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs`

**Sharp edges.**
- Materialize the query result before removing: you cannot mutate the document
  while enumerating a live query.
- Markup evolves. The walker handles both a bare heading element and a newer
  wrapped form, and recurses through section wrappers.
- Skipped things are counted by kind and reported as warnings rather than silently
  dropped, which makes the parser debuggable against a live site.

### Upgrade thumbnail URLs to print resolution

**When you want this.** The images on a web page are screen-sized and you need
print resolution without upscaling anything.

**The MVVM shape.** A pure internal static method with a clear contract,
unit-tested with a theory and no I/O.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs
/// <summary>
/// Derives a print-resolution rendition URL from a Wikimedia thumbnail URL.
/// Thumbnail URLs look like …/thumb/6/66/Name.jpg/250px-Name.jpg — the pixel
/// prefix of the final segment selects the rendition size.
/// </summary>
internal static string DerivePrintUrl(string src, int fileWidth, string urlPath)
{
    if (!src.Contains("/thumb/")) { return src; }
    // ...
    var currentWidth = int.Parse(pxMatch.Groups[1].Value);

    //SVG renditions (….svg/NNNpx-….svg.png) can be rasterized at any size;
    //  raster files cannot be upscaled beyond their true file width.
    var isSvgRendition = urlPath.EndsWith(".svg.png", StringComparison.OrdinalIgnoreCase);
    var target = isSvgRendition
        ? TargetImagePixelWidth
        : (fileWidth > 0 ? Math.Min(TargetImagePixelWidth, fileWidth) : TargetImagePixelWidth);

    if (target <= currentWidth) { return src; }

    var newSegment = $"{target}px-{lastSegment[(pxMatch.Length)..]}";
    return src[..(lastSlash + 1)] + newSegment;
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ImagePipeline.cs
private async Task<bool> TryPrepareOneAsync(ArticleImage image, CancellationToken cancellationToken)
{
    var bytes = await _client.TryDownloadMediaAsync(image.PrintUrl, cancellationToken);

    //High-resolution rendition may 404 (e.g. odd file types) — fall back to the page thumbnail
    if (bytes is null && (!image.PrintUrl.Equals(image.ThumbUrl, StringComparison.Ordinal)))
    {
        bytes = await _client.TryDownloadMediaAsync(image.ThumbUrl, cancellationToken);
    }
    // ...
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ImagePipeline.cs`

**Sharp edges.**
- Vector sources rasterize at any size; raster sources must be clamped to their
  true file width, read from the markup's own attribute.
- Non-thumbnail URLs are returned unchanged, so the caller must not assume a
  rewrite happened, and the higher-resolution rendition may not exist - fall back
  to the original.
- Icons are filtered out earlier by a minimum pixel width, and unsupported file
  extensions are rejected before a download is attempted.
- Normalizing the downloaded bytes for print is a separate step; see the
  graphics area.

### Run a multi stage pipeline behind one service method

**When you want this.** You want the view model to call one method and get a
result object, while the stages stay separate and testable.

**The MVVM shape.** The service validates the request, runs the stages in order,
reports progress between them, honors a cancellation token, wraps the CPU-bound
stages in `Task.Run` because the caller is the UI thread, writes the file, and
returns a result record with everything the UI wants to display.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Services/ArticleRenderService.cs
// 1. Fetch
progress?.Report(new RenderProgress(RenderStage.FetchingArticle, "Fetching the article…", 5));
var html = await _client.GetArticleHtmlAsync(request.ArticleUrl, cancellationToken);

// 2. Parse
progress?.Report(new RenderProgress(RenderStage.ParsingArticle, "Reading the article…", 12));
var parser = new ArticleParser(request.ArticleUrl);
var article = parser.Parse(html);

if (article.Blocks.Count == 0)
{
    throw new InvalidOperationException(
        "No readable article content was found at the given URL. " +
        "Make sure the URL points at a Wikipedia article page.");
}

// 3. Images
// ... ImagePipeline.PrepareImagesAsync, then AttributionResolver.ResolveAsync ...

// 4. Compose
cancellationToken.ThrowIfCancellationRequested();
progress?.Report(new RenderProgress(RenderStage.ComposingBook, "Laying out the book…", 74));
var theme = BookTheme.For(request.PageSize);
var composer = new BookComposer(article, theme, DateTime.Now);
var document = composer.Compose();

// 5. Render + save
progress?.Report(new RenderProgress(RenderStage.SavingPdf, "Rendering the PDF…", 82));
var renderer = new PdfDocumentRenderer(unicode: true) { Document = document };
renderer.RenderDocument();

// ... resolve outputPath, create the folder ...
renderer.PdfDocument.Save(outputPath);
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs
// 3 + 4. Compose and render off the caller's thread (both are CPU-bound,
//    and the caller is typically the UI thread)
progress?.Report(new CreateProgress(CreateStage.ComposingBook, "Laying out the book…", 72));
var composer = new BookComposer(chapters, context);
var outputPath = request.OutputFilePath.Trim();

var pageCount = await Task.Run(() =>
{
    var document = composer.Compose();

    progress?.Report(new CreateProgress(CreateStage.SavingPdf, "Rendering the PDF…", 82));
    var renderer = new PdfDocumentRenderer(unicode: true) { Document = document };
    renderer.RenderDocument();

    var folder = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(folder)) { Directory.CreateDirectory(folder); }
    renderer.PdfDocument.Save(outputPath);
    return renderer.PdfDocument.PageCount;
}, cancellationToken);
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Models/DocumentModels.cs
public sealed class CreateRequest
{
    public IReadOnlyList<string> PageIds { get; init; } = [];
    public string OutputFilePath { get; init; } = "";
    public PageSizeOption PageSize { get; init; } = PageSizeOption.EightByTen;

    /// <summary>When false, images are skipped entirely (text-only rendering).</summary>
    public bool IncludeImages { get; init; } = true;

    /// <summary>
    /// When false, non-image media (video poster frames, audio/file cards' downloads)
    /// are not fetched; those blocks render as cards from metadata alone.
    /// </summary>
    public bool IncludeMedia { get; init; } = true;
}

public sealed class CreatedDocument
{
    public string OutputFilePath { get; init; } = "";
    public string Title { get; init; } = "";
    public int PageCount { get; init; }
    public int ChapterCount { get; init; }
    public int ImageCount { get; init; }
    public TimeSpan Elapsed { get; init; }

    /// <summary>Non-fatal notes collected during the creation.</summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Services/ArticleRenderService.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Models/DocumentModels.cs`

**Sharp edges.**
- Validate the request up front, so a failure surfaces before any network traffic.
- The document renderer is created with its Unicode option on, which is what
  embedded fonts need for non-ASCII text.
- Where two ways of naming the output exist - a full path, or a directory plus an
  optional name - the request record documents the precedence.
- The result carries a warning list; collect non-fatal problems as you go and show
  them all at the end.
- Progress percentages are fixed per stage, with a long stage interpolating across
  its own range, so the bar never goes backwards.
- `ObjectDisposedException.ThrowIf` guards every public method; the service owns
  its HTTP client and is disposable.

### Register embedded OFL fonts with the PDF font system

**When you want this.** Your generated documents must look identical everywhere,
regardless of what fonts the host has installed.

**The MVVM shape.** A static, lock-guarded, idempotent registration in the
library, called at the top of composition. The fonts are embedded resources of the
same assembly, with their license texts embedded beside them.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookFonts.cs
using CodeBrix.Imaging.PixelFormats;
using CodeBrix.PdfDocuments.Drawing;
using CodeBrix.PdfDocuments.Fonts;
using CodeBrix.PdfDocuments.Utils;

internal static class BookFonts
{
    public const string SerifFamily = "EB Garamond";
    public const string SansFamily = "Source Sans 3";
    public const string MonoFamily = "Source Code Pro";
    public const string EmojiFamily = "Noto Emoji";

    private const string ResourcePrefix = "NotionDocumentCreator.CreateDocument.Fonts.";

    private static readonly object _locker = new();
    private static bool _registered;

    public static void EnsureRegistered()
    {
        lock (_locker)
        {
            if (_registered) { return; }

            //The PDF image pipeline needs an imaging implementation before any image can be placed
            ImageSource.ImageSourceImpl ??= new ImagingImageSource<Rgba32>();

            var assembly = typeof(BookFonts).Assembly;

            var serifFaces = new[]
            {
                "EBGaramond-Regular", "EBGaramond-Italic", "EBGaramond-Bold", "EBGaramond-BoldItalic"
            };
            var serifResolver = new EmbeddedFontResolver(
                fontFamilyName: SerifFamily,
                fontFaceResources:
                [
                    new EmbeddedResourceFontFace(FaceName: "EBGaramond-Regular", EmbeddedResourceName: $"{ResourcePrefix}EBGaramond-Regular.ttf"),
                    // ... one EmbeddedResourceFontFace per face ...
                ],
                fontEmbeddedResourceAssembly: assembly);

            // ... sans, mono and emoji resolvers built the same way ...

            //MetaFontResolver routes family-name lookups (ResolveTypeface) via any registered
            //  resolver whose DefaultFontName matches, but face-name lookups (GetFont) require
            //  a registration per face name.
            foreach (var face in serifFaces)
            {
                MetaFontResolver.Instance.RegisterFontResolver(face, serifResolver);
            }
            // ... same loop for the other three families ...

            _registered = true;
        }
    }
}
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/PolyHavenBrowser.CreateDocument.csproj -->
<ItemGroup>
  <None Remove="Fonts\*.ttf" />
  <None Remove="Fonts\OFL-*.txt" />
</ItemGroup>

<ItemGroup>
  <EmbeddedResource Include="Fonts\*.ttf" />
  <EmbeddedResource Include="Fonts\OFL-*.txt" />
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Internal/SheetFonts.cs
        //The face is deliberately named "Roboto-Heavy" (not "-ExtraBold"): the resolver
        //  matches faces by looking for "bold"/"italic" in their names, so a non-bold
        //  request for this single-face family must not see "bold" in the face name.
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookFonts.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Internal/SheetFonts.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookFonts.cs`

**Also shown by.**
`InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfFonts.cs`
(two family aliases built from eight embedded faces and registered once per face
name under a double-checked lock, so the call is safe from both the report's
constructor and its `Build()`; the quieter family deliberately points its "Bold"
face at Merriweather Medium, because the resolver decides what is bold by looking
for "bold" in the face name)

**Sharp edges.**
- The same method sets the PDF layer's imaging back-end. Forget it and image
  placement fails, not the font lookup.
- One registration call per face name is required, not one per family; the comment
  in the code explains the difference between family-name and face-name lookups.
- The resolver matches faces by looking for weight and style words in their names,
  so a heavy single-face family must be named without "bold" in it, while a
  semibold face intended to serve bold requests should contain it. Weights beyond
  regular and bold need their own family name.
- The resource prefix is the library's default root namespace, which is why these
  libraries deliberately do not override it.
- Registration is idempotent under a lock and is called from more than one entry
  point, so a test that exercises only the renderer still gets fonts, and parallel
  renders in one process are safe.
- Embed the license text alongside the font files so it travels with the binary.

### Drop characters your embedded fonts cannot render

**When you want this.** Text from an arbitrary source may contain characters your
embedded fonts have no glyph for, and empty boxes would ruin a printed page.

**The MVVM shape.** A filter applied where text enters the document, counting
removals into a warning the result carries. Two workable forms appear in this
repository: a static range filter, and a coverage cache that parses each embedded
font's character map once and asks it per codepoint.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/GlyphFilter.cs
/// <summary>
/// Filters article text down to the character ranges covered by the embedded book
/// fonts (EB Garamond and Source Sans 3: Latin, Latin Extended, Greek, Cyrillic and
/// common punctuation). Characters outside those ranges — for example Cuneiform,
/// CJK or Arabic glyphs quoted inline in an article — would otherwise render as
/// "tofu" boxes in the PDF, which ruins a printed page.
/// </summary>
internal static class GlyphFilter
{
    public static string Sanitize(string text, out int removedCount)
    {
        // ... walks the string, keeping supported characters ...
        //Tidy the holes left behind: empty bracket pairs, doubled spaces,
        //  stray space before closing punctuation
        cleaned = Regex.Replace(cleaned, @"[\(\[«“‘]\s*[\)\]»”’]", "");
        cleaned = Regex.Replace(cleaned, @"\s+([,;.:!?)\]])", "$1");
        cleaned = Regex.Replace(cleaned, @"[ \t]{2,}", " ");

        return cleaned;
    }

    private static bool IsSupported(char c)
    {
        int code = c;

        return code switch
        {
            0x0009 or 0x000A or 0x000D => true,   //Tab, newline
            >= 0x0020 and <= 0x007E => true,      //ASCII
            >= 0x00A0 and <= 0x024F => true,      //Latin-1, Latin Extended A/B
            // ... Greek, Cyrillic, Latin Extended Additional, punctuation, currency, number forms ...
            _ => false
        };
    }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/FontCoverage.cs
/// <summary>Whether the given embedded font file has a glyph for the codepoint.</summary>
public static bool Covers(string fontFileName, int codepoint)
{
    var coverage = _coverageByFile.GetOrAdd(fontFileName, LoadCoverage);
    return coverage.Contains(codepoint);
}

/// <summary>
/// Whether an emoji codepoint can actually be PRINTED: it must be in the
/// emoji face's cmap AND inside the Basic Multilingual Plane — the PDF text
/// engine addresses glyphs per UTF-16 code unit, so astral-plane emoji
/// (U+1F300 and friends) would print as tofu even though the font has them.
/// </summary>
public static bool EmojiPrintable(int codepoint) =>
    codepoint <= 0xFFFF && Covers(EmojiRegular, codepoint);
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs
//Variation selectors and the zero-width joiner shape emoji sequences the
//  monochrome face cannot compose anyway — drop them without counting
if (codepoint is 0xFE0E or 0xFE0F or 0x200D)
{
    i += charCount - 1;
    continue;
}

bool isEmoji;
if (codepoint is '\n' or '\r' or '\t' || FontCoverage.Covers(FontCoverage.SerifRegular, codepoint))
{
    isEmoji = false;
    if (codepoint == '\r') { i += charCount - 1; continue; } //Normalise CRLF to the \n that follows
}
else if (FontCoverage.EmojiPrintable(codepoint))
{
    isEmoji = true;
}
else
{
    DroppedCharacterCount++;
    i += charCount - 1;
    continue;
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/FontCoverage.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/GlyphFilter.cs`

**Sharp edges.**
- Astral-plane characters are dropped even when the font has the glyph, because
  the text engine addresses glyphs per code unit.
- Skip surrogate pairs in both halves, or you leave an orphaned half behind.
- Removing characters leaves empty brackets and doubled spaces; the cleanup passes
  matter as much as the filter.
- An unparseable font should report empty coverage rather than throwing, so
  callers fall back safely.
- Where a glyph is needed and no family has it, print a text stand-in rather than
  nothing.
- The count of removed characters becomes a warning in the result, so the user is
  told rather than silently shortchanged.

### Derive a whole document theme from one page size choice

**When you want this.** Your document must be laid out proportionately at several
page sizes rather than tuned at one.

**The MVVM shape.** An immutable theme object built by a factory from the chosen
trim; every style in the document derives its metrics from the theme, so the
picker in the view model changes one enum value and the whole document
re-proportions.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookTheme.cs
public static BookTheme For(PageSizeOption option)
{
    var page = PageSizeInfo.For(option);

    //Every metric derives from the trim size, so each of the four trims (and any
    //  added later) gets a proportionate book layout instead of one tuned at a
    //  single size: margins as fractions of the page, the inner (binding) margin
    //  a little larger than the outer, and type scaled to the resulting measure
    var innerMargin = page.WidthPoints * 0.125;
    var outerMargin = page.WidthPoints * 0.106;
    var topMargin = page.HeightPoints * 0.09;
    var bottomMargin = page.HeightPoints * 0.113;
    var textWidth = page.WidthPoints - innerMargin - outerMargin;

    //Cap the measure at the classic book line length (~65-75 characters) — on
    //  wide trims (US Letter, A4) the excess goes into the side margins instead
    const double maxMeasure = 435;
    if (textWidth > maxMeasure)
    {
        var extra = (textWidth - maxMeasure) / 2;
        innerMargin += extra;
        outerMargin += extra;
        textWidth = maxMeasure;
    }

    var bodySize = System.Math.Clamp(8.0 + textWidth * 0.0058, 9.0, 11.5);
    // ... construct the theme ...
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookTheme.cs
// ── Palette (warm ink on paper with an oxblood accent) ──────────────
public static readonly Color Ink = new(31, 30, 28);
public static readonly Color Accent = new(122, 44, 38);
public static readonly Color Muted = new(112, 108, 102);
public static readonly Color Hairline = new(203, 197, 189);

/// <summary>The width of the text block (page width minus side margins).</summary>
public double TextWidth => PageWidth - InnerMargin - OuterMargin;

/// <summary>Body text size in points; every other size in the scale derives from this.</summary>
public double BodySize { get; private init; }

/// <summary>Body leading (line spacing) in points.</summary>
public double Leading => BodySize * 1.47;

public double H1Size => BodySize * 1.62;
public double H2Size => BodySize * 1.27;
public double CaptionSize => BodySize * 0.81;
public double RaisedCapSize => BodySize * 2.35;

public static BookTheme For(PageSizeOption option)
{
    var page = PageSizeInfo.For(option);

    return option switch
    {
        // ... one record per trim size: its own margins, body size and cover title size ...
    };
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookTheme.cs`
and `Internal/BookStyles.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookTheme.cs`

**Sharp edges.**
- Every dimension is in typographic points, matching the document model's own unit
  type.
- The theme is the right home for a letterspacing helper, and its comment records
  a layout rule worth knowing: the layout engine collapses runs of ordinary
  spaces, so the helper uses non-breaking spaces; a second variant keeps word gaps
  breakable so a long display line can still wrap.
- A picker bound to display-name strings needs the view model to map the name back
  to the enum with a fallback.

### Compose a book with sections styles running heads and folios

**When you want this.** You need real book structure - a cover section, a body
section with mirrored margins, running heads and page numbers - not a flat report.

**The MVVM shape.** A composer class takes the parsed model plus the theme and
returns a document; the service renders and saves it. No UI type appears anywhere.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
public Document Compose()
{
    BookFonts.EnsureRegistered();

    var document = new Document();
    document.Info.Title = _article.Title;
    document.Info.Subject = string.IsNullOrWhiteSpace(_article.ShortDescription)
        ? $"Wikipedia article: {_article.Title}"
        : _article.ShortDescription;
    document.Info.Author = "Wikipedia contributors";

    DefineStyles(document);

    ComposeFrontMatter(document);
    ComposeContent(document);
    ComposeColophon();

    return document;
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
_content = document.AddSection();
_content.PageSetup.PageWidth = Unit.FromPoint(t.PageWidth);
_content.PageSetup.PageHeight = Unit.FromPoint(t.PageHeight);
_content.PageSetup.TopMargin = Unit.FromPoint(t.TopMargin);
_content.PageSetup.BottomMargin = Unit.FromPoint(t.BottomMargin);
_content.PageSetup.LeftMargin = Unit.FromPoint(t.InnerMargin);
_content.PageSetup.RightMargin = Unit.FromPoint(t.OuterMargin);
_content.PageSetup.MirrorMargins = true;
_content.PageSetup.DifferentFirstPageHeaderFooter = true;
_content.PageSetup.StartingNumber = 1;
_content.PageSetup.HeaderDistance = Unit.FromPoint(t.TopMargin * 0.48);
_content.PageSetup.FooterDistance = Unit.FromPoint(t.BottomMargin * 0.42);

var header = _content.Headers.Primary.AddParagraph(BookTheme.Letterspace(_article.Title));
header.Style = "RunningHead";
header.Format.SpaceAfter = 0;

var folio = _content.Footers.Primary.AddParagraph();
folio.Style = "Folio";
folio.AddPageField();

var firstFolio = _content.Footers.FirstPage.AddParagraph();
firstFolio.Style = "Folio";
firstFolio.AddPageField();
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
var normal = document.Styles["Normal"];
normal.Font.Name = BookFonts.SerifFamily;
normal.Font.Size = t.BodySize;
normal.Font.Color = BookTheme.Ink;
normal.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
normal.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
normal.ParagraphFormat.LineSpacing = t.Leading;
normal.ParagraphFormat.SpaceAfter = 0;
normal.ParagraphFormat.WidowControl = true;

//Body paragraph that opens a section (no indent), and continuation paragraphs
//  (classic book first-line indent, no inter-paragraph space)
var bodyOpen = document.AddStyle("BodyOpen", "Normal");
bodyOpen.ParagraphFormat.FirstLineIndent = 0;

var bodyIndented = document.AddStyle("BodyIndented", "Normal");
bodyIndented.ParagraphFormat.FirstLineIndent = Unit.FromPoint(t.BodySize * 1.55);
```

**Numbering across several sections.** Where each chapter is its own section, set
the starting number on exactly one of them:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookComposer.cs
//The unnumbered cover is "page 0": printed numbering starts at 1 on the
//  first content chapter, and later sections must NOT set StartingNumber
//  again — that would restart every chapter at 1
if (isFirstContentChapter)
{
    setup.StartingNumber = 1;
}

//Running heads — chapter title on recto, book title on verso, none on
//  the chapter opener page
var recto = section.Headers.Primary.AddParagraph(BookTheme.Letterspace(chapter.Title));
recto.Style = "RunningHead";
var verso = section.Headers.EvenPage.AddParagraph(BookTheme.Letterspace(_chapters[0].Title));
verso.Style = "RunningHead";

AddFolio(section.Footers.Primary);
AddFolio(section.Footers.EvenPage);
AddFolio(section.Footers.FirstPage);
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
if (pageId is not null
    && _context.PagesInBook.TryGetValue(NotionConvert.NormalizeId(pageId), out var bookPage))
{
    //The target is in the book: give the reader the printed folio
    var paragraph = target.AddParagraph();
    paragraph.Style = "RefLine";
    paragraph.AddText($"See {bookPage.Title}, page ");
    paragraph.AddPageRefField(bookPage.BookmarkName);
    paragraph.AddText(".");
    return;
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookComposer.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/PageNumberingTests.cs`

**Sharp edges.**
- A different first-page header and footer means the first page needs its own
  folio paragraph, or page one silently loses its number.
- Setting the starting number on more than one section restarts numbering; there
  is a test asserting later sections keep the default.
- Styles inherit by name, so define the base styles first.
- Heading styles set an outline level, which is what produces the document's
  outline bookmarks; a style that should not appear there resets it.
- Bookmarks are added where a title is written, and page-reference fields resolve
  them at render time, so forward references work.

### Build a table of contents with real page numbers and dot leaders

**When you want this.** A generated document needs a contents page whose numbers
are correct after layout.

**The MVVM shape.** Headings get bookmarks as they are composed; entries are a
hyperlink to the bookmark, a tab, and a page-reference field; the leader dots come
from a right-aligned tab stop on the entry style.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
var tocEntry = document.AddStyle("TocEntry", "Normal");
tocEntry.ParagraphFormat.Alignment = ParagraphAlignment.Left;
tocEntry.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
tocEntry.ParagraphFormat.LineSpacing = Unit.FromPoint(t.Leading * 1.15);
tocEntry.ParagraphFormat.AddTabStop(Unit.FromPoint(t.TextWidth), TabAlignment.Right, TabLeader.Dots);
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
private void ComposeToc(Section frontMatter)
{
    var headings = _article.Blocks
        .Where(b => b.Type == ArticleBlockType.Heading && b.HeadingLevel <= 2)
        .ToList();
    var level1Count = headings.Count(h => h.HeadingLevel == 1);
    if (level1Count < 2) { return; }

    //Only include sub-headings when the contents stay comfortably on one page
    var includeLevel2 = headings.Count <= 24;

    frontMatter.AddPageBreak();
    // ... title and rule ...

    foreach (var heading in headings)
    {
        if (heading.HeadingLevel == 1) { sectionIndex++; }
        else if (!includeLevel2) { continue; }

        var bookmark = BookmarkNameFor(heading);
        var entry = frontMatter.AddParagraph();
        entry.Style = heading.HeadingLevel == 1 ? "TocEntry" : "TocEntry2";

        var link = entry.AddHyperlink(bookmark, HyperlinkType.Bookmark);
        link.AddText(heading.Text);
        entry.AddTab();
        entry.AddPageRefField(bookmark);
    }
}

private string BookmarkNameFor(ArticleBlock heading)
{
    //Stable, unique bookmark names derived from block identity
    var index = _article.Blocks.IndexOf(heading);
    return $"sec.{index}";
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`

**Sharp edges.**
- Bookmark names must be unique and stable; deriving them from the block's index
  in the parsed model means the contents and the body agree without a shared
  counter.
- The contents are composed into the front-matter section before the body exists;
  the page-reference fields resolve at render time.
- No contents page is emitted for a short document, and sub-headings are dropped
  when the list would overflow.

### Place numbered framed figures with credit lines

**When you want this.** Images in a document need consistent sizing, a frame where
it flatters them, a credit and a numbered caption.

**The MVVM shape.** The composer chooses a width from the aspect ratio, clamps the
height against the text block, and only ever places images that actually
downloaded.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
private void ComposeFigure(ArticleImage articleImage)
{
    if (articleImage?.ProcessedBytes is null
        || articleImage.ProcessedWidth <= 0 || articleImage.ProcessedHeight <= 0)
    {
        return;
    }

    var t = _theme;
    _figureNumber++;

    var aspect = (double)articleImage.ProcessedWidth / articleImage.ProcessedHeight;
    var width = aspect switch
    {
        >= 1.25 => t.TextWidth,
        >= 0.85 => t.TextWidth * 0.70,
        _ => t.TextWidth * 0.52
    };

    var maxHeight = t.TextHeight * 0.58;
    if (width / aspect > maxHeight)
    {
        width = maxHeight * aspect;
    }

    var paragraph = _content.AddParagraph("", "Figure");
    var image = paragraph.AddImage(CreateImageSource(articleImage));
    image.LockAspectRatio = true;
    image.Width = Unit.FromPoint(width);
    ApplyKeyline(image, articleImage);
    PlacedImageCount++;

    //A small credit line for the photographer/illustrator and licence, directly under the
    //  image and above the caption, when Wikimedia supplied attribution for the file
    if (!string.IsNullOrWhiteSpace(articleImage.Attribution))
    {
        _content.AddParagraph(articleImage.Attribution, "Credit");
    }

    var caption = _content.AddParagraph();
    caption.Style = "Caption";
    var label = caption.AddFormattedText($"FIG. {_figureNumber}");
    label.Font.Bold = true;
    label.Font.Size = t.LabelSize;
    label.Font.Color = BookTheme.Accent;
    // ... caption text ...
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
private static ImageSource.IImageSource CreateImageSource(ArticleImage articleImage)
{
    var bytes = articleImage.ProcessedBytes;
    return ImageSource.FromBinary(
        $"img-{articleImage.FileName}-{Guid.NewGuid():N}",
        () => bytes,
        quality: 90);
}

private static void ApplyKeyline(CodeBrix.PdfDocCreate.DocumentObjectModel.Shapes.Image image, ArticleImage articleImage)
{
    //A hairline keyline flatters photographs; diagrams and transparent
    //  graphics (PNG) read better without a frame
    if (IsJpeg(articleImage.ProcessedBytes))
    {
        image.LineFormat.Width = 0.4;
        image.LineFormat.Color = BookTheme.Hairline;
    }
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`

**Sharp edges.**
- The image source takes a name that acts as a cache key; add a unique suffix so
  two renders in one process never collide on a stale entry.
- Lock the aspect ratio and give only the width; setting both dimensions distorts.
- A credit style should use exact line spacing, because an at-least rule reserves
  a full text line above a tiny credit.
- Increment the figure counter before the placement, and report the placed count
  back in the result.

### Pair a figure with the credit paragraph that follows it

**When you want this.** The source format has no notion of an image credit, but
authors write one in the paragraph immediately after the picture, and it should be
typeset with the figure instead of as body text.

**The MVVM shape.** A pure static look-ahead in the library that the block loop
calls; it returns the index the loop should continue from, so a consumed paragraph
is never rendered twice. It is unit-tested with no network and no document.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
/// <summary>
/// The credit look-ahead: when the sibling immediately after an image/video is a
/// paragraph shaped like a rights line, it is consumed and rendered with the
/// figure. Returns that sibling's index, or the block's own index when the next
/// paragraph is ordinary body text.
/// </summary>
internal static int FindCreditParagraph(IReadOnlyList<NotionBlockNode> blocks, int index)
{
    if (index + 1 >= blocks.Count) { return index; }
    if (blocks[index + 1].Block is not ParagraphBlock paragraph) { return index; }

    var text = NotionConvert.PlainText(paragraph.Paragraph?.RichText).TrimStart();
    string[] creditShapes = ["Credit", "Image credit", "Photo", "Source", "©", "Public domain", "CC "];
    return creditShapes.Any(shape => text.StartsWith(shape, StringComparison.Ordinal))
        ? index + 1
        : index;
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
private int RenderImage(IBlockTarget target, IReadOnlyList<NotionBlockNode> blocks, int index,
    ImageBlock image)
{
    if (!_context.IncludeImages) { return index; }

    var creditIndex = FindCreditParagraph(blocks, index);
    var creditRuns = creditIndex > index
        ? ((ParagraphBlock)blocks[creditIndex].Block).Paragraph?.RichText
        : null;

    if (_context.MediaByBlockId.TryGetValue(image.Id, out var media) && media.HasImage)
    {
        PlaceFigure(target, media.Image, "FIG.", image.Image?.Caption, creditRuns, urlLine: null);
    }
    else
    {
        _context.Warnings.Add(BuildMediaWarning("Image", image.Image, media));
        RenderMediaFileCard(target, "IMAGE", image.Image, image.Id);
        RenderConsumedCredit(target, creditRuns);
    }
    return creditIndex;
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/CreditPairingTests.cs`

**Sharp edges.**
- The loop variable is assigned from the return value, which is how the consumed
  paragraph is skipped; returning the block's own index means "nothing consumed".
- Both the success path and the fallback card path render the credit, so a failed
  download never silently drops the rights line.
- The figure placement chooses its width from the aspect ratio, caps its height
  against the text block, and adds a keyline only for photographic formats.

### Render booktabs style tables from parsed rows

**When you want this.** Tables in a typeset document should have horizontal rules
only.

**The MVVM shape.** The parser produces a rectangular table model and refuses
anything it cannot lay out; the composer applies the rules.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
var table = _content.AddTable();
table.Borders.Visible = false;
table.TopPadding = Unit.FromPoint(3);
table.BottomPadding = Unit.FromPoint(3);
table.LeftPadding = Unit.FromPoint(2);
table.RightPadding = Unit.FromPoint(4);

var columnWidth = t.TextWidth / articleTable.ColumnCount;
for (var c = 0; c < articleTable.ColumnCount; c++)
{
    table.AddColumn(Unit.FromPoint(columnWidth));
}

for (var r = 0; r < articleTable.Rows.Count; r++)
{
    var sourceRow = articleTable.Rows[r];
    var row = table.AddRow();

    //Booktabs styling: strong top and bottom rules, a light rule under the header,
    //  and no vertical rules at all
    if (r == 0)
    {
        row.Borders.Top.Width = 1.0;
        row.Borders.Top.Color = BookTheme.Ink;
        if (articleTable.HasHeaderRow)
        {
            row.Borders.Bottom.Width = 0.5;
            row.Borders.Bottom.Color = BookTheme.Ink;
            row.HeadingFormat = true;
        }
    }
    if (r == articleTable.Rows.Count - 1)
    {
        row.Borders.Bottom.Width = 1.0;
        row.Borders.Bottom.Color = BookTheme.Ink;
    }
    // ... cells, with MergeRight for column spans ...
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs
foreach (var tr in table.QuerySelectorAll("tr"))
{
    //Only rows belonging to this table (not nested tables)
    if (tr.Closest("table") != table) { continue; }
    // ...
    if (int.TryParse(cell.GetAttribute("rowspan"), out var rowSpan) && rowSpan > 1)
    {
        skipReason = "uses row spans";
        return null;
    }
    // ...
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs`

**Sharp edges.**
- Marking the first row as a heading is what repeats it on a page break.
- Nested tables would be picked up by a plain descendant query; filter by nearest
  ancestor.
- The parser refuses tables it cannot represent - row spans, too few rows, too
  many rows or columns - and records the reason as a warning rather than producing
  a broken layout.
- A zero-height spacer paragraph after the table supplies the space below it,
  because a table object carries no space-after.

### Open a document with a raised initial

**When you want this.** The first paragraph of a book should start with a large
colored capital.

**The MVVM shape.** A one-shot flag on the composer; the first body paragraph
whose text starts with a letter gets its first character split into its own
formatted run.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
private void ComposeParagraph(ArticleBlock block)
{
    var paragraph = _content.AddParagraph();
    paragraph.Style = _previousWasBodyParagraph ? "BodyIndented" : "BodyOpen";

    var runs = block.Runs;

    //A raised initial marks the opening of the book (the first lead paragraph)
    if (!_raisedCapPlaced && runs.Count > 0 && runs[0].Text.Length > 0
        && char.IsLetter(runs[0].Text[0]))
    {
        _raisedCapPlaced = true;
        paragraph.Format.SpaceBefore = Unit.FromPoint(_theme.BodySize * 0.6);

        var first = runs[0];
        var initial = paragraph.AddFormattedText(first.Text[..1]);
        initial.Font.Size = _theme.RaisedCapSize;
        initial.Font.Color = BookTheme.Accent;

        AppendRuns(paragraph, [first with { Text = first.Text[1..] }, .. runs.Skip(1)]);
        return;
    }

    AppendRuns(paragraph, runs);
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`

**Sharp edges.**
- The style choice is driven by a "previous was a body paragraph" flag the block
  loop maintains, which is how classic first-line indents are applied only to
  continuation paragraphs.
- The run type is a record, so a `with` expression keeps the formatting of the
  remaining text.

### Write rich text runs into a paragraph or a hyperlink

**When you want this.** Source text arrives as annotated runs - bold, italic,
code, links - and the document model's paragraph and hyperlink types share no base
type for adding runs.

**The MVVM shape.** A writer class plus a tiny private adapter interface with two
implementations, so annotation handling is written once.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs
//Paragraph and Hyperlink share no add-text base type, so a tiny adapter pair
//  lets annotated runs target either
private interface IRunTarget
{
    FormattedText AddFormattedText(string text);
    void AddLineBreak();
}

private sealed class ParagraphTarget : IRunTarget
{
    private readonly Paragraph _paragraph;
    public ParagraphTarget(Paragraph paragraph) { _paragraph = paragraph; }
    public FormattedText AddFormattedText(string text) => _paragraph.AddFormattedText(text);
    public void AddLineBreak() => _paragraph.AddLineBreak();
}

private sealed class HyperlinkTarget : IRunTarget
{
    private readonly Hyperlink _hyperlink;
    public HyperlinkTarget(Hyperlink hyperlink) { _hyperlink = hyperlink; }
    public FormattedText AddFormattedText(string text) => _hyperlink.AddFormattedText(text);

    //Hyperlinks cannot hold line breaks — a newline inside link text becomes a space
    public void AddLineBreak() => _hyperlink.AddText(" ");
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs
private void ApplyAnnotations(FormattedText formatted, Annotations annotations, bool isLink, bool isEmoji)
{
    if (isEmoji)
    {
        formatted.Font.Name = BookFonts.EmojiFamily;
    }

    if (isLink)
    {
        formatted.Font.Color = BookTheme.Accent;
    }

    if (annotations is null) { return; }

    if (annotations.IsBold) { formatted.Font.Bold = true; }
    if (annotations.IsItalic) { formatted.Font.Italic = true; }
    if (annotations.IsUnderline) { formatted.Font.Underline = Underline.Single; }
    if (annotations.IsStrikeThrough) { formatted.Font.Strikethrough = Strikethrough.Single; }
    if (annotations.IsCode && !isEmoji)
    {
        formatted.Font.Name = BookFonts.MonoFamily;
        formatted.Font.Size = _theme.BodySize * 0.88;
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/RichTextWriterTests.cs`

**Sharp edges.**
- A link URL can arrive either on the run itself or inside the text run's own link
  object; check both.
- Newlines inside a run become explicit line breaks, except inside a hyperlink,
  where they become a space.
- Content the document cannot typeset - an inline equation, for instance - is
  rendered as its source text and adds a warning.

### Render into either a section or a table cell

**When you want this.** The same content renderer must work at top level and
inside a table cell, where the document model forbids nested tables.

**The MVVM shape.** One private interface with a capability flag, two adapters,
and every render method taking the interface. Callers ask the flag before choosing
a boxed layout or a flat one.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
//Section and Cell share no content-adding base type, so the renderer targets
//  either through a tiny adapter. Tables cannot nest inside cells in MigraDoc,
//  which is what SupportsTables guards.
private interface IBlockTarget
{
    bool SupportsTables { get; }
    Paragraph AddParagraph();
    Paragraph AddParagraph(string text, string style);
    Table AddTable();
}

private sealed class SectionTarget : IBlockTarget
{
    private readonly Section _section;
    public SectionTarget(Section section) { _section = section; }
    public bool SupportsTables => true;
    public Paragraph AddParagraph() => _section.AddParagraph();
    public Paragraph AddParagraph(string text, string style) => _section.AddParagraph(text, style);
    public Table AddTable() => _section.AddTable();
}

private sealed class CellTarget : IBlockTarget
{
    private readonly Cell _cell;
    public CellTarget(Cell cell) { _cell = cell; }
    public bool SupportsTables => false;
    public Paragraph AddParagraph() => _cell.AddParagraph();
    public Paragraph AddParagraph(string text, string style)
    {
        var paragraph = _cell.AddParagraph(text);
        paragraph.Style = style;
        return paragraph;
    }
    public Table AddTable() =>
        throw new InvalidOperationException("MigraDoc cannot nest a table inside a table cell.");
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
if (target.SupportsTables)
{
    var table = target.AddTable();
    table.Borders.Visible = false;
    table.Borders.Left.Width = 1.4;
    table.Borders.Left.Color = BookTheme.Accent;
    table.Shading.Color = BookTheme.PanelTint;
    // ...
    var cell = table.AddRow().Cells[0];
    FillCallout(new CellTarget(cell), node, titleRuns, bodyRuns, iconGlyph);
    AddPanelSpacer(target);
}
else
{
    //Inside a table cell (a column): tinted paragraphs, no nested table
    FillCallout(target, node, titleRuns, bodyRuns, iconGlyph, tintParagraphs: true);
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`

**Sharp edges.**
- Content that cannot be represented in the constrained target degrades to a
  simpler form plus a warning, rather than throwing.
- The cell adapter's styled overload has to set the style itself, because the cell
  version does not take one.
- A hair-high spacer paragraph after each panel keeps consecutive panels from
  touching.

### Keep unsupported content visible instead of failing the document

**When you want this.** You are mapping an open-ended source format and new input
kinds will appear after you ship.

**The MVVM shape.** The renderer's switch has a default arm that prints a visible
marker and records a warning; the service collects warnings and notes on a context
object and returns the warnings in the result, while notes are only logged.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
private void RenderUnsupported(IBlockTarget target, IBlock block)
{
    target.AddParagraph("[unsupported Notion block]", "UnsupportedMarker");
    var typeName = (block as UnsupportedBlock)?.Unsupported?.BlockType;
    _context.Warnings.Add(string.IsNullOrWhiteSpace(typeName)
        ? "A block of an unsupported type could not be rendered; a marker was printed instead."
        : $"A block of unsupported type \"{typeName}\" could not be rendered; a marker was printed instead.");
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RenderContext.cs
/// <summary>Non-fatal problems worth showing the user (surfaced in the result dialog).</summary>
public IList<string> Warnings { get; init; } = new List<string>();

/// <summary>Informational notes (logged, not shown as warnings).</summary>
public IList<string> Notes { get; init; } = new List<string>();
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RenderContext.cs`

**Sharp edges.**
- The class comment states the contract: unknown types render a visible marker and
  a warning; nothing silently vanishes and nothing ever throws mid-document.
- Two sinks, deliberately: warnings reach the user, notes only the log.

### Compose a fixed layout poster with the CodeBrix PdfDocuments library

**When you want this.** You want a designed, fixed-layout page - a one-sheet, a
certificate, a label - rather than a flowing document.

**The MVVM shape.** A separate library with a plain request object as its whole
input, no UI and no graphics-backend dependency, so the composition is exercised
headlessly. The view model gathers a snapshot into the request and calls the
creator on a worker thread.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Services/MarketingSheetCreator.cs
private static PdfDocument Compose(MarketingSheetRequest request)
{
    SheetFonts.EnsureRegistered();

    var document = new PdfDocument();
    document.Info.Title = $"{request.ModelName} — Poly Haven model one-sheet";
    document.Info.Subject = "A marketing one-sheet generated by PolyHavenBrowser";

    var page = document.AddPage();
    page.Width = SheetTheme.PageWidth;
    page.Height = SheetTheme.PageHeight;

    var accent = AccentColorSampler.Sample(request.CatalogThumbnailBytes);
    var theme = new SheetTheme(accent);

    using var gfx = XGraphics.FromPdfPage(page);
    new SheetComposer(request, theme).Compose(gfx);

    return document;
}
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/PolyHavenBrowser.CreateDocument.csproj -->
<!-- The marketing one-sheet is a poster (absolute placement of shots, rules and type),
     so it draws directly with CodeBrix.PdfDocuments' XGraphics rather than composing a
     flowing document through the CodeBrix.PdfDocCreate document object model the way
     NotionDocumentCreator's book pipeline does. -->
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Internal/SheetComposer.cs
public void Compose(XGraphics gfx)
{
    ArgumentNullException.ThrowIfNull(gfx);

    DrawHeader(gfx);
    DrawHeroRow(gfx);
    DrawCopyColumns(gfx);
    DrawTags(gfx);
    DrawGallery(gfx);
    DrawSpecs(gfx);
    DrawFooter(gfx);
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Services/MarketingSheetCreator.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Internal/SheetComposer.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Models/MarketingSheetRequest.cs`

**Also shown by.**
`InannaRosette/src/libs/InannaRosette.Reading/Services/PdfReportBuilder.cs` and
`Services/Pdf/`
(a multi-page report drawn straight onto `XGraphics` - cover, full-page spread
diagram, one section per station, an appendix of lore - over a page-flow cursor,
a text kit and a palette the library grows for itself, with the reason for
drawing rather than composing written into the library's own project file;
`Build()` returns bytes rather than writing a file)

**Sharp edges.**
- Choose the API to match the document: absolute placement wants the graphics
  object directly; a flowing, paginated document wants a document object model
  instead. The project file says so where a future reader will find it.
- The graphics object has no character-spacing property, so a letterspaced label
  is made by interleaving thin spaces into the string itself.
- Every band of the poster clamps or truncates its own content - the title shrinks
  in one-point steps until it measures within the content width - which is how the
  sheet stays exactly one page.
- Offering both a save-to-file and a return-bytes entry point is what lets the
  tests parse the result.

### Open a PDF and read its page count with the CodeBrix PdfRasterizer library

**When you want this.** You need to know how many pages a user-chosen PDF has, and
you want a clear error when the file is missing or is not a PDF.

**The MVVM shape.** A plain document type in the library owns the file: it reads
the bytes once, asks the rasterizer for the page count, and exposes a one-based
cursor that cannot leave the range. The view model holds the document; it never
touches the rasterizer.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/PdfPageDocument.cs
    /// <summary>The raw PDF bytes, handed to the rasterizer so the file is never re-read.</summary>
    internal byte[] PdfBytes { get; }

    public static async Task<PdfPageDocument> OpenAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = DocumentPath.Normalize(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The PDF file was not found.", fullPath);
        }

        var pdfBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);

        int pageCount;
        try
        {
            using var rasterizer = new PageRasterizer();
            pageCount = await rasterizer.GetPageCount(pdfBytes, cancellationToken: cancellationToken);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            throw new InvalidDataException(
                $"“{Path.GetFileName(fullPath)}” could not be read as a PDF document.", e);
        }

        if (pageCount < 1)
        {
            throw new InvalidDataException($"“{Path.GetFileName(fullPath)}” has no pages.");
        }

        return new PdfPageDocument(fullPath, pdfBytes, pageCount);
    }
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/PdfPageDocument.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/PdfPageDocumentTests.cs`

**Sharp edges.**
- The bytes are read once and kept, and every later render is given the same
  array. The file is never re-opened, so the user can move or delete it
  mid-session.
- The rasterizer is disposable. Here it is created and disposed just for the page
  count; the long-lived one lives in the renderer.
- The exception filter is what keeps a cancellation from being reported as "not a
  PDF".
- The page count is checked, not trusted: a result below one becomes a typed
  exception with a message naming the file.

### Rasterize a PDF page to PNG off the UI thread

**When you want this.** You want a page image to hand to a bound image element,
and the rasterizer is synchronous underneath.

**The MVVM shape.** A renderer service in the library does the work and returns a
small immutable record. The view model awaits it and never sees a bitmap API; the
pane view model turns the bytes into an image source.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs
        //PDFium renders synchronously, so keep it off the caller's (UI) thread
        var pngBytes = await Task.Run(async () =>
        {
            using var image = await _rasterizer.RasterizeToImage(
                document.PdfBytes, pageNumber, dpi, cancellationToken: cancellationToken);
            using var stream = new MemoryStream();
            await image.SaveAsync(stream, PngFormat.Instance, cancellationToken);
            return (Width: image.Width, Height: image.Height, Bytes: stream.ToArray());
        }, cancellationToken);

        var rendered = new RenderedPage(document.FilePath, pageNumber, pngBytes.Width, pngBytes.Height, pngBytes.Bytes);
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/RenderedPage.cs
public sealed record RenderedPage(string FilePath, int PageNumber, int PixelWidth, int PixelHeight, byte[] PngBytes);
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs`
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/RenderedPage.cs`

**Sharp edges.**
- The comment states the reason for `Task.Run`: the engine renders synchronously,
  so awaiting alone would still block the calling thread.
- The rendered image is disposed inside the `Task.Run` body; only the encoded
  bytes and the pixel size escape, so nothing that needs disposal crosses back to
  the UI thread.
- The pixel size is read from the image, not computed; the tests rely on it.
- One rasterizer instance is created per renderer and disposed with it, rather
  than one per render.

### Keep two documents in step while letting the user offset one

**When you want this.** Two editions of a document rarely paginate identically,
and the user needs to line them up once and then page through both together.

**The MVVM shape.** A plain model class holds both documents and both cursors and
exposes the moves as boolean-returning methods with matching "can" properties, so
the view model can wire them straight to command predicates. No UI type appears in
it.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/PdfComparison.cs
    /// <summary>Whether <see cref="MoveBothNext"/> would move at least one cursor.</summary>
    public bool CanMoveBothNext => IsReady && (Left.CanMoveNext || Right.CanMoveNext);

    /// <summary>
    /// Steps both documents to their next page; a document already on its last page stays
    /// there. Returns whether any cursor moved.
    /// </summary>
    public bool MoveBothNext()
    {
        if (!IsReady) { return false; }
        var movedLeft = Left.MoveNext();
        var movedRight = Right.MoveNext();
        return ResetViewIf(movedLeft || movedRight);
    }

    /// <summary>Steps only the right document to its next page. Returns whether it moved.</summary>
    public bool AdjustRightNext() => ResetViewIf(IsReady && Right.MoveNext());

    //A page change always comes back at fit-the-page, both panes centred
    private bool ResetViewIf(bool moved)
    {
        if (moved) { View.Reset(); }
        return moved;
    }
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/PdfComparison.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/PdfComparisonTests.cs`

**Sharp edges.**
- Both moves are performed unconditionally and their results combined afterwards.
  Short-circuiting would skip the second document, so assigning to two locals
  first is deliberate.
- Each cursor clamps at its own last page, so the offset the user set is preserved
  until one document runs out.
- Every move that actually changes a page resets the zoom and pan; a move that
  goes nowhere leaves the view alone, decided by the return value.
- The model owns the view state too, so the "reset on page change" rule cannot be
  forgotten by a caller.

### Treat two spellings of one path as the same file

**When you want this.** You need to reject "the user picked the same file twice",
and the two paths may be spelled differently and may be on a case-insensitive file
system.

**The MVVM shape.** A tiny internal helper in the library, used by the model and
covered by tests through the internals-visible attribute.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/DocumentPath.cs
internal static class DocumentPath
{
    private static readonly StringComparison Comparison =
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    /// <summary>Returns the absolute, separator-trimmed form of path.</summary>
    public static string Normalize(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }

    /// <summary>Whether pathA and pathB refer to the same file.</summary>
    public static bool AreSame(string pathA, string pathB) =>
        string.Equals(Normalize(pathA), Normalize(pathB), Comparison);
}
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/DocumentPath.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/DocumentPathTests.cs`

**Sharp edges.**
- The comparison is chosen by operating system, not hard-coded.
- Getting the full path collapses relative segments and makes the path absolute;
  the trailing separator has to be trimmed separately or two spellings of a
  directory differ.
- The normalized form is what the document stores, so everything downstream - the
  cache key, the duplicate check, the displayed path - agrees.

### Register import and export formats at startup through one entry point

**When you want this.** You want your codec set to be data, not scattered
registration calls, and you want a library to own it.

**The MVVM shape.** The model's format manager starts empty; a library exposes one
static registration method the application calls at startup. Each format is a
descriptor pairing an importer and an exporter - either may be absent - with
display name, extensions, media types and a capability flag.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.FileFormats/Registration/FileFormats.cs
public static void RegisterAll (ImageConverterManager formats)
{
	// --- SkiaSharp-codec formats (Skia decodes and encodes these)

	SkiaCodecFormat pngHandler = new ("png", SKEncodedImageFormat.Png);
	formats.RegisterFormat (new FormatDescriptor (
		displayPrefix: "PNG",
		extensions: ["png", "PNG"],
		mimes: ["image/png"],
		importer: pngHandler,
		exporter: pngHandler));
	// ...
	// --- Formats Skia decodes but cannot encode (CodeBrix.Imaging exports)

	formats.RegisterFormat (new FormatDescriptor (
		displayPrefix: "BMP",
		extensions: ["bmp", "BMP"],
		mimes: ["image/bmp"],
		importer: new SkiaCodecFormat ("bmp"),
		exporter: CodeBrixImagingFormat.CreateBmp ()));
	// ...
	// --- This library's own format implementations

	OraFormat oraHandler = new ();
	formats.RegisterFormat (new FormatDescriptor (
		displayPrefix: "OpenRaster",
		extensions: ["ora", "ORA"],
		mimes: ["image/openraster"],
		importer: oraHandler,
		exporter: oraHandler,
		supportsLayers: true));

	// Export-only, matching upstream
	formats.RegisterFormat (new FormatDescriptor (
		displayPrefix: "TGA",
		extensions: ["tga", "TGA"],
		mimes: ["image/x-tga"],
		importer: null,
		exporter: new TgaExporter ()));
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Engine bootstrap: install the UI-layer services and register the
//file formats and core effects/adjustments with the engine
Pinta.Brix.Engine.PintaCore.InitializeResources(new Pinta.Brix.Controls.SkiaResourceService());
Pinta.Brix.Engine.PintaCore.InitializeTimer(
    new Pinta.Brix.Controls.DispatcherTimerService(MainWindow.DispatcherQueue));
Pinta.Brix.FileFormats.FileFormatsRegistration.RegisterAll(Pinta.Brix.Engine.PintaCore.ImageFormats);
Pinta.Brix.Effects.CoreEffects.Register(Pinta.Brix.Engine.PintaCore.Services);
Pinta.Brix.Tools.CoreTools.Register(Pinta.Brix.Engine.PintaCore.Services);
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.FileFormats/Registration/FileFormats.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Effects/Registration/CoreEffects.cs`
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Sharp edges.**
- The effects and tools registrations take a service provider and resolve what
  each item needs, so the registration list is the only place that knows the
  catalog.
- Extensions are listed in both cases because file matching is ordinal. The registry
  drops the upper case spellings again when it builds a dialog's filter entries, so a
  page never has to know the rule.
- Registration happens after the window exists, because one of the installed
  services needs the window's dispatcher queue.

### Add codec coverage beyond SkiaSharp with the CodeBrix Imaging library

**When you want this.** Your users expect formats the base graphics library cannot
encode, or cannot handle at all.

**The MVVM shape.** One importer and exporter class wrapping the imaging library,
with factory methods per format, sitting behind the same interfaces as every other
codec. Nothing above the format registry knows which library answered.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.FileFormats/CodeBrixImagingFormat.cs
/// <summary>Windows bitmap, written as uncompressed 32-bit BGRA.</summary>
public static CodeBrixImagingFormat CreateBmp ()
	=> new (
		"bmp",
		BmpFormat.Instance,
		new BmpEncoder {
			BitsPerPixel = BmpBitsPerPixel.Pixel32,
			SupportTransparency = true,
		});

/// <summary>GIF, quantized to a 256-color palette by the encoder.</summary>
public static CodeBrixImagingFormat CreateGif ()
	=> new ("gif", GifFormat.Instance, new GifEncoder ());

/// <summary>TIFF, written with the encoder's default settings.</summary>
public static CodeBrixImagingFormat CreateTiff ()
	=> new ("tiff", TiffFormat.Instance, new TiffEncoder ());

/// <inheritdoc/>
public void Export (Document document, string file)
{
	using ImageSurface flattenedImage = document.GetFlattenedImage ();

	ReadOnlySpan<ColorBgra> source = flattenedImage.GetReadOnlyPixelData ();
	Bgra32[] destination = new Bgra32[flattenedImage.Width * flattenedImage.Height];

	// Premultiplied engine pixels back to straight alpha for the encoder.
	for (int i = 0; i < destination.Length; i++) {
		ColorBgra p = source[i].ToStraightAlpha ();
		destination[i] = new Bgra32 (r: p.R, g: p.G, b: p.B, a: p.A);
	}

	using CodeBrix.Imaging.Image<Bgra32> image = ImagingImage.LoadPixelData<Bgra32> (
		destination,
		flattenedImage.Width,
		flattenedImage.Height,
		image_format);

	using FileStream stream = File.Create (file);
	encoder.Encode (image, stream);
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.FileFormats/CodeBrixImagingFormat.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.FileFormats.Tests/CodeBrixImagingFormatTests.cs`

**Sharp edges.**
- The alpha convention differs on each side: the imaging library's pixels are
  straight alpha, the engine's surfaces are premultiplied. Both directions convert
  explicitly, and this is the bug that would otherwise show up as dark halos on
  transparent edges.
- Import and export can come from different libraries for the same format.
- The project file's comment states the reason for the dependency in one line:
  codec coverage beyond the other library's encoders.

### Save a document through a native picker with format filters

**When you want this.** A save-as that offers exactly the formats you can write
and warns before a lossy conversion.

**The MVVM shape.** The model exposes a handler delegate; the UI layer installs an
async implementation that shows the picker, resolves an exporter, warns when the
chosen format cannot hold everything, and marks the document clean. In a
view-model shape the handler is a bridge interface the page implements and the
view model consumes.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/WorkspaceManager.cs
/// <summary>
/// Handler the application installs to perform document saving (shows
/// pickers, runs exporters). Returns true when the save completed.
/// (Upstream routed this through the app-level action manager.)
/// </summary>
public Func<Document, bool, System.Threading.Tasks.Task<bool>>? SaveDocumentHandler { get; set; }
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
var picker = new Windows.Storage.Pickers.FileSavePicker
{
    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
    SuggestedFileName = document.DisplayName,
};
//The registry owns which formats a save dialog offers and under
//what name; the page just hands the list to the picker.
foreach (FileDialogFilter filter in PintaCore.ImageFormats.GetExportFilters())
{
    picker.FileTypeChoices.Add(filter.Name, [.. filter.Extensions]);
}

StorageFile file = await picker.PickSaveFileAsync();
if (file is null) { return false; }
// ...
//Saving to a format that cannot hold layers flattens the image; upstream
//asks first, because the layers are gone from the FILE either way.
if (document.Layers.Count() > 1 && !descriptor.SupportsLayers)
{
    ContentDialog flattenDialog = new()
    {
        Title = "Flatten Image?",
        Content = $"The {descriptor.FilterName} format does not support layers. "
            + "The saved file will contain a flattened copy of the image.",
        PrimaryButtonText = "Flatten",
        CloseButtonText = "Cancel",
        DefaultButton = ContentDialogButton.Primary,
        XamlRoot = XamlRoot,
    };

    if (await flattenDialog.ShowAsync() != ContentDialogResult.Primary) { return false; }
}

descriptor.Exporter.Export(document, path);
document.File = path;
document.FileType = fileType;
document.Workspace.History.SetClean();
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/WorkspaceManager.cs` and
`Managers/ImageConverterManager.cs`

**Related.**
[Build a file dialog's filter list in the format registry](#build-a-file-dialogs-filter-list-in-the-format-registry)
is where the entries the picker is handed come from.

**Sharp edges.**
- The page does not decide what the dialog offers. It copies the filter entries the
  format registry hands it, so the rules about which formats are offered and which
  spelling of an extension is used are stated once, in the library.
- The picker returns null on cancel and the whole save must return false, or a
  cancelled save marks the document clean.
- Clearing the history's dirty flag after a successful export is what clears the
  marker on the tab title.
- A failed or cancelled save inside a close prompt must abort the close; the close
  path checks the return value.

### Raise a UI hook from a codec through a static event

**When you want this.** A library needs one optional interaction - a quality
slider, an overwrite confirmation - without taking a UI dependency.

**The MVVM shape.** The codec exposes a static event carrying mutable event
arguments; if nothing subscribes, the stored value is used unchanged. The UI layer
subscribes if and when it wants to show the dialog.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.FileFormats/JpegFormat.cs
/// <summary>
/// Raised before a document's first JPEG save of the session so a UI layer
/// can let the user adjust the compression quality. Handlers may update
/// <see cref="ModifyCompressionEventArgs.Quality"/> or set
/// <see cref="ModifyCompressionEventArgs.Cancel"/> to abort the save.
/// With no handler installed the pending quality value is used as-is.
/// </summary>
public static event EventHandler<ModifyCompressionEventArgs>? ModifyCompression;

protected override void DoSave (ImageSurface flattenedImage, Document document, string file, SKEncodedImageFormat format)
{
	//Load the JPG compression quality, but use the default value if there is no saved value.
	int level = PintaCore.Settings.GetSetting (JpgQualitySetting, DefaultQuality);

	//Check to see if the Document has been saved before.
	if (!document.HasBeenSavedInSession) {
		level = RaiseModifyCompression (level);

		if (level == -1)
			throw new OperationCanceledException ();
	}

	//Store the "previous" JPG compression quality value (before saving with it).
	PintaCore.Settings.PutSetting (JpgQualitySetting, level);

	SaveBitmap (flattenedImage, file, format, level);
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.FileFormats/JpegFormat.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/EventArgs/ModifyCompressionEventArgs.cs`

**Sharp edges.**
- The setting key is duplicated as a private constant with a comment saying it
  matches the engine's internal name, because the engine's constants class is
  internal. Duplicating a key across an assembly boundary needs that comment or
  the two silently drift.
- The event is static, so a subscriber must unsubscribe or it lives for the
  process.
- Cancel is signalled by a sentinel value converted into a cancellation exception,
  so a cancel is distinguishable from a failure.

### Offer a data folder by rule rather than by a hard-coded list

**When you want this.** An application presents the contents of a folder it ships
or a folder a person points it at, and you do not want a list of names in the code
that somebody has to remember to edit when the data changes.

**The MVVM shape.** A static class per kind of data, in a UI-free library: it
takes a folder, applies a rule - every sub-folder except the ones named, every
file whose extension this application can open - and returns a list of small
immutable items. The view model calls it, wraps each item in a bound row and knows
nothing about the file system. Because the scanner is static and takes a folder,
it is tested against the real shipped files.

**Code.**

```csharp
// From CodeBrix.Samples/SimpleCbxVideoPlayer/src/libs/SimpleCbxVideoPlayer.SkiaVideo/Assets/VideoCorpus.cs
/// <remarks>
/// The rule is deliberately a rule rather than a list of folders: EVERY sub-folder of the corpus is read,
/// except the ones named in <see cref="ExcludedFolderNames" />, and every file whose extension is in
/// <see cref="PlayableExtensions" /> is offered. So MKV/, WebM/, CodeBrix-Mode1/ and CodeBrix-Mode2/ are all
/// listed today - twenty-four files - MP4/ is not, because nothing in this family reads an MP4, and a folder
/// added tomorrow appears in the drop-down without a line of code changing. That is not a hypothetical:
/// CodeBrix-Mode2/ landed after this class was written and needed no change at all.
/// </remarks>
public static class VideoCorpus
{
    /// <summary>The extensions this application can open: Matroska, WebM and the bespoke container.</summary>
    public static readonly IReadOnlyList<string> PlayableExtensions = [".mkv", ".webm", ".cbv"];

    /// <summary>Corpus folders that are skipped whatever they hold.</summary>
    /// <remarks>
    /// MP4 is the corpus's ORIGINALS folder - H.264 video in an ISOBMFF container, which this family reads
    /// no part of. Offering those files would offer a failure.
    /// </remarks>
    public static readonly IReadOnlyList<string> ExcludedFolderNames = ["MP4"];

    public static IReadOnlyList<VideoCorpusItem> Scan(string authoringFolder)
    {
        List<VideoCorpusItem> items = [];

        if (string.IsNullOrWhiteSpace(authoringFolder) || !Directory.Exists(authoringFolder)) { return items; }

        foreach (var folder in Directory.GetDirectories(authoringFolder))
        {
            var folderName = Path.GetFileName(folder);

            if (IsExcludedFolder(folderName)) { continue; }

            foreach (var file in Directory.GetFiles(folder))
            {
                if (!IsPlayable(file)) { continue; }

                items.Add(new VideoCorpusItem(folderName, Path.GetFileName(file), file));
            }
        }

        return items
            .OrderBy(item => item.FolderName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
    // ...
}
```

A second scanner in the same shape groups what it finds and reads only the header
of each file, because a full parse of everything just to fill a list is work nobody
asked for:

```csharp
// From CodeBrix.Samples/SimpleCbxVideoPlayer/src/libs/SimpleCbxVideoPlayer.SkiaVideo/Assets/LutCatalog.cs
/// <remarks>
/// Two of the corpus's three folders are read, in this order: <c>generated/</c> then <c>found/</c>, each
/// searched recursively because <c>found/</c> keeps one folder per upstream project. The third folder,
/// <c>invalid/</c>, is NEVER read: the files in it are malformed on purpose, as negative test fixtures, and
/// offering one would offer a parse failure.
/// </remarks>
public static class LutCatalog
{
    /// <summary>The corpus groups that are read, in the order the list shows them.</summary>
    public static readonly IReadOnlyList<string> GroupFolderNames = ["generated", "found"];

    public static IReadOnlyList<LutCatalogEntry> Scan(string lutsFolder)
    {
        List<LutCatalogEntry> entries = [];

        if (string.IsNullOrWhiteSpace(lutsFolder) || !Directory.Exists(lutsFolder)) { return entries; }

        foreach (var groupName in GroupFolderNames)
        {
            var groupFolder = Path.Combine(lutsFolder, groupName);

            if (!Directory.Exists(groupFolder)) { continue; }

            List<LutCatalogEntry> group = [];

            foreach (var file in Directory.GetFiles(groupFolder, "*" + CubeExtension, SearchOption.AllDirectories))
            {
                group.Add(new LutCatalogEntry(groupName, Path.GetFileName(file), file, ReadTitle(file)));
            }

            entries.AddRange(group.OrderBy(entry => entry.DisplayName, StringComparer.OrdinalIgnoreCase));
        }

        return entries;
    }
    // ...
}
```

```csharp
// From CodeBrix.Samples/SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs
foreach (VideoCorpusItem item in VideoCorpus.Scan(SampleAssets.GetAuthoringFolder(assetsRoot)))
{
    Videos.Add(new VideoListItem(item));
}

foreach (LutCatalogEntry entry in LutCatalog.Scan(SampleAssets.GetLutsFolder(assetsRoot)))
{
    //The catalogue is kept beside the rows, index for index, so a name can find its row.
    lutCatalogue.Add(entry);
    Luts.Add(new LutListItem(entry, OnLutPanelEdited));
}
```

**Where to look.**
`SimpleCbxVideoPlayer/src/libs/SimpleCbxVideoPlayer.SkiaVideo/Assets/VideoCorpus.cs`
`SimpleCbxVideoPlayer/src/libs/SimpleCbxVideoPlayer.SkiaVideo/Assets/LutCatalog.cs` and
`src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs`
`SimpleCbxVideoPlayer/tests/libs/SimpleCbxVideoPlayer.SkiaVideo.Tests/VideoCorpusTests.cs` and
`LutCatalogTests.cs`

**Sharp edges.**
- Exclude by rule too, and say in the code why each exclusion exists. A folder of
  deliberately malformed fixtures and a folder in a format the application cannot
  open are both "do not offer these", and both need the reason written down or the
  next reader deletes the exclusion.
- Compare extensions and folder names with an ordinal ignore-case comparison; a
  culture-aware comparison of file names is a bug waiting for a Turkish locale.
- Sort explicitly. Directory enumeration order is whatever the file system feels
  like, and a list that reorders itself between runs looks broken.
- Read only the header when a list needs a display name. The full parse belongs to
  the moment the file is actually used.
- A missing folder returns an empty list, not an exception: the caller shows an
  empty list and a sentence, which is a better answer than a dialog.
- Keep the scanned items and the bound rows in the same order so an index into one
  finds the other; that is what lets a command line or a search box name a file and
  tick its row.

### Build a file dialog's filter list in the format registry

**When you want this.** Several pages open and save files, every one of them needs the
same list of formats, and you do not want each picker working that list out from the
format descriptors itself.

**The MVVM shape.** The registry that owns the formats owns the filter list as well. It
hands out one small record per entry - the name a dialog shows and the extensions it
stands for - and the page's only work is to copy that list into the picker. Nothing
about a descriptor, including which case its extensions are spelled in and whether it
can be written as well as read, leaves the library.

**Code.**

The record is deliberately tiny, and it is the only shape that crosses the line:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/FileDialogFilter.cs
/// <summary>
/// A display name paired with the file extensions it stands for, ready to be
/// handed to a file dialog's filter list.
/// </summary>
public sealed class FileDialogFilter
{
	/// <summary>
	/// The name the dialog shows for this entry, for example
	/// <c>"OpenRaster image (*.ora)"</c>.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The extensions the entry accepts, each including its leading dot and
	/// in lower case, for example <c>".ora"</c>.
	/// </summary>
	public ImmutableArray<string> Extensions { get; }
	// ...
}
```

Two questions, one builder: an open dialog and a save dialog differ by a predicate,
and an open dialog that wants extensions rather than named entries gets them from the
same list:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ImageConverterManager.cs
	/// <summary>
	/// The filter entries an "open image" dialog should offer: one per format
	/// that can be imported, carrying the lower case extensions it reads.
	/// </summary>
	public IReadOnlyList<FileDialogFilter> GetImportFilters ()
		=> BuildFilters (f => f.IsImportAvailable ());

	/// <summary>
	/// The filter entries a "save image as" dialog should offer: one per
	/// format that can be exported, carrying the lower case extensions it
	/// writes.
	/// </summary>
	public IReadOnlyList<FileDialogFilter> GetExportFilters ()
		=> BuildFilters (f => f.IsExportAvailable ());

	/// <summary>
	/// Every lower case extension an "open image" dialog should accept, in
	/// registration order and with no duplicates.
	/// </summary>
	public IReadOnlyList<string> GetImportExtensions ()
		=> [.. GetImportFilters ().SelectMany (f => f.Extensions).Distinct ()];

	/// <summary>
	/// Builds one filter entry per available format. The registry lists every
	/// extension in both cases so that matching is case-insensitive; a dialog
	/// only wants one of each, so the upper case spellings are dropped here
	/// rather than by every caller.
	/// </summary>
	private List<FileDialogFilter> BuildFilters (Func<FormatDescriptor, bool> isAvailable)
	{
		List<FileDialogFilter> filters = [];

		foreach (FormatDescriptor format in formats.Where (isAvailable)) {

			List<string> extensions = [
				.. format.Extensions
					.Where (x => x.All (char.IsLower))
					.Select (x => $".{x}")];

			if (extensions.Count > 0)
				filters.Add (new FileDialogFilter (format.FilterName, extensions));
		}

		return filters;
	}
```

What is left in the page is a copy loop, and it reads the same either way round:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Palette.cs
        //The palette registry owns which formats a save dialog offers.
        foreach (FileDialogFilter filter in PintaCore.PaletteFormats.GetSaveFilters())
        {
            picker.FileTypeChoices.Add(filter.Name, [.. filter.Extensions]);
        }
```

The rule about extension case is now somewhere a test can hold it to account:

```csharp
// From CodeBrix.Samples/Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/ImageConverterManagerTests.cs
	[Fact]
	public void GetExportFilters_drops_the_upper_case_spellings ()
	{
		//Arrange
		ImageConverterManager formats = CreateManager ();

		//Act
		FileDialogFilter filter = formats.GetExportFilters ().First (f => f.Extensions.Contains (".bth"));

		//Assert
		filter.Extensions.Should ().BeEquivalentTo ([".bth", ".bo"]);
	}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/FileDialogFilter.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ImageConverterManager.cs` and
`Managers/PaletteFormatManager.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`, `Views/MainPage.Palette.cs` and
`Views/MainPage.Actions.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/ImageConverterManagerTests.cs` and
`PaletteFormatManagerTests.cs`

**Also shown by.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs`, whose
`DescribeExtension` names the one destination format a save dialog is being opened for;
the page passes the extension in and puts the answer on the picker, so the label is
decided by the same class that decides everything else about a format

**Related.**
[Register import and export formats at startup through one entry point](#register-import-and-export-formats-at-startup-through-one-entry-point)
is where the entries come from, and
[Save a document through a native picker with format filters](#save-a-document-through-a-native-picker-with-format-filters)
is the save path that consumes them.

**Sharp edges.**
- A format that can be read but not written must never appear in a save dialog. The two
  filter methods differ by exactly one predicate, which is the cheapest way to be sure
  they cannot drift apart.
- Extensions listed in both cases for ordinal matching are a library detail. Drop the
  spellings a dialog does not want inside the builder, once, rather than in every page
  that opens a picker.
- Two registries in the same application can answer the same question differently and
  both be right: the image registry offers the lower case spellings only, and the
  palette registry offers every spelling it holds, because its dialog matches literally.
  Each rule is written down beside the formats it governs.
- An empty registry produces an empty list rather than an exception, and a picker with
  no filters offers everything. Register the formats before a dialog can be opened.

### Describe vector art once and render it through two renderers

**When you want this.** The same artwork has to appear on screen and in a
document you generate, and you refuse to draw it twice. Two drawings drift
apart; a bitmap shared between them looks soft when it is printed.

**The MVVM shape.** The artwork is plain data in the UI-free library: one list
of layer records per motif, each record a path string authored in a fixed
square design box plus the few flags a renderer needs. Neither renderer owns
the art. A page control turns a layer list into XAML shapes inside a canvas
that a viewbox scales; the report turns the same list into paths on the
document's graphics context. Adding a motif is adding data, and both sides
pick it up with no further work.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Data/EmblemArt.cs
/// <summary>
/// One drawable layer of an emblem. <see cref="PathData"/> is SVG path mini-language
/// (absolute commands only) understood by WinUI <c>Path.Data</c> and <c>SKPath.ParseSvgPathData</c>.
/// </summary>
/// <param name="PathData">The geometry, in a 0..100 x 0..100 box.</param>
/// <param name="Filled">True to fill the geometry; false to stroke it.</param>
/// <param name="StrokeWidth">Stroke width in box units (only meaningful when <paramref name="Filled"/> is false).</param>
/// <param name="Opacity">Layer opacity, 0..1.</param>
/// <param name="Accent">True when the renderer should use the card's secondary colour for this layer.</param>
public sealed record EmblemLayer(string PathData, bool Filled, double StrokeWidth = 0, double Opacity = 1.0, bool Accent = false);
// ...
/// <summary>
/// Procedural vector art for every <see cref="Emblem"/>, plus the deck's shared ornaments.
/// Every emblem is drawn inside a 0..100 x 0..100 box, centered at (50,50),
/// designed to sit inside a circle of radius ~46.
/// </summary>
public static class EmblemArt
{
    private static EmblemLayer F(string d) => new(d, true);
    private static EmblemLayer F(string d, double opacity) => new(d, true, 0, opacity);
    private static EmblemLayer A(string d) => new(d, true, 0, 1.0, true);
    private static EmblemLayer S(string d, double w = 3) => new(d, false, w);
    private static EmblemLayer S(string d, double w, double opacity) => new(d, false, w, opacity);
    private static EmblemLayer SA(string d, double w = 3) => new(d, false, w, 1.0, true);

    private static IReadOnlyList<EmblemLayer> L(params EmblemLayer[] layers) => layers;

    /// <summary>Returns the layers for <paramref name="emblem"/>, back to front.</summary>
    public static IReadOnlyList<EmblemLayer> Layers(Emblem emblem) => emblem switch
```

The screen renderer is a page-side helper: it is the only place in the
application that knows what a XAML shape is, and it returns a fixed-size canvas
so that the caller can scale it to whatever size a card currently happens to
be.

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs
    /// <summary>
    /// Render a stack of emblem layers authored in a 0..<paramref name="box"/> coordinate box
    /// into a fixed-size Canvas, ready for a Viewbox. Filled layers take <paramref name="fill"/>
    /// (accent layers take <paramref name="accent"/>); stroked layers take <paramref name="stroke"/>.
    /// </summary>
    internal static Canvas LayerCanvas(IReadOnlyList<EmblemLayer> layers, double box,
                                       Brush? fill, Brush? accent, Brush? stroke)
    {
        var canvas = new Canvas { Width = box, Height = box };
        if (layers is null) { return canvas; }

        foreach (var layer in layers)
        {
            if (layer is null || string.IsNullOrWhiteSpace(layer.PathData)) { continue; }
            var brush = layer.Accent ? (accent ?? fill) : fill;
            canvas.Children.Add(new Microsoft.UI.Xaml.Shapes.Path
            {
                Data = ParseGeometry(layer.PathData),
                Fill = layer.Filled ? brush : null,
                Stroke = layer.Filled ? null : (layer.Accent ? (accent ?? stroke) : stroke),
                StrokeThickness = layer.Filled ? 0 : Math.Max(0.4, layer.StrokeWidth),
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Opacity = layer.Opacity <= 0 ? 1 : layer.Opacity,
            });
        }
        return canvas;
    }
```

The document renderer takes the identical list and the rectangle it should
fill, and resolves each layer's color from the same three inputs.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfOrnaments.cs
    /// <summary>
    /// Draws a stack of <see cref="EmblemLayer"/>s into <paramref name="box"/>.
    /// Layers marked as accents take <paramref name="accent"/>; other filled layers take
    /// <paramref name="main"/>; other stroked layers take <paramref name="stroke"/>.
    /// </summary>
    public static void DrawLayers(
        XGraphics gfx,
        IReadOnlyList<EmblemLayer> layers,
        XRect box,
        XColor main,
        XColor accent,
        XColor stroke,
        double opacity = 1.0,
        double designBox = SvgPathToPdf.DefaultDesignBox)
    {
        if (layers.Count == 0) return;
        var scale = Math.Min(box.Width, box.Height) / designBox;

        foreach (var layer in layers)
        {
            if (string.IsNullOrWhiteSpace(layer.PathData)) continue;

            XGraphicsPath path;
            try
            {
                path = SvgPathToPdf.Build(layer.PathData, box, designBox);
            }
            catch (FormatException)
            {
                continue;   // one malformed glyph must never take the whole report down
            }

            var layerOpacity = Math.Clamp(layer.Opacity, 0, 1) * Math.Clamp(opacity, 0, 1);
            if (layerOpacity <= 0) continue;

            if (layer.Filled)
            {
                var colour = layer.Accent ? accent : main;
                gfx.DrawPath(new XSolidBrush(colour.Alpha(layerOpacity)), path);
            }
            else
            {
                var colour = layer.Accent ? accent : stroke;
                var width = Math.Max(0.22, layer.StrokeWidth * scale);
                gfx.DrawPath(new XPen(colour.Alpha(layerOpacity), width)
                {
                    LineCap = XLineCap.Round,
                    LineJoin = XLineJoin.Round,
                }, path);
            }
        }
    }
```

**Where to look.**
`InannaRosette/src/libs/InannaRosette.Reading/Data/EmblemArt.cs`
`InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs` and
`Controls/Ornament.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfOrnaments.cs`

**Sharp edges.**
- Stroke widths are authored in design-box units, not in output units, so each
  renderer has to scale them itself. The document side takes the smaller of the
  two box-to-rectangle ratios and floors the result, because a hairline that
  rounds to zero disappears entirely in print while it still shows on screen.
- The document renderer catches a malformed-path failure per layer and skips
  that layer, so one bad glyph cannot take a whole report down. The screen
  renderer cannot do the same thing, because its parser rejects shapes the
  document parser accepts; it has its own fallback chain instead.
- Only the accent flag varies per card. Everything else about a motif is fixed
  by the data, which is what keeps the two renderings recognizably the same
  picture rather than two interpretations of one description.
- A layer with zero opacity is skipped before a path is built, not drawn
  invisibly. The order of that check matters when a caller multiplies a whole
  stack down by a group opacity.

### Convert SVG path data into a PDF graphics path

**When you want this.** Your artwork is SVG path strings and it has to reach a
PDF as real vector segments. This is a different job from
[Rasterize SVG art with the CodeBrix SkiaSvg library](BLUEPRINTS-GraphicsAndRendering.md#rasterize-svg-art-with-the-codebrix-skiasvg-library),
which renders whole SVG documents to pixels: here there is no SVG document, no
rasterizer and no extra library - just the path mini-language, parsed into the
document library's own path type so the marks stay vector at any print
resolution.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/SvgPathToPdf.cs
    /// <summary>
    /// Builds a path from <paramref name="pathData"/>, scaling the square design box
    /// <c>0..designBox</c> so that it fits centred inside <paramref name="destination"/>.
    /// </summary>
    public static XGraphicsPath Build(string pathData, XRect destination, double designBox = DefaultDesignBox)
    {
        var scale = Math.Min(destination.Width / designBox, destination.Height / designBox);
        var offsetX = destination.X + (destination.Width - designBox * scale) / 2.0;
        var offsetY = destination.Y + (destination.Height - designBox * scale) / 2.0;
        return Build(pathData, scale, scale, offsetX, offsetY);
    }

    /// <summary>Builds a path, mapping source point <c>(x, y)</c> to <c>(x * sx + dx, y * sy + dy)</c>.</summary>
    public static XGraphicsPath Build(string pathData, double sx, double sy, double dx, double dy)
    {
        var path = new XGraphicsPath { FillMode = XFillMode.Winding };
        var builder = new Builder(path, sx, sy, dx, dy);
        Parse(pathData, builder);
        return path;
    }
```

The parser is one loop over a scanner. A command letter is sticky, so a
letter followed by several coordinate sets repeats; a move-to that repeats
becomes a line-to, which is the one place the grammar changes the command under
you.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/SvgPathToPdf.cs
        while (true)
        {
            scanner.SkipSeparators();
            if (scanner.AtEnd) break;

            var c = scanner.Peek;
            if (IsCommand(c))
            {
                command = c;
                scanner.Advance();
            }
            else if (command == '\0')
            {
                throw new FormatException($"Path data must begin with a command; found '{c}'.");
            }
            else if (command is 'M') command = 'L';      // implicit line-to after a move-to
            else if (command is 'm') command = 'l';
            else if (command is 'Z' or 'z')
            {
                throw new FormatException("Unexpected number after a close-path command.");
            }

            var relative = char.IsLower(command);
            var upper = char.ToUpperInvariant(command);
// ...
        // Deliberately no implicit close: an un-terminated sub-path stays open, so stroked
        // figures are not silently joined back to their starting point. A fill closes it anyway.
```

Quadratic segments are raised to cubics exactly rather than approximated,
and an elliptical arc is converted by the endpoint-to-center parameterization
of the specification and emitted as pieces of ninety degrees or less.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/SvgPathToPdf.cs
    private static void EmitQuadratic(ISink sink, double x0, double y0, double qx, double qy, double x, double y)
    {
        // Exact degree elevation from quadratic to cubic.
        var c1X = x0 + 2.0 / 3.0 * (qx - x0);
        var c1Y = y0 + 2.0 / 3.0 * (qy - y0);
        var c2X = x + 2.0 / 3.0 * (qx - x);
        var c2Y = y + 2.0 / 3.0 * (qy - y);
        sink.CurveTo(c1X, c1Y, c2X, c2Y, x, y);
// ...
        // Emit in pieces of at most 90 degrees; the cubic approximation of a circular arc is
        // excellent below that and the error grows quickly above it.
        var pieces = Math.Max(1, (int)Math.Ceiling(Math.Abs(deltaTheta) / (Math.PI / 2) - 1e-9));
        var delta = deltaTheta / pieces;
        var t = 4.0 / 3.0 * Math.Tan(delta / 4.0);
```

Tokenizing is where hand-written parsers usually go wrong, so the scanner
is explicit about what separates one number from the next and about the two
arc flags, which the grammar allows to be written with nothing between them.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/SvgPathToPdf.cs
    /// <summary>
    /// Tokenises the path string. Numbers may be separated by whitespace, commas, or nothing at
    /// all where a sign or a second decimal point unambiguously begins the next number.
    /// </summary>
    private struct Scanner(string text)
    {
        private int _i = 0;

        public readonly bool AtEnd => _i >= text.Length;

        public readonly char Peek => text[_i];

        public void Advance() => _i++;

        public void SkipSeparators()
        {
            while (_i < text.Length && (char.IsWhiteSpace(text[_i]) || text[_i] is ',')) _i++;
        }
// ...
        /// <summary>
        /// Reads an arc flag. The grammar allows these to be written without separators
        /// (<c>"a5 5 0 1150 20"</c>), so exactly one character is consumed.
        /// </summary>
        public bool Flag()
        {
            SkipSeparators();
            if (_i >= text.Length) throw new FormatException("Path data ended while an arc flag was expected.");
            var c = text[_i];
            if (c is not ('0' or '1')) throw new FormatException($"Arc flag must be 0 or 1; found '{c}'.");
            _i++;
            return c == '1';
        }
```

**Where to look.**
`InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/SvgPathToPdf.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfOrnaments.cs`
`InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs`

**Sharp edges.**
- Numbers may run together where a sign or a second decimal point makes the
  break unambiguous. A scanner that splits on whitespace and commas alone
  mis-parses `"10-5"` and `".5.5"` silently, producing a plausible wrong shape
  rather than an error.
- The two arc flags are single characters, not numbers. Reading them with the
  number scanner swallows the coordinates that follow.
- An unterminated sub-path is deliberately left open. Closing it implicitly
  joins a stroked figure back to its start with a line nobody drew; a filled
  figure is closed by the fill anyway.
- After a close-path the current point returns to the start of the sub-path,
  and any following segment opens a fresh sub-path there - so the builder
  re-issues the move before it draws. Forget that and the next segment either
  vanishes or attaches to the wrong figure.
- A zero radius degenerates to a straight line and coincident endpoints omit
  the arc entirely. Both are in the specification and both are easy to leave
  out, and either one produces a division by zero if you do.

### Give a hand-placed document a page cursor with margins and breaks

**When you want this.** You are drawing a document straight onto a graphics
context, the way
[Compose a fixed layout poster with the CodeBrix PdfDocuments library](BLUEPRINTS-DocumentsAndData.md#compose-a-fixed-layout-poster-with-the-codebrix-pdfdocuments-library)
does, but it runs to many pages. The low-level API has no notion of a margin,
a cursor or a page break, and you want those three things without giving up
hand placement for a flowing document model.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfPageFlow.cs
    public PdfPageFlow(PdfDocument document, PageSize pageSize, string headerText)
    {
        _document = document;
        _pageSize = pageSize;
        _headerText = headerText;

        // Page metrics are fixed by the page size; take them from a throwaway probe so that
        // callers can plan a layout before the first page exists.
        var probe = new PdfPage { Size = pageSize, Orientation = PageOrientation.Portrait };
        PageWidth = probe.Width.Point;
        PageHeight = probe.Height.Point;
    }
```

The cursor is one settable number and everything else is derived from it,
so a caller can ask how much room is left before it decides what to draw.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfPageFlow.cs
    public double ContentLeft => MarginLeft;
    public double ContentRight => PageWidth - MarginRight;
    public double ContentWidth => PageWidth - MarginLeft - MarginRight;
    public double ContentTop => MarginTop;
    public double ContentBottom => PageHeight - MarginBottom;

    /// <summary>The vertical cursor, in points from the top of the page.</summary>
    public double Y { get; set; }

    /// <summary>Space left on the current page below the cursor.</summary>
    public double Remaining => ContentBottom - Y;

    /// <summary>The graphics context of the current page.</summary>
    public XGraphics Gfx => _gfx ?? throw new InvalidOperationException("No page has been started.");

    /// <summary>1-based number of the current page.</summary>
    public int PageNumber { get; private set; }
```

Starting a page is also where the page's furniture goes on: the parchment
ground, the running header with its rule and closing star, and the centered
folio. The cover opts out.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfPageFlow.cs
    /// <summary>
    /// Closes the current page and starts a new one. Decorated pages get the parchment ground,
    /// the running header and the centred page number; the cover does not.
    /// </summary>
    public XGraphics BeginPage(bool decorate = true)
    {
        _gfx?.Dispose();

        var page = _document.AddPage();
        page.Size = _pageSize;
        page.Orientation = PageOrientation.Portrait;
        PageNumber = _document.PageCount;

        _gfx = XGraphics.FromPdfPage(page);
        _gfx.DrawRectangle(new XSolidBrush(PdfPalette.Ivory), new XRect(0, 0, PageWidth, PageHeight));

        if (decorate)
        {
            DrawRunningHeader(_gfx);
            DrawFooter(_gfx);
        }

        Y = ContentTop;
        return _gfx;
    }

    /// <summary>Starts a new page if <paramref name="needed"/> points would not fit below the cursor.</summary>
    /// <returns><c>true</c> when a page break happened.</returns>
    public bool EnsureSpace(double needed)
    {
        if (_gfx is null)
        {
            BeginPage();
            return true;
        }

        if (Y + needed <= ContentBottom) return false;
        BeginPage();
        return true;
    }
```

The whole report is then a sequence of section methods over one flow.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/PdfReportBuilder.cs
        using var flow = new PdfPageFlow(document, _options.UseLetter ? PageSize.Letter : PageSize.A4, header)
        {
            MarginLeft = 62,
            MarginRight = 62,
            MarginTop = 84,
            MarginBottom = 66,
        };

        DrawCover(flow, interpretation, querent, date);
        DrawSpreadPage(flow, interpretation);
        DrawOpeningAndInsights(flow, interpretation);
        DrawStations(flow, interpretation);
        DrawCounsel(flow, interpretation);
        DrawClosingBlessing(flow, interpretation);
        DrawAppendix(flow, interpretation);
```

**Where to look.**
`InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfPageFlow.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Services/PdfReportBuilder.cs`
(`DrawFlowedParagraph` breaks one paragraph across pages line by line, leaving
at least two lines behind and carrying at least three forward)

**Sharp edges.**
- The page metrics come from a throwaway probe page that is never added to the
  document. Adding a real page just to read its size leaves a blank sheet in
  the output that nothing explains.
- The graphics property throws until a page has been started, which is
  deliberate: the first space check opens the first page rather than silently
  drawing nowhere.
- Advancing the cursor clamps at the bottom of the text area. An over-long
  advance therefore parks the cursor at the margin instead of pushing it off
  the page, where drawing would continue invisibly below the paper.
- The flow owns the graphics context and disposes the previous one each time it
  starts a page, so nothing may draw onto a page after the next one begins.
  Any code holding a saved reference to the old context is drawing into a
  finished page.
- Decoration is a flag on the page start, not a property of the flow. A cover
  with a running header and a folio reading one is the failure this prevents.

### Typeset tracked capitals justified copy and fitted lines by hand

**When you want this.** The drawing API gives you one call that puts a string
at a point. A designed report wants letter-spaced small capitals, justified
body copy whose last line stays ragged, headings that shrink until they fit
their column, and - above all - a way to know how tall a block will be before
committing to it.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfText.cs
    /// <summary>Width of <paramref name="text"/> when drawn with the given letter spacing.</summary>
    public static double MeasureTracked(XGraphics gfx, string text, XFont font, double tracking)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        double width = 0;
        foreach (var ch in text) width += gfx.MeasureString(ch.ToString(), font).Width + tracking;
        return width - tracking;
    }

    /// <summary>
    /// Draws letter-spaced text with its baseline at <paramref name="baselineY"/>.
    /// Returns the width consumed.
    /// </summary>
    public static double DrawTracked(
        XGraphics gfx, string text, XFont font, XBrush brush,
        double x, double baselineY, double tracking, TrackedAlign align = TrackedAlign.Left, double boxWidth = 0)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var width = MeasureTracked(gfx, text, font, tracking);
        var penX = align switch
        {
            TrackedAlign.Center => x + (boxWidth - width) / 2.0,
            TrackedAlign.Right => x + boxWidth - width,
            _ => x,
        };

        foreach (var ch in text)
        {
            var s = ch.ToString();
            gfx.DrawString(s, font, brush, new XPoint(penX, baselineY));
            penX += gfx.MeasureString(s, font).Width + tracking;
        }

        return width;
    }
```

Justification is a second pass over the wrapped lines: measure the words,
share the slack between the gaps, and leave the real last line alone.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfText.cs
    /// <summary>
    /// Draws a justified paragraph with the last line left aligned - which the built-in
    /// <c>XTextFormatter</c>'s Justify mode does not do. Returns the height consumed.
    /// </summary>
    public static double DrawJustified(
        XGraphics gfx, string text, XFont font, XBrush brush,
        double x, double top, double width, double lineHeight)
        => DrawJustifiedCore(gfx, text, font, brush, x, top, width, lineHeight, justifyLastLine: false);
// ...
        for (var i = 0; i < lines.Count; i++)
        {
            var words = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var isLast = !justifyLastLine && i == lines.Count - 1;

            if (isLast || words.Length < 2)
            {
                gfx.DrawString(lines[i], font, brush, new XPoint(x, baseline));
            }
            else
            {
                var wordsWidth = words.Sum(w => gfx.MeasureString(w, font).Width);
                var gap = (width - wordsWidth) / (words.Length - 1);

                // Guard against a pathologically short line being stretched into a ladder.
                var naturalGap = gfx.MeasureString(" ", font).Width;
                if (gap > naturalGap * 4.5)
                {
                    gfx.DrawString(lines[i], font, brush, new XPoint(x, baseline));
                }
                else
                {
                    var penX = x;
                    foreach (var w in words)
                    {
                        gfx.DrawString(w, font, brush, new XPoint(penX, baseline));
                        penX += gfx.MeasureString(w, font).Width + gap;
                    }
                }
            }

            baseline += lineHeight;
        }
```

Fitting is a bounded shrink loop rather than a solve, which is both simpler
and safe against a string that can never fit.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfText.cs
    /// <summary>
    /// Draws a single line, shrinking the point size until it fits <paramref name="width"/>.
    /// Returns the font actually used.
    /// </summary>
    public static XFont DrawFitted(
        XGraphics gfx, string text, string family, double emSize, XFontStyle style, XBrush brush,
        double x, double baselineY, double width, TrackedAlign align = TrackedAlign.Left)
    {
        var size = emSize;
        var font = new XFont(family, size, style);
        for (var guard = 0; guard < 20 && gfx.MeasureString(text, font).Width > width; guard++)
        {
            size *= 0.94;
            font = new XFont(family, size, style);
        }
```

Measuring without drawing is what lets the report decide where a page break
belongs. The station sections keep a kicker, a heading, a keyword line and the
first few lines of prose together by measuring that block first and asking the
flow for the room.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/PdfReportBuilder.cs
            var firstParagraph = paragraphs.Count > 0 ? paragraphs[0] : string.Empty;

            // Keep the kicker, heading, keyword line and the first two lines of prose together.
            var headBlock = 14 + 22 + 14 + Math.Min(
                PdfText.MeasureParagraph(flow.Gfx, firstParagraph, bodyFont, textWidth, BodyLeading),
                BodyLeading * 3);
// ...
            first = false;
            flow.EnsureSpace(headBlock);
```

**Where to look.**
`InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfText.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Services/PdfReportBuilder.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfPageFlow.cs`
(the running header and the folio are both tracked text)

**Sharp edges.**
- Tracked text is measured a character at a time and the trailing gap is
  subtracted at the end. Measure the whole string at once and centering is off
  by the accumulated spacing, which grows with the length of the line.
- The justifier leaves the final line ragged, which the built-in formatter's
  justify mode does not. When a paragraph is split across a page break the last
  line on this page is not really the last line, so there is a second entry
  point that justifies it too.
- A short line with few words would be stretched into a ladder of white. The
  guard compares the computed gap against the natural space width and falls
  back to plain left alignment rather than producing something unreadable.
- Every fitting loop carries a guard count. Without one, a string that can
  never fit its column spins forever at run time rather than merely looking
  wrong.
- Measuring a paragraph wraps it, and drawing it wraps it again. That is cheap
  at report sizes and it keeps the two calls independent, but it is real work
  repeated - do not put it inside a per-glyph loop.

### Blend a tint to opaque because gradient stops ignore alpha

**When you want this.** You want a soft wash behind a title, or a medallion
that fades from a card's own color into the dark, and the transparent gradient
stops you wrote came out as flat, fully opaque bands. The renderer honors alpha
on a solid brush and on a pen, and ignores it on a gradient stop.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfPalette.cs
    /// <summary>Same colour with an explicit alpha (0..1). Safe for solid brushes and pens.</summary>
    public static XColor Alpha(this XColor color, double alpha)
    {
        var a = (int)Math.Round(Math.Clamp(alpha, 0, 1) * 255);
        return XColor.FromArgb(a, (int)color.R, (int)color.G, (int)color.B);
    }

    /// <summary>
    /// Opaque blend of <paramref name="fore"/> over <paramref name="back"/>.
    /// Used for tinted text and for gradient stops, whose alpha channel the renderer ignores.
    /// </summary>
    public static XColor Over(this XColor fore, XColor back, double amount)
    {
        var t = Math.Clamp(amount, 0, 1);
        return XColor.FromArgb(
            (int)Math.Round(back.R + (fore.R - back.R) * t),
            (int)Math.Round(back.G + (fore.G - back.G) * t),
            (int)Math.Round(back.B + (fore.B - back.B) * t));
    }
```

Every stop is therefore written as a blend that is already opaque: the
tint mixed into the ground it will sit on, at the strength the stop wanted.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/PdfReportBuilder.cs
        // A radial dusk behind the star. Gradient stops ignore alpha, so every stop is opaque.
        var dusk = new XShadingBrush(starCenter, 0, starCenter, bandHeight * 0.95,
        [
            new XGradientStop(0.00, PdfPalette.Lapis.Over(PdfPalette.Night2, 0.55)),
            new XGradientStop(0.38, PdfPalette.Lapis.Over(PdfPalette.Night, 0.28)),
            new XGradientStop(0.78, PdfPalette.Night),
            new XGradientStop(1.00, PdfPalette.Kohl),
        ]);
        gfx.DrawRectangle(dusk, new XRect(0, 0, pw, bandHeight));
```

A related refusal in the same layer: the rounded-rectangle call does not
honor a shading brush at all. The fix is to lay a solid ground in the rounded
shape, then paint the gradient through a rounded-rectangle clip with the plain
rectangle call, which does honor it.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfCardPainter.cs
        // Body: Night2 with a soft lapis glow falling from the top edge.
        // DrawRoundedRectangle does not honour a shading brush, so lay a solid ground first and
        // paint the gradient through a rounded-rectangle clip, which DrawRectangle does honour.
        gfx.DrawRoundedRectangle(new XSolidBrush(PdfPalette.Night2), rect, ellipse);

        var glowCenter = new XPoint(cx, rect.Y + h * 0.06);
        var body = new XShadingBrush(glowCenter, 0, glowCenter, h * 0.98,
        [
            new XGradientStop(0.0, PdfPalette.LapisLight.Over(PdfPalette.Night2, 0.52)),
            new XGradientStop(0.40, PdfPalette.Lapis.Over(PdfPalette.Night2, 0.26)),
            new XGradientStop(1.0, PdfPalette.Night),
        ]);

        var clipState = gfx.Save();
        var clip = new XGraphicsPath();
        clip.AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, corner * 2, corner * 2);
        gfx.IntersectClip(clip);
        gfx.DrawRectangle(body, rect);
        gfx.Restore(clipState);
```

**Where to look.**
`InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfPalette.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Services/Pdf/PdfCardPainter.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Services/PdfReportBuilder.cs`

**Sharp edges.**
- The two helpers are not interchangeable and the difference is invisible at
  the call site. The alpha helper is correct for a solid brush or a pen and
  wrong for a gradient stop; the blend helper is correct for a stop and wrong
  wherever something really does have to show through.
- The blend needs the color it will actually sit on. Blending against the page
  ground and then drawing over a dark panel gives a band that is too light, and
  nothing in the type system catches it.
- The same palette is written three times in three type systems - the
  application resource dictionary, the page's ornament constants and this
  file - because the library has no UI reference and must still match the
  screen. Changing one and not the other two is a silent divergence between
  what the person sees and what they print.
- A card's own accent color comes from data and may be very dark. The palette
  lifts such a color toward white until it reads against the deep card body,
  rather than trusting the data to be sensible.

### Save only identity and rehydrate the text from the data tables

**When you want this.** Your document is a selection out of a body of content
you already ship - which cards, which slots, which way up - and you are about
to write the content into the saved file alongside it. Do not. Save the
identity and look the content up again when the file is opened.

**The MVVM shape.** The serializer lives in the UI-free library behind an
interface the view model resolves, and both directions go through a plain
storage record that is not the domain type. The view model turns its own table
into the library's reading type to save, and lays a reading that comes back
into its own table, telling the page about each card as it arrives.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/ReadingSerializer.cs
/// <summary>
/// Saves and loads a <see cref="RosetteReading"/> as JSON. Only the identifying data is stored
/// (card ids, station indexes, orientation, querent, question, timestamp); the card text
/// and the spread text are rehydrated from <see cref="DeckData"/> and <see cref="RosetteSpread"/>.
/// </summary>
public sealed class ReadingSerializer : IReadingSerializer
{
    /// <summary>The document actually written to disk. Public so callers may inspect or build one.</summary>
    public sealed record ReadingDocument
    {
        /// <summary>The document-format version; 1 is the only one written so far.</summary>
        public int Version { get; init; } = 1;

        /// <summary>When the reading was laid.</summary>
        public DateTime Created { get; init; }

        /// <summary>Who the reading is for; may be empty.</summary>
        public string Querent { get; init; } = "";

        /// <summary>The question put to the rosette; may be empty.</summary>
        public string Question { get; init; } = "";

        /// <summary>The cards on the rosette, ordered by station.</summary>
        public List<PlacementDocument> Placements { get; init; } = [];
    }

    /// <summary>One card at one station, in storage form.</summary>
    public sealed record PlacementDocument
    {
        /// <summary>The station the card sits on: 0 is the Heart, 1-8 the petals.</summary>
        public int Position { get; init; }

        /// <summary>The card's identifier in the deck.</summary>
        public int CardId { get; init; }

        /// <summary>Whether the card was laid reversed.</summary>
        public bool Reversed { get; init; }
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
```

Reading back is tolerant on purpose, and the doc comment says so. An
unknown identifier or an out-of-range slot is skipped, a second card on a slot
that is already filled is ignored, and only text that is not the document at
all is an error.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/ReadingSerializer.cs
        if (doc is null) throw new FormatException("The text is not a valid reading document.");

        var reading = new RosetteReading
        {
            Created = doc.Created == default ? DateTime.Now : doc.Created,
            Querent = doc.Querent,
            Question = doc.Question,
        };

        foreach (var p in doc.Placements.OrderBy(p => p.Position))
        {
            var position = RosetteSpread.At(p.Position);
            var card = DeckData.ById(p.CardId);
            if (position is null || card is null) continue;
            if (reading.At(p.Position) is not null) continue;
            reading.Placements.Add(new PlacedCard(position, card, p.Reversed));
        }

        return reading;
    }
```

The view model's side is where a loaded reading becomes a table again.

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.Core/ViewModels/MainViewModel.cs
    //Lays a reading that was read back from a file: the table starts empty and each placement
    //  arrives already on its station, so the page places it rather than dealing it to the tray
    private void ApplyReading(RosetteReading reading)
    {
        GatherToDeck();

        Querent = reading.Querent ?? string.Empty;
        Question = reading.Question ?? string.Empty;

        var laid = 0;
        foreach (var placement in reading.Placements)
        {
            var index = placement.Position.Index;
            if (index < 0 || index >= StationCount || _stations[index] != null) { continue; }

            //A card on the table is never also in the stack, so the deck hands it over rather
            //  than keeping a second copy of it; a file naming the same card twice is refused
            //  here rather than being drawn again later
            if (!_deck.Remove(placement.Card)) { continue; }

            var card = new ReadingCard(placement.Card)
            {
                IsReversed = placement.IsReversed,
                Station = index,
            };
            _cards.Add(card);
            _stations[index] = card;
            laid++;
            CardAdded?.Invoke(card);
        }

        //Whatever was not laid is the deck now, and a reading just opened deserves a fresh order
        _deck.Shuffle();

        RefreshCounts(refreshGuidance: false);
        SetStatus($"Opened a reading with {laid} cards laid; {DeckRemaining} cards remain in the deck.");
```

**Where to look.**
`InannaRosette/src/libs/InannaRosette.Reading/Services/ReadingSerializer.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Data/DeckData.cs` and
`Data/RosetteSpread.cs`
`InannaRosette/src/InannaRosette.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The card identifier is a dense one-based index into the deck, so the deck's
  *order* is now part of the file format. Inserting a card in the middle
  silently re-points every reading anyone has saved. Either never reorder, or
  give the cards stable keys before the first file is written.
- The version field is written and not yet read. That is a hook, not a
  mechanism, and it is worth saying so out loud: a reader that ignores the
  version cannot refuse a future document it does not understand.
- A missing timestamp falls back to now, so a hand-written file with no
  timestamp still opens instead of dating itself to the year zero.
- Rehydrating means a saved reading picks up later corrections to the content
  for free - and equally, that a correction changes what an old file says. That
  is the right trade for lore and the wrong one for a receipt.
- Because each placement arrives with its slot already set, the page's
  card-added handler has to read the card's slot rather than assume a new card
  goes to the tray. Missing that is invisible until someone opens a file.

### Make generated prose deterministic from a seed the data makes

**When you want this.** Your application writes prose - a summary, a reading,
a report narrative - and you want it to vary so it does not read like a form
letter, while still being the same text every time for the same inputs, so a
test can assert on an exact sentence.

**The MVVM shape.** The generator is a pure service in the UI-free library
behind the interface the view model resolves. It takes the model and returns a
record, holds no state between calls, and reaches for no clock and no random
source. The view model runs it on a worker and marshals the result back.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/ReadingInterpreter.cs
    /// <inheritdoc />
    public ReadingInterpretation Interpret(RosetteReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        var placed = reading.Placements
            .Where(p => p is not null)
            .GroupBy(p => p.Position.Index)
            .Select(g => g.First())
            .OrderBy(p => p.Position.Index)
            .ToList();

        var seed = SeedFor(placed);
        var title = BuildTitle(reading);

        if (placed.Count == 0)
        {
            return new ReadingInterpretation(
                reading, title, EmptyOpening, [], EmptyInsights(), EmptyCounsel, EmptyClosing)
            {
                Closing = ClosingBlessing,
            };
        }
```

The seed is a hash of the placements themselves, so turning one card over
changes every sentence the seed reaches.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/ReadingInterpreter.cs
    private static int SeedFor(IReadOnlyList<PlacedCard> placed)
    {
        unchecked
        {
            var h = 2166136261u;
            foreach (var p in placed)
            {
                h = (h ^ (uint)p.Position.Index) * 16777619u;
                h = (h ^ (uint)p.Card.Id) * 16777619u;
                h = (h ^ (uint)(p.IsReversed ? 7 : 3)) * 16777619u;
            }
            return (int)(h & 0x7FFFFFFF);
        }
```

The sentence stock is a separate internal class, reached through a
`using static` at the top of the generator. Each slot in the output mixes the
one seed with its own salt, so two slots choose independently rather than in
lockstep.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Services/InterpretationTemplates.cs
/// <summary>
/// The sentence stock the <see cref="ReadingInterpreter"/> draws on. Every slot offers
/// several variants, chosen by a seed derived from the cards themselves, so that two
/// different layouts do not read alike while the same layout always reads the same.
/// </summary>
internal static class InterpretationTemplates
{
// ...
    /// <summary>Scrambles a seed with a salt so that different slots make independent choices.</summary>
    public static int Mix(int seed, int salt)
    {
        unchecked
        {
            var h = (uint)seed * 2654435761u ^ (uint)(salt * 40503 + 0x9E37);
            h ^= h >> 15;
            h *= 2246822519u;
            h ^= h >> 13;
            h *= 3266489917u;
            h ^= h >> 16;
            return (int)(h & 0x7FFFFFFF);
        }
    }

    /// <summary>Picks one of the variants deterministically.</summary>
    public static T Pick<T>(IReadOnlyList<T> variants, int seed, int salt) =>
        variants[Mix(seed, salt) % variants.Count];
```

**Where to look.**
`InannaRosette/src/libs/InannaRosette.Reading/Services/ReadingInterpreter.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Services/InterpretationTemplates.cs`
`InannaRosette/src/libs/InannaRosette.Reading/InternalsVisibleTo.cs`

**Sharp edges.**
- The seed comes from the data and nothing else. Seed it from a clock, a
  process-wide random source or a file path and every test that asserts on the
  prose becomes a test that asserts the call did not throw.
- Every slot needs its own salt. Two slots sharing a salt pick the same index
  out of their variant lists forever, which reads as a tic rather than as a
  coincidence.
- The input is de-duplicated by slot before it is hashed, because the model may
  have come from a file rather than from the screen. Hash the raw list and two
  files describing the same layout produce different prose.
- The empty case is a documented shape rather than an error, and a partial one
  is a real result. Deciding that up front is much easier than retrofitting it
  once callers expect an exception.
- The chooser indexes with a remainder, so a variant list must never be empty.
  An empty list is a divide-by-zero at run time, in a code path a rare seed may
  be the first to reach.
- The templates are internal and visible to the test project only through the
  assembly attribute, which keeps a large body of sentence data off the
  library's public surface without putting it out of reach of the tests.

### Build a static data table lazily because field initializers run in order

**When you want this.** A static class holds a large content table assembled
from several static arrays declared further down the same file, and the
assembled list comes out empty or throws with nothing obviously wrong.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Data/DeckData.cs
public static class DeckData
{
    /// <summary>Terse list builder so the card literals below stay readable.</summary>
    private static string[] L(params string[] items) => items;

    // Built on first access: the suit arrays below are static field initializers, which run
    // in textual order, so the list cannot be assembled in a field initializer up here.
    private static IReadOnlyList<Card>? _cards;

    /// <summary>All forty cards, ordered by Id (1..40): Goddess 1..24, Gate 1..8, Emblem 1..8.</summary>
    public static IReadOnlyList<Card> Cards => _cards ??= Build();

    /// <summary>Look up a card by its Id, or null when no such card exists.</summary>
    public static Card? ById(int id) => id >= 1 && id <= Cards.Count ? Cards[id - 1] : null;

    /// <summary>All cards of one suit, in Number order.</summary>
    public static IReadOnlyList<Card> BySuit(CardSuit suit) => Cards.Where(c => c.Suit == suit).ToList();
```

Two more habits from the same folder are worth copying. Relationships that
are arithmetic on an index are written as arithmetic, not as a lookup table
somebody can get out of step with the titles.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Models/Spread.cs
/// <summary>
/// One of the nine stations of the Rosette spread: the heart (index 0) and eight petals
/// (index 1..8, starting at the top and proceeding clockwise, 45 degrees apart).
/// </summary>
public sealed record SpreadPosition(
    int Index,
    string Title,
    string Subtitle,
    string Question,
    double AngleDegrees,
    string Description)
{
    /// <summary>True for the Heart at the middle of the flower, false for the eight petals.</summary>
    public bool IsCenter => Index == 0;

    /// <summary>The petal directly opposite this one on the rosette (petals only), or null for the center.</summary>
    public int? OppositeIndex => Index == 0 ? null : ((Index - 1 + 4) % 8) + 1;
}
```

And where a derived grouping is read by more than one caller - the
generator, the report and the tests - it is exposed once, from the data, rather
than rebuilt at each call site.

```csharp
// From CodeBrix.Samples/InannaRosette/src/libs/InannaRosette.Reading/Data/RosetteSpread.cs
    /// <summary>The four opposite petal pairs of the rosette: (1,5), (2,6), (3,7), (4,8).</summary>
    public static IReadOnlyList<(SpreadPosition A, SpreadPosition B)> Axes { get; } =
    [
        (_positions[1], _positions[5]),
        (_positions[2], _positions[6]),
        (_positions[3], _positions[7]),
        (_positions[4], _positions[8]),
    ];
```

**Where to look.**
`InannaRosette/src/libs/InannaRosette.Reading/Data/DeckData.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Models/Spread.cs`
`InannaRosette/src/libs/InannaRosette.Reading/Data/RosetteSpread.cs`

**Sharp edges.**
- Static field initializers run in textual order. A list assembled in a field
  initializer above the arrays it concatenates sees nulls, and that is not a
  compile error - it is an empty table or a null reference at first use. The
  comment saying why the lazy form is there is what stops somebody tidying it
  back.
- The lazy assignment is idempotent rather than synchronized: two threads
  racing build the table twice and one wins. That is harmless because the build
  is pure, and it would not be if construction had a side effect such as
  registering something.
- The lookup indexes straight into the list on the assumption that identifiers
  are a dense one-based run. That assumption is a contract the saved-file
  format depends on, so it belongs in a test rather than in a comment.
- Deriving the opposite index arithmetically only works while the ordering
  convention holds. That convention - the center first, then clockwise from the
  top - is the one thing both the page geometry and the generated prose
  silently assume, which is why it is asserted by name in the tests rather than
  left to the reader.
