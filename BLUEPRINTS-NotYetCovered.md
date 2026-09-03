# CodeBrix.Samples Blueprints: Not yet covered by a sample

This file lists the topics that no application in this repository demonstrates
yet, so a reader looking for one of them knows the gap is known rather than
hidden in another file. It holds no recipes.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Related blueprints

- [BLUEPRINTS-AppStructureAndStartup.md](BLUEPRINTS-AppStructureAndStartup.md) - the closest existing material on windows, pages and navigation
- [BLUEPRINTS-SettingsAndPersistence.md](BLUEPRINTS-SettingsAndPersistence.md) - the only persistent store the applications currently show, the settings store
- [BLUEPRINTS-DocumentsAndData.md](BLUEPRINTS-DocumentsAndData.md) - the data access the applications do show: archives, REST services and generated documents
- [BLUEPRINTS-ProjectLayoutAndPackaging.md](BLUEPRINTS-ProjectLayoutAndPackaging.md) - the packaging configuration that does exist, for the native WinUI head

---

## Not yet covered by a sample

- Navigating between several pages with a back stack, rather than one page per
  application.
- Opening a second top-level window from a running application.
- Localized user-interface strings, and formatting for a culture other than the
  running machine's.
- Printing a document or a page from a Skia head.
- Drag and drop between the application and the desktop.
- A database-backed data layer, beyond the settings store the AppSettings
  add-in provides.
- Publishing and installers for the Skia heads; only the native WinUI head's
  packaging configuration appears here.
- Accessibility: naming elements for a screen reader, or a keyboard-only path
  through a page.
