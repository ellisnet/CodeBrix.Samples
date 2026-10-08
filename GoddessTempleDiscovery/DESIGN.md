# Goddess Temple Discovery - Game Design

This is the design the Goddess Temple Discovery libraries code against. The numbers here are the numbers in
`GoddessTempleDiscovery.Rules` (the package-free rules engine and every card's content) and, for the screens, the
pacing, the input and the stored settings, in `GoddessTempleDiscovery.Game`. Where this document and the code
disagree, the code's unit tests are the tie-breaker and this document gets fixed.

The teams are fictional. The history, the buildings, the finds, the people and the Goddess on the cards are real
and sourced; every card carries its sources.

---

## 1. The game in one paragraph

Two to four fictional expeditions dig at Warka, the ancient Uruk, through the twelve real pre-war campaigns of the
German excavations: 1912/13, then 1928/29 to 1938/39. Each season a Season card tells what really happened that
winter and applies one effect to every team. Each team rolls two dice - its two work crews - and spends them to
dig trenches in the Site Row, recruit Specialists, study Tablets and survey the row, and it publishes its finds as
numbered Preliminary Reports. When the 1938/39 season ends, the Second World War closes the dig and the reports are
tallied.

| Item | Value | Where |
| --- | --- | --- |
| Seats | 2 to 4, each a human or a computer | `GameRules.MinSeats`, `MaxSeats` |
| Seasons | 12, in a fixed order | `Catalog.Seasons` |
| Turns per team per season | 1 to 3, chosen at setup | `GameRules.MinTurnsPerSeason`, `MaxTurnsPerSeason` |
| Suggested turns per season | two seats 3, three or four seats 2 | `GameRules.DefaultTurnsPerSeason` |
| Difficulty | Easy, Standard, Hard: every Dig Number -1, 0, +1 | `GameRules.DifficultyShift` |
| Seed | optional; with none the engine takes `Environment.TickCount` and reports it | `GameSetup.Seed`, `GameState.Seed` |
| Randomness | one xorshift64* generator (`GameRandom`) seeded through SplitMix64; every shuffle and every die comes from it | `Engine/GameRandom.cs` |
| Determinism | the same setup, seed and action sequence give the same game, action for action | `GameEngine.StateHash` |

The turns a season are suggested by seat count so that the Site deck lasts the twelve seasons and a game runs
about an hour; the setup pane preselects the suggestion each time the seat count changes, and any of 1, 2 or 3 may
be chosen. The engine runs all twelve seasons whatever happens to the decks.

---

## 2. The decks and the table

| Deck or row | What it holds | Rule |
| --- | --- | --- |
| Site deck (the Tell) | One Discovery card per real find | Built in three depth bands (section 4); dealt face-down |
| Site Row | 5 slots, face-down | Refilled from the top of the Site deck after every dig; a slot stays empty only when the deck is empty |
| Tablet deck | Tablets of four kinds: Goddess, Pantheon, Culture, Timeline | Shuffled; spent and discarded Tablets are reshuffled into a new deck when it runs out |
| Expedition deck | Six Specialist roles, three copies each | Shuffled |
| Expedition Row | 4 slots, face-up | Refilled from the top of the Expedition deck after every recruit |
| Favor deck | Favors of the Goddess | Shuffled; drawn Favors are reshuffled into a new deck when it runs out |
| Season deck | The twelve campaigns | Not shuffled: one per season, in order |
| Team profiles | Fictional expedition names with an emblem and a color | A seat may take one or type its own name |

Card backs are the CardsAndDice add-on's celestial back for every deck. The dice are the add-on's traditional pip
dice; a third die joins only for the turn after an extra-die Favor.

### Setup

| Step | Rule |
| --- | --- |
| Seat order | shuffled; a team's place in it is its index |
| Team names | an empty name takes the profile's name; a missing profile gets a plain one and a fallback color; a name already taken gets " 2", " 3" appended |
| Workers | each team starts with 2 (`GameRules.StartingWorkers`) |
| Tablets | each team starts with 1 (`GameRules.StartingTablets`), drawn before the first Season card flips |
| Rows | the Site Row is dealt 5 face-down, the Expedition Row 4 face-up |
| First team | the first team of each season rotates: season `i` starts with team `i mod teams` |

---

## 3. A season and a turn

### A season

1. The Season card flips and its effect applies (section 7).
2. Each team takes its turns, in seat order starting from that season's first team, for as many rounds as the
   turns-per-season setting.
3. Every team gains 1 Worker (`GameRules.SeasonEndWorkers`), and the season's grants (a free survey, a re-roll)
   lapse.
4. After the twelfth season the game is over and the final scores stand (section 6).

### Phases (`GamePhase`)

| Phase | Legal actions |
| --- | --- |
| SeasonStart | Roll (the season's first team, just after the Season card flipped) |
| AwaitRoll | Roll |
| Spend | Re-roll, Dig, Recruit, Study, Survey, Publish, End turn |
| TurnEnd | Discard, one card at a time, until the hand is within the limit |
| GameOver | nothing |

### Rolling

| Rule | Value |
| --- | --- |
| Dice | 2 six-sided; 3 on the turn after an extra-die Favor |
| Kept dice | with three dice the best two are kept automatically (on a tie the earlier die); Die A is the kept die with the lower index, Die B the other |
| Spending | each kept die once, alone, or both together as one sum (`DiceChoice.DieA`, `DieB`, `Both`) |
| Re-roll | only in the re-roll season: one re-roll per team for the whole season, of a kept die not yet spent; a re-rolled die stays kept even if an unkept third die now shows more |
| Doubles | when the two kept dice match, the team draws one Favor: as soon as both dice are spent, or when the turn ends with doubles unspent (before the hand limit is checked) |
| Unspent dice | lost when the turn ends |

### The actions

| Action | Cost | Rule |
| --- | --- | --- |
| Dig | a die or the sum, plus modifiers | The total must reach the site's Dig Number (section 4). The card flips, goes to the team's hand as an unpublished find, and the slot is refilled face-down |
| Recruit | a die or the sum at least the Specialist's cost | At most 4 Specialists, never two of the same role; the slot is refilled |
| Study | a die or the sum, any value | Draws 1 Tablet; draws 2 when the value spent is 6 or more, when the team has the Epigrapher, or in the study season (never more than 2) |
| Survey | a die or the sum, or a free survey | The Site Row card goes to the bottom of the Site deck and the top card is dealt into the slot; not possible when the Site deck is empty |
| Publish | nothing | Three or more Discoveries from the hand, laid down as the team's next Preliminary Report (section 5) |
| End turn | nothing | Draws a waiting doubles Favor, then checks the hand limit |
| Discard | nothing | Only at the hand limit: a discarded Discovery leaves the game, a discarded Tablet goes to the Tablet discards |

### Dig modifiers

| Modifier | Effect | Rule |
| --- | --- | --- |
| Workers | +1 each | any number the team holds; spent on the dig |
| Tablet | +2 (`TabletCard.DigBonus`) | one per dig; the Tablet is discarded |
| Architect | +1 | on a Building |
| Epigrapher | +1 | on an Inscription |
| Small-Finds Keeper | +1 | on an Object or a Deposit |
| Foreman | +1 | once per turn, added only when the dig would fall short without it, so it is never wasted |
| Favor "+2 on your next dig" | +2 (`GameRules.FavorDigBonus`) | always used by the team's next dig, this turn or next; then it lapses |
| Difficulty | Dig Number -1, 0 or +1 | Easy, Standard, Hard |
| Season | Dig Number -1 | tier 3 sites in the Kassite season, tier 9 sites in the deep-trench season |
| Worker cap | at most 2 Workers on one dig | in the currency-controls season; the team still needs the Workers it adds, but they are not spent |

### Free surveys

A free survey comes from the Surveyor (once per turn), from the first and eleventh Season cards (one each, lasting
the season), or from a Favor (kept until used). When several are open, the one that expires soonest is used: the
Surveyor's, then the season's, then a Favor's.

### Recruit cost

The printed cost, one less in the recruit season, one less again after a recruit-discount Favor (which waits for the
team's next recruit, this turn or later), never below 1.

### The hand limit

At the end of a turn a team may hold at most 7 cards, Discoveries and Tablets together (`GameRules.HandLimit`);
over it, the turn stays open in the TurnEnd phase until the team discards down to 7. The "hand limit does not apply"
Favor lifts the check for that turn.

---

## 4. Dig Numbers, points and depth

| Tier | Layers (`DepthTiers.Name`) | Dig Number | Points |
| --- | --- | --- | --- |
| 1 | Seleucid and Parthian surface | 3 | 1 |
| 2 | Neo-Babylonian and Achaemenid | 4 | 1 |
| 3 | Kassite and Old Babylonian | 5 | 2 |
| 4 | Third Dynasty of Ur | 6 | 2 |
| 5 | Early Dynastic and Akkadian | 7 | 3 |
| 6 | Jemdet Nasr, Level III | 8 | 3 |
| 7 | Uruk IV | 9 | 4 |
| 8 | Uruk V | 10 | 5 |
| 9 | The deep sounding | 11 | 6 |

A single die reaches tiers 1 to 3 on its own. Both dice together reach tier 7 (a sum of 9 or more) a little more than one roll in four unaided, and
Workers, Tablets and Specialists make tiers 8 and 9 reachable from the middle of the game.

### The Site deck in three bands (`SiteDeckBuilder`)

The Discoveries are ordered by tier (equal tiers in random order), cut into three bands of a third each, and each
band is shuffled on its own; in the bottom band the deep-sounding cards (tier 9) are moved to the end. With the
catalog's spread of tiers the top band then holds tiers 1 to 4, the middle 4 to 7 and the bottom 7 to 9, so the row
starts shallow and deepens.

### Her stars

A Discovery marked `IsStarred` is one of Holy Inanna's own finds: Her temples, Her vase, Her mask. Its face has a
gold band and Her eight-pointed star, the label under its trench shows the star, and it counts toward the Star of
Holy Inanna at the end (section 6).

### The Mask of Warka

When the last Season card flips, the Mask of Warka (the catalog's `mask-of-warka`) is taken out of the Site deck if
it is still there and put into the Site Row in place of the shallowest card (the leftmost on a tie), which goes to
the bottom of the deck.

---

## 5. Preliminary Reports

| Report kind (`ReportKind`) | When | Bonus |
| --- | --- | --- |
| Stratigraphy | every card from the same period | +1 per card (`GameRules.StratigraphyBonusPerCard`) |
| Sequence | every card from a different period, the periods consecutive in `Period` order, three or more of them | +2 per card (`GameRules.SequenceBonusPerCard`) |
| Plain | anything else | none |

| Rule | Value |
| --- | --- |
| Cards in a report | 3 or more (`GameRules.MinReportCards`) |
| Points | the printed points of every card, plus the kind's bonus |
| Photographer | +1 on every report the team publishes (`GameRules.PhotographerBonus`) |
| Publishing season | +1 on every report published in the 1936/37 season (`GameRules.SeasonPublishBonus`) |
| Numbering | per team, like the real ones: First Preliminary Report, Second, Third, up to the Twentieth, then "Preliminary Report 21" |
| Final season | nothing is published automatically; whatever is still in the hand at the end scores half |

---

## 6. Final scoring (`FinalScoring`)

| Part | Points |
| --- | --- |
| Published | every report's points, bonuses included, plus points scored by Favors |
| Unpublished | half the summed printed points of the Discoveries still in the hand, rounded down once for the whole hand |
| Tablets | 2 per Tablet held (`TabletCard.PointValue`) |
| Tablet sets | +3 for each complete set of the four kinds (`GameRules.TabletSetBonus`) |
| Star of Holy Inanna | +5 to the team with the most star-marked Discoveries, published and unpublished (`GameRules.StarBonus`); teams level on the most all gain it; nobody gains it when nobody holds one |

Ties on the total are broken by the most Discoveries held (published and unpublished), then by the deepest
Discovery's tier; teams still level share the rank. The scores are valid at any moment of the game, which is how the
team ribbons show points as they stand.

---

## 7. Season cards

| Season | Title | Director | Effect (`SeasonEffect`) | In play |
| --- | --- | --- | --- | --- |
| 1912/13 | The Ottoman Permit | Julius Jordan and Conrad Preusser | FreeSurvey | every team has one free survey for the season |
| 1928/29 | Back to Uruk | Julius Jordan | GainWorker | every team gains 1 Worker at once |
| 1929/30 | Christmas in the Dig House | Julius Jordan | KassiteCheaper | tier 3 sites: Dig Number -1 |
| 1930/31 | The Deep Trench | Julius Jordan | DeepCheaper | tier 9 sites: Dig Number -1 |
| 1931/32 | The Mudbrick Lads | Arnold Nöldeke | RerollOnce | every team may re-roll one kept die once this season |
| 1932/33 | Andrae's Wish | Arnold Nöldeke | StudyDrawsTwo | every Study draws 2 Tablets |
| 1933/34 | Heinrich's Winter | Ernst Heinrich | RecruitCheaper | every recruit costs 1 less (never below 1) |
| 1934/35 | Money Grows Short | Arnold Nöldeke | WorkersCapped | Workers are not spent on digs, but at most 2 count on one dig |
| 1935/36 | Unrest Near Samawa | Arnold Nöldeke | None | the story only |
| 1936/37 | The Money Fights | Arnold Nöldeke | PublishBonus | every report published this season scores 1 more |
| 1937/38 | The Dating Trench | Arnold Nöldeke | FreeSurvey | every team has one free survey for the season |
| 1938/39 | The Last Permit | Arnold Nöldeke | FinalSeason | the Mask of Warka enters the Site Row if it has not appeared (section 4); the last season |

The presentation prints the final season's effect from `CardText.FinalSeasonEffectText`: "The Mask of Warka site
enters the Site Row if it has not appeared yet. This is the final season: publish what you can; what stays in the
crates scores half."

---

## 8. Favors of the Goddess (`FavorEffect`)

Each Favor carries one true line about Her. Each effect appears on two Favors.

| Effect | What it does |
| --- | --- |
| ExtraDieNextTurn | the team's next roll uses three dice and keeps the best two |
| FreeTablet | draw a Tablet now |
| FreeWorker | gain a Worker now |
| FreeSurvey | one free survey, kept until used |
| PlusTwoNextDig | +2 on the team's next dig, this turn or next; then it lapses |
| RecruitDiscount | the team's next recruit costs 1 less, this turn or later |
| OnePoint | 1 point at once, added to the published points |
| NoHandLimit | the hand limit does not apply at the end of this turn |

---

## 9. Specialists (`SpecialistRole`)

| Role | Cost | Power | Copies |
| --- | --- | --- | --- |
| Foreman | 2 | +1 on any one dig a turn, used only when it makes the difference | 3 |
| Small-Finds Keeper | 3 | +1 on Object and Deposit sites | 3 |
| Surveyor | 3 | one free survey a turn | 3 |
| Architect | 4 | +1 on Building sites | 3 |
| Photographer | 4 | +1 on every report | 3 |
| Epigrapher | 5 | +1 on Inscription sites; every Study draws 2 | 3 |

The roles are fictional; each card's historical note names the real person or crew who did that work at Uruk.

---

## 10. Computer teams (`ComputerBrain`, `TemperamentWeights`)

One decision procedure, three weightings. Every call returns one action from `GameEngine.LegalActions`, so a
computer team never makes an illegal move.

| Phase | Choice |
| --- | --- |
| SeasonStart, AwaitRoll | roll |
| TurnEnd | discard the card it values least (keep value plus up to 0.1 of jitter) |
| Spend | score every legal action except ending the turn, add up to `ComputerBrain.Jitter` (0.25) of jitter, take the best; end the turn when nothing scores above zero |

### Scores

| Action | Score |
| --- | --- |
| Dig | desirability - (Workers x Worker cost) - Tablet cost (if one is spent) - dice cost - 0.12 x (total - Dig Number) - 0.4 when the hand is at the limit |
| Recruit | role value x (1 - 0.8 x progress) - dice cost - 0.08 x (die value - cost) |
| Study | draws x (1.1 + Study weight) - dice cost - 0.1 x value - 0.6 when the draws would pass the hand limit |
| Survey, free | 0.5 when the card is out of reach, else 0.25; minus 0.1 x desirability |
| Survey, with a die | 0.15 + 0.3 when out of reach - dice cost - 0.05 x value - 0.1 x desirability |
| Publish | 0.5 x (report points - half the printed points) + kind weight + 0.8 x cards over the hand limit - wait + 0.05 x cards |
| Re-roll | (3.5 - value) x 0.5 for a die showing 1 or 2, else -1 |

| Term | Value |
| --- | --- |
| Dice cost | 0.15 for one die, 0.8 for both |
| Desirability | printed points + Star weight (starred) + DeepTier weight (tier 7 or deeper) + ShallowTier weight (tier 4 or shallower) + SamePeriod weight (a hand card shares the period) + AdjacentPeriod weight (a hand card is one period away) |
| Worker cost | WorkerDeep for tier 7 or deeper, else WorkerShallow; 0.05 in the Worker-cap season; then x max(0.1, 1 - progress) x 4 / (4 + Workers held) |
| Progress | (season index + round / turns per season) / seasons |
| Out of reach | Dig Number + difficulty shift > 12 + Workers + 2 x Tablets + 2 |
| Publish wait | 2.0 for a plain report, 0.6 otherwise, 0 on the team's last turn of the last season |
| Keep value, Tablet | 1.0 + 0.8 when it is the only one of its kind + 0.2 x Tablet cost |
| Keep value, Discovery | 0.75 x points + Star weight (starred) + SamePeriod weight (another hand card shares its period) |

### Temperament weights

| Weight | Surveyor | Deep Digger | Scholar |
| --- | --- | --- | --- |
| WorkerShallow | 0.7 | 1.5 | 0.8 |
| WorkerDeep | 0.7 | 0.3 | 0.6 |
| TabletCost | 1.6 | 0.8 | 1.4 |
| Star | 0.5 | 1.5 | 1.0 |
| DeepTier | 0.0 | 1.5 | 0.8 |
| ShallowTier | 0.5 | -0.3 | 0.0 |
| SamePeriod | 0.6 | 0.2 | 0.2 |
| AdjacentPeriod | 0.1 | 0.2 | 0.7 |
| Study | 0.2 | 0.0 | 0.8 |
| PublishStratigraphy | 1.5 | 0.5 | 0.5 |
| PublishSequence | 0.5 | 0.5 | 2.0 |

| Role value | Surveyor | Deep Digger | Scholar |
| --- | --- | --- | --- |
| Architect | 2.0 | 2.2 | 1.4 |
| Epigrapher | 1.2 | 1.4 | 3.0 |
| Small-Finds Keeper | 2.2 | 1.8 | 1.2 |
| Photographer | 1.5 | 1.2 | 3.0 |
| Foreman | 2.0 | 2.6 | 1.4 |
| Surveyor | 2.5 | 0.8 | 1.0 |

The temperaments double as the difficulty of the computer opponents, on purpose: the Deep Digger is the ambitious
opponent, the Surveyor the steady one and the Scholar the gentle one who studies and publishes sequences, and the
setup pane says so. A new player should face Scholars and Surveyors first. The jitter is drawn from a
`System.Random` seeded with the game's seed, so a seeded game also plays its computer turns the same way.

---

## 11. The table, the pacing and the presentation

| Item | Value | Where |
| --- | --- | --- |
| Render resolution | 1280 x 800, widened with the window's aspect (never narrower than 1280; a narrower window is letterboxed) | `GoddessTempleGameHost.RenderWidth`, `RenderHeight` |
| Window | opens at 1440 x 900; cannot be dragged below 1180 x 740 | `App.xaml.cs` |
| Engine | 60 frames a second target, fixed step 60 a second | `OnEngineInitialized` |
| Render tier | GPU unless `GODDESSTEMPLE_USE_CPU=1` | `GoddessTempleGameHost.UseGpuTier` |
| Card faces | 250 x 400 SVG, at most 200 KB of text each; registered on the table at most 12 ms per step while a progress bar fills | `CardFaceComposer`, `GoddessTempleGameHost.Prepare` |
| Face sizes on the table | Site Row 104 x 166, Expedition Row 96 x 154, hand 100 x 160, small (reports, shelf, Favors) 50 x 80, a drawn Favor 150 x 240, dice 66 | `TableLayout` |
| Inspector pictures | the face at 500 x 800, the art with its long side 520 pixels | `SvgRaster`, `CardViews.PhotoLongSide` |
| Computer pause | 0.8 s between two computer actions, divided by the animation speed (at least 0.25); 0.1 s with reduced motion | `ComputerPauseSeconds` |
| Inspector, own find | a human team's own discovery stays open until it is closed | `AutoCloseFor` |
| Inspector, everything else | closes itself after 6 s (1.5 s in autoplay) unless the pointer is over it; a computer turn waits while it is open | `InspectorAutoCloseSeconds`, `AutoPlay.InspectorHoldSeconds` |
| Reveal computer discoveries | on by default: the inspector opens for every team's finds and Favors | settings |
| Presentation holds | Season 0.6 s, roll 0.2 s, flip 0.5 s, survey 0.2 s, report 0.4 s, Favor 1.4 s (tossed in 0.6 s); each divided by the animation speed, none with reduced motion | `TableSession.Translate` |

### The newspaper

Every Discovery that flips opens an EXTRA! edition of the Warka Herald: the masthead, a dateline such as "WARKA,
IRAQ — WINTER 1930/31", the banner headline in capitals, the sub-head, a byline naming the team's correspondent,
the story in two columns (the card text, then the long text), the card's art framed as a photograph with a
caption, and the WHERE IT IS NOW and THE RECORD boxes. A Season card is a front page, a Tablet a Learned Society
column, a Favor a small notice, and the final scores the Final Edition.

### Headline editions (`Catalog.HeadlineFor`, `Editions`)

Every Discovery and every Season card is written up in three editions, as if by different editors: edition 0 is the
catalog's headline table, editions 1 and 2 its variants (`Catalog.HeadlineVariantCount(id)` gives the number, and
`Catalog.HeadlineFor(id, title, edition)` wraps the edition modulo it). An edition carries a banner, a sub-head and
usually a byline of its own.

| Rule | Value |
| --- | --- |
| Edition a game prints | 32-bit FNV-1a hash of the card id's UTF-8 bytes, the game's seed mixed in byte by byte, then `hash ^= hash >> 15` and one more multiply by the FNV prime; the edition is that hash modulo the edition count (`Editions.Edition`) |
| Same game, same card | always the same edition, in the inspector, the season banner on the table, the season line over it, the journal pane and the Field Journal; another seed usually prints another |
| Byline | the edition's own; when it has none, one of the paper's desks in `Catalog.Bylines` ("From our correspondent with the expedition" first) chosen by the same hash divided by 7; "By " is set in front unless it opens with "By" or "From" |
| A headline on the card itself | when a Discovery carries its own `Headline`, that is its only edition |
| No headline written | the banner is made from the title ("... FOUND AT WARKA"), the sub-head from the card text's first sentence |
| Missing sub-head | the card text's first sentence (a Season's story's first sentence) |

The Field Journal adds " · with <team>" to the byline of a team's discovery.

### The Field Journal

The engine writes an entry the first time each Season, Tablet, Favor and Specialist is read, and an entry for every
Discovery and every report. The journal pane lists them with a filter; the PDF is the bound run of the game's
editions: a cover, a front page for each season and an EXTRA! page for each discovery in the order they were read,
the Tablets, Favors, Specialists and reports as notices between them, the Final Edition, the Note on Cultural
Heritage and History, and the sources. The suggested file name is "Goddess Temple Discovery - Field Journal
<date>.pdf".

---

## 12. Screens

| Screen | What it holds | Leads to |
| --- | --- | --- |
| Title | the drawn "Goddess Temple Discovery!" wordmark, the dateline "WARKA, IRAQ — WINTER 1912/13 — TWO TO FOUR EXPEDITIONS", Julius Jordan's descent passage as the epigraph | New Game, Continue (while a game is in progress), Settings, How to Play, History, Credits, Gallery |
| Setup | two to four seats (human or computer, team name or profile, temperament), turns per season with the suggestion for the seat count, difficulty, an optional seed; the last setup is restored | Start, Title |
| Prologue | "Before the first spade": Loftus, Koldewey and Sachau, Andrae, Eduard Meyer, the Ottoman permit, the four tasks of 1912/13 | Begin |
| Play | the table, with a thin strip over it (season line, ticker, standings); the Field Journal, Settings and Gallery panes open over it from the table's header | the Epilogue when the game ends |
| Epilogue | "After 1939": the dig after the war, the Iraq Museum in 2003, the site today | the Final Edition |
| Final Edition | the results table (rank, team, published, unpublished, Tablets, sets, star, total) and the winner | Play again |
| How to Play, History, Credits | the rules; the real people and the timeline of periods; the art, the fonts, the table, the sources and the Note on Cultural Heritage and History, verbatim | back to where they were opened from |
| Gallery | every embedded picture of the Assets art, by category (everything, buildings, plans, objects, symbols, icons, deities, scenes, plates, chrome, deco; `ArtInfo.Categories`, the catalog's folders with the `plan-` and `icon-` pictures set apart), 24 thumbnails a page (`GalleryPageSize`, rendered at 150 pixels the first time they show and kept for the run), "Page n of m · k pictures"; a click shows one picture large (rendered at 700 pixels) with the Title, Subject and "Sources consulted" lines of its SVG header comment, its key and category, and the art's license line (`ArtCatalog.LicenseText`) | Back to the gallery; Close returns to the screen it was opened over |
| Inspector | a page of the Warka Herald for any card (section 11) | closes on Escape, its close button or its timer |

---

## 13. Input

| Key or gesture | Action |
| --- | --- |
| Space or Enter | Roll; once the dice are rolled, End turn |
| 1 to 5 | Dig that trench with the player's selection if it is legal, otherwise the legal dig of that trench that spends least |
| R | Re-roll (the selected die, or the lower unspent kept die) when the season allows it |
| J | Open or close the Field Journal |
| Escape | Close what is on top - the gallery's large view, the gallery, the inspector, the Field Journal, the settings - and leave the Survey or Publish mode |
| Left or Page Up, Right or Page Down | In the gallery (not its large view), the previous or next page; the pages wrap round |
| Click a die | Select or deselect it; select both for their sum |
| Click a trench | Dig it (or survey it in Survey mode) |
| Click a Specialist in the Expedition Row | Recruit it with the selected die, or the cheapest legal die |
| Click a Tablet in the hand | Spend it (+2) on the next dig, or not |
| Click a find in the hand | In Publish mode, add it to the report or take it out |
| Click a card in the hand at the hand limit | Discard it |
| Right-click a face-up card | Read it in the inspector |
| HUD buttons | ROLL, END TURN, STUDY, SURVEY (SURVEY · FREE when a free survey is open), PUBLISH, RE-ROLL, the Worker stepper (- and +); GALLERY, JOURNAL and SETTINGS on the header; in Publish mode the confirm and CANCEL buttons, in Survey mode CANCEL |

Every button enables itself from the engine's legal actions; the HUD never works a rule out itself. A HUD button acts
on the release, as a XAML button does: the press arms the button under the pointer, a release on that same button
fires it, and a release anywhere else disarms it, so the release reaches the table before the pane a button opens
covers it. While any pane covers the table - a screen other than Play, the Field Journal, the settings, the gallery
or the inspector - the view model reports it through `GoddessTempleGameHost.SetPanesOpen`, and neither the table nor
the HUD takes a click.

---

## 14. Persistence keys (`SettingsService`)

The store is the AppSettings add-in's, registered under the application name `GoddessTempleDiscovery` in the
per-user default folder; an autoplay run keeps a scratch store in `GoddessTempleDiscovery.AutoPlay` under the system
temporary folder.

| Key | Type | Default | Notes |
| --- | --- | --- | --- |
| `GoddessTempleDiscovery.SoundEnabled` | bool | true | the card and dice sounds and the paper rustle |
| `GoddessTempleDiscovery.ReducedMotion` | bool | false | animations complete at once |
| `GoddessTempleDiscovery.AnimationSpeed` | double | 1.0 | clamped to 0.25 to 4, stored to two decimals; the settings pane offers 0.5x, 1x, 1.5x, 2x, 3x |
| `GoddessTempleDiscovery.RevealComputerDiscoveries` | bool | true | the inspector opens for the computer teams' finds |
| `GoddessTempleDiscovery.TurnsPerSeason` | int | 2 | clamped to 1 to 3; written when a game starts |
| `GoddessTempleDiscovery.Difficulty` | string | Standard | Easy, Standard or Hard; written when a game starts |
| `GoddessTempleDiscovery.LastSeats` | string | empty | the last setup's seats as a JSON array of `SeatRecord` (team name, profile id, computer or not, temperament); malformed data reads as none |

---

## 15. Diagnostics

| Switch | Effect |
| --- | --- |
| `GODDESSTEMPLE_USE_CPU=1` | the CPU render tier instead of the GPU tier |
| `GODDESSTEMPLE_AUTOPLAY=1` | the game presses New Game itself, seats four computer teams (Surveyor, Deep Digger, Scholar, Surveyor) at one turn a season, plays to the end, writes the Field Journal to `GoddessTempleDiscovery-FieldJournal-autoplay.pdf` in the temporary folder, logs `GODDESSTEMPLE AUTOPLAY PASS` and stops the engine |
| `GODDESSTEMPLE_AUTOPLAY_SEED=n` | pins the autoplay game's seed |

Every line the game logs starts with `[GoddessTemple]`.
