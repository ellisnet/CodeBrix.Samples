using System;

namespace PdfSideBySide.Services;

/// <summary>
/// Where the main view model reads the command line that may name the two PDFs to pre-load. Nothing
/// registers it in the application, so the view model reads <see cref="Environment.GetCommandLineArgs"/>;
/// a host whose own process arguments are not documents (a test host, for one) registers its own.
/// </summary>
public interface IStartupArguments
{
    /// <summary>
    /// Returns the command line in the shape <see cref="Environment.GetCommandLineArgs"/> does: the
    /// program first, then the arguments; the second and third elements are the left and right PDFs.
    /// </summary>
    string[] GetCommandLineArgs();
}
