# BrixInvaders - Game Design

This is the design every BrixInvaders library codes against. The numbers here are the numbers in
`BrixInvaders.GameLogic` (the pure, deterministic rules engine); where this document and the code disagree, the
code's unit tests are the tie-breaker and this document gets fixed.

Ships, sounds and planets by Kenney - kenney.nl - CC0.

---

## 1. Playfield and simulation

| Item | Value |
| --- | --- |
| Logical playfield | 1280 x 720 world units, origin top-left, x right, y down |
| Letterboxing | the Game library scales the playfield uniformly to the window and letterboxes the rest |
| HUD band | top 48 units (drawn over the play area; nothing spawns in it) |
| Side margin | 24 units: formations, bosses and the player never cross x < 24 or x > 1256 |
| Fixed step | 1/60 s (`Playfield.FixedStep`); the Game library calls `GameSimulation.Step(dt, input)` once per fixed step |
| Collisions | axis-aligned rectangles (`Box`), centre + half size; touching edges do not collide |
| Randomness | one `GameRandom` (xorshift64*) per game, seeded by `GameSetup.Seed` |
| Determinism | same `GameSetup` + same inputs = same game, step for step (fenced by golden-seed tests) |

`GameSimulation.Events` lists everything that happened in the last step (explosions, sounds, HUD flashes). It is
cleared at the start of every step. The Game library turns events into sprites, sounds and particles; it never
changes simulation state itself.

### Hit boxes (world units)

| Object | Width x height |
| --- | --- |
| Player ship | 64 x 48, centre y fixed at 660 (top edge 636) |
| Formation enemy | 48 x 40 |
| UFO | 64 x 28, flight lane centre y = 76 |
| Player bolt | 6 x 24 |
| Enemy bolt | 6 x 18 |
| Homing missile | 14 x 14 |
| Meteor | big 64 x 64, small 32 x 32 |
| Power-up drop | 30 x 30 |
| Boss sections | see section 5 |

---

## 2. Structure

- 5 sectors x (6 waves + 1 boss). After sector 5 the five designs repeat (loop 1 = sectors 6-10, loop 2 = 11-15,
  ...) while the sector counter keeps climbing. Sector names get a Roman numeral on loops ("Outer Picket II").
- Per loop: every enemy interval x 0.85, enemy bolt and missile speed +10%, boss health +50%, +2 enemy bolts on
  screen, +1 diver out at once, +1 missile on screen, UFO values x (loop + 1).

### Stage flow inside a sector (`StagePhase`)

| Phase | Length | What happens |
| --- | --- | --- |
| WaveIntro | 2.0 s | formation visible and still; the player can move and fire; "WAVE n" banner |
| WaveActive | until the wave is cleared | formation steps, enemies fire/dive, UFO may appear |
| WaveIntermission | 2.0 s, or a 6.0 s meteor shower in designs 3-5 | between waves 1-5 and the next wave |
| BossWarning | 3.0 s | after wave 6; `BossIncoming` event (music duck + stinger) |
| BossFight | until the boss is gone | boss enters, fights, explodes for 2.0 s |
| SectorComplete | until `BeginNextSector()` | sector bonus paid; simulation idles (player may move, not fire) |
| GameOver | forever | no lives left; the simulation is frozen |

### Sector designs

| Design | Name | Introduces | Briefing line |
| --- | --- | --- | --- |
| 1 | Outer Picket | UFO bonus ship | The invasion fleet's picket line. Watch the top of the screen for the bonus UFO. |
| 2 | Raider Lanes | Divers | Raiders break away from the formation, swoop at you and return to their slots. |
| 3 | Aegis Belt | Shielded enemies + meteor showers | Shielded ships take two hits. Meteor showers sweep the sector between waves. |
| 4 | Missile Reach | Homing missiles | Missile carriers launch homing missiles. Shoot them down or out-turn them. |
| 5 | Mothership Gate | Split formation | Everything at once - and the formation splits into two groups. |

On loops the briefing adds: "The fleet is faster and fires tighter now."

---

## 3. Enemies

### Roles (five shapes = five roles)

Each role is one Kenney enemy ship shape from the remastered pack (`enemy<Colour>1` .. `enemy<Colour>5`: Grunt 1,
Shooter 2, Shielded 3, Diver 4, Missile carrier 5 - `AssetKeys.Enemies`); everything below uses role names only.

| Role | Row letter | Hits | Behaviour | Role bonus |
| --- | --- | --- | --- | --- |
| Grunt | G | 1 | fires only through the column-fire rule | +0 |
| Shooter | S | 1 | aimed bolt every shooter interval (plus column fire) | +10 |
| Shielded | H | 2 | first hit breaks the shield (`EnemyShieldBroken`, draw the shield sprite while `HasShield`) | +20 |
| Diver | D | 1 | leaves the formation, swoops, returns; fires once per dive | +30 |
| Missile carrier | M | 1 | homing missile every missile interval while under the cap | +40 |

### Colours (four colours = point tiers)

Colour is decided by formation row: row 0 (top) Red, row 1 Green, row 2 Blue, rows 3+ Black.

Colour points in sector s = 10 x (tier + 1) + 5 x (s - 1):

| Colour (tier) | Sector 1 | Sector 2 | Sector 3 | Sector 4 | Sector 5 | Sector s |
| --- | --- | --- | --- | --- | --- | --- |
| Black (0) | 10 | 15 | 20 | 25 | 30 | 10 + 5(s-1) |
| Blue (1) | 20 | 25 | 30 | 35 | 40 | 20 + 5(s-1) |
| Green (2) | 30 | 35 | 40 | 45 | 50 | 30 + 5(s-1) |
| Red (3) | 40 | 45 | 50 | 55 | 60 | 40 + 5(s-1) |

Base points of an enemy = colour points + role bonus, doubled when it is shot out of formation (diving or
returning). A diver that rams the player is destroyed for 0 points.

### Formation layout per wave

Row codes, top row first (4 letters = 4 rows). Columns: wave 1 = 8, wave 2 = 9, wave 3 = 10, waves 4-6 = 11.
Column spacing 72, row spacing 56. Top-row centre y at wave start = 120 + 12 x (wave - 1).

| Design | Wave 1 | Wave 2 | Wave 3 | Wave 4 | Wave 5 | Wave 6 |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | SGGG | SSGG | SGGGG | SSGGG | SGSGG | SSSGG |
| 2 | SDGG | SDDGG | DSGGG | SDSGG | DSDGG | SDDSG |
| 3 | HSGG | SHHGG | HSDGG | SHDHG | HHSDG | SHHDG |
| 4 | MSGG | SMHGG | MSDGG | MHSDG | SMDHG | MMSHG |
| 5 | MSHD | MSHDG | SMDHG | MHSDD | MSHHD | MMSHD |

Design 5, waves 2-6 are split formations (section 4).

### Enemy fire and behaviour numbers

| Behaviour | Rule |
| --- | --- |
| Column fire | every column-fire interval, the lowest live in-formation enemy of a random column fires straight down |
| Shooter cadence | each shooter fires exactly every shooter interval (first shot at a random 50-100% of it); bolt vx = clamp((player.x - x) x 0.5, -120, 120) |
| Bolt cap | no new enemy bolt while the on-screen count is at the cap (boss bolts ignore the cap) |
| Dive launch | every dive interval, if fewer than max divers are out, a random in-formation diver dives |
| Dive path | cubic Bezier over 1.8 s: P0 = slot, P1 = (x + 180 side, y - 60), P2 = (player x at launch, 900), P3 = (player x + 160 side, 560); side = -1 left of centre, +1 right; x clamped to the margins, y to at most 696 |
| Dive fire | one straight-down bolt when the dive passes 50% |
| Return | 360 units/s straight to its (moving) slot, then back in formation |
| Missile | launched straight down at 220 units/s (x loop speed scale); turns toward the player at most 90 degrees/s; expires after 6 s; shootable (25 base points) |

---

## 4. Formation movement

- Every step interval each group moves 12 units sideways in its direction.
- If that step would carry any live enemy of the group past the group's bound, the step is a drop instead: the
  group moves down by exactly the difficulty's drop height and reverses direction (one `FormationDropped` event
  per group drop). Enemy extents are computed from the live enemies' formation slots (dead edge columns let the
  formation travel further; divers keep their slot).
- Step interval: base interval = difficulty formation base interval x loop interval scale x (1 - 0.03 x (wave - 1)).
  With `alive` of `total` enemies left: interval = 0.06 + (base - 0.06) x (alive - 1) / (total - 1); full formation =
  base, last enemy = 0.06 s. It never increases as enemies die.
- `FormationStepped` carries a march note 0, 1, 2, 3, 0, ... for the classic four-note march.
- Landing: when a live in-formation enemy's bottom edge reaches y = 620, the player loses a life (shield and
  invulnerability do not help), `FormationLanded` fires, and the formation is pushed back to the wave's start height.
- Split formation (design 5, waves 2-6): the left ceil(columns/2) columns form group A confined to x 24..628, the
  rest group B confined to x 652..1256; each is centred in its half; A starts moving left, B right.

---

## 5. Bosses

The boss enters from y = -160 at 110 units/s to y = 170 (armoured while entering), then weaves:
x = 640 + 340 sin(angle), where the angle advances at 0.5 / 0.8 / 1.1 rad/s in phase 1 / 2 / 3. All sections
stay inside the margins.

Section geometry (offset from boss centre, size):

| Section | Offset | Size | Attack (phase 1 / 2 / 3 interval, x difficulty boss fire scale x loop interval scale) |
| --- | --- | --- | --- |
| Core | (0, 0) | 160 x 96 | armoured while any other section stands; once exposed, a 3-bolt fan (-10, 0, +10 degrees) every 2.0 / 1.5 / 1.1 s |
| Turret (left, right) | (-150, 20), (150, 20) | 56 x 56 | aimed bolt every 1.8 / 1.3 / 0.9 s |
| Cannon | (0, 70) | 64 x 40 | 5-bolt fan (-30, -15, 0, 15, 30 degrees) every 3.0 / 2.4 / 1.8 s |
| Missile bay (left, right) | (-230, -10), (230, -10) | 56 x 48 | homing missile every 5.0 / 4.0 / 3.0 s while under the missile cap |

Base health (Pilot, first loop). Actual health = round(base x difficulty boss health % x (1 + 0.5 x loop)), min 1:

| Design | Core | Each turret | Cannon | Each missile bay | Total | Phases | Thresholds (remaining / total health) |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 60 | 20 | - | - | 100 | 2 | 0.50 |
| 2 | 80 | 25 | 35 | - | 165 | 3 | 0.66, 0.33 |
| 3 | 100 | 30 | 40 | - | 200 | 3 | 0.66, 0.33 |
| 4 | 120 | 30 | - | 35 | 250 | 3 | 0.66, 0.33 |
| 5 | 150 | 35 | 45 | 40 | 305 | 3 | 0.66, 0.33 |

- Phase = 1 + number of thresholds at or above the remaining fraction; it never goes back down.
- A player bolt does 1 damage; a bomb does 10 to every damageable section.
- Hits on armour stop the bolt (even a piercing one) and count as hits for the chain (`BossHit` value 1).
- Points: each non-core section 250 base; the core (boss defeated) 2500 + 500 x (sector - 1) base; both with chain
  and difficulty multipliers. The HUD health bar shows `Boss.HealthFraction`.
- Events: `BossIncoming`, `BossAppeared` (value = design), `BossHit`, `BossPhaseChanged` (value = 2 or 3),
  `BossSectionDestroyed`, `BossDefeated`; the boss is `Gone` 2.0 s after defeat, then the sector completes.

---

## 6. UFO and meteors

| Item | Rule |
| --- | --- |
| UFO timing | a per-sector clock of 16-24 s (random, re-rolled at every appearance) that only runs during WaveActive while no UFO is on screen |
| UFO flight | from off-screen on a random side at 180 units/s across the lane at y = 76 |
| UFO value | one of 50 / 100 / 150 / 300 at random, x (loop + 1); always drops a power-up when shot |
| Meteor shower | designs 3-5, between waves 1-5 only: 6.0 s; a meteor every 0.4 s until 1.0 s before the end (13 per shower) |
| Meteor spawn | x 40..1240, y -40, vx -60..60, vy 180..300 (x loop speed scale), 30% big |
| Meteor hits | big 2 hits (20 base points), small 1 hit (10 base points); touching a vulnerable ship is a hit on the ship and destroys the meteor |

---

## 7. Player

| Stat | Value |
| --- | --- |
| Ship choices | 3 shapes x 4 colours (cosmetic: blue, green, orange, red in the pack's naming) |
| Movement | 420 units/s (x 1.5 with speed boost), centre x clamped to 56..1224 |
| Fire | hold to auto-fire: one bolt every 0.30 s (0.15 s with rapid fire), 900 units/s straight up |
| Spread shot | 3 bolts at -12, 0, +12 degrees |
| Hull | 3 per ship; damage overlay tier 1 at 2/3 hull, tier 2 at 1/3 hull (`PlayerShip.DamageTier`) |
| Hit | shield first (strength -1, 0.5 s invulnerable), else hull -1 (1.0 s invulnerable); hull 0 = ship destroyed |
| Destroyed | a life is lost; respawn after 1.5 s at x = 640 with full hull and 2.5 s invulnerability (blink) |
| Lives | difficulty table; max 9 |
| Bombs | difficulty table start stock; max 3; edge-triggered (holding uses one) |
| Bomb effect | removes every enemy bolt, missile and meteor; one hit on every live formation enemy (shields break, others die, scored with the current chain multiplier, chain unchanged); 10 damage to every damageable boss section |
| Invulnerable | enemy bolts, missiles, meteors and divers pass through (they are not used up); power-ups can still be collected |

---

## 8. Power-ups

Drop chance per destroyed formation enemy = difficulty power-up %. The UFO always drops one. Drops drift down
at 120 units/s and time out after 8 s (blink the last 2 s) or when they fall off the bottom (`PowerUpLost`).

| Power-up | Weight | Duration | Effect | Stacking |
| --- | --- | --- | --- | --- |
| Spread shot | 20 | 12 s | 3-bolt fan | refresh to full duration |
| Rapid fire | 20 | 12 s | fire interval 0.15 s | refresh |
| Piercing laser | 15 | 10 s | bolts pass through targets, hitting each once | refresh |
| Shield bubble | 15 | until used | +1 strength (max 3), each strength absorbs one hit | at 3: 250 bonus points |
| Speed boost | 15 | 10 s | movement x 1.5 | refresh |
| Extra life | 5 | instant | +1 life (max 9) | at 9: 1000 bonus points |
| Bomb | 10 | instant | +1 bomb (max 3) | at 3: 500 bonus points |

Different timed power-ups run together (spread + rapid + piercing combine). Collecting an active one resets its
timer to the full duration; it does not add. The HUD shows `PowerUpSystem.ActiveTimers()` (kind + seconds left)
and the shield strength.

### Visible upgrades

While a capability power-up is active the player's ship carries Kenney parts that show it (`ShipLoadout`, drawn by
`PlayfieldPainter`). Parts are laid out in hull art pixels per ship shape, so they sit on the wing tips, the nose and
the flanks of all three shapes in every colour, and they follow the ship. Draw order: parts under the hull, the thrust
flame, the hull, its damage overlay, parts over the hull, then the shield bubble. Stacked power-ups show all their parts
at once; a power-up's parts blink in its last 2 s and disappear when it runs out.

| Power-up | What changes on the ship | Kenney frames |
| --- | --- | --- |
| Spread shot | a gun pod on each wing tip (over the hull) | extension `spaceParts_093` |
| Rapid fire | a long cannon barrel poking forward from under the nose | remastered `gun08` |
| Piercing laser | two emitter barrels beside the cockpit, each with a pulsing green charge on its tip | extension `spaceParts_092`, remastered `laserGreen08` |
| Speed boost | an engine nacelle under each wing with its own flickering blue flame (plus the speed streaks) | remastered `engine1`, `fire04` `fire05` `fire14` `fire15` |
| Shield bubble | none - the bubble itself is the visible change | - |
| Extra life, bomb | none - they are not capabilities | - |

Every change of the part set writes one log line: `[BrixInvaders] loadout: ship shape <n> parts [<name> (<frame>), ...]`.

---

## 9. Difficulty table

| Setting | Cadet | Pilot | Ace | Legend |
| --- | --- | --- | --- | --- |
| Lives | 5 | 3 | 3 | 2 |
| Starting bombs | 2 | 1 | 1 | 0 |
| Column-fire interval (s) | 1.4 | 1.0 | 0.75 | 0.55 |
| Shooter interval (s) | 4.0 | 3.2 | 2.6 | 2.0 |
| Enemy bolts on screen | 4 | 6 | 8 | 10 |
| Enemy bolt speed | 260 | 300 | 340 | 380 |
| Drop height (descent) | 16 | 20 | 24 | 28 |
| Formation base step interval (s) | 0.60 | 0.50 | 0.42 | 0.35 |
| Dive interval (s) | 3.5 | 3.0 | 2.5 | 2.0 |
| Divers out at once | 1 | 2 | 3 | 4 |
| Missile interval (s) | 5.0 | 4.2 | 3.5 | 2.8 |
| Missiles on screen | 1 | 2 | 3 | 4 |
| Boss health | 75% | 100% | 130% | 170% |
| Boss fire scale | 1.25 | 1.0 | 0.85 | 0.7 |
| Power-up drop chance | 12% | 9% | 7% | 5% |
| Score multiplier | x1 | x1.5 | x2 | x3 |

---

## 10. Scoring

- Awarded points = base points x chain multiplier x difficulty % / 100, integer arithmetic, rounded down.
- Chain: every player-bolt hit on anything (enemy, shield, boss section or armour, UFO, missile, meteor) adds 1.
  A player bolt that leaves the top of the playfield without having hit anything is a miss: the chain resets to 0
  (`ChainBroken`, value = the chain lost). Bomb kills neither add to nor break the chain.
- Chain multiplier = 1 + floor(chain / 10), capped at x5 (10 hits = x2, 20 = x3, 30 = x4, 40+ = x5).
  `ChainMultiplierChanged` fires on every change; the HUD shows chain count and multiplier.
- Bonuses skip the chain multiplier but keep the difficulty multiplier: sector clear 1000 x sector; surplus
  power-ups (250 / 500 / 1000).

| Target | Base points |
| --- | --- |
| Formation enemy | colour points + role bonus (x2 out of formation) |
| UFO | 50 / 100 / 150 / 300 x (loop + 1) |
| Homing missile | 25 |
| Meteor | big 20, small 10 |
| Boss section | 250 |
| Boss defeated | 2500 + 500 x (sector - 1) |

---

## 11. High scores

- Top 10 per difficulty, highest first; an equal score goes below the existing one.
- A score qualifies when it is above 0 and would take rank 1-10.
- Names: exactly 3 characters from A-Z and 0-9; entry starts from the last name used ("AAA" the first time).
- Name entry: Up = next character (A -> B ... Z -> 0 ... 9 -> A, the "^" arrow above the letter), Down = previous
  (A -> 9); holding a direction scrolls (see section 13). Left/Right move the cursor, Confirm moves right and
  completes on the third letter ("Next", then "Done"), Back moves left. The footer shows "[D-PAD] [STICK]" or
  "[ARROWS]" and "hold to scroll".
- Serialised form (`HighScoreTable.ToLines()` / `FromLines()`): one line per entry, `Difficulty|Name|Score|Sector`,
  e.g. `Pilot|JER|48210|4`. Malformed lines are skipped on load.

---

## 12. Screens

```
            +--------+  overlay done / Confirm / 8 s
            | Splash | ---------------------------------+
            +--------+                                  v
   +-------------------------------------------------------------+
   |                         Title                               |<---------------------------+
   |  Play | High scores | Settings | Credits | Quit (15 s idle -> Attract, any input/45 s back) |
   +-------------------------------------------------------------+                            |
      | Play                                                                                   |
      v                                                                                        |
  ShipSelect --Confirm--> DifficultySelect --Confirm--> SectorBriefing --Confirm (after 1 s)--> Playing
     ^ Back                    | Back -> ShipSelect          ^                                  |   ^
     |                                                        |                   Pause/Start   v   | Pause/Start/Back/Resume
  Title <---Back                                              |                              Paused ---Quit to title--> Title
                                                              |                                 |
                                   SectorClear --Confirm (after 1 s)----+   <--sector cleared-- Playing
                                                                              <--game over----- Playing
                          GameOver --Confirm (after 2 s) or 8 s--> HighScoreEntry (if it qualifies) --> HighScores --> Title
                                                              \--> Title (if it does not qualify)
```

| Screen | Inputs | Leaves to |
| --- | --- | --- |
| Splash | Confirm/Start/Back skip | Title (also on `NotifySplashComplete()` or after 8 s) |
| Title | Up/Down move (wrap), Confirm/Start select | ShipSelect / HighScores / Settings / Credits; Quit raises `QuitGame` (the app closes); Attract after 15 s without input |
| Attract | any input | Title (also after 45 s or `NotifyAttractEnded()`) |
| ShipSelect | Left/Right shape (wrap 3), Up/Down colour (wrap 4) | Confirm -> DifficultySelect, Back -> Title |
| DifficultySelect | Up/Down level (wrap 4), Left/Right start sector (1..unlocked) | Confirm -> SectorBriefing (`StartNewGame`), Back -> ShipSelect |
| SectorBriefing | K / Y opens the Kenney link | Confirm/Start after 1 s -> Playing (`BeginSector`) |
| Playing | Pause/Start | Paused (`PauseGame`); SectorClear / GameOver by notification |
| Paused | Up/Down Resume / Quit to title | Pause/Start/Back/Resume -> Playing (`ResumeGame`); Quit -> Title (`AbandonGame`) |
| SectorClear | K / Y opens the Kenney link | Confirm/Start after 1 s -> SectorBriefing of the next sector |
| GameOver | - | Confirm/Start after 2 s (or 8 s) -> HighScoreEntry if it qualifies, else Title |
| HighScoreEntry | name entry | HighScores (`SubmitHighScore`, text = name) |
| HighScores | Left/Right difficulty tab | Confirm/Back -> Title |
| Settings | Up/Down row (wrap), Left/Right `AdjustSetting` -1/+1, Confirm `ActivateSetting` | Back -> Title |
| Credits | K / Y opens the Kenney link | Confirm/Back -> Title |

Start-sector unlock: clearing sector s on a difficulty unlocks start sectors up to min(s + 1, 5) on that
difficulty.

Per frame the Game library calls `ScreenStateMachine.Update(dt, menuInput)`, steps the simulation while the
screen is Playing (or Attract), calls `NotifySectorCleared` / `NotifyGameOver` / `NotifyAttractEnded` from the
simulation's events, then acts on `ScreenStateMachine.Commands`.

Attract mode runs its own `GameSimulation` (Cadet, sector 1, any seed) driven by `AttractPilot.Decide`.

### Settings rows (the Game library owns their meaning; the machine only moves the cursor)

| Row | Setting | Left/Right | Confirm |
| --- | --- | --- | --- |
| 0 | Master volume | -/+ 0.1 (0..1) | - |
| 1 | Music volume | -/+ 0.1 (0..1) | - |
| 2 | Effects volume | -/+ 0.1 (0..1) | - |
| 3 | Music model | SkyTNT / MuPT | - |
| 4 | Instrument library | ModestSynthGm / FluidR3Gm | - |
| 5 | Gamepad profile | Classic / Shoulder | - |
| 6 | Default ship | cycles the 12 shape x colour combinations | - |
| 7 | Default difficulty | Cadet .. Legend | - |
| 8 | Reset high scores | - | the first Confirm arms it, the second clears every table |

---

## 13. Input actions

Keyboard and gamepad work simultaneously. `GameInput` (in play): MoveAxis -1..1, Fire (held), Bomb (held,
edge-detected). `MenuInput` (screens): edge-triggered Up, Down, Left, Right, Confirm, Back, Pause, Start,
KenneyLink, Other.

| Action | Keyboard | Gamepad (Classic) | Gamepad (Shoulder) | Maps to |
| --- | --- | --- | --- | --- |
| Move | A/D or Left/Right | Left stick X (dead zone 0.15) or D-pad | same | `GameInput.MoveAxis` |
| Fire | Space | A | Right shoulder | `GameInput.Fire` |
| Bomb | Left Shift | B | Left shoulder | `GameInput.Bomb` |
| Pause | Escape | Start | Start | `MenuInput.Pause` (keyboard) / `MenuInput.Start` (gamepad) |
| Kenney link (on cards) | K | Y | Y | `MenuInput.KenneyLink` |
| Menu navigate | arrows | D-pad or stick | same | `MenuInput.Up/Down/Left/Right` |
| Menu confirm | Enter | A | A | `MenuInput.Confirm` |
| Menu back | Escape | B | B | `MenuInput.Back` (Escape sets Back and Pause) |

On-screen prompts switch glyph set (keyboard vs gamepad) based on the last device that produced input.

---

### Held directions repeat

Menu directions (arrows, D-pad, left stick) repeat while held: the press acts at once, the first repeat comes
0.35 s later, then one every 0.1 s (0.12 s on the high-score name entry) at a constant rate, no acceleration,
until released. The stick counts as a direction past 0.5 and is released below 0.3; after a release that axis
ignores the stick for 0.08 s either way, because a released stick springs back past centre and the overshoot must
not read as a push the opposite way. Anything held when the game starts neither presses nor repeats until released.
Confirm, Back, Pause, Start and the Kenney link stay single edges. The repeat clock is the host's elapsed cycle time.

## 14. Persistence keys

Stored through the Game library's settings facade (`AppName = "BrixInvaders"`). All keys are namespaced
`BrixInvaders.*`.

| Key | Type | Default |
| --- | --- | --- |
| `BrixInvaders.HighScores.Cadet` / `.Pilot` / `.Ace` / `.Legend` | JSON array of `{ "Name", "Score", "Sector" }` | `[]` |
| `BrixInvaders.ShipShape` | int 0..2 | 0 |
| `BrixInvaders.ShipColour` | int 0..3 | 0 |
| `BrixInvaders.Difficulty` | string (`Cadet`, `Pilot`, `Ace`, `Legend`) | `Pilot` |
| `BrixInvaders.MasterVolume` | double 0..1 | 0.8 |
| `BrixInvaders.MusicVolume` | double 0..1 | 0.6 |
| `BrixInvaders.EffectsVolume` | double 0..1 | 0.8 |
| `BrixInvaders.MusicGenerator` | string (`SkyTNT`, `MuPT`) | `SkyTNT` |
| `BrixInvaders.InstrumentLibrary` | string (`ModestSynthGm`, `FluidR3Gm`) | `ModestSynthGm` |
| `BrixInvaders.GamepadProfile` | string (`Classic`, `Shoulder`) | `Classic` |
| `BrixInvaders.HighestSectorCleared.Cadet` / `.Pilot` / `.Ace` / `.Legend` | int | 0 |
| `BrixInvaders.LastInputDevice` | string (`Keyboard`, `Gamepad`) | `Keyboard` |
| `BrixInvaders.LastName` | string, 3 characters | `AAA` |

The start-sector unlock restored into the screen machine is min(highest sector cleared + 1, 5).

---

## 15. Music

The music is generated while the game runs: a model writes it (SkyTNT - electronica with drums, or MuPT - folk and
classical tunes in parts, no drums) and an instrument library plays it (ModestSynthGm - synthesized, or FluidR3Gm -
recorded). The player picks both on the settings screen (rows 3 and 4); the defaults are SkyTNT through ModestSynthGm.
`BrixInvaders.Music` owns the choices and the table below; the Game library owns when things happen.

### Start-up

1. `AudioSystem.Initialize(MusicSetup.RecommendedSampleRate, MusicSetup.RecommendedChannels)` (48000 Hz, stereo).
2. `MusicSetup.RegisterEverything()` - ModestSynthGm first (so it is the default library), then FluidR3Gm, then the
   SkyTNT and MuPT generators. Nothing loads; it logs one line:
   `[BrixInvaders] music: registered instrument libraries ModestSynthGm (default), FluidR3Gm; generators SkyTNT, MuPT;
   model files: SkyTNT found, MuPT found`. If no model generator is registered it also logs
   `MusicSetup.NoModelFallbackNote` (the embedded replay plays instead).
3. `Engine.UseGeneratedMusic(MusicSetup.OptionsFor(settings, SectorMusic.TitleSector))` - the title music fades in
   while the model loads in the background (silence until then is normal).

`OptionsFor(settings, sector, boss)` fills: Generator and InstrumentLibrary (the player's choices, exact registry
names), Preset (the table), BeatsPerMinute (the table; it becomes the SESSION tempo and every fresh piece is carried
to it), SeamCrossfade 4 s (where the music moves on to a fresh piece, the setting listening settled on for both
models), MasterVolume (`MusicSettings.MusicVolume`), TrackKey `brixinvaders-music`, StartImmediately on. Priming is
left to the package (Alternate for SkyTNT, Fresh for MuPT). Unknown names throw `ArgumentException` listing the
valid ones.

### Per-sector table

Sector 0 is the title screen. Sectors 6-10, 11-15, ... repeat designs 1-5 (`SectorMusic.For(sector)`).

| Design | Sector | Mood | SkyTNT preset | MuPT preset | BPM | Boss SkyTNT | Boss MuPT | Boss BPM |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | Title | calm and spacious - waiting in orbit before the invasion | AmbientElectronica | WaltzDuetInAMinor | 108 | - | - | - |
| 1 | Outer Picket | steady and bright - the first patrol along the picket line | FourOnTheFloor | HornpipeInG | 120 | ClubArrangement | ReelInGMinor | 128 |
| 2 | Raider Lanes | restless and swooping - raiders break from the formation | ClubArrangement | JigInD | 126 | FourOnTheFloor | ReelInGMinor | 134 |
| 3 | Aegis Belt | wide and eerie - drifting rock and shielded ships | AmbientElectronica | AirInDMixolydian | 104 | ClubArrangement | ReelInGMinor | 112 |
| 4 | Missile Reach | tense and minor - hunted by homing missiles | ClubArrangement | WaltzInAMinor | 132 | FourOnTheFloor | ReelInGMinor | 140 |
| 5 | Mothership Gate | grand and driving - everything at once at the gate | FourOnTheFloor | DuetInC | 138 | ClubArrangement | ReelInGMinor | 146 |

Why these:
- SkyTNT has three presets, so the sectors alternate the two with drums (FourOnTheFloor, ClubArrangement) and keep
  AmbientElectronica (no percussion) for the title and the meteor-strewn Aegis Belt. A boss always switches to the
  OTHER drum preset, so the change is heard.
- MuPT's presets are tunes: a bright hornpipe for the first patrol, a jig for the swooping raiders, the slow air for
  the eerie belt, a minor waltz for the missile hunt, the two-part duet for the finale, and the highest-rated
  two-part waltz for the title. Every boss plays the fast G-minor reel - the MuPT "boss theme".
- Neighbouring sectors never share a preset for either model (fenced by a test).

### While the game runs

| Moment | What the Game library does |
| --- | --- |
| Title / menus | the session started at start-up keeps playing; screen changes never restart it |
| Sector briefing / start of a sector | `provider.FollowUp(MusicSetup.FollowUpFor(settings, sector))` - the new preset takes over at a bar line |
| Boss warning (`BossIncoming`) | engine music duck (under the boss-warning stinger the sound table plays on the effects bus), then `provider.FollowUp(MusicSetup.FollowUpFor(settings, sector, boss: true))` |
| Sector clear | `FollowUp` to the next sector's preset when its briefing opens |
| Pause | the pause menu is a game pause (the engine keeps running), so the music is DUCKED under it (`MusicManager.PushDuck`, 0.35) rather than suspended - it keeps breathing quietly and is back at full level a moment after Resume |
| Window minimized | the engine's GLOBAL pause (`Engine.Pause()` from the window's `VisibilityChanged`): the loop parks and every voice suspends, music included; a pause request is latched so the pause menu is up on the first cycle after the window is shown again (`Engine.Resume()`) |
| Game over | fade the music down on the engine's music bus (a held duck to 0.2) under the game-over stinger the sound table plays, keep the session alive; back on the title restore the level and `FollowUp(MusicSetup.FollowUpFor(settings, SectorMusic.TitleSector))` |
| Settings: model or library changed | `Engine.UseGeneratedMusic(MusicSetup.OptionsFor(settings, currentSector))` again - a fresh session with its own fade-in |

A follow-up keeps the session's tempo and instrument library: the BPM column applies where a session STARTS (the
title at start-up, or wherever the player changes the model or library); the boss BPM applies when a session starts
during a boss fight. Follow-ups change the character of the music; the session keeps one pulse.

Volume: the player's Music volume slider drives the engine's music bus (`AudioMixer.MusicVolume`); pass
`MusicSettings.MusicVolume = 1` so the level is not applied twice (or drive the level through `MusicVolume` and leave
the bus at 1 - one or the other, never both).

---

## 16. Credits

Ships, sounds and planets by Kenney - kenney.nl - CC0. Every screen that shows Kenney content says so; the
briefing, sector-clear and credits screens carry the Kenney card and the clickable bundle link
`https://kenney.itch.io/kenney-game-assets` (mouse click, K or gamepad Y). The credits screen lists every pack (its
licence title, or its display name when the licence has no usable title line), links to `https://kenney.nl` and
Kenney's Patreon (`https://www.patreon.com/kenney/`), and ends with the music card - which model is writing the music
and which instrument library plays it, read from the running music. When no browser can be opened the screen shows
"No browser was available." for 4 s.
