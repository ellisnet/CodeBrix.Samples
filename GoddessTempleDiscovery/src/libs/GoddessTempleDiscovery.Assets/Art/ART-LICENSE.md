# The Goddess Temple Discovery art

This folder holds the 263 vector pictures of Goddess Temple Discovery!: the buildings of
Holy Inanna's Eanna precinct at Uruk and their ground plans, the finds, the symbols and
signs, the deities and heroes of Sumer, scenes of the city and of the excavations of 1912
to 1939, traced plates of public-domain photographs and engravings, the game's chrome and
its 1920s Art Deco pieces. They were drawn for the game and are meant to be used again: in
your own games, books, lessons, slides and projects about Sumer, Uruk, Holy Inanna and
ancient Mesopotamia. Copy the whole folder, or any picture in it.

## License

Every picture in this folder is original work, licensed under the Apache License,
Version 2.0, the license of the CodeBrix.Samples repository. The full text is the
[LICENSE](../../../../../LICENSE) file at the repository root and
<https://www.apache.org/licenses/LICENSE-2.0>. You may use, change and redistribute the
pictures, commercially or not, as that license allows. Keep this page, or a copy of the
license and the attribution line below, with the pictures you redistribute, and say so
when you have changed them.

Attribution line:

> Goddess Temple Discovery art, copyright (c) 2026 Jeremy Ellis and contributors,
> licensed under the Apache License, Version 2.0.

The art's notice also travels inside the game's assets library as
`ArtCatalog.LicenseText`, and the game prints it on its Credits screen.

## The categories

Each picture is one SVG file named in lower kebab case with a prefix that says what it is.
The file name without `.svg` is the picture's key in the game.

| Folder | Prefixes | What is in it |
| --- | --- | --- |
| `buildings/` | `building-`, `plan-` | The temples, terraces, courts and ziggurats of Eanna and Uruk in one three-quarter view, and their ground plans simplified from the published plans, north up |
| `objects/` | `object-` | The finds: the Warka Vase, the Mask of Warka, clay cones and cone mosaic, cylinder seals, archaic and foundation tablets, stamped and glazed bricks, figurines and more |
| `symbols/` | `symbol-`, `icon-` | Her eight-pointed star, the eight-petal rosette, the reed-bundle gatepost, the Dingir star, the lion, the boat of heaven, Venus morning and evening, and the game's small action icons |
| `deities/` | `deity-`, `hero-` | Holy Inanna and the gods of Sumer, and the heroes and poets of Uruk's stories |
| `scenes/` | `scene-` | Moments of the excavations and of the ancient city, as portrait cards |
| `plates/` | `plate-` | Vector tracings of public-domain and CC0 photographs and engravings of the real finds and the real dig |
| `chrome/` | `motif-`, `period-`, `specialist-`, `team-`, `title-`, `splash-` | Deck motifs, an icon for each period of Uruk's history, the specialists, the fictional teams' emblems and the title art |
| `deco/` | `deco-` | The Art Deco chrome: card frames, plates, ribbons, borders, sunbursts, the Warka Herald masthead and the drawn wordmarks |

## The style and the palette

All the pictures share one style: flat vector shapes with a dark outline, light from the
upper left, a second flat tone for shading, and no text elements - every letter in a
wordmark is a drawn shape. They use one palette of eighteen colors and pure white:

| Token | Hex | Use |
| --- | --- | --- |
| INK | `#1E1A17` | outlines, pupils, the deepest shadow |
| CLAY | `#C8955A` | mudbrick, tells, dust, ground |
| CLAY_SHADE | `#9C6B3C` | shaded mudbrick, baked brick |
| CLAY_LIGHT | `#E8C597` | sunlit brick, plaster, light limestone |
| LIMESTONE | `#EDE6D6` | limestone, alabaster, gypsum, the mask |
| LIMESTONE_SHADE | `#C9BFA8` | shaded stone |
| MOSAIC_RED | `#B8322A` | cone-mosaic red, carnelian, Her war aspect |
| MOSAIC_BLACK | `#2B2622` | cone-mosaic black, bitumen |
| MOSAIC_WHITE | `#F4EFE3` | cone-mosaic white |
| LAPIS | `#2A4B8D` | lapis lazuli, the night sky, Her star field |
| LAPIS_LIGHT | `#5B7BC4` | water, glazed brick, lighter lapis |
| GOLD | `#D9A441` | gold, Her star, rosette centers, highlights |
| GOLD_DEEP | `#A8761F` | shaded gold |
| REED | `#7A8A3A` | reeds, marsh, date-palm fronds |
| REED_DEEP | `#4E5A22` | shaded reeds |
| SKY | `#EFE3C8` | a background sky, where there is one |
| NIGHT | `#16213A` | night backgrounds, and the ground of the Deco pieces |
| SILVER | `#BFC3C7` | silver, metal, the expedition's cars |

To recolor a picture, replace its hex values; nothing else sets a color.

## Opening and using the pictures

The files are plain SVG: inline attributes only, no fonts, no embedded images, no
scripts, no style sheets and no references to anything outside the file. They open in any
web browser, in Inkscape and in other vector editors, and they scale to any size without
loss. Most pictures use a `0 0 400 400` view box; building portraits, scenes and some
plates are `0 0 400 560` portrait cards, and the Deco card frames are `0 0 250 400`, with
their corners inside the outer 60 units so they stretch cleanly. Shapes are grouped by
part with kebab-case `id` values, and each file begins with a comment giving its title,
its subject and the sources consulted for it.

To make a bitmap, render the SVG at the size you need, for example with
`rsvg-convert -w 800 deities/deity-inanna.svg -o deity-inanna.png`, or export it from your
editor.

## Where the pictures come from

The drawings are original: each was drawn from an understanding of the published
excavation reports, plans and museum objects it shows, and no existing image was copied,
embedded or converted into them. The header comment of each file names what was
consulted.

The plates in `plates/` are the exception in method: each is a vector tracing,
posterized to the palette and redrawn as paths, of a photograph or engraving that is in
the public domain or released under CC0 - photographs on Wikimedia Commons released under
CC0, Annemarie Schwarzenbach's photographs of the dig in the Swiss Literary Archives,
Julius Jordan's plates in his preliminary reports, a studio portrait of 1900, and an
engraving from W. K. Loftus's travel book of 1857. Each plate's header comment names its
source, its creator and the license line it was chosen under, and the application's
`THIRD-PARTY-NOTICES.txt` lists them all. The tracings themselves are under the Apache
License with the rest of the art; when you use a plate, crediting the original
photographer or engraver as the header names them is a courtesy worth keeping.
