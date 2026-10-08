using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.Simple;
using GoddessTempleDiscovery.Game.Journal;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.Helpers;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Services;
using Microsoft.UI.Xaml;

namespace GoddessTempleDiscovery.ViewModels;

public partial class MainViewModel
{
    private const string AllKinds = "Everything";

    private readonly List<JournalItem> _allJournalItems = new List<JournalItem>();

    #region | The Field Journal |

    /// <summary>True while the journal is open over the table.</summary>
    [AffectsProperties(nameof(JournalVisibility))]
    public bool IsJournalOpen
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>The journal overlay.</summary>
    public Visibility JournalVisibility => GetVisibility(IsJournalOpen);

    /// <summary>The entries shown (after the filter).</summary>
    public ObservableCollection<JournalItem> JournalItems { get; } = new ObservableCollection<JournalItem>();

    /// <summary>The filter choices.</summary>
    public IReadOnlyList<string> JournalFilterOptions { get; } =
        new[] { AllKinds }.Concat(Enum.GetNames<JournalEntryKind>()).ToArray();

    /// <summary>The chosen filter.</summary>
    public string JournalFilter
    {
        get;
        set
        {
            SetProperty(ref field, value ?? AllKinds);
            FilterJournal();
        }
    } = AllKinds;

    /// <summary>"23 entries read this game".</summary>
    public string JournalCountText => string.Format(CultureInfo.InvariantCulture, "{0} entries read this game", _journal.Count);

    /// <summary>What the last export did, or empty.</summary>
    public string JournalStatus
    {
        get;
        private set => SetProperty(ref field, value ?? string.Empty);
    } = string.Empty;

    /// <summary>The folder the last journal was saved in, or empty.</summary>
    [AffectsCommands(nameof(OpenJournalFolderCommand))]
    public string LastJournalFolder
    {
        get;
        private set => SetProperty(ref field, value ?? string.Empty);
    } = string.Empty;

    /// <inheritdoc />
    public Func<string, string, string, Task<string>> PickSavePathAsync { get; set; }

    /// <summary>Opens or closes the journal.</summary>
    public SimpleCommand JournalCommand => field ??= new SimpleCommand(() => IsJournalOpen = !IsJournalOpen);

    /// <summary>Closes the journal.</summary>
    public SimpleCommand CloseJournalCommand => field ??= new SimpleCommand(() => IsJournalOpen = false);

    /// <summary>Exports the Field Journal as a PDF through the save picker.</summary>
    public SimpleCommand ExportJournalCommand => field ??= new SimpleCommand(() => _journal.Count > 0 && !IsExporting, (Func<Task>)ExportJournalAsync);

    /// <summary>Opens the folder of the last export.</summary>
    public SimpleCommand OpenJournalFolderCommand => field ??= new SimpleCommand(() => LastJournalFolder.Length > 0, (Func<Task>)OpenJournalFolderAsync);

    /// <summary>True while a journal is being written.</summary>
    [AffectsCommands(nameof(ExportJournalCommand))]
    public bool IsExporting
    {
        get;
        private set => SetProperty(ref field, value);
    }

    private void FilterJournal()
    {
        JournalItems.Clear();
        foreach (var item in _allJournalItems.Where(i => JournalFilter == AllKinds || i.Kind.ToString() == JournalFilter))
        {
            JournalItems.Add(item);
        }
    }

    private async Task ExportJournalAsync()
    {
        if (_journal.Count == 0 || IsExporting)
        {
            return;
        }

        //An alternate host may register its own file bridge; the page's save picker is the default
        var pick = GetService<IJournalFileBridge>()?.PickSavePathAsync ?? PickSavePathAsync;
        if (pick == null)
        {
            JournalStatus = "This head has no save dialog.";
            return;
        }

        IsExporting = true;
        try
        {
            var date = DateTime.Now;
            var path = await pick(JournalPdfBuilder.SuggestedFileName(date), "PDF document", ".pdf");
            if (string.IsNullOrWhiteSpace(path))
            {
                JournalStatus = "Export cancelled.";
                return;
            }

            path = FileDialogHelper.ToFileSystemPath(path);
            FileDialogHelper.RemoveEmptyPlaceholder(path);
            var input = new JournalPdfInput(_journal.ToArray(), _teams.Select(t => t.Name).ToArray(), _finalScores, date, Host?.GameSeed ?? 0);
            var pdf = await Task.Run(() => new JournalPdfBuilder().Build(input));
            await File.WriteAllBytesAsync(path, pdf.Bytes);
            LastJournalFolder = Path.GetDirectoryName(Path.GetFullPath(path)) ?? string.Empty;
            JournalStatus = string.Format(CultureInfo.InvariantCulture, "Saved {0} ({1} pages).", Path.GetFileName(path), pdf.PageCount);
        }
        catch (Exception ex)
        {
            JournalStatus = "The journal could not be saved: " + ex.Message;
        }
        finally
        {
            IsExporting = false;
        }
    }

    private async Task OpenJournalFolderAsync()
    {
        try
        {
            var opened = await Windows.System.Launcher.LaunchUriAsync(new Uri(Path.GetFullPath(LastJournalFolder)));
            if (!opened)
            {
                JournalStatus = "No application was available to open that folder.";
            }
        }
        catch (Exception ex)
        {
            JournalStatus = "The folder could not be opened: " + ex.Message;
        }
    }

    #endregion

    #region | Settings |

    /// <summary>True while the settings are open.</summary>
    [AffectsProperties(nameof(SettingsVisibility))]
    public bool IsSettingsOpen
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>The settings overlay.</summary>
    public Visibility SettingsVisibility => GetVisibility(IsSettingsOpen);

    /// <summary>Opens or closes the settings.</summary>
    public SimpleCommand SettingsCommand => field ??= new SimpleCommand(() => IsSettingsOpen = !IsSettingsOpen);

    /// <summary>Closes the settings.</summary>
    public SimpleCommand CloseSettingsCommand => field ??= new SimpleCommand(() => IsSettingsOpen = false);

    /// <summary>The card and dice sounds.</summary>
    public bool SoundEnabled
    {
        get;
        set
        {
            SetProperty(ref field, value);
            Store(() => SettingsService.SoundEnabled = value);
        }
    }

    /// <summary>Animations complete at once.</summary>
    public bool ReducedMotion
    {
        get;
        set
        {
            SetProperty(ref field, value);
            Store(() => SettingsService.ReducedMotion = value);
        }
    }

    /// <summary>The inspector opens for the computer teams' discoveries.</summary>
    public bool RevealComputerDiscoveries
    {
        get;
        set
        {
            SetProperty(ref field, value);
            Store(() => SettingsService.RevealComputerDiscoveries = value);
        }
    }

    /// <summary>The animation speed choices.</summary>
    public IReadOnlyList<string> SpeedOptions { get; } = new[] { "0.5×", "1×", "1.5×", "2×", "3×" };

    /// <summary>The chosen animation speed.</summary>
    public string SelectedSpeed
    {
        get;
        set
        {
            SetProperty(ref field, value ?? "1×");
            var speed = double.TryParse((value ?? "1").TrimEnd('×'), NumberStyles.Float, CultureInfo.InvariantCulture, out var s) ? s : 1;
            Store(() => SettingsService.AnimationSpeed = speed);
        }
    } = "1×";

    private bool _loadingSettings;

    private void LoadSettings()
    {
        _loadingSettings = true;
        SoundEnabled = SettingsService.SoundEnabled;
        ReducedMotion = SettingsService.ReducedMotion;
        RevealComputerDiscoveries = SettingsService.RevealComputerDiscoveries;
        var speed = SettingsService.AnimationSpeed;
        SelectedSpeed = SpeedOptions.OrderBy(o => Math.Abs(double.Parse(o.TrimEnd('×'), CultureInfo.InvariantCulture) - speed)).First();
        _loadingSettings = false;
    }

    private void Store(Action write)
    {
        if (_loadingSettings || !SettingsService.IsInitialized)
        {
            return;
        }

        write();
        Host?.ApplySettings();
    }

    #endregion
}
