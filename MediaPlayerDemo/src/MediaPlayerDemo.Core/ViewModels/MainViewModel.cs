using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace MediaPlayerDemo.ViewModels;

[Microsoft.UI.Xaml.Data.Bindable]
public class MainViewModel : SimpleViewModel
{
    private const string DefaultMediaAddress =
        "https://mdn.github.io/learning-area/html/multimedia-and-embedding/video-and-audio-content/rabbit320.mp4";
        
    private void LoadMedia()
    {
        try
        {
            var uri = new Uri(MediaAddress);
            PlayerSource = MediaSource.CreateFromUri(uri);
            StatusText = $"Loaded: {uri}";
        }
        catch (Exception ex)
        {
            StatusText = $"Cannot load '{MediaAddress}': {ex.Message}";
        }
    }

    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Debug.WriteLine("Main view model startup.");

        //Load (and, because the player has AutoPlay enabled, start) the default media on startup
        LoadMedia();
    }

    #region | Bindable properties |

    private string _mediaAddress = DefaultMediaAddress;
    [AffectsCommands(nameof(LoadCommand))]
    public string MediaAddress
    {
        get => _mediaAddress;
        set => SetProperty(ref _mediaAddress, value ?? string.Empty);
    }

    private IMediaPlaybackSource _playerSource;
    public IMediaPlaybackSource PlayerSource
    {
        get => _playerSource;
        private set => SetProperty(ref _playerSource, value);
    }

    private string _statusText = "Ready";
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value ?? string.Empty);
    }

    //The stretch modes offered by the ComboBox. The Stretch enum's member names ("Uniform",
    //  "UniformToFill", "Fill", "None") are exactly the text we want shown, so the ComboBox can
    //  bind straight to the enum values with no separate label list.
    public IReadOnlyList<Stretch> StretchOptions { get; } = new[]
    {
        Stretch.Uniform,
        Stretch.UniformToFill,
        Stretch.Fill,
        Stretch.None,
    };

    //The player's stretch mode, two-way bound to the ComboBox's SelectedItem.
    private Stretch _selectedStretch = Stretch.Uniform;
    public Stretch SelectedStretch
    {
        get => _selectedStretch;
        set => SetEnumProperty(ref _selectedStretch, value);
    }

    #endregion

    #region | Commands and their implementations |

    #region LoadCommand

    private SimpleCommand _loadCommand;
    public SimpleCommand LoadCommand =>
        (_loadCommand ??= new SimpleCommand(CanLoad, DoLoad));

    private bool CanLoad() => !string.IsNullOrWhiteSpace(MediaAddress);

    private void DoLoad()
    {
        if (CanLoad())
        {
            LoadMedia();
        }
    }

    #endregion

    #endregion

    #region | IDisposable implementation |

    public override void Dispose()
    {
        _loadCommand?.Dispose();
        _loadCommand = null;
        base.Dispose();
    }

    #endregion
}
