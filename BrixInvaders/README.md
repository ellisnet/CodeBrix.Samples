# BrixInvaders

BrixInvaders is a Space Invaders game for the desktop. An alien fleet marches back and
forth across the top of the screen, stepping down a row every time it reaches an edge
and speeding up as it thins out; you fly a small ship along the bottom and shoot it
down before it lands. The campaign is five sectors of six waves each, and every sector
ends in a boss built from station parts, with turrets, cannons and missile bays you
shoot off one by one before its core is exposed. Each sector adds one new trick and
keeps the old ones - a bonus UFO, raiders that dive out of the formation, shielded
ships and meteor showers, homing missiles, and finally a formation that splits in two -
and after the fifth sector the whole campaign loops, faster and with tighter fire, for
as long as you last. Power-ups drop from destroyed ships, a chain multiplier rewards
shooting without missing, four difficulty levels change almost everything, and each
difficulty keeps its own top-ten high-score table. It plays with the keyboard and a
gamepad at the same time, and every picture, sound and letter on screen comes straight
out of five Kenney asset zips shipped exactly as they were downloaded, while the music
is written live, as you play, by a music model.

It is the CodeBrix.Samples reference for a complete game on the CodeBrix.Platform
GameEngine: a page that is nothing but a game canvas, a view model that owns the game
host, a pure rules engine with no engine types in it, a Kenney asset library read
straight from its zips, gamepads through the engine's SDL2 add-in, endless generated
music through the engine's generated-music add-in, and settings and high scores kept
through the AppSettings add-in. GameEngineMusicDemo, a sibling application, is the
reference for the music system underneath; KenneyAssetBrowser is the one for looking
inside a Kenney zip before building anything from it.

## How to play

| Action | Keyboard | Gamepad (Classic profile) | Gamepad (Shoulder profile) |
| --- | --- | --- | --- |
| Move | A and D, or Left and Right | Left stick or D-pad | Left stick or D-pad |
| Fire (hold for auto-fire) | Space | A | Right shoulder |
| Bomb | Left Shift | B | Left shoulder |
| Pause | Escape | Start | Start |
| Open the Kenney bundle link (on the Kenney cards) | K | Y | Y |
| Menus: move, confirm, back | Arrows, Enter, Escape | D-pad or stick, A, B | D-pad or stick, A, B |

Both devices work at once, and the prompts on screen switch between keyboard and
gamepad glyphs to match whichever one you touched last. A gamepad can be plugged in at
any time; without one the game is simply keyboard-only. The mouse clicks the Kenney
bundle card and the links on the credits screen.

- **A game.** Play from the title leads through ship select (three hull shapes in four
  colors, purely cosmetic), difficulty select - Cadet, Pilot, Ace or Legend, which set
  your lives and bombs, the enemy fire rate and bolt speed, how fast the formation
  drops, boss health, how often power-ups drop and a score multiplier - and a sector
  briefing that names the sector's new threat. Clearing a sector on a difficulty
  unlocks starting at the next one on that difficulty.
- **Enemies.** Five ship shapes are five roles: grunts, shooters that aim at you,
  shielded ships that take two hits, divers that swoop down and return to their slots,
  and missile carriers. Four colors are four point tiers, worth more toward the top
  rows, and a ship shot while it is out of formation is worth double.
- **Power-ups.** Spread shot, rapid fire, a piercing laser, a shield bubble that stacks
  up to three hits, a speed boost, an extra life and an extra bomb. The timed ones run
  together and the HUD shows each with its time left. A bomb clears every enemy bolt,
  missile and meteor on screen and hits every ship in the formation once. The ship
  shows what it is carrying: gun pods on the wings for spread shot, a nose cannon for
  rapid fire, beam emitters for the piercing laser and extra engines for speed boost,
  all Kenney parts, all gone when the power-up runs out.
- **Bosses.** A boss warning sounds after the sixth wave. The boss weaves across the
  top, its core armored until every other section is destroyed, and it changes its
  attack pattern as it loses health; a health bar at the top of the screen tracks it.
- **Scoring.** Every hit adds to a chain, and every ten in a row raises the multiplier,
  up to five times; a shot that leaves the screen without hitting anything breaks the
  chain. Game over with a qualifying score asks for three letters or digits, entered
  with either device, and the table is saved at once.
- **Settings.** Master, music and effects volume; the music model and the instrument
  library that plays it; the gamepad profile; the default ship and difficulty; and
  resetting the high scores. The settings screen also says whether a gamepad is
  connected.
- **Attract mode.** Left idle on the title, the game plays a demo wave by itself; any
  input returns to the title.
- **Menus.** Arrows, D-pad or stick move the cursor; hold a direction to scroll, which
  is how the three-letter high-score name is entered quickly.
- **Pause.** The game freezes under a menu with Resume and Quit to title, and the music
  keeps playing quietly underneath it rather than stopping. Minimizing the window pauses
  everything, music included, and the pause menu is waiting when the window comes back.

`DESIGN.md` in this folder has every number the rules use - hit boxes, speeds,
intervals, point values, the difficulty table, the boss section layouts, the power-up
weights, the settings keys and the per-sector music table.

## What this sample shows a CodeBrix.Platform developer

- Hand the view model the game canvas at its first real layout size, through a
  one-method interface the page calls from the canvas's first-started event:
  [Hand the view model a game canvas at its first real layout size](../BLUEPRINTS-GameEngine.md#hand-the-view-model-a-game-canvas-at-its-first-real-layout-size).
- Choose the render tier and pin the render resolution to a fixed playfield before
  anything reads the canvas's host, and let the engine letterbox the playfield into any
  window and hand link clicks over in playfield pixels:
  [Pin a fixed playfield size and let the engine letterbox drawings and clicks](../BLUEPRINTS-GameEngine.md#pin-a-fixed-playfield-size-and-let-the-engine-letterbox-drawings-and-clicks).
- Build the game host's seams in a view model with no bindings and no commands, and
  close the application on the UI thread when the game asks to quit:
  [Build a game host's seams in the view model and close the application from its quit event](../BLUEPRINTS-MVVM.md#build-a-game-hosts-seams-in-the-view-model-and-close-the-application-from-its-quit-event).
- Attach the engine's window helper in one call, so the whole engine pauses while the
  window is minimized and keyboard focus comes back on activation, and come back to the
  game's own pause menu:
  [Pause the engine while the window is minimized with GameWindowLifecycle and come back to the game's pause menu](../BLUEPRINTS-GameEngine.md#pause-the-engine-while-the-window-is-minimized-with-gamewindowlifecycle-and-come-back-to-the-games-pause-menu).
- Ship the Kenney zips exactly as they were downloaded, register them in one call,
  check every key the game uses and credit every pack from the provider, and load
  sprites from their atlases, sounds and fonts by asset key:
  [Register downloaded Kenney zip files with RegisterKenneyAssets and load sprites, sounds and fonts from them](../BLUEPRINTS-GameEngine.md#register-downloaded-kenney-zip-files-with-registerkenneyassets-and-load-sprites-sounds-and-fonts-from-them).
- Start endless generated music with one call, move it on at a bar line for each
  sector and boss, and start a fresh session only when the player changes the model or
  the instruments:
  [Start endless generated music with one call](../BLUEPRINTS-GameEngine.md#start-endless-generated-music-with-one-call).
- Put the whole music policy behind one interface with a silent default, reach the
  engine only through its own music interfaces, and pin every rule with a fake of
  those and a recording director:
  [Put the music policy behind an interface and test it against fakes of the engine's music interfaces](../BLUEPRINTS-GameEngine.md#put-the-music-policy-behind-an-interface-and-test-it-against-fakes-of-the-engines-music-interfaces).
- Duck the music under the pause menu instead of suspending it, play the game-over
  stinger on the effects bus with a deeper duck held until the title, and release both
  on every way out:
  [Duck the music for a pause menu and hold a game-over duck with PlayStingerWithHeldDuck until the title](../BLUEPRINTS-GameEngine.md#duck-the-music-for-a-pause-menu-and-hold-a-game-over-duck-with-playstingerwithheldduck-until-the-title).
- Keep each sector's presets and tempo in a table of plain data, offer the models
  and instrument libraries by their registry names on the settings screen, and let
  tests hold the table to the packages' preset lists:
  [Keep the music for each level in a table the tests can read](../BLUEPRINTS-GameEngine.md#keep-the-music-for-each-level-in-a-table-the-tests-can-read).
- Credit the model and instruments that are really playing, including when the
  model is warming up or missing:
  [Show the player which model and instruments are really playing](../BLUEPRINTS-GameEngine.md#show-the-player-which-model-and-instruments-are-really-playing).
- Pin the audio output format before the first effect is preloaded, so the effects and
  the generated music agree on one rate:
  [Pin the audio device format before anything plays](../BLUEPRINTS-GameEngine.md#pin-the-audio-device-format-before-anything-plays).
- Duck the music under the boss warning with the timed form of a duck, and hold it
  down on the game-over screen and under the pause menu with the handle form:
  [Duck the music for exactly as long as a line lasts](../BLUEPRINTS-GameEngine.md#duck-the-music-for-exactly-as-long-as-a-line-lasts).
- Bind named actions to keys, gamepad buttons, D-pad and stick directions with the
  engine's input-action map, read the keyboard and every connected gamepad once per
  fixed step without losing a short tap, and let the last device used choose the
  on-screen prompts:
  [Read keyboard and gamepad together through an InputActionMap and switch on-screen prompts](../BLUEPRINTS-GameEngine.md#read-keyboard-and-gamepad-together-through-an-inputactionmap-and-switch-on-screen-prompts).
- Repeat a held menu direction at a steady rate and never read a released stick's
  spring-back as a push the other way:
  [Repeat a held menu direction with InputRepeat and ignore a stick springing back](../BLUEPRINTS-GameEngine.md#repeat-a-held-menu-direction-with-inputrepeat-and-ignore-a-stick-springing-back).
- Compose a splash picture from the game's own assets at start-up, show it through the
  engine's splash overlay and hand over to the title when it finishes or the player
  skips it:
  [Show a splash card and hand over to a title screen](../BLUEPRINTS-GameEngine.md#show-a-splash-card-and-hand-over-to-a-title-screen).
- Give every screen a painter of its own that follows the screen state machine, and
  test the screens by reading the draw lists they produce:
  [Paint each game screen with its own painter and test the screens as draw lists](../BLUEPRINTS-GameEngine.md#paint-each-game-screen-with-its-own-painter-and-test-the-screens-as-draw-lists).
- Build each frame into two engine draw lists on the engine thread and paint their
  published copies from two engine draw-list drawings, one under the particles and
  one over everything:
  [Build each frame as engine DrawList commands and paint only the published copy](../BLUEPRINTS-GameEngine.md#build-each-frame-as-engine-drawlist-commands-and-paint-only-the-published-copy).
- Show each active power-up on the player's ship with Kenney parts layered under and
  over the hull:
  [Show power-ups on the player's ship by layering sprite parts over the hull](../BLUEPRINTS-GameEngine.md#show-power-ups-on-the-players-ship-by-layering-sprite-parts-over-the-hull).
- Use the engine's particles on a pixel layer (no tile grid) between the game's own
  world and overlay drawings, driven from the rules' events:
  [Stack engine particles on a pixel layer between your own world and overlay drawings](../BLUEPRINTS-GameEngine.md#stack-engine-particles-on-a-pixel-layer-between-your-own-world-and-overlay-drawings).
- Keep the rules in a library with no engine, no Skia and no I/O, stepped at a fixed
  rate from the engine's fixed-step hook and fenced by golden-seed tests:
  [Keep a deterministic game simulation apart from the engine and step it from OnFixedUpdate](../BLUEPRINTS-GameEngine.md#keep-a-deterministic-game-simulation-apart-from-the-engine-and-step-it-from-onfixedupdate).
- Keep every setting and high score behind one application-named facade over the
  AppSettings add-in, opened first thing in the `App` constructor:
  [Wrap the AppSettings add-in in one application named facade](../BLUEPRINTS-SettingsAndPersistence.md#wrap-the-appsettings-add-in-in-one-application-named-facade)
  and
  [Open the settings store before any other startup work](../BLUEPRINTS-SettingsAndPersistence.md#open-the-settings-store-before-any-other-startup-work).
- Give the settings screen a menu model that changes the stored value and says
  what changed, and let the session route each kind of change to the mixer, the
  music or the next game:
  [Let a menu model change stored settings and report what changed](../BLUEPRINTS-SettingsAndPersistence.md#let-a-menu-model-change-stored-settings-and-report-what-changed).
- Give every stored value a typed, validated property, keep the high-score tables
  as one JSON value per difficulty, and let the game reach all of it through an
  interface the tests implement in memory:
  [Type and validate every stored value behind an interface the game can fake](../BLUEPRINTS-SettingsAndPersistence.md#type-and-validate-every-stored-value-behind-an-interface-the-game-can-fake).
- Open the settings store in a fresh temporary folder for each test that needs it,
  deleted when the test is done, so no test ever reaches the player's high scores:
  [Point a process-global store at a throwaway folder in tests](../BLUEPRINTS-Testing.md#point-a-process-global-store-at-a-throwaway-folder-in-tests).
- Walk the whole game session through its screens in unit tests, with a
  recording fake for every seam it takes and a driver that presses the same
  buttons a player does:
  [Drive a game session through recording fakes for every seam it takes](../BLUEPRINTS-Testing.md#drive-a-game-session-through-recording-fakes-for-every-seam-it-takes).
- Read every asset key back out of the constants class by reflection and prove
  each one against the real zips copied beside the test binary:
  [Read every asset key back by reflection and prove each one against the real zips](../BLUEPRINTS-Testing.md#read-every-asset-key-back-by-reflection-and-prove-each-one-against-the-real-zips).
- Open a web link from the game through a small interface the view model supplies,
  with a no-op default that only logs:
  [Put a platform service behind an interface with a no-op default](../BLUEPRINTS-PlatformServices.md#put-a-platform-service-behind-an-interface-with-a-no-op-default)
  and
  [Open a URL in the default browser from a view model](../BLUEPRINTS-PlatformServices.md#open-a-url-in-the-default-browser-from-a-view-model).
- Open a link from the engine thread through the engine's link helper, answer with a
  task, and test it with a recorder standing in for the helper:
  [Open a link from the game engine thread with ExternalLinks and answer with a task](../BLUEPRINTS-PlatformServices.md#open-a-link-from-the-game-engine-thread-with-externallinks-and-answer-with-a-task).
- Drive the whole game unattended from an environment switch - an autopilot that
  plays through the player's own input path, with its settings and scores kept in a
  scratch store chosen where the store first opens - and read what happened from the
  log:
  [Let an autopilot play through the player's input path and keep its saves apart](../BLUEPRINTS-Testing.md#let-an-autopilot-play-through-the-players-input-path-and-keep-its-saves-apart)
  (compare
  [Drive a running application from an environment variable and report to the log](../BLUEPRINTS-Testing.md#drive-a-running-application-from-an-environment-variable-and-report-to-the-log),
  a hook that runs whatever script the variable holds).
- Let the game library own the engine packages and the Core project depend on it, with
  a root namespace of its own so the generated resources type is not declared twice:
  [Give a library that references CodeBrix Platform its own root namespace](../BLUEPRINTS-ProjectLayoutAndPackaging.md#give-a-library-that-references-codebrix-platform-its-own-root-namespace).
- See what one package reference brings with it, and name a transitive package
  directly when your code registers it:
  [Know what a transitive package brings and name what you depend on](../BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on).
- Copy data beside every head's executable from the one library they all reference,
  and find it from the application base directory:
  [Ship a data corpus as content items and find it under the application base directory](../BLUEPRINTS-ProjectLayoutAndPackaging.md#ship-a-data-corpus-as-content-items-and-find-it-under-the-application-base-directory).
- Organize the application as a shared UI project, a Core project, four libraries
  under `src/libs` and mirrored test projects under `tests/libs`:
  [Organize an application as src libs plus tests libs around a shared UI project](../BLUEPRINTS-ProjectLayoutAndPackaging.md#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project).
- Carry the platform, font, hosting and logging packages in the Core project, leave
  the engine packages to the game library, and give each head exactly one platform
  runtime package:
  [Carry every package in one Core library and give each head exactly one runtime package](../BLUEPRINTS-ProjectLayoutAndPackaging.md#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package).
- Keep every head's `Program.Main` to the same few lines, differing only in the call
  that names the platform:
  [Start each head from a Program Main and pick the platform backend](../BLUEPRINTS-AppStructureAndStartup.md#start-each-head-from-a-program-main-and-pick-the-platform-backend).
- Install a console logger only in Debug builds, so the game's diagnostic lines appear
  in a Debug run and a Release run stays quiet:
  [Turn on console logging only in Debug builds](../BLUEPRINTS-AppStructureAndStartup.md#turn-on-console-logging-only-in-debug-builds).
- Write every diagnostic line through one prefixed log that reaches the console
  in Debug builds and a sink the tests capture:
  [Write every diagnostic line through one prefixed log that tests can capture](../BLUEPRINTS-AppStructureAndStartup.md#write-every-diagnostic-line-through-one-prefixed-log-that-tests-can-capture).
- Guard the view model constructor against the XAML designer:
  [Guard a view model constructor for the XAML designer](../BLUEPRINTS-MVVM.md#guard-a-view-model-constructor-for-the-xaml-designer).
- Set up xUnit v3 test projects the family's runner discovers, each library naming
  only its own test assembly in `InternalsVisibleTo.cs`:
  [Set up an xUnit v3 test project for a CodeBrix library](../BLUEPRINTS-Testing.md#set-up-an-xunit-v3-test-project-for-a-codebrix-library)
  and
  [Expose library internals to its test project](../BLUEPRINTS-Testing.md#expose-library-internals-to-its-test-project).
- Record the bundled Kenney packs, fonts and the music models' notices in the
  application's own notices file:
  [Record bundled third-party content in a notices file](../BLUEPRINTS-ProjectLayoutAndPackaging.md#record-bundled-third-party-content-in-a-notices-file).

## Building, running and testing

There is one solution, `BrixInvaders.slnx`, and it holds everything: the shared UI
project, the Core project, four heads, the four libraries under a `Libraries` solution
folder and their test projects under a `Tests` solution folder.

| Head project | Platform | Host-builder call |
| --- | --- | --- |
| `src/BrixInvaders.LinuxX11` | Linux, X11 | `UseLinuxX11()` |
| `src/BrixInvaders.LinuxWayland` | Linux, Wayland | `UseLinuxWayland()` |
| `src/BrixInvaders.MacOS` | macOS | `UseMacOS()` |
| `src/BrixInvaders.Win32Skia` | Windows, Win32 window | `UseWindowsWin32()` |

Every `Program.cs` is the same but for that call: it calls `App.InitializeLogging()`
first, then builds the host with `UseDirectSkiaCanvasMode()` before `Build()`.

Prerequisites:

- The .NET 10 SDK. Every project targets `net10.0`, and no workload is needed.
- All CodeBrix code arrives as packages. No CodeBrix library is referenced as a source
  project, so this folder builds on its own.
- An audio output device to hear the effects and the music, and optionally a gamepad.
  Nothing else: no accounts, no network access at run time, no files to download and
  nothing to install for the music. The Kenney zips are in `assets/kenney`, and the
  music models and the recorded instrument set arrive with their packages and are
  copied beside the executable by the build.

To run one head from the command line, from this folder:

```text
dotnet run --project src/BrixInvaders.LinuxX11
dotnet run --project src/BrixInvaders.LinuxWayland
dotnet run --project src/BrixInvaders.MacOS
dotnet run --project src/BrixInvaders.Win32Skia
```

Four test projects mirror the four libraries. The rules engine's tests cover every
component of the simulation and pin whole games played from fixed seeds; the asset
tests open the real zips, copied beside the test binary, and prove that every asset key
the game uses resolves; the music tests check the registration, the choices and the
per-sector table, and build the music options for every model, instrument library and
sector without loading a model or opening an audio device; the game tests drive the
screen flow, the game's controls over the engine input-action map, the settings
facade (against a throwaway store), the music director and the credits against
fakes. None of them needs a window, a sound card or a gamepad. The repository's
`global.json` selects the Microsoft.Testing.Platform runner, and each test project
runs with:

```text
dotnet test --project tests/libs/BrixInvaders.GameLogic.Tests/BrixInvaders.GameLogic.Tests.csproj
dotnet test --project tests/libs/BrixInvaders.Assets.Tests/BrixInvaders.Assets.Tests.csproj
dotnet test --project tests/libs/BrixInvaders.Music.Tests/BrixInvaders.Music.Tests.csproj
dotnet test --project tests/libs/BrixInvaders.Game.Tests/BrixInvaders.Game.Tests.csproj
```

### Diagnosing a run

Console logging is compiled in only for Debug builds - the body of
`App.InitializeLogging()` sits inside `#if DEBUG` - so run a Debug build to see the
log. Every line the game writes starts with `[BrixInvaders]`, and the word after it says
which part of the game wrote it:

| Prefix | What it reports |
| --- | --- |
| `[BrixInvaders] render tier` | The tier requested and the tier actually running, and the render resolution |
| `[BrixInvaders] settings store:` | Where the settings and high scores are kept |
| `[BrixInvaders] audio:` | The output format pinned at start-up |
| `[BrixInvaders] gamepad:` | Whether gamepad support is available and which controllers are connected |
| `[BrixInvaders] assets:` | One line per Kenney pack read, the provider's warnings, and the provider's check of every asset key the game uses |
| `[BrixInvaders] sounds:`, `fonts:`, `pictures:` | What was loaded from the packs, and anything missing |
| `[BrixInvaders] music:` | What is registered, whether each model's files were found, every state change of the music, what is really playing, and every follow-up |
| `[BrixInvaders] screen:` | Every screen change |
| `[BrixInvaders] splash:` | What the splash card was composed from, or that it could not be shown |
| `[BrixInvaders] new game:`, `game over:` | The setup a game started with; the score, sector and difficulty it ended on |
| `[BrixInvaders] loadout:` | The ship's power-up parts as they change |
| `[BrixInvaders] settings:` | Every stored value the menus change: defaults, music choice, gamepad profile, cleared high scores |
| `[BrixInvaders] attract:`, `autopilot:` | The demo pilot taking over and how its game ended; the hands-off switch being on |
| `[BrixInvaders] performance:` | Frames a second and draw commands, now and then |
| `[BrixInvaders] input:`, `link:`, `high scores:` | Presses on the title, links opened, scores saved |
| `[BrixInvaders] window:` | The window being minimized (the engine pauses; the pause menu waits for the player) |

The engine adds its own lines about the music track and the generated music under the
same run. A missing model shows up as a `music:` line saying that the embedded replay
is playing; a missing zip shows up as an `assets:` warning, or, when none of the zips
is there, as an exception that names the folder it looked in.

Two environment switches change how the game runs, and both are for diagnosis rather
than play:

- `BRIXINVADERS_USE_CPU=1` renders on the CPU tier instead of the GPU tier. The
  `render tier` lines say which tier is live, including when the GPU tier was asked for
  and could not start.
- `BRIXINVADERS_AUTOPILOT=1` plays the game hands-off: it walks the menus and plays real
  games with the attract-mode pilot, so an unattended run reaches waves, bosses, game
  over and the high-score save, and their log lines. It keeps its settings and scores in
  a scratch store under the system temporary folder, so its games never reach the
  player's own. Keyboard and gamepad still work alongside it.

The player's own settings store is created on the first run, in a `CodeBrix/BrixInvaders`
folder under the user's per-user application-settings location.

## How the projects and folders are organized

```text
BrixInvaders/
  BrixInvaders.slnx                     The one solution: UI, Core, four heads, four libraries, four test projects
  DESIGN.md                             The game design, with every number the rules use
  THIRD-PARTY-NOTICES.txt               Third-party content used by this application
  assets/
    kenney/                             The five Kenney zips as downloaded, plus Kenney's bundle promo picture
  src/
    BrixInvaders.UI/                    Shared items project: the XAML every head compiles
      App.xaml, App.xaml.cs             Settings store first, then fonts, service resolver, design mode, window and frame
      Views/MainPage.xaml(.cs)          One full-window game canvas; forwards its first start to the view model
    BrixInvaders.Core/                  Class library; carries the framework, font, hosting and logging packages
      BrixInvaders.Core.csproj          RootNamespace BrixInvaders; copies assets/** beside every head's executable
      Helpers/HostHelper.cs             The IHostBuilderProvider SimpleServiceResolver builds its container from
      ViewModels/MainViewModel.cs       Owns the game host; declares and implements IManageGameCanvas
    BrixInvaders.LinuxX11/              Head: Program.cs plus a csproj with one runtime package
    BrixInvaders.LinuxWayland/          Head: Program.cs plus a csproj with one runtime package
    BrixInvaders.MacOS/                 Head: Program.cs plus a csproj with one runtime package
    BrixInvaders.Win32Skia/             Head: Program.cs plus a csproj with one runtime package
    libs/
      BrixInvaders.GameLogic/           The rules engine: no package, no engine, no Skia, no I/O
        Simulation/                     GameSimulation, its setup, events, stage phases and state hash
        Screens/                        The screen state machine, menu input and name entry
        Formation/, Enemies/, Bosses/   The marching formation, enemy behaviors and multi-section bosses
        Player/, Projectiles/, PowerUps/, Hazards/, Collision/, Scoring/, Difficulty/, Sectors/, Attract/, Core/
      BrixInvaders.Assets/              The Kenney zips: pack names, every asset key, one-call registration, typed loaders
        Packs/, Keys/, Loading/, Audio/, Fonts/
      BrixInvaders.Music/               Model and instrument choices, the per-sector music table, the options builder
        Choices/, Sectors/, Setup/
      BrixInvaders.Game/                The game host and everything drawn, heard and pressed
        Hosting/                        BrixInvadersGameHost, start-up, the game log
        Session/                        GameSession: screens, simulations and commands; the autopilot
        Screens/, Hud/, Rendering/      One painter per screen, the HUD, draw lists, particles, the splash
        Input/                          The action names, binding profiles and input numbers
        Audio/                          The sound table and the generated-music director
        Settings/                       The settings facade over the AppSettings add-in
        Credits/, Links/                Credits content and the link opener seams
  tests/
    libs/
      BrixInvaders.GameLogic.Tests/     Mirrors src/libs/BrixInvaders.GameLogic, golden-seed runs included
      BrixInvaders.Assets.Tests/        Mirrors src/libs/BrixInvaders.Assets, against the real zips
      BrixInvaders.Music.Tests/         Mirrors src/libs/BrixInvaders.Music
      BrixInvaders.Game.Tests/          Mirrors src/libs/BrixInvaders.Game, with fakes for sound, music and links
```

Dependency direction is strictly one way. Each head references `BrixInvaders.Core`
and file-links the shared UI through an `Import` of `BrixInvaders.UI.projitems`, so
`App.xaml` and `MainPage.xaml` are compiled into every head. `BrixInvaders.Core`
references the four libraries. `BrixInvaders.Game` references the other three and owns
the engine packages; `BrixInvaders.Assets` and `BrixInvaders.Music` each reference only
what their own job needs; `BrixInvaders.GameLogic` references nothing at all. Nothing
flows back: the rules engine knows nothing about the engine, the libraries know nothing
about the view model, and the Core project knows nothing about the UI.

## Five zip files, nothing extracted

Kenney publishes game assets as zip files, and this game ships five of them exactly as
they come off the download page: Space Shooter Remastered (the ships, enemies, lasers,
effects, meteors, power-ups, HUD pictures, backgrounds, the two KenVector Future fonts
and a few sounds), Space Shooter Extension (the boss parts, missiles and smoke),
Planets, Sci-Fi Sounds and Digital Audio. Nothing is unpacked, renamed, converted or
repacked, and no asset file of any kind sits beside them - the only other file in
`assets/kenney` is Kenney's own promotional picture for the full asset bundle, which
the Kenney cards show.

One call registers all five with the engine's Kenney asset provider, which reads each
zip where it lies and gives every file in it a key. After that the game asks the engine
for a sprite atlas, a sound or a font by key, exactly as it would ask for anything
else, and the frames of the packs' own sprite-sheet atlases are addressed by the names
Kenney gave them. `BrixInvaders.Assets` keeps every key the game uses in one place, and
its tests prove each one still resolves, so a key that goes wrong fails a test rather
than a frame.

That is the point of the sample: download the zips, drop them in a folder, and start
building the game. The sector briefing, the sector-clear screen and the credits carry a
Kenney card that says so - "Ships, sounds and planets by Kenney - kenney.nl - CC0" and
"everything on this screen came straight out of five zip files" - with the bundle
picture, which opens <https://kenney.itch.io/kenney-game-assets> when you click it or
press K or Y. The credits name every pack and link to <https://kenney.nl> and to
Kenney's Patreon at <https://www.patreon.com/kenney/>. Kenney's content is CC0, so the
credit is not required; it is given anyway, everywhere Kenney's work is on screen.

## Three package references and one call

The music is not a file. A music model writes it while the game runs, an instrument
library plays it, and the engine pulls it through its own music bus, so the music
volume slider, fades and ducking all work on it as they would on a recording. The
recipe is three package references - the engine's generated-music add-on, a model and
an instrument library - one `Register()` line for each model and library at start-up,
and one `UseGeneratedMusic` call. The music fades in over the title and never ends.

BrixInvaders references both models and both instrument libraries, so the player can
choose on the settings screen: SkyTNT, which writes electronica with drums, or MuPT,
which writes folk and classical tunes in parts with no drums; played through
ModestSynthGm, a synthesized General MIDI set, or FluidR3Gm, a recorded one. The
defaults are SkyTNT through ModestSynthGm. Each sector has a preset of its own and each
boss switches to another, and those changes are follow-ups on the same music session,
which take over at the next bar line rather than restarting the music. Only changing
the model or the instrument library on the settings screen starts a fresh session.

If no model is registered or its files are missing, the music package plays its
embedded replay instead - a piece a model wrote once, the same every time - and a log
line says so. The game never waits for the music: while a model loads, the title is
silent for a moment, and if the machine cannot write music as fast as it plays, the
music waits for the next phrase rather than the game waiting for the music.

The two models and the recorded instrument set are by far the heaviest things this
application ships: each is a large file copied beside the executable by the build. The
synthesized instrument set and the embedded replay weigh almost nothing, so a game that
needs to stay small can reference one model, or none, and skip the recorded
instruments.

## CodeBrix libraries and add-ins used

| Library or add-in | What it does in this application | Where |
| --- | --- | --- |
| CodeBrix.Platform | The application framework: `Application`, `Window`, `Frame`, `Page`, the default-font feature configuration, and the "Simple" toolkit (`SimpleViewModel`, `SimpleServiceResolver`, `IHostBuilderProvider`, `CodeBrixPlatformHostBuilder`) | `src/BrixInvaders.Core/BrixInvaders.Core.csproj`, `src/BrixInvaders.UI/`, `src/BrixInvaders.Core/ViewModels/MainViewModel.cs` |
| CodeBrix.Platform runtime for each head | Exactly one runtime package per head supplies that head's windowing and Skia surface | The four head csproj files and their `Program.cs` |
| CodeBrix.Platform.Fonts.Merriweather | The default XAML font; the game itself draws its text in Kenney's fonts | `src/BrixInvaders.Core/BrixInvaders.Core.csproj`, `src/BrixInvaders.UI/App.xaml.cs` |
| CodeBrix.Platform.GameEngine | The engine loop, `CodeBrixGameHost`, `GameSurfaceCanvas` and its CPU and GPU render tiers, scenes, direct drawings, the draw lists the game paints every frame into, particles, the splash overlay, the input-action map, the window lifecycle helper, the link helper, the audio system, the effects voice pool and the music manager. One package supplies both the engine core and the Host layer | `src/libs/BrixInvaders.Game/`, `src/libs/BrixInvaders.Assets/`, `src/BrixInvaders.UI/Views/MainPage.xaml`, `src/BrixInvaders.UI/App.xaml.cs` |
| CodeBrix.Platform.GameEngine.KenneyAssets | Reads the five Kenney zips where they lie and serves their sprites, atlases, sounds and fonts by asset key | `src/libs/BrixInvaders.Assets/Loading/BrixInvadersAssets.cs`, `src/libs/BrixInvaders.Assets/Keys/AssetKeys.cs` |
| CodeBrix.Platform.GameEngine.Sdl2 | Gamepads, hot-plugged at any time | `src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs`, `src/libs/BrixInvaders.Game/Input/GameControls.cs` |
| CodeBrix.Platform.GameEngine.GeneratedMusic | Endless generated music on the engine's music bus, started with one call and moved on with follow-ups | `src/libs/BrixInvaders.Music/Setup/MusicSetup.cs`, `src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs`, `src/BrixInvaders.Core/ViewModels/MainViewModel.cs` |
| CodeBrix.Audio.MusicGeneration.SkyTNT and CodeBrix.Audio.MusicGeneration.MuPT | The two music models the player chooses between | `src/libs/BrixInvaders.Music/BrixInvaders.Music.csproj`, `src/libs/BrixInvaders.Music/Setup/MusicSetup.cs` |
| CodeBrix.Audio.ModestSynth and CodeBrix.Audio.Samples.FluidR3Gm | The synthesized and the recorded General MIDI instrument libraries the player chooses between | `src/libs/BrixInvaders.Music/BrixInvaders.Music.csproj`, `src/libs/BrixInvaders.Music/Setup/MusicSetup.cs` |
| CodeBrix.Platform.AppSettings | Stores the settings and the high-score tables between runs, behind one application-named facade | `src/libs/BrixInvaders.Game/Settings/SettingsService.cs`, `src/BrixInvaders.UI/App.xaml.cs` |
| SilverAssertions | The assertion style in every test project | `tests/libs/` |

Third-party libraries:

| Library | What it does in this application | Where |
| --- | --- | --- |
| SkiaSharp | Composes the splash picture and supplies the colors, typefaces and pictures the game's draw lists carry; it arrives with the engine package, and the asset and game test projects add its Linux native library, which a head would otherwise supply | `src/libs/BrixInvaders.Game/Rendering/`, `tests/libs/BrixInvaders.Assets.Tests/`, `tests/libs/BrixInvaders.Game.Tests/` |
| Microsoft.Extensions.Hosting | `Host.CreateDefaultBuilder()` behind an `IHostBuilderProvider`, which `SimpleServiceResolver` uses to build the container | `src/BrixInvaders.Core/Helpers/HostHelper.cs` |
| Microsoft.Extensions.Logging.Console | The console logger wired into the platform's ambient logger in Debug builds | `src/BrixInvaders.UI/App.xaml.cs` |
| xUnit v3 and Microsoft.Testing.Platform | The test framework and the runner for the test projects | `tests/libs/*/` |

## Worth studying in this application

### A page that is only a canvas, and a view model that owns the game

`MainPage.xaml` holds one `GameSurfaceCanvas` and nothing else: the engine draws the
HUD, the menus and every screen, and letterboxes the fixed playfield into whatever size
the window is. The page forwards the canvas's first start to the view model through
`IManageGameCanvas`, and the view model does the two things that have to happen before
the canvas builds its scene pipeline - choose the render tier and pin the render
resolution - then builds the host with its three seams: the music director, the link
opener that hands web links to the engine's link helper, and the credits content. Quit
on the title raises an event the view model answers by closing the application on the
UI thread. Read `src/BrixInvaders.UI/Views/MainPage.xaml.cs`, then
`CanvasFirstStart` in `src/BrixInvaders.Core/ViewModels/MainViewModel.cs` and
`PrepareCanvas` in `src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs`. See
[Build a game host's seams in the view model and close the application from its quit event](../BLUEPRINTS-MVVM.md#build-a-game-hosts-seams-in-the-view-model-and-close-the-application-from-its-quit-event),
[Pin a fixed playfield size and let the engine letterbox drawings and clicks](../BLUEPRINTS-GameEngine.md#pin-a-fixed-playfield-size-and-let-the-engine-letterbox-drawings-and-clicks),
[Pause the engine while the window is minimized with GameWindowLifecycle and come back to the game's pause menu](../BLUEPRINTS-GameEngine.md#pause-the-engine-while-the-window-is-minimized-with-gamewindowlifecycle-and-come-back-to-the-games-pause-menu),
[Open a link from the game engine thread with ExternalLinks and answer with a task](../BLUEPRINTS-PlatformServices.md#open-a-link-from-the-game-engine-thread-with-externallinks-and-answer-with-a-task)
and
[Show the player which model and instruments are really playing](../BLUEPRINTS-GameEngine.md#show-the-player-which-model-and-instruments-are-really-playing).

### The host's start-up order

`BrixInvadersGameHost` is a `CodeBrixGameHost`, and its overrides run in the order the
engine calls them: open the settings store and pin the audio output, set up the mouse
and gamepad input, register the zips and load the sounds and fonts, load the
atlases and loose pictures, build the scene and the direct drawings (the world layer,
the particle surface and the overlay), create the game session and attach the
input-action map, start the music, and finally show the splash. Each step writes one `[BrixInvaders]`
line, so a run's log reads as the start-up sequence. Read the `CodeBrixGameHost
overrides` region of the host. See
[Stack engine particles on a pixel layer between your own world and overlay drawings](../BLUEPRINTS-GameEngine.md#stack-engine-particles-on-a-pixel-layer-between-your-own-world-and-overlay-drawings)
and
[Write every diagnostic line through one prefixed log that tests can capture](../BLUEPRINTS-AppStructureAndStartup.md#write-every-diagnostic-line-through-one-prefixed-log-that-tests-can-capture).

### A fixed step inside a variable loop

The engine loop runs as fast as it can; the rules advance in fixed steps. The host
turns on the engine's fixed-step hook at the rules' rate, and the engine calls the
host's `OnFixedUpdate` once per step - capped so a stall cannot make the game run away,
and frozen while the engine is paused. Each step reads the input-action map and advances
the game; after a cycle's steps, `OnAfterFixedUpdates` has the screens fill the engine draw lists
and publishes them, and the two drawings paint only the published copies. The simulation never sees a variable
time step, and the renderer never sees a half-updated game. Read `OnFixedUpdate`,
`Step`, `OnAfterFixedUpdates` and `BuildFrame` in the host. See
[Keep a deterministic game simulation apart from the engine and step it from OnFixedUpdate](../BLUEPRINTS-GameEngine.md#keep-a-deterministic-game-simulation-apart-from-the-engine-and-step-it-from-onfixedupdate),
[Build each frame as engine DrawList commands and paint only the published copy](../BLUEPRINTS-GameEngine.md#build-each-frame-as-engine-drawlist-commands-and-paint-only-the-published-copy)
and
[Paint each game screen with its own painter and test the screens as draw lists](../BLUEPRINTS-GameEngine.md#paint-each-game-screen-with-its-own-painter-and-test-the-screens-as-draw-lists).

### Music that follows the game without restarting

`GeneratedMusicDirector` is the whole music policy in one class behind an interface, so
the session's tests drive it with a fake. The session calls it only when the musical
moment really changes - the title, a sector, a boss, game over. A sector or a boss is a
follow-up that takes over at a bar line; the boss warning plays as an engine stinger on
the effects bus with the music ducked under it; the game-over stinger holds a duck until
the title; the pause menu holds a lighter one. The class comment explains why pause is a duck rather than a suspension:
the pause menu is a game pause while the engine keeps running, and a suspended stream
can come back through a moment of silence. The music slider drives the engine's music
bus, and the session's own level stays at full, so the level is applied exactly once.
Read `src/libs/BrixInvaders.Game/Audio/GeneratedMusicDirector.cs` beside
`src/libs/BrixInvaders.Music/Setup/MusicSetup.cs` and
`src/libs/BrixInvaders.Music/Sectors/SectorMusic.cs`. See
[Start endless generated music with one call](../BLUEPRINTS-GameEngine.md#start-endless-generated-music-with-one-call),
[Put the music policy behind an interface and test it against fakes of the engine's music interfaces](../BLUEPRINTS-GameEngine.md#put-the-music-policy-behind-an-interface-and-test-it-against-fakes-of-the-engines-music-interfaces),
[Duck the music for a pause menu and hold a game-over duck with PlayStingerWithHeldDuck until the title](../BLUEPRINTS-GameEngine.md#duck-the-music-for-a-pause-menu-and-hold-a-game-over-duck-with-playstingerwithheldduck-until-the-title)
and
[Keep the music for each level in a table the tests can read](../BLUEPRINTS-GameEngine.md#keep-the-music-for-each-level-in-a-table-the-tests-can-read).

### What this application does not show

- No bindings or commands on the page. Everything the player does is a key, a gamepad
  button or a click on the game surface, so the view model has no bound properties and
  no `SimpleCommand`; the page is a canvas.
- No framebuffer or WPF head. The application builds four of the six heads.
- No networking, no downloads and no accounts; the only web access is a link the
  player chooses to open in a browser.
- No music files. There is no recorded soundtrack; the music is written while the game
  runs.

## Third-party content

`THIRD-PARTY-NOTICES.txt` in this folder is the attribution record for this
application: the five Kenney packs and the fonts that come with one of them, Kenney's
bundle promo picture, and the notices of the music models and the recorded instrument
set, which arrive as packages. Every code dependency arrives as a NuGet package carrying
its own license and notices.

## License

BrixInvaders is licensed under the Apache License, Version 2.0, see
[../LICENSE](../LICENSE).

Copyright (c) 2026 Jeremy Ellis and contributors
