namespace WikipediaPublisher.Helpers;

/// <summary>
/// Capabilities of the current application head, set by each head's entry point
/// before the UI starts.
/// </summary>
public static class AppCapabilities
{
    /// <summary>
    /// True when this head supports the embedded WebView browser for picking
    /// articles (Win32, Skia-on-WPF, macOS, WinUI and WPF heads). Heads without
    /// WebView support (the Linux heads, for now) show a native search-results
    /// list and an article-URL box instead.
    /// </summary>
    public static bool HasWebView { get; set; }
}
