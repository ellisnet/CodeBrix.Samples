# The inscription in the spread's Octaquetra

The gold band of the large Octaquetra behind the spread is engraved in cuneiform with
**Inana's claiming of the ME** from the Sumerian composition *Inana and Enki*, as one continuous strand
that follows the band round every petal and through the heart.

## Text

- **Source:** The Electronic Text Corpus of Sumerian Literature (ETCSL), Oxford, composition
  c.1.3.1 *Inana and Enki*, **Segment E, lines 1–36**, in which Inana declares
  "he has given me …" (*ma-an-šum₂*) for each of the divine powers.
  Transliteration: https://etcsl.orinst.ox.ac.uk/cgi-bin/etcsl.cgi?text=c.1.3.1
  Translation: https://etcsl.orinst.ox.ac.uk/cgi-bin/etcsl.cgi?text=t.1.3.1
- **Signs:** every reading was mapped to its sign through the Oracc Global Sign List
  (https://github.com/oracc/osl, `00lib/osl.asl`), using each sign's own Unicode character.
  `build_tablet.py` reproduces the mapping; `tablet-E.json` is its output, sign by sign.

## Editorial treatment

The ribbon follows the ETCSL edition as read, with every sign engraved alike:

| Edition | Engraved as |
| --- | --- |
| `[ … ]` restored by the editors where the tablet is broken (for example the refrain *ma-an-šum₂*) | engraved like every other sign |
| `⌜ … ⌝` damaged but legible | engraved like every other sign |
| `⟨ … ⟩` omitted by the scribe (the refrain abbreviated to *ma*) | not engraved, as it was never written |
| `X` illegible, the unidentified inner sign of `DAG.KISIM₅×X` in line 21 | left unlettered, one sign wide |

Line 8 follows the manuscript: ETCSL normalises *kaš₄ di-di* but notes that the source reads *du*,
so *kaš₄ du* is engraved.

## Layout and script

- The band's centre line is measured from the authored artwork (a distance transform of the filled shape
  gives the ridge down the middle of the band) and a smooth, eight-fold lobe curve is fitted to it
  (mean error about 2 artwork units against a 27.2-unit band).
- The strand runs the four infinity loops end to end: out along a petal, round its tip, through the heart
  and into the opposite petal. The text starts at the upper petal and reads along the strand; poetic lines
  are separated by a wider space. The heart itself, where every pass crosses, is left unlettered.
- The tallest sign spans 94% of the band and signs within a word sit close together. The passage takes
  about 60% of the strand, so it continues from its first line again, like a litany, and stops at the end
  of a whole poetic line (59 lines in all: lines 1–36, then 1–23).
- Where the band crosses itself, the later pass lies over the earlier, like a woven ribbon.
- Font: **Noto Sans Cuneiform** (SIL Open Font License), chosen because it covers all 62 signs in one
  consistent hand. Its sign shapes are the standard Unicode reference forms, not the Old Babylonian
  ductus of the surviving manuscripts. Santakku (Old Babylonian, by Sylvie Vanséveren, Hethitologie
  Portal Mainz) lacks du₁₄, ni₂, simug and the NU₁₁ component of ĝizzal, so a single consistent Old
  Babylonian hand was not possible.

## Rebuild

From the `InannaOctaquetra` folder (the app embeds the band from the Reading library, so the
rendered file goes there):

```bash
dotnet run --project Packaging/IconRenderer -- ribbon Packaging/Artwork/Tablet/tablet-E.json \
  Packaging/Artwork/Tablet/Fonts/NotoSansCuneiform-Regular.ttf \
  src/libs/InannaOctaquetra.Reading/Art/octaquetra-inscribed.png 2000
```

IconRenderer references SkiaSharp's macOS native assets, so it runs on a Mac.

## The font

`Fonts/NotoSansCuneiform-Regular.ttf` is kept here so the band can be rebuilt from this repository
alone. It is used only to render `octaquetra-inscribed.png`; the font itself is not shipped in the
app, and the app does not need it.

- **Font:** Noto Sans Cuneiform Regular, version 2.001 (ttfautohint v1.8.4.7)
- **Copyright:** Copyright 2022 The Noto Project Authors (https://github.com/notofonts/cuneiform)
- **License:** SIL Open Font License, Version 1.1 - the full text, with the copyright line, is in
  `Fonts/OFL-NotoSansCuneiform.txt`
- **Obtained from:** Google Fonts (https://fonts.google.com/noto/specimen/Noto+Sans+Cuneiform),
  downloaded 2026-10-12
- **SHA-256:** `914131c044cf06c43dd96a49eda9e8ed3d2491d48ce6ac7955a813ea6d5cbb47`
- Covers all 62 distinct signs in `tablet-E.json`.
