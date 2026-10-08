# GoddessTempleDiscovery

Goddess Temple Discovery! is an educational cards-and-dice board game for two to four
archaeological expeditions, played on a 1920s Art Deco table. It is winter 1912/13 at
Warka, the ancient city of Uruk in southern Iraq, and every team wants to be the one that
brings the House of Heaven - Eanna, the precinct of Holy Inanna - back into the light. The
game runs through the twelve real pre-war campaigns of the German excavations, from
1912/13 to 1938/39. Each season a Season card tells what really happened that winter and
nudges the play; each team rolls two dice, its two work crews, and spends them to dig
trenches, recruit specialists, study and survey; and the finds are laid down as numbered
Preliminary Reports, as the excavators published theirs. Every discovery that comes out of
the ground opens as an EXTRA! edition of a newspaper, the Warka Herald, with a banner
headline, the story in columns and the find framed as a photograph. Every find and every
season is written up in several editions, each with its own banner and byline, and the
game's seed picks the edition a game prints, so the same find makes different news from one
game to the next while a seeded game reprints its own. When the last season ends the Second
World War closes the dig, the reports are tallied in the Final Edition, and the whole game
can be printed as a Field Journal: a PDF of every edition the game produced. A Gallery,
opened from the title screen or the table's header, browses every picture in the game by
category and shows each one large with its title, subject, sources and license.

The teams are fictional. Everything on the cards is real and sourced: the people - Julius
Jordan, Conrad Preusser, Arnold Nöldeke, Ernst Heinrich, Heinrich Lenzen, Walter Andrae,
Gertrude Bell, the foremen and workers from Babylon and Hilla and many more; the finds - the
Limestone Temple, the cone-mosaic courts, Karaindash's temple of Inanna, the ziggurat of
Ur-Nammu, the Warka Vase, the Mask of Warka found on 22 February 1939; the periods from the
Ubaid villages to the Arab conquest; and the Goddess Herself, with Tablets that teach Her
myths, Her pantheon and Her city, Enheduanna among them - high priestess of the moon god
Nanna at Ur, the first author in human literature whose name we know, whose hymns to Holy
Inanna are why she is in the game. The Site deck draws on a library of the excavation
reports and the scholarship, on museum catalogues and on the Oracc and CDLI corpora. Every
card names its sources, and the inspector and the Field Journal print them in full.

It is the CodeBrix.Samples reference for a turn-based game on the CodeBrix.Platform
GameEngine and its CardsAndDice add-on: a table of cards and dice inside a game host, a
rules engine that knows nothing about the table, card faces composed at start-up as
self-contained SVG with every letter a vector path, a newspaper inspector and a set of Art
Deco panes in XAML over the game canvas, computer opponents paced to the table's
animations, and a hand-placed PDF whose art is drawn as vectors. The art - every picture on
every card and every piece of the Deco chrome - is original vector work made for this
application and ships under the repository's Apache License as a free resource; see
`src/libs/GoddessTempleDiscovery.Assets/Art/ART-LICENSE.md`. InannaRosette, a sibling
application, is the reference for the hand-placed PDF technique the Field Journal builds
on; BrixInvaders is the one for a real-time game on the same engine.

## Cultural heritage and history

\================================================================

Note on Cultural Heritage and History

\================================================================

In exploring the sacred history of ancient Mesopotamia and the important cultural heritage of the area — that, I believe, provided the foundation of cultural heritage for my own European ancestors — I wish to acknowledge the deep and ongoing pain felt by the people of Iraq (and neighboring countries) regarding the extraction of their national heritage. The early archeological excavations of sites like Uruk, Ur and Babylon occurred during a period of immense power imbalance and colonial influence. While this document references the people involved and findings of a portion of those missions, it is not an endorsement of the "heritage-looting", colonization, or cultural extraction that resulted in so many significant artifacts being removed from their homeland. I hope to not be another agent of cultural appropriation; but instead to honor the legacy of the people who have lived in the Tigris and Euphrates valleys for millennia; and I present this work with the utmost respect for their enduring connection to this history. My intent is to celebrate the numinous presence of the Divine as understood through ancient and explored through modern sources, without diminishing the modern right of Iraqis to their own cultural and historical birthright.

Also, this creative work references German archaeologists and scholars — some with documented Nazi-party ties, some who participated in Axis-aligned political activity in Iraq (notably around the 1941 Rashid Ali coup and its aftermath). No part of this creative work is intended to celebrate individuals who espoused or were aligned with Nazi ideology, or who were involved in actions against the Iraqi people.

\================================================================

The same note is printed on the Credits screen and in every Field Journal. Where the record
documents a person's Nazi-era ties, that person's card on the History screen says so
plainly.

## How to play

The table's buttons act when the mouse button is released over them, as the XAML buttons
do, and while a pane is open over the table the table takes no clicks.

| Action | Mouse | Keys |
| --- | --- | --- |
| Roll the dice | the ROLL button | Space or Enter |
| Choose a die, or both for their sum | click the die | |
| Dig a trench | click it in the Site Row | 1 to 5 |
| Add Workers or a Tablet to a dig | the Worker stepper under the dice; click a Tablet in the hand | |
| Recruit a specialist | click it in the Expedition Row | |
| Study, Survey, Publish | the STUDY, SURVEY and PUBLISH buttons | |
| Re-roll a die (one season allows it) | the RE-ROLL button | R |
| End the turn | the END TURN button | Space or Enter, once the dice are rolled |
| Read any face-up card | right-click it | |
| Close what is on top, leave a mode | the inspector's or the pane's close button | Escape |
| The Field Journal | the JOURNAL button | J |
| The Gallery of the game's art | the GALLERY button on the title screen or the table's header; click a picture to see it large | Left and Right, or Page Up and Page Down, turn its pages |

- **A turn.** Roll both dice and spend them one at a time, each alone or both together as
  one sum. A dig succeeds when the dice plus modifiers reach the trench's Dig Number, which
  runs from 3 for the Seleucid surface down to 11 for the deep sounding; deeper layers are
  worth more. Workers add one each, a Tablet adds two, and specialists add one on the kinds
  of find they know. Recruiting needs a die at least the specialist's cost, studying draws
  Tablets, and surveying sends an unwanted trench to the bottom of the Tell.
- **Publishing.** Three or more finds from the hand make a Preliminary Report. A report of
  one period is a stratigraphy report and a run of consecutive periods a sequence report,
  and both earn a bonus per card. Finds left unpublished at the end score half.
- **Seasons and Favors.** Each Season card applies one mild effect to every team - a free
  survey, cheaper recruits, a re-roll, a publishing bonus. Doubles draw a Favor of the
  Goddess: a small gift, each with a true line about Her.
- **The end.** After 1938/39 the Final Edition adds up the reports, half the unpublished
  finds, the Tablets and their sets, and the Star of Holy Inanna for the team with the most
  of Her own finds.
- **Computer teams.** Any seat can be a computer team with one of three temperaments: the
  Surveyor, steady and shallow; the Deep Digger, the ambitious opponent who banks Workers
  for the deepest layers and chases Her stars; and the Scholar, the gentle one who studies
  and publishes sequences. Their turns play out on the table at animation pace, and their
  discoveries open in the inspector too, unless the setting says otherwise.
- **Setup and settings.** Two to four seats, a season length the setup suggests for the seat
  count, Easy, Standard or Hard digging, and an optional seed for a repeatable game. The
  settings pane holds sound, reduced motion, the animation speed and whether the computer
  teams' finds open in the inspector. The settings and the last setup are kept between runs.

`DESIGN.md` in this folder has every number the rules use - the tiers and Dig Numbers, the
points, the season and Favor effects, the specialists' powers and costs, the report
bonuses, the hand limit, the final scoring, the temperament weights, the pacing, the
screens, the input and the stored settings.

## What this sample shows a CodeBrix.Platform developer

- Hand the view model the game canvas at its first real layout size, through a small
  interface the page calls from the canvas's first-started event:
  [Hand the view model a game canvas at its first real layout size](../BLUEPRINTS-GameEngine.md#hand-the-view-model-a-game-canvas-at-its-first-real-layout-size).
- Put the CardsAndDice add-on's table inside a `CodeBrixGameHost`: create it while the host
  loads its assets, subscribe to its clicks once the engine is up, advance it from the
  fixed step, forward the pointer to it, and paint it into the game's own draw list between
  the board and the HUD:
  [Wire a CardsAndDice table into a CodeBrixGameHost and paint it into the game's draw list](../BLUEPRINTS-GameEngine.md#wire-a-cardsanddice-table-into-a-codebrixgamehost-and-paint-it-into-the-games-draw-list).
- Compose every card face at start-up as one self-contained SVG - a Deco frame, the card's
  art inlined with its ids prefixed, the title and text set as glyph outlines from embedded
  fonts - on a worker thread, and register the faces on the table a few at a time within a
  budget per step:
  [Compose card faces as SVG with every letter a vector path](../BLUEPRINTS-GameEngine.md#compose-card-faces-as-svg-with-every-letter-a-vector-path).
- Let the rules engine roll and make the table's tumbling dice land on its values:
  [Force the table's dice to show the rules engine's roll](../BLUEPRINTS-GameEngine.md#force-the-tables-dice-to-show-the-rules-engines-roll).
- Turn every event the rules engine raises into a queued table step that waits for the
  table's animation, and let the computer teams act only when the queue is idle:
  [Play a rules engine's events on the table one step at a time and pace the computer turns](../BLUEPRINTS-GameEngine.md#play-a-rules-engines-events-on-the-table-one-step-at-a-time-and-pace-the-computer-turns).
- Keep the rules and every card's content in a library with no package reference at all,
  where every action is validated before it is applied, the legal actions can be listed,
  and a seeded game played by the computer brain is pinned by a hash of the whole state:
  [Keep a turn-based rules engine package-free and pin seeded games with a state hash](../BLUEPRINTS-GameEngine.md#keep-a-turn-based-rules-engine-package-free-and-pin-seeded-games-with-a-state-hash).
- Open a newspaper page over the game canvas from the engine thread through a two-method
  bridge the view model implements, with the card's pictures rendered off the UI thread
  and the headline's edition picked from the game's seed by a stable hash of the card id:
  [Open a newspaper-style inspector over a game canvas through a bridge the view model implements](../BLUEPRINTS-MVVM.md#open-a-newspaper-style-inspector-over-a-game-canvas-through-a-bridge-the-view-model-implements).
- Browse every embedded picture in a paged, filtered gallery whose thumbnails are cell view
  models with their own open command, rendered on a worker the first time they show:
  [Give each grid cell its own command and lazily loaded thumbnail](../BLUEPRINTS-MVVM.md#give-each-grid-cell-its-own-command-and-lazily-loaded-thumbnail).
  Each picture's title, subject and sources are read from its SVG header comment; there is
  no blueprint for that yet, read `Rendering/ArtInfo.cs` in the game library.
- Draw whole SVG pictures - groups, transforms, every basic shape - onto a PDF page as
  vectors, so the Field Journal's card art stays sharp at any zoom:
  [Draw whole SVG pictures onto a PDF page as vector shapes](../BLUEPRINTS-DocumentsAndData.md#draw-whole-svg-pictures-onto-a-pdf-page-as-vector-shapes),
  on top of
  [Convert SVG path data into a PDF graphics path](../BLUEPRINTS-DocumentsAndData.md#convert-svg-path-data-into-a-pdf-graphics-path).
- Register the embedded Merriweather and Noto Sans Cuneiform faces with the PDF font
  system once per process:
  [Register embedded OFL fonts with the PDF font system](../BLUEPRINTS-DocumentsAndData.md#register-embedded-ofl-fonts-with-the-pdf-font-system).
- Build a frame into one engine draw list and hit-test the published copy for the HUD's
  buttons, which arm on the press and fire on the release, and drop the table's clicks
  while the view model reports a pane over it:
  [Build each frame as engine DrawList commands and paint only the published copy](../BLUEPRINTS-GameEngine.md#build-each-frame-as-engine-drawlist-commands-and-paint-only-the-published-copy).
  The table keeps its height and widens its render resolution with the window's aspect,
  spreading its layout into the extra width; a window narrower than the base table is
  letterboxed by the engine, as in
  [Pin a fixed playfield size and let the engine letterbox drawings and clicks](../BLUEPRINTS-GameEngine.md#pin-a-fixed-playfield-size-and-let-the-engine-letterbox-drawings-and-clicks).
- Bind named actions to keys with the engine's input-action map and read them once per
  fixed step:
  [Read keyboard and gamepad together through an InputActionMap and switch on-screen prompts](../BLUEPRINTS-GameEngine.md#read-keyboard-and-gamepad-together-through-an-inputactionmap-and-switch-on-screen-prompts)
  (this game binds the keyboard only).
- Attach the engine's window helper in one call, so the engine pauses while the window is
  minimized and keyboard focus returns on activation:
  [Pause the engine while the window is minimized with GameWindowLifecycle and come back to the game's pause menu](../BLUEPRINTS-GameEngine.md#pause-the-engine-while-the-window-is-minimized-with-gamewindowlifecycle-and-come-back-to-the-games-pause-menu).
- Keep every setting and the last setup behind one application-named facade over the
  AppSettings add-in, opened first thing in the `App` constructor, with a scratch store for
  unattended runs:
  [Wrap the AppSettings add-in in one application named facade](../BLUEPRINTS-SettingsAndPersistence.md#wrap-the-appsettings-add-in-in-one-application-named-facade)
  and
  [Open the settings store before any other startup work](../BLUEPRINTS-SettingsAndPersistence.md#open-the-settings-store-before-any-other-startup-work).
- Open the settings store in a throwaway folder in the tests:
  [Point a process-global store at a throwaway folder in tests](../BLUEPRINTS-Testing.md#point-a-process-global-store-at-a-throwaway-folder-in-tests).
- Bind the setup and settings panes the family way - `SetProperty`, lazily created
  `SimpleCommand`s and `[AffectsCommands]` - so Start enables itself as the seats change:
  [Write bound properties and commands the family way](../BLUEPRINTS-MVVM.md#write-bound-properties-and-commands-the-family-way).
- Ask for the Field Journal's file name through a save picker the page fills in as a
  delegate, and clean up the path it hands back:
  [Save a file through a native dialog from the view model](../BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model)
  and
  [Clean up the path a file picker returns](../BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns).
- Assign the page's pieces through the interfaces the view model declares, never through
  its concrete type:
  [Assign every bridge through the interface that declares it](../BLUEPRINTS-PlatformServices.md#assign-every-bridge-through-the-interface-that-declares-it).
- Guard the view model constructor against the XAML designer:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- Open the window at a launch size and keep it from shrinking below the table's minimum:
  [Set the window's launch size](../BLUEPRINTS-AppStructureAndStartup.md#set-the-windows-launch-size)
  and
  [Keep the window from shrinking below a minimum](../BLUEPRINTS-AppStructureAndStartup.md#keep-the-window-from-shrinking-below-a-minimum).
- Make Merriweather the application's default text font:
  [Set a bundled font as the default text font and register script fallbacks](../BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks).
- Write every diagnostic line through one prefixed log with a sink the tests can capture,
  and install a console logger only in Debug builds:
  [Write every diagnostic line through one prefixed log that tests can capture](../BLUEPRINTS-AppStructureAndStartup.md#write-every-diagnostic-line-through-one-prefixed-log-that-tests-can-capture)
  and
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Play a whole game unattended from an environment switch and report it to the log:
  [Drive a running application from an environment variable and report to the log](../BLUEPRINTS-Testing.md#drive-a-running-application-from-an-environment-variable-and-report-to-the-log).
  Here the switch seats four computer teams and the host plays them to the end, then writes
  the Field Journal.
- Let the game library own the engine packages, with a root namespace of its own:
  [Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace)
  and
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- Organize the application as a shared UI project, a Core project, three libraries under
  `src/libs` and mirrored test projects under `tests/libs`:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- Keep every head's `Program.Main` to the same few lines:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Set up xUnit v3 test projects on Microsoft.Testing.Platform, each library naming only its
  own test assembly in `InternalsVisibleTo.cs`:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library)
  and
  [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- Record the fonts, the add-on's art and sounds, the traced plates' sources and the quoted
  translations in the application's own notices file:
  [Record bundled third-party content in a notices file](../BLUEPRINTS-ProjectLayoutAndPackaging.md#record-bundled-third-party-content-in-a-notices-file).
- The Art Deco chrome is drawn from the same embedded SVG pieces on both sides of the page:
  the HUD crops each piece to its drawn part once and stretches it onto table rectangles,
  and the view model renders the wordmarks, the masthead and the ornaments to bitmaps for
  the XAML panes. There is no blueprint for this yet; read `Hud/DecoPieces.cs` and
  `LoadChromeArtAsync` in the view model.

## Building, running and testing

There is one solution, `GoddessTempleDiscovery.slnx`, and it holds everything: the shared
UI project, the Core project, four heads, the three libraries under a `Libraries` solution
folder and the test projects under a `Tests` solution folder.

| Head project | Platform | Host-builder call |
| --- | --- | --- |
| `src/GoddessTempleDiscovery.LinuxX11` | Linux, X11 | `UseLinuxX11()` |
| `src/GoddessTempleDiscovery.LinuxWayland` | Linux, Wayland | `UseLinuxWayland()` |
| `src/GoddessTempleDiscovery.MacOS` | macOS | `UseMacOS()` |
| `src/GoddessTempleDiscovery.Win32Skia` | Windows, Win32 window | `UseWindowsWin32()` |

Every `Program.cs` is the same but for that call: it calls `App.InitializeLogging()` first,
then builds the host with `UseDirectSkiaCanvasMode()` before `Build()`. There is no
LinuxFrameBuffer head and no WinWpfSkia head: the game is a mouse-driven table on a window
that opens at 1440 x 900 and will not shrink below 1180 x 740.

Prerequisites:

- The .NET 10 SDK. Every project targets `net10.0`, and no workload is needed.
- All CodeBrix code arrives as packages. No CodeBrix library is referenced as a source
  project, so this folder builds on its own.
- Nothing else: no accounts, no network access, no downloads and no data files. The art,
  the fonts and every card's content travel inside the assemblies. A sound device is
  optional; without one the table is silent.
- A native save dialog to export the Field Journal; every head in this folder has one.

To run one head from the command line, from this folder:

```text
dotnet run --project src/GoddessTempleDiscovery.LinuxX11
dotnet run --project src/GoddessTempleDiscovery.LinuxWayland
dotnet run --project src/GoddessTempleDiscovery.MacOS
dotnet run --project src/GoddessTempleDiscovery.Win32Skia
```

Building the Windows head on Linux or macOS is supported; running it is not.

Three test projects mirror the three libraries, and one more drives the running
application. The rules tests cover every action, every season and Favor effect, the hand
limit, the reports and the final scoring against small made-up catalogs, so they never
depend on the content, and they pin whole seeded games played by the computer brain. The
asset tests check that every embedded picture is a self-contained SVG in the art's palette
and that every font travels with its license, and the content integrity tests check every
card's art key against the art and every find's and season's headline editions. The game
tests compose and rasterize every card face, check how the headline editions are chosen,
drive a whole game headless on a real CardsAndDice table and keep it in step with the
engine, build the newspaper views, build a Field Journal and check the heritage note in it
word for word, and exercise the settings facade against a throwaway store. None of them
needs a window or a sound card. The PlayTests project runs the real page, view model and
game on the CodeBrix.Platform PlayTest head; see its `README.md`. The folder's `global.json`
selects the Microsoft.Testing.Platform runner, and each test project runs with:

```text
dotnet test --project tests/libs/GoddessTempleDiscovery.Rules.Tests/GoddessTempleDiscovery.Rules.Tests.csproj
dotnet test --project tests/libs/GoddessTempleDiscovery.Assets.Tests/GoddessTempleDiscovery.Assets.Tests.csproj
dotnet test --project tests/libs/GoddessTempleDiscovery.Game.Tests/GoddessTempleDiscovery.Game.Tests.csproj
dotnet test --project tests/GoddessTempleDiscovery.PlayTests/GoddessTempleDiscovery.PlayTests.csproj
```

If a plain `dotnet test` reports that no tests ran, build the test project and run the
executable it produces directly.

### Diagnosing a run

Console logging is compiled in only for Debug builds - the body of
`App.InitializeLogging()` sits inside `#if DEBUG` - so run a Debug build to see the log.
Every line the game writes starts with `[GoddessTemple]`:

| Line | What it reports |
| --- | --- |
| `render tier requested:`, `render tier:` | The tier asked for and the tier actually running, and the render resolution |
| `settings store:` | Where the settings and the last setup are kept |
| `art preparation:` | How long the card faces took to compose and to register on the table |
| `new game requested:`, `game started:` | The setup a game began with: seats, turns a season, difficulty, seed |
| `season` | Every Season card as it flips, with its effect |
| `action:` | Every action any team takes, as the wire-service line the ticker shows |
| `inspector:`, `sound:` | A newspaper page that could not be built, a sound that could not play |
| `final score:` | Each team's total and its parts |
| `autoplay:` | The hands-off switch being on, and the Field Journal it wrote |

Two environment switches change how the game runs, and both are for diagnosis rather
than play:

- `GODDESSTEMPLE_USE_CPU=1` renders on the CPU tier instead of the GPU tier.
- `GODDESSTEMPLE_AUTOPLAY=1` plays a whole game hands-off: four computer teams at one turn
  a season, every discovery shown briefly in the inspector, then the Field Journal written
  to the system temporary folder and `GODDESSTEMPLE AUTOPLAY PASS` logged.
  `GODDESSTEMPLE_AUTOPLAY_SEED` pins the game. It keeps its settings in a scratch store under
  the system temporary folder, so its games never reach the player's own.

The player's own settings store is created on the first run, in a `GoddessTempleDiscovery`
folder under the user's per-user application-settings location.

## How the projects and folders are organized

```text
GoddessTempleDiscovery/
  GoddessTempleDiscovery.slnx          The one solution: UI, Core, four heads, three libraries, the test projects
  global.json                          Selects the Microsoft.Testing.Platform test runner
  DESIGN.md                            The game design, with every number the rules use
  THIRD-PARTY-NOTICES.txt              Third-party content used by this application
  src/
    GoddessTempleDiscovery.UI/         Shared items project: the XAML every head compiles
      App.xaml                         The Deco palette, brushes and the pane, button and text styles
      App.xaml.cs                      Settings store first, default font, resolver, launch size, window, frame
      Views/MainPage.xaml(.cs)         The game canvas with the Deco panes and the newspaper layered over it
    GoddessTempleDiscovery.Core/       Class library; carries the framework, font, hosting and logging packages
      Helpers/                         The host-builder provider and the save-picker path cleanup
      Services/IJournalFileBridge.cs   The save dialog the page fills in for the Field Journal
      ViewModels/MainViewModel*.cs     The panes, the setup, the inspector, the journal, the gallery and settings; owns the host
      ViewModels/SeatViewModel.cs      One seat of the setup pane
    GoddessTempleDiscovery.LinuxX11/   Head: Program.cs plus a csproj with one runtime package
    GoddessTempleDiscovery.LinuxWayland/  Head
    GoddessTempleDiscovery.MacOS/      Head
    GoddessTempleDiscovery.Win32Skia/  Head
    libs/
      GoddessTempleDiscovery.Rules/    The rules engine and every card's content: no package, no engine, no I/O
        Cards/                         The card records, the periods and the depth tiers
        Engine/                        GameEngine (setup, flow, legality, apply, hash), actions, events, state
        Scoring/                       Report scoring and the final tally
        Brains/                        The computer teams' decision procedure and the three temperaments
        Journal/                       The Field Journal entries the engine writes
        Content/                       Catalog: discoveries, tablets, seasons, favors, specialists, people,
                                         periods, glossary, quotations, headlines, prologue and epilogue
      GoddessTempleDiscovery.Assets/   Every picture and font, embedded; no package
        Art/                           The vector art in eight category folders, with ART-LICENSE.md
        Fonts/                         Merriweather and Noto Sans Cuneiform, with their license texts
        ArtCatalog.cs, FontAssets.cs   Art by key, fonts by resource name
      GoddessTempleDiscovery.Game/     The game host and everything drawn, heard and clicked; owns the engine packages
        Hosting/                       GoddessTempleGameHost, start-up, the game log, the autoplay switch
        Session/                       TableSession over the CardsAndDice table, the queue, layout, controls
        Cards/                         The face composer, vector text, the SVG inliner, the palette, the headline editions
        Hud/                           The HUD painter and the Deco pieces it draws with
        Bridges/                       The inspector and session bridges and the card views they carry
        Journal/                       The Field Journal PDF and the SVG-to-PDF drawing it uses
        Rendering/, Settings/, Credits/  SVG to PNG and the gallery's picture headers, the settings facade, the heritage note
  tests/
    GoddessTempleDiscovery.PlayTests/  UI tests of the running game on the PlayTest head
    libs/
      GoddessTempleDiscovery.Rules.Tests/   Mirrors src/libs/GoddessTempleDiscovery.Rules, golden seeds included
      GoddessTempleDiscovery.Assets.Tests/  Mirrors src/libs/GoddessTempleDiscovery.Assets, content integrity included
      GoddessTempleDiscovery.Game.Tests/    Mirrors src/libs/GoddessTempleDiscovery.Game, headless table included
```

Dependency direction is strictly one way. Each head references `GoddessTempleDiscovery.Core`
and file-links the shared UI through an `Import` of `GoddessTempleDiscovery.UI.projitems`,
so `App.xaml` and `MainPage.xaml` are compiled into every head. `GoddessTempleDiscovery.Core`
references the three libraries. `GoddessTempleDiscovery.Game` references the other two and
owns the engine, CardsAndDice, AppSettings and PDF packages; `GoddessTempleDiscovery.Rules`
and `GoddessTempleDiscovery.Assets` reference nothing at all. The Assets tests also
reference the Rules library, so the content can be checked against the art. Nothing flows
back: the rules know nothing about the table, the libraries know nothing about the view
model, and the Core project knows nothing about the UI.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The application framework: `Application`, `Window`, `Frame`, `Page`, the XAML panes and the newspaper, the save picker, `Windows.System.Launcher`, the default-font feature configuration, and the "Simple" toolkit (`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, `IHostBuilderProvider`, `IXamlRootGetter`, `[AffectsCommands]`, `[AffectsProperties]`, `CodeBrixPlatformHostBuilder`) | `src/GoddessTempleDiscovery.Core/`, `src/GoddessTempleDiscovery.UI/` |
| CodeBrix.Platform runtime for each head | Exactly one runtime package per head supplies that head's windowing and Skia surface | The four head csproj files and their `Program.cs` |
| CodeBrix.Platform.Fonts.Merriweather | The default XAML font; the cards, the HUD and the PDF use the Merriweather faces embedded in the Assets library | `src/GoddessTempleDiscovery.Core/GoddessTempleDiscovery.Core.csproj`, `src/GoddessTempleDiscovery.UI/App.xaml.cs` |
| CodeBrix.Platform.GameEngine | The engine loop, `CodeBrixGameHost`, `GameSurfaceCanvas` and its render tiers, scenes, the draw list the table and the HUD paint into, the input-action map, the window lifecycle helper, the engine dispatcher and the audio the paper rustle plays through. One package supplies both the engine core and the Host layer | `src/libs/GoddessTempleDiscovery.Game/`, `src/GoddessTempleDiscovery.UI/Views/MainPage.xaml`, `src/GoddessTempleDiscovery.UI/App.xaml.cs` |
| CodeBrix.Platform.GameEngine.CardsAndDice | The table: cards, decks, piles and areas with stack, row and fan layouts, deals, flips and tosses, the traditional dice and their tumble, card clicks and right-click inspection, SVG registration and rasterizing, the celestial card back, and the card and dice sounds | `src/libs/GoddessTempleDiscovery.Game/Session/TableSession.cs`, `src/libs/GoddessTempleDiscovery.Game/Hosting/GoddessTempleGameHost.cs` |
| CodeBrix.Platform.AppSettings | Stores the settings and the last setup between runs, behind one application-named facade | `src/libs/GoddessTempleDiscovery.Game/Settings/SettingsService.cs` |
| CodeBrix.PdfDocuments | The Field Journal: `PdfDocument`, `XGraphics`, `XGraphicsPath` for the vector art, `XFont`, and the embedded-font resolvers | `src/libs/GoddessTempleDiscovery.Game/Journal/` |
| SilverAssertions | The assertion style in every test project | `tests/libs/`, `tests/GoddessTempleDiscovery.PlayTests/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SkiaSharp | Turns the embedded fonts' glyphs into SVG path data for the card faces, measures text, and carries the typefaces and images the draw list paints; it arrives with the engine package, and the asset and game test projects add its Linux native library, which a head would otherwise supply | `src/libs/GoddessTempleDiscovery.Game/Cards/VectorText.cs`, `src/libs/GoddessTempleDiscovery.Game/Hud/`, `tests/libs/GoddessTempleDiscovery.Assets.Tests/`, `tests/libs/GoddessTempleDiscovery.Game.Tests/` |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()` behind an `IHostBuilderProvider`, which `SimpleServiceResolver` uses to build the container | `src/GoddessTempleDiscovery.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.Logging.Console | The console logger wired into the platform's ambient logger in Debug builds | `src/GoddessTempleDiscovery.UI/App.xaml.cs` |
| xUnit v3, Microsoft.NET.Test.Sdk and Microsoft.Testing.Platform | The test framework and the runner for the test projects | `tests/libs/*/`, `tests/GoddessTempleDiscovery.PlayTests/` |

## Worth studying in this application

### A CardsAndDice table inside a game host

`GoddessTempleGameHost` is a `CodeBrixGameHost` that owns one `CardsAndDiceTable` for its
whole life. The table is created in `LoadAssets`, its card, die and inspect events are
subscribed in `OnEngineInitialized`, and every fixed step resizes it to the window, feeds it
the queued mouse events, advances the session (which advances the table) and paces the
computer teams. After the steps, `OnAfterFixedUpdates` clears one draw list, has the HUD
painter draw the Deco board, then `table.Draw`, then the HUD over it, and publishes the
list; the host hit-tests the published copy for its buttons before the table sees the
click. A press on a button only arms it and the release on the same button fires it, so the
release reaches the canvas before the pane the button opens covers it; while the view model
reports a pane over the table through `SetPanesOpen`, the host gives neither the table nor
the HUD a click. `TableSession` builds one add-on `Deck` per rules deck from the catalog the engine
was dealt from, with each card's rules record in its `Data`, so a clicked table card leads
straight back to its discovery. Read `LoadAssets`, `OnEngineInitialized`, `OnFixedUpdate`,
`OnAfterFixedUpdates` and `Mouse` in the host, then `BuildTable` in the session. See
[Wire a CardsAndDice table into a CodeBrixGameHost and paint it into the game's draw list](../BLUEPRINTS-GameEngine.md#wire-a-cardsanddice-table-into-a-codebrixgamehost-and-paint-it-into-the-games-draw-list).

### Card faces composed as SVG, with text as vector paths

No card face is drawn by hand and none is a bitmap. `CardFaceComposer` builds every face at
start-up from the Assets art: the art window's ground, the card's picture inlined and
clipped to its view box, the Deco frame stretched over both, a colored band by kind, the
title, the cuneiform line and two or three lines of text, and the number badges and Her
star. `SvgInliner` moves each picture's children under a fitting transform and prefixes its
ids so two inlined pictures never collide. `VectorText` sets the lettering as glyph
outlines from the embedded Merriweather and Noto Sans Cuneiform faces, with Merriweather
covering the modifier letters the cuneiform face lacks, so every face is self-contained:
no font and no external reference, the same on any machine and in any renderer. The faces
are composed on a worker thread once per process, and the host registers them on the table
a few at a time within a budget per step while a progress bar fills. Read
`Cards/CardFaceComposer.cs`, `Cards/VectorText.cs`, `Cards/SvgInliner.cs` and `Prepare` in
the host. See
[Compose card faces as SVG with every letter a vector path](../BLUEPRINTS-GameEngine.md#compose-card-faces-as-svg-with-every-letter-a-vector-path).

### The Art Deco chrome

The look is 1920s Art Deco: gold rules on night blue and lapis, sunbursts, stepped corners
and chevrons, tall tracked capitals. All of it is drawn from the `deco` pieces of the Assets
art, which are plain SVG like the rest. On the table, `DecoPieces` rasterizes each piece
through the table's artwork cache, measures its drawn part once, and stretches or fits it
onto table rectangles - the board tile, the plates under the rows, the slot plates, the dice
tray, the corner brackets - while `HudPainter` draws the tracked capitals letter by letter in
Merriweather. In the XAML panes the view model renders the drawn wordmark, the masthead,
the badges and the ornaments to bitmaps once, and `App.xaml` carries the same palette as
brushes, so the panes and the table agree. The hand-drawn art of the cards is not restyled;
only the frames and the furniture around it are Deco. Read `Hud/DecoPieces.cs`,
`Hud/HudPainter.cs` (`Board`, `Plate`, `Tracked`), `LoadChromeArtAsync` in the view model and
the resources at the top of `App.xaml`.

### The newspaper inspector

Every card the game turns up can open large as a page of the Warka Herald, and which
layout a page takes follows the card's kind: a Discovery is an EXTRA! edition with a banner
headline, a sub-head, a byline, the story in two columns, the
art framed as a photograph and the WHERE IT IS NOW and THE RECORD boxes; a Season card is a
front page, a Tablet a Learned Society column and a Favor a small notice. The host decides
when a page opens - a team's discovery, a Favor, a new season, a right-click - and builds a
`CardView` on a worker thread, with the face and the art rendered to PNG there, then hands it
to `IInspectorBridge.ShowCard` on the UI thread. The view model implements the bridge, sets
its newspaper properties from the view and starts the page's auto-close timer; the page
slides the newspaper in and tells the view model when the pointer is over it, which holds
the timer. A computer turn waits while a page is open. Each Discovery and Season has
several editions in the content, and `Editions` picks the one a game prints from a stable
hash of the game's seed and the card's id, so a game prints the same edition of a card
everywhere - the inspector, the season banner, the journal and the PDF - and another game
usually prints another; a byline the edition lacks is picked from the paper's desks by the
same hash. Read `ShowCard` and `OnEventPresented` in the host, `Bridges/CardViews.cs`,
`Cards/Editions.cs`, `Catalog.HeadlineFor` in the Rules content, and
`MainViewModel.Inspector.cs`.
See
[Open a newspaper-style inspector over a game canvas through a bridge the view model implements](../BLUEPRINTS-MVVM.md#open-a-newspaper-style-inspector-over-a-game-canvas-through-a-bridge-the-view-model-implements).

### The rules engine apart from the engine

`GoddessTempleDiscovery.Rules` is the game: the card records, the content of every card, the
engine, the scoring, the computer brain and the journal, in a library with no package
reference. The engine rolls its own dice from its own seeded generator, validates every
action before it applies it and throws without changing anything when one is not legal,
lists the legal actions in a stable order, previews a dig or a report without applying it,
raises events for whatever happened, and hashes its whole state. The presentation never
decides a rule: `TableSession.Apply` passes the action to the engine and moves cards only
for the events that come back, the HUD enables its buttons from the legal actions, and the
labels under the trenches read their Dig Numbers from the engine's dig preview. The tests
play full seeded games with the computer brain against small made-up catalogs and pin the
action count, the scores and the state hash in one assertion. Read `Engine/GameEngine*.cs`,
`Session/TableSession.cs` and `tests/libs/GoddessTempleDiscovery.Rules.Tests/Engine/GameEngineDeterminismTests.cs`.
See
[Keep a turn-based rules engine package-free and pin seeded games with a state hash](../BLUEPRINTS-GameEngine.md#keep-a-turn-based-rules-engine-package-free-and-pin-seeded-games-with-a-state-hash),
[Force the table's dice to show the rules engine's roll](../BLUEPRINTS-GameEngine.md#force-the-tables-dice-to-show-the-rules-engines-roll)
and
[Play a rules engine's events on the table one step at a time and pace the computer turns](../BLUEPRINTS-GameEngine.md#play-a-rules-engines-events-on-the-table-one-step-at-a-time-and-pace-the-computer-turns).

### The computer temperaments

`ComputerBrain` is one decision procedure with three sets of weights. In the spending phase
it scores every legal action - the points a dig is worth now and what it costs in dice,
Workers and Tablets, the value of a specialist to this temperament fading as the game runs
on, the draws of a study, whether a survey clears a trench the team cannot reach, and how
much a report gains over leaving the finds in the crates - adds a small jitter, and takes
the best, ending the turn when nothing scores above zero. The temperaments are the weights
in `TemperamentWeights`, and they double as the difficulty of the opponents on purpose: the
Deep Digger wins most often, the Surveyor plays steadily, and the Scholar is the gentle
opponent a new player should meet first. The jitter is seeded from the game's seed, so a
seeded game plays its computer turns the same way every time. Read `Brains/ComputerBrain.cs`
and `Brains/TemperamentWeights.cs`, and section 10 of `DESIGN.md`.

### The Field Journal PDF

`JournalPdfBuilder` draws the journal page by page straight onto `XGraphics`, in the manner
of InannaRosette's report: a cover with the masthead and the wordmark, then the game's
editions in the order they were read - a front page for each season, an EXTRA! page for each
discovery with its headline, columns, framed art and boxes, and the Tablets, Favors,
specialists and reports as notices between them - then the Final Edition with the results,
the Note on Cultural Heritage and History, and the sources. Every picture, the masthead and
the ornaments are drawn as vector shapes by `SvgToPdf`, which walks the same SVG the table
shows and hands each path to `SvgPathToPdf`, so the art stays sharp at any zoom, and
`JournalFonts` registers the embedded faces with the PDF font system once per process so
the text is embedded as subsets. The builder returns the bytes, the page count and every
line it drew, which the tests read. Read `Journal/JournalPdfBuilder.cs`,
`Journal/Pdf/SvgToPdf.cs` and `Journal/Pdf/JournalFonts.cs`, and `ExportJournalAsync` in
`MainViewModel.Journal.cs` for the save picker. See
[Draw whole SVG pictures onto a PDF page as vector shapes](../BLUEPRINTS-DocumentsAndData.md#draw-whole-svg-pictures-onto-a-pdf-page-as-vector-shapes)
and
[Register embedded OFL fonts with the PDF font system](../BLUEPRINTS-DocumentsAndData.md#register-embedded-ofl-fonts-with-the-pdf-font-system).

### The gallery

The Gallery turns the Assets art into a browsable collection without a catalog file of its
own. `ArtInfo` reads each picture's header comment - its Title, Subject and "Sources
consulted" lines - and sorts the pictures into the catalog's folders, with the plans and the
icons set apart. The view model filters them by the chosen category, shows them a page at a
time as `GalleryItem` cell view models, each with its own open command, and renders each
thumbnail on a worker through `SvgRaster` the first time it shows, keeping it for the rest of
the run; a click shows the picture large, rendered again at a larger size, with its title,
subject, sources and the art's license line from `ArtCatalog.LicenseText`. The title screen
opens it with a bound XAML command, and the table's header opens it with a HUD button that
the host forwards through `ISessionBridge.PaneRequested`. While it is open the view model
reports a pane over the table, so the table takes no clicks, and the page passes the keys
the game surface did not take to the view model, which pages the gallery on the arrows and
closes the topmost layer on Escape. Read `Rendering/ArtInfo.cs`, `MainViewModel.Gallery.cs`,
the gallery pane in `MainPage.xaml` and `tests/GoddessTempleDiscovery.PlayTests/GalleryTests.cs`.
See
[Give each grid cell its own command and lazily loaded thumbnail](../BLUEPRINTS-MVVM.md#give-each-grid-cell-its-own-command-and-lazily-loaded-thumbnail).

### MVVM chrome over a game canvas

The page is a `GameSurfaceCanvas` with the XAML panes stacked over it in one grid: the
title, the setup, the prologue, the epilogue, the Final Edition, How to Play, History and
Credits, the Field Journal, settings and gallery panes, and the newspaper. One view model owns all of
them and the game host. It implements four interfaces - `IManageGameCanvas` for the page,
`IInspectorBridge` and `ISessionBridge` for the host, `IJournalFileBridge` for the save
picker - and the page wires each through its interface, never the concrete type. The host
reports seasons, standings, the ticker, the journal and the end of the game through
`ISessionBridge` on the UI thread; the view model turns them into bound properties and
collections. Its commands stay on the XAML side until a game has to change: Begin and the
autoplay hand the host a setup, and the host starts the game on the engine thread; closing
the newspaper tells the host, so a waiting computer turn goes on. Which pane shows is one
field and a set of computed visibilities. Read `MainViewModel.cs` and its partial files, and
`MainPage.xaml.cs`, which keeps to the canvas wiring, the save picker and the newspaper's
slide. See
[Hand the view model a game canvas at its first real layout size](../BLUEPRINTS-GameEngine.md#hand-the-view-model-a-game-canvas-at-its-first-real-layout-size)
and
[Assign every bridge through the interface that declares it](../BLUEPRINTS-PlatformServices.md#assign-every-bridge-through-the-interface-that-declares-it).

### What this application does not show

- No gamepad, no music and no recorded audio of its own. The only sounds are the
  CardsAndDice add-on's card and dice sounds, and its card-slide sound reused as the paper
  rustle when the newspaper opens.
- No dragging. The table's drag is switched off; everything is a click, a right-click or a
  key, and the setup and settings panes are bound XAML controls.
- No networked play. Two to four teams share one table and one screen; every human seat
  plays on the same machine.
- No saved games. The settings and the last setup are kept; a game in progress is not, and
  closing the window ends it.
- No photographs. Every picture is vector art; the traced plates are vector tracings of
  public-domain photographs and engravings, not the images themselves.
- No LinuxFrameBuffer or WinWpfSkia head. The application builds four of the six heads.
- No downloads and no accounts; nothing is fetched at run time.

## Third-party content

[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) in this folder is the attribution record
for this application: the embedded Merriweather and Noto Sans Cuneiform fonts under the SIL
Open Font License, the CardsAndDice add-on's card back, dice and Kenney sounds that reach the
screen through its package, the public-domain sources of the traced plates, the
quotations from the Electronic Text Corpus of Sumerian Literature and Oracc with their terms,
and the museum catalogues, CDLI records and other online sources the cards quote or draw on.
The art in `src/libs/GoddessTempleDiscovery.Assets/Art/` is original work under the Apache
License, Version 2.0, and its own
[ART-LICENSE.md](src/libs/GoddessTempleDiscovery.Assets/Art/ART-LICENSE.md) says how to reuse
it. Every code dependency arrives as a NuGet package carrying its own license and notices,
and nothing is downloaded at run time.

## License

GoddessTempleDiscovery is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
