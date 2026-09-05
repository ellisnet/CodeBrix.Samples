# GitHubIssueFinder

GitHubIssueFinder is a desktop application for finding the issues and pull requests
nobody has picked up. The user types a GitHub user or organization into the owner box
and the application lists every open issue and pull request across that owner's public
repositories that has no assignee, grouped by repository, newest activity first. Typing
a login into the assignee box instead turns the same list into everything that one
person is carrying. A checkbox widens either search to closed items as well. Every row
is a link: clicking one opens that issue or pull request in the host's default browser,
and clicking a repository header opens the repository.

The application talks to GitHub without an account, which is what makes it interesting
to build. An unauthenticated caller gets a small allowance - a handful of search calls
a minute and a modest number of other calls an hour - and a search for a busy
organization needs far more calls than that. So the application paces itself: it holds
itself one call below each published ceiling, it reads the allowance back out of every
response's headers, and when it has to wait it says so, counting the seconds down on the
status line while the rest of the window stays live. Two pills in the status bar show
what is left of each pool at all times. A search of a large organization therefore takes
minutes rather than seconds, and the whole point of the interface is that those minutes
are legible: results arrive a page at a time, the counts tick up, and Cancel is always
there.

As a reference, this is the sample for a paced, long-running network read whose results
stream into a grouped list. It shows an `IAsyncEnumerable` of pages folded into bound
collections on the UI thread, a rate limiter driven by response headers with a clock seam
that lets its tests run an hour in a millisecond, progress and a visible wait reported
through one channel, a two-level list built from group and row view models, five color
schemes painted by mutating keyed brushes in place, and a "System default" scheme that
follows the operating system until the user overrides it.

## What this sample shows a CodeBrix.Platform developer

- **A long read that streams.** `IGitHubIssueSearchService.SearchAsync` returns
  `IAsyncEnumerable<IssueSearchPage>`. The view model enumerates it with
  `await foreach` under `ConfigureAwait(false)` and marshals every page into the bound
  collections with `InvokeOnMainThread`, so rows appear as they arrive and the window
  never stops responding.
- **Cancellation that is always available.** Cancel sits beside Search, disabled when
  there is nothing to cancel and enabled the instant a search starts, and Escape does
  the same from anywhere on the page. A cancelled search reports how far it got.
- **A rate limiter that reads its budget from the server.** `RateLimitThrottle` treats
  the `X-RateLimit-*` headers of the last response as the authority, keeps a sliding
  count of its own calls as the guard before the first response, and waits when either
  says it must. It takes a `TimeProvider`, so its tests exercise real waits with no real
  time passing.
- **A wait the user can see.** The waiting throttle reports itself once a second through
  the same `IProgress<SearchProgress>` channel the pages use, so the status line counts
  down, its glyph and color change, and the search-quota pill warms to the attention
  color, all without a second mechanism.
- **A fallback plan for a capped API.** GitHub's search endpoint serves at most a
  thousand results for any one query. When the first page reports a larger total, the
  service abandons the whole-owner query, lists the owner's repositories through a
  different API, and searches them one at a time - drawing on two independent quota
  pools, each with its own throttle.
- **A grouped list from two view models.** An outer `ListView` of repository groups,
  each carrying an inner `ItemsControl` of rows, with everything a row draws worked out
  once when the row is built. Neither the group nor the row holds a reference to its
  owner: opening a page in the browser is a delegate they were handed.
- **Five color schemes over one set of brushes.** Every color the application draws is
  a keyed `SolidColorBrush` declared at application level. Switching a scheme assigns a
  new `Color` to each of those brushes in place, which repaints every consumer - the
  page, the stock control chrome and the rows already realized in the list - without
  rebuilding anything or losing scroll position.
- **A scheme that follows the operating system.** "System default" resolves to the light
  or dark palette from the operating system's own preference and follows it live through
  `UISettings.ColorValuesChanged`. Choosing any other scheme overrides the operating
  system completely.
- **Color arithmetic as a tested helper.** Label pills wear each label's own color from
  GitHub, blended over the current page ground and pushed far enough away from it to stay
  visible. The arithmetic is plain numbers in the library, with no drawing types in sight,
  and it is unit tested.
- **A settings facade.** One small library wraps the settings store and is the only
  project in the application that references that add-in. The owner, the assignee, the
  closed-items switch and the chosen scheme survive a restart.

## Building, running and testing

There is one solution, `GitHubIssueFinder.slnx`, and it opens on Linux, macOS and
Windows. It contains the shared UI project, the Core project, the four head projects, a
`Libraries` solution folder for the two `src/libs` projects and a `Tests` solution folder
for their test projects.

| Head project | Platform |
| --- | --- |
| `src/GitHubIssueFinder.LinuxX11` | Linux, X11 session |
| `src/GitHubIssueFinder.LinuxWayland` | Linux, Wayland compositor |
| `src/GitHubIssueFinder.MacOS` | macOS |
| `src/GitHubIssueFinder.Win32Skia` | Windows, native Win32 window |

All four target `net10.0` and all four build on any of the three operating systems; each
runs only on the platform it is named for.

Prerequisites:

- The .NET 10 SDK. No workloads.
- A session appropriate to the head: an X11 display, a Wayland compositor, macOS, or
  Windows.
- Network access to GitHub. Nothing else: there is no account, no token and no
  configuration file to fill in. The application reads public data as an anonymous
  caller.

Build everything from this application folder:

```text
dotnet build GitHubIssueFinder.slnx
```

Run one head:

```text
dotnet run --project src/GitHubIssueFinder.LinuxX11/GitHubIssueFinder.LinuxX11.csproj
dotnet run --project src/GitHubIssueFinder.LinuxWayland/GitHubIssueFinder.LinuxWayland.csproj
dotnet run --project src/GitHubIssueFinder.MacOS/GitHubIssueFinder.MacOS.csproj
dotnet run --project src/GitHubIssueFinder.Win32Skia/GitHubIssueFinder.Win32Skia.csproj
```

`global.json` in this folder contains nothing but a test-runner selection - it sets the
runner to `Microsoft.Testing.Platform`. Both test projects are self-executing binaries
built as `Exe`, so a plain `dotnet test` on the solution can report that no tests were
found. Build the solution and run each test binary directly instead:

```text
dotnet build GitHubIssueFinder.slnx
./tests/libs/GitHubIssueFinder.GitHub.Tests/bin/Debug/net10.0/GitHubIssueFinder.GitHub.Tests
./tests/libs/GitHubIssueFinder.Settings.Tests/bin/Debug/net10.0/GitHubIssueFinder.Settings.Tests
```

The tests need nothing but a filesystem: no display, no network. Every HTTP call in them
is answered by a stub handler from saved GitHub responses that live beside the test
binary, and every wait runs on a fake clock.

One class is the exception. `GitHubLiveApiTests` calls the real api.github.com and is
skipped unless an environment variable asks for it:

```text
GITHUBISSUEFINDER_RUN_LIVE_TESTS=1 ./tests/libs/GitHubIssueFinder.GitHub.Tests/bin/Debug/net10.0/GitHubIssueFinder.GitHub.Tests
```

Those cases search one small owner, make loose assertions, and exist to prove that the
query strings, the request headers and the response parsing still match the live service.
They spend a few calls of the anonymous allowance when they run.

### Where the remembered settings live

The owner, the assignee, the closed-items switch and the chosen color scheme are written
to a per-user settings store the moment they change or a search starts. The store is a
single file in the CodeBrix per-application configuration folder - on Linux
`~/.config/CodeBrix/GitHubIssueFinder/settings/settings.sqlite`, and the equivalent
per-user application-data location on Windows and macOS. Deleting that folder resets the
application to its defaults, which are an empty owner, an empty assignee, closed items
excluded, and the "System default" color scheme.

## How the projects and folders are organized

```text
GitHubIssueFinder/
  GitHubIssueFinder.slnx                  The one solution; opens on Linux, macOS and Windows
  global.json                             Selects the Microsoft.Testing.Platform test runner
  THIRD-PARTY-NOTICES.txt                 Attribution record for this application
  src/
    GitHubIssueFinder.UI/                 Shared project: App.xaml(.cs) and Views/MainPage.xaml(.cs)
    GitHubIssueFinder.Core/               The library every head references; carries the packages
      Helpers/                            HostHelper, the generic-host provider
      Theming/                            The scheme table, the role list, the brush map, the glyphs
      ViewModels/                         MainViewModel, the group, row, label and picker view models
    GitHubIssueFinder.LinuxX11/           Head: Program.cs plus one runtime package
    GitHubIssueFinder.LinuxWayland/       Head: Program.cs plus one runtime package
    GitHubIssueFinder.MacOS/              Head: Program.cs plus one runtime package
    GitHubIssueFinder.Win32Skia/          Head: Program.cs plus one runtime package
    libs/
      GitHubIssueFinder.GitHub/           The whole GitHub conversation; no UI types at all
        Models/                           The public shapes: request, item, label, page, progress, snapshot
        Search/                           The service, the query builder, the throttle, the options, the exception
        Serialization/                    Source-generated JSON, the wire types, and the mapper to the models
        Helpers/                          LabelColorMath and RelativeTime, both pure
        DependencyInjection/              The AddGitHubIssueSearch() registration extension
      GitHubIssueFinder.Settings/         SettingsService and LoggingService facades over the settings add-in
  tests/
    libs/
      GitHubIssueFinder.GitHub.Tests/     Mirrors the GitHub library
        Fixtures/                         Saved GitHub responses, copied beside the binary
        TestDoubles/                      Stub handler, fake clock, recording progress, fixture loader
      GitHubIssueFinder.Settings.Tests/   Mirrors the Settings library; drives the store in a throwaway folder
```

Dependencies point one way. Each head project references `GitHubIssueFinder.Core` by
project reference and file-links the shared UI with
`<Import Project="..\GitHubIssueFinder.UI\GitHubIssueFinder.UI.projitems" Label="Shared" />`,
so `App.xaml` and `MainPage.xaml` compile into every head rather than into an assembly of
their own. Core project-references both `src/libs` libraries and carries every package
that is not a head runtime; each head adds exactly one runtime package for its own
platform. The two libraries reference neither Core nor each other:
`GitHubIssueFinder.GitHub` knows nothing about UI and takes one package beyond the
framework, and `GitHubIssueFinder.Settings` only wraps the settings add-in. Each test
project references only the one library it mirrors.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The framework itself: `Application`, `Window`, `Frame`, `Page`, the controls the page is built from, and the Simple MVVM toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`) | `src/GitHubIssueFinder.Core/GitHubIssueFinder.Core.csproj` |
| CodeBrix.Platform runtime for each head | The per-platform runtime; exactly one package per head project | the four `src/GitHubIssueFinder.<Head>/*.csproj` |
| CodeBrix.Platform Roboto font package | Supplies Roboto as the default text font, with the Noto Sans faces as fallbacks | `src/GitHubIssueFinder.UI/App.xaml.cs` |
| CodeBrix.Platform.FlexPanel add-in | The header bar that wraps its scheme picker under the identity block on a narrow window, the search row that wraps the same way, and the label pills that flow after a row title | `src/GitHubIssueFinder.UI/Views/MainPage.xaml` |
| CodeBrix.Platform.AppSettings add-in | The whole settings store - typed get and set, startup auto-backup and pruning, corruption recovery - wrapped by this application's own facade | `src/libs/GitHubIssueFinder.Settings/SettingsService.cs` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| Microsoft.Extensions.DependencyInjection.Abstractions | The one dependency of the GitHub library beyond the framework: the abstractions its `AddGitHubIssueSearch` extension is written against | `src/libs/GitHubIssueFinder.GitHub/GitHubIssueFinder.GitHub.csproj` |
| Microsoft.Extensions.Hosting and Logging.Console | The generic host that backs `SimpleServiceResolver`, and the Debug-only console logger factory | `src/GitHubIssueFinder.Core/Helpers/HostHelper.cs`, `src/GitHubIssueFinder.UI/App.xaml.cs` |
| xUnit v3, Microsoft.NET.Test.Sdk, SilverAssertions | The test stack both test projects use | the two `tests/libs/*/*.csproj` |

Everything else the application needs is in the framework: `HttpClient` over a
`SocketsHttpHandler`, `System.Text.Json` with a source-generated context, `TimeProvider`,
and `IAsyncEnumerable`.

## Worth studying in this application

### The service that does all the talking

`src/libs/GitHubIssueFinder.GitHub` is the whole GitHub conversation and has no UI type in
it anywhere. `IGitHubIssueSearchService` is three members: `SearchAsync`, which streams
pages, and two properties that report what is left of each quota pool.
`GitHubIssueSearchService` owns one `HttpClient` over one `SocketsHttpHandler` for its
lifetime, sets a pooled-connection lifetime and automatic decompression once, and sends the
three headers GitHub asks a client for on every request. The client's own timeout is
disabled and each request gets a linked token carrying the real deadline instead, so a
request that times out can be told apart from a caller who cancelled: the first becomes a
typed `GitHubApiException` naming the address and the deadline, and the second comes out of
the enumerator as the `OperationCanceledException` it was.

`SearchAsync` is deliberately not an iterator. It validates its arguments and copies the
request, then returns a private iterator method. A null request or a blank owner therefore
throws when the call is made rather than on the first `MoveNextAsync`, and a caller who
edits the request object while enumerating cannot change the query underneath the walk.

See [Build a typed REST client with source generated JSON and its own exceptions](../BLUEPRINTS-DocumentsAndData.md#build-a-typed-rest-client-with-source-generated-json-and-its-own-exceptions)
and [Be a polite HTTP client to a public API](../BLUEPRINTS-DocumentsAndData.md#be-a-polite-http-client-to-a-public-api).

### Two plans, because the search API is capped

GitHub serves at most the first thousand results of any one search. `SearchAsync` asks the
whole-owner query for page one and reads its `total_count`. If the total fits, it walks the
pages and yields them. If it does not, it changes plan: it lists the owner's repositories
through the core API, keeps the ones that are not archived, have issues enabled and have
something open, and runs the same search against each of them in turn. The two plans draw
on two different quota pools, and each pool has its own throttle with its own ceiling and
its own window, so the repository listing cannot starve the searches.

The switch is invisible from the outside: both plans yield the same `IssueSearchPage` type
and the view model folds them the same way. A page from the per-repository plan names its
repository; a page from the whole-owner walk leaves that null, and every item carries its
own repository name either way.

See [Fall back to one search per repository when a search API caps its results](../BLUEPRINTS-DocumentsAndData.md#fall-back-to-one-search-per-repository-when-a-search-api-caps-its-results).

### The throttle, and the clock it runs on

`RateLimitThrottle` is one quota pool and the promise that the application will not empty
it. Two things hold a call back. The response headers are the authority: when GitHub says
one call is left, the throttle waits for the pool to refill rather than spend it, because
GitHub knows about every other caller sharing the address. Before the first response there
are no headers, so a sliding count of the calls this instance has made stands in, capped one
below the published ceiling. Waits are measured on an injected `TimeProvider`, and reported
once a second through a callback.

The snapshot the interface hands out is worked out on every read rather than stored: it is
the smaller of what GitHub says is left and what the application's own ceiling still allows,
so the pill in the status bar reaches zero at the moment the search starts waiting, and
climbs back by itself as the window slides and as GitHub's own pool passes its reset moment.

See [Throttle from the rate limit headers an API sends back](../BLUEPRINTS-DocumentsAndData.md#throttle-from-the-rate-limit-headers-an-api-sends-back).

### Pages into a list, on the right thread

`MainViewModel.DoSearch` copies the owner, the assignee and the closed-items switch into
locals before the first request, so what the user types next cannot change a running search.
It creates its `Progress<SearchProgress>` on the UI thread, which means the platform already
marshals those callbacks and they must not be marshalled again. The enumeration itself runs
under `ConfigureAwait(false)`, and each page is handed to `InvokeOnMainThread` to be folded
in.

Two ordering rules in that method are worth copying. The new `CancellationTokenSource` is
published to the field *before* the old one is cancelled, so a superseded run sees at once
that it is no longer current and leaves the status line, the results and the busy flags
alone on its way out; every callback it posts re-checks that it is still the current run
before touching anything. And the tidying-up in the `finally` is queued on the UI thread
rather than done on the background one, so it lands behind everything the run has already
posted: clearing the field eagerly made the last page and the closing sentence look
superseded, and the search appeared to stop one page short.

Folding a page builds every row and groups them by repository *before* anything reaches the
bound collections, so a repository new to the search arrives on screen with its rows already
inside it. A group inserted empty and filled a moment later can be measured while it is
still empty, and draws as a bare header until something else forces a fresh layout.

See [Stream an IAsyncEnumerable of pages into a bound collection](../BLUEPRINTS-MVVM.md#stream-an-iasyncenumerable-of-pages-into-a-bound-collection)
and [Run a long job from a command with progress cancellation and a busy flag](../BLUEPRINTS-MVVM.md#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag).

### One progress channel, four kinds of news

`SearchProgress` is a record carrying the phase, how much has been fetched, how many pages,
how long any wait has left, and a snapshot of both quota pools. Its `ToString` writes the
status-line sentence for every phase, so the view model's progress handler is mostly a
switch on the phase that decides the color and the glyph. The two phases the library never
reports are cancellation and failure: those belong to the view model, which is the thing
that caught the exception and knows the repository count and how long the run took.

The waiting phase is what makes the throttle legible. Every second of every wait is a report
on the same channel as the pages, so nothing else had to be built for it: the status line
turns amber, gains a clock glyph and counts down, and the search-quota pill changes its three
colors to match.

See [Make a rate limit wait visible through the progress channel](../BLUEPRINTS-MVVM.md#make-a-rate-limit-wait-visible-through-the-progress-channel).

### The grouped list

The results are an outer `ListView` bound to `Groups`, whose item template holds a
repository header and an inner `ItemsControl` bound to that group's `Rows`. Both the group
header and each row are stretched `Button`s, so hover and press come from the re-keyed
button brushes and the click is a `Command` on the item's own view model rather than a
selection event the page has to interpret. The `ListView` has selection turned off; nothing
here is selectable, only clickable.

`IssueRowViewModel` works out everything its row draws once, in its constructor: the state
glyph and its color role, the meta line, the tooltip, the comment count, whether the "PR"
chip is drawn, and the label pills. The template binds to plain strings and visibilities, so
a list of thousands of rows stays cheap. Neither the row nor the group holds a reference to
the view model that made it; opening a page in the browser is a `Func<string, Task>` they
were handed.

See [Build a grouped list from group and row view models](../BLUEPRINTS-ViewsAndControls.md#build-a-grouped-list-from-group-and-row-view-models)
and [Show a relative date with the exact one in a ToolTip](../BLUEPRINTS-ViewsAndControls.md#show-a-relative-date-with-the-exact-one-in-a-tooltip).

### Five schemes, painted in place

`Theming/ColorSchemes.cs` holds four hand-authored palettes - Light, Light High Contrast,
Dark and Dark Dimmed - as plain ARGB numbers with a base theme each, and no drawing type
anywhere. `App.xaml` declares every color the application draws as a keyed
`SolidColorBrush` after the merged control resources: the role brushes the page binds to,
and the stock control keys re-pointed at the same values, with every state key present.
`Theming/SchemeBrushMap.cs` says which role each of those keys carries.

Applying a scheme is then one walk of that map, assigning a new `Color` to the brush that is
already in the dictionary. Mutating a brush in place repaints every `{StaticResource}`
consumer in the same frame - the page, the stock control chrome, and the rows already
realized inside the list - with no re-navigation, no rebuilt tree and no lost scroll
position. Three things cannot be shared resources because they change with application state
rather than with the scheme, so the view models own those brushes and re-point them the same
way: the status line's color, the search-quota pill, and each row's state glyph and label
pills.

The page also sets `RequestedTheme` on the root grid from the scheme's base theme. That is
complementary rather than redundant: it governs everything the application does *not* re-key
- focus visuals, the caret and selection highlight, tooltips and the popup layer.

See [Model a color scheme as plain data in a UI free library](../BLUEPRINTS-ThemingAndStyling.md#model-a-color-scheme-as-plain-data-in-a-ui-free-library),
[Choose a repaint mechanism that can carry more than two schemes](../BLUEPRINTS-ThemingAndStyling.md#choose-a-repaint-mechanism-that-can-carry-more-than-two-schemes),
[Re-key every control brush family the platform ships](../BLUEPRINTS-ThemingAndStyling.md#re-key-every-control-brush-family-the-platform-ships)
and [Switch between several color schemes by mutating keyed brushes in place](../BLUEPRINTS-ViewsAndControls.md#switch-between-several-color-schemes-by-mutating-keyed-brushes-in-place).

### Following the operating system, and overriding it

The picker's first entry is "System default", and it names what it currently resolves to -
"System default (Dark)". Choosing it leaves `Application.RequestedTheme` alone, which is what
keeps the platform following the operating system; the page holds one `UISettings` instance
in a field and re-points the role brushes when `ColorValuesChanged` fires. Choosing any named
scheme sets `Application.RequestedTheme` in the `App` constructor on the next launch, and
setting it at all is what makes the platform stop following the operating system - exactly the
override the user asked for.

The `UISettings` instance has to be a field, not a local: the platform holds only a weak
reference to it, so a local one is collected and the notifications stop. And the "System
default" entry is *replaced* rather than renamed when the operating system flips, because a
picker's closed face reads its item once and does not listen for a rename.

See [Follow or override the desktop appearance and check it from a shell](../BLUEPRINTS-ThemingAndStyling.md#follow-or-override-the-desktop-appearance-and-check-it-from-a-shell)
and [Follow the operating system light and dark preference with a System default entry](../BLUEPRINTS-ViewsAndControls.md#follow-the-operating-system-light-and-dark-preference-with-a-system-default-entry).

### Label pills that keep their own color

Every label GitHub returns carries a color, and the pills wear it, so the labels look the way
they look on the website whichever scheme is showing. `LabelColorMath` does the arithmetic in
plain numbers: the fill is the label color laid faintly over the page ground, the border more
strongly, and the text is the label color with its lightness clamped for the ground it sits
on. One extra rule keeps the pill a pill: a border that lands too close to the page ground on
the lightness axis is pushed away from it, because a label the color of the page - black on a
dark scheme - would otherwise blend in and the pill would read as bare text. A label with no
color, or one that is not six hexadecimal digits, falls back to the neutral role.

See [Give each item its own brushes and re-tint them on a scheme change](../BLUEPRINTS-ThemingAndStyling.md#give-each-item-its-own-brushes-and-re-tint-them-on-a-scheme-change).

### Opening a page in the browser

One private method in the whole application asks the host to open a URL, and the group and
row view models reach it through the delegate they were given. It parses the address, calls
the launcher, and turns both a refused launch and a thrown exception into status-line text.
No dialog, no exception reaching the user: every failure in this application is a sentence on
the status line, in the danger color, with an error glyph.

See [Open a URL in the default browser from a view model](../BLUEPRINTS-PlatformServices.md#open-a-url-in-the-default-browser-from-a-view-model).

### Two libraries, two mirrored test projects, saved responses on disk

Because both libraries hold no UI types, they are testable on their own, and each has a test
project that mirrors it. `GitHubIssueFinder.GitHub.Tests` answers every request with a stub
`HttpMessageHandler` keyed by path and query, which records what it was asked for and never
disposes what it was given; the bodies are real GitHub responses saved under `Fixtures/`,
copied beside the binary and found through `AppContext.BaseDirectory`. Waits run on a fake
`TimeProvider` that treats being asked for a timer as being asked to wait: it records the
wait, jumps its own clock to the due moment and hands the callback to the thread pool, so a
test of an hour-long wait finishes in a millisecond against the shipping code, unchanged. One
case still proves the real clock, with a one-second window and a stopwatch.
`GitHubIssueFinder.Settings.Tests` points the process-wide store at a throwaway folder from a
module initializer. Both libraries carry an `InternalsVisibleTo.cs` naming their test
assembly.

See [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library),
[Point a process-global store at a throwaway folder in tests](../BLUEPRINTS-Testing.md#point-a-process-global-store-at-a-throwaway-folder-in-tests)
and [Make live tests opt in and keep them out of the default run](../BLUEPRINTS-Testing.md#make-live-tests-opt-in-and-keep-them-out-of-the-default-run).

## Known limits

- **The window opens at 1180 by 800 and cannot be dragged smaller than 760 by 520.**
- **Public data only.** The application signs in to nothing, so it sees public
  repositories and gets the anonymous allowance. There is no place to put a token.
- **A thousand results per repository.** GitHub serves at most the first thousand results
  of any one search. A whole-owner search above that switches to one search per repository,
  but a single repository with more than a thousand matching items is listed up to that
  point and no further.
- **Archived repositories are skipped by the fallback plan.** The per-repository plan keeps
  only repositories that are not archived, have issues enabled and have something open, so
  its total can be smaller than the whole-owner total the first page reported. The
  difference is the archived and issue-less repositories, which have nothing anyone can pick
  up anyway.
- **The popup layer keeps the theme family it started with.** `Application.RequestedTheme`
  cannot change after startup, so switching between a light and a dark scheme at run time
  repaints everything the application owns but leaves the popup layer's own chrome on the
  family it launched with, until the next run. The application raises no dialogs, so what
  this can affect is a tooltip's chrome.
- **Enter runs the search from the two text boxes.** Elsewhere on the page Enter does what
  the focused control does. Escape cancels a running search from anywhere.

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder is the attribution record
for this application. Nothing third-party is bundled here: no fonts, no images, no vendored
code. The four color schemes are hand-authored for this application. Third-party code
arrives as packages, each carrying its own license and notices. The issue and repository
data the application shows is fetched from GitHub at run time and belongs to its owners;
none of it is redistributed here.

## License

GitHubIssueFinder is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
