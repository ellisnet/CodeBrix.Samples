using System;
using System.Threading.Tasks;

namespace GoddessTempleDiscovery.Services;

/// <summary>
/// The one thing the view model cannot do for itself when it exports the Field Journal: ask a person where to write
/// the file. The page fills it in with a save picker; an alternate host (the PlayTests) may register its own
/// implementation in the service container, which the view model then prefers.
/// </summary>
public interface IJournalFileBridge
{
    /// <summary>
    /// Shows a "save as" dialog seeded with a suggested name and returns the chosen full path, or null when cancelled.
    /// Signature: <c>Func&lt;suggestedFileName, typeName, extension, Task&lt;chosenPathOrNull&gt;&gt;</c>.
    /// </summary>
    Func<string, string, string, Task<string>> PickSavePathAsync { get; set; }
}
