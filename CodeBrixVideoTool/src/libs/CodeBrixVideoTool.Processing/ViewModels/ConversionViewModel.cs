using CodeBrix.Platform.Simple;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Operations;
using CodeBrixVideoTool.Processing.Planning;
using CodeBrixVideoTool.Processing.Probing;
using CodeBrixVideoTool.Processing.Resolution;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CodeBrixVideoTool.Processing.ViewModels;

/// <summary>
/// The operation panel: which format the selected file is going to, at what size, what the action is
/// called, and how the one conversion that may be running at a time is getting on.
/// </summary>
/// <remarks>
/// Only one conversion runs at a time, on purpose. Everything a person can start is disabled while
/// one is under way, the progress bar shows a real percentage wherever FFmpeg can supply one, and
/// Cancel is always live.
/// </remarks>
[Microsoft.UI.Xaml.Data.Bindable]
public class ConversionViewModel : SimpleViewModel, IOutputPathBridge
{
    private readonly IConversionRunner runner;
    private CancellationTokenSource cancellation;

    /// <summary>Creates the view model, resolving the conversion runner from the container.</summary>
    public ConversionViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        runner = GetService<IConversionRunner>() ?? new ConversionRunner();
    }

    /// <summary>Raised when a conversion finishes, whether it succeeded, failed or was stopped.</summary>
    public event EventHandler<ConversionOutcome> ConversionFinished;

    #region | Bindable properties |

    /// <summary>What the panel is set up to convert, or null when nothing is selected.</summary>
    [AffectsCommands(nameof(RunCommand))]
    public SourceMediaInfo Source
    {
        get;
        set
        {
            SetProperty(ref field, value);
            RefreshForSource();
        }
    }

    /// <summary>The formats the selected file can go to.</summary>
    public ObservableCollection<DestinationOption> Destinations { get; } = new();

    /// <summary>The format the result will be written in.</summary>
    [AffectsCommands(nameof(RunCommand))]
    public DestinationOption SelectedDestination
    {
        get;
        set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(ActionLabel));
            NotifyPropertyChanged(nameof(AudioCodecText));
            NotifyPropertyChanged(nameof(VideoCodecText));
            NotifyPropertyChanged(nameof(RouteText));
        }
    }

    /// <summary>The sizes the result may be written at, from the source's own size downwards.</summary>
    public ObservableCollection<ResolutionOption> Resolutions { get; } = new();

    /// <summary>The size the result will be written at.</summary>
    public ResolutionOption SelectedResolution
    {
        get;
        set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(RouteText));
        }
    }

    /// <summary>What the action button says: "Import", "Transcode" or "Export".</summary>
    public string ActionLabel
    {
        get
        {
            if (Source is null || SelectedDestination is null)
            {
                return "Convert";
            }

            try
            {
                return MediaFormats.ActionVerb(MediaFormats.OperationFor(Source.Format, SelectedDestination.Kind));
            }
            catch (ArgumentException)
            {
                return "Convert";
            }
        }
    }

    /// <summary>The audio codec the chosen destination is written with. Not a choice - a consequence.</summary>
    public string AudioCodecText => SelectedDestination is null
        ? string.Empty
        : "Audio: " + MediaFormats.AudioCodecFor(SelectedDestination.Kind);

    /// <summary>The video codec the chosen destination is written with. Not a choice - a consequence.</summary>
    public string VideoCodecText => SelectedDestination is null
        ? string.Empty
        : "Video: " + MediaFormats.VideoCodecFor(SelectedDestination.Kind);

    /// <summary>A sentence saying what the conversion is about to do.</summary>
    public string RouteText
    {
        get
        {
            if (Source is null || SelectedDestination is null)
            {
                return "Select a file to convert.";
            }

            var size = SelectedResolution is { IsOriginal: false }
                ? $"{SelectedResolution.Width} x {SelectedResolution.Height}"
                : "its own size";

            var demux = Source.Format == MediaFormatKind.CodeBrixMode2
                ? " The bespoke container is demultiplexed first, without re-encoding."
                : string.Empty;

            return $"{ActionLabel} {Source.FileName} to {SelectedDestination.Label} at {size}.{demux}";
        }
    }

    /// <summary>True while a conversion is under way.</summary>
    [AffectsCommands(nameof(RunCommand), nameof(CancelCommand))]
    public bool IsRunning
    {
        get;
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(ProgressVisibility));
        }
    }

    /// <summary>True once Cancel has been pressed and the conversion has not stopped yet.</summary>
    [AffectsCommands(nameof(CancelCommand))]
    public bool IsCancelling
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>How far the conversion has got, from 0 to 100.</summary>
    public double ProgressPercent
    {
        get;
        private set
        {
            //No SetProperty overload takes a double; compare-and-notify by hand.
            if (field.Equals(value)) { return; }
            field = value;
            NotifyPropertyChanged(nameof(ProgressPercent));
        }
    }

    /// <summary>True while the stage under way cannot say how far through it is.</summary>
    public bool IsProgressIndeterminate
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>What the conversion is doing right now.</summary>
    public string ProgressText
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>What happened last, for the status bar.</summary>
    public string StatusText
    {
        get;
        private set => SetProperty(ref field, value);
    } = "Ready.";

    /// <summary>Whether to show the progress bar and its Cancel button.</summary>
    public Visibility ProgressVisibility => GetVisibility(IsRunning);

    /// <summary>Whether the panel has anything to offer.</summary>
    public Visibility PanelVisibility => GetVisibility(Source is not null);

    #endregion

    #region | Commands and their implementations |

    /// <summary>Starts the conversion the panel is set up for.</summary>
    public SimpleCommand RunCommand => field ??= new SimpleCommand(
        () => !IsRunning && Source is not null && SelectedDestination is not null,
        (Func<object, Task>)(_ => RunAsync()));

    /// <summary>Stops the conversion that is under way.</summary>
    public SimpleCommand CancelCommand => field ??= new SimpleCommand(
        () => IsRunning && !IsCancelling, _ => DoCancel());

    private async Task RunAsync()
    {
        if (IsRunning || Source is null || SelectedDestination is null)
        {
            return;
        }

        var destination = SelectedDestination.Kind;
        var suggested = ConversionPlanner.SuggestOutputFileName(Source, destination);
        var extension = MediaFormats.Extension(destination);

        string outputPath;
        if (PickOutputPathAsync is null)
        {
            outputPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Source.Path)) ?? ".", suggested);
        }
        else
        {
            outputPath = await PickOutputPathAsync(suggested, extension);
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                StatusText = "Cancelled - no destination was chosen.";
                return;
            }
        }

        ConversionPlan plan;
        try
        {
            plan = ConversionPlanner.Create(Source, destination, outputPath, SelectedResolution);
        }
        catch (VideoToolProcessingException exception)
        {
            StatusText = exception.Message;
            return;
        }

        IsRunning = true;
        IsCancelling = false;
        ProgressPercent = 0d;
        IsProgressIndeterminate = true;
        ProgressText = "Starting...";
        StatusText = plan.ToString();

        cancellation = new CancellationTokenSource();
        var progress = new Progress<ConversionProgress>(report =>
        {
            ProgressPercent = report.OverallPercent;
            IsProgressIndeterminate = report.IsIndeterminate;
            ProgressText = report.ToString();
        });

        ConversionOutcome outcome;
        try
        {
            outcome = await runner.RunAsync(plan, progress, cancellation.Token);
        }
        finally
        {
            cancellation.Dispose();
            cancellation = null;
            IsRunning = false;
            IsCancelling = false;
        }

        ProgressPercent = outcome.Succeeded ? 100d : 0d;
        IsProgressIndeterminate = false;
        ProgressText = string.Empty;
        StatusText = outcome.ToString();

        ConversionFinished?.Invoke(this, outcome);
    }

    private void DoCancel()
    {
        if (cancellation is null)
        {
            return;
        }

        IsCancelling = true;
        ProgressText = "Stopping...";
        cancellation.Cancel();
    }

    #endregion

    #region | Head-capability bridges |

    /// <inheritdoc />
    public Func<string, string, Task<string>> PickOutputPathAsync { get; set; }

    #endregion

    private void RefreshForSource()
    {
        Destinations.Clear();
        Resolutions.Clear();

        if (Source is null)
        {
            SelectedDestination = null;
            SelectedResolution = null;
            NotifyPropertyChanged(nameof(PanelVisibility));
            NotifyPropertyChanged(nameof(RouteText));
            return;
        }

        foreach (var destination in MediaFormats.DestinationsFor(Source.Format))
        {
            Destinations.Add(new DestinationOption(destination));
        }

        foreach (var rung in ResolutionLadder.Build(Source.Width, Source.Height))
        {
            Resolutions.Add(rung);
        }

        SelectedDestination = Destinations.Count > 0 ? Destinations[0] : null;
        SelectedResolution = Resolutions.Count > 0 ? Resolutions[0] : null;

        NotifyPropertyChanged(nameof(PanelVisibility));
        NotifyPropertyChanged(nameof(RouteText));
    }
}
