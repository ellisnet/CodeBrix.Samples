using CodeBrix.VideoProcessing.Core;
using CodeBrix.VideoProcessing.Core.Tools;

namespace CodeBrixVideoTool.Processing.Tools;

/// <summary>
/// The video-processing engine the application registered at start-up, as this library sees it.
/// </summary>
/// <remarks>
/// <para>
/// Everything this library asks of an engine goes through CodeBrix.VideoProcessing.Core, the
/// engine-neutral contract, so it never names one: each head registers the engine for its platform -
/// the FFmpeg engine on the desktop, the MediaCodec engine on Android - and the same code runs on both.
/// </para>
/// <para>
/// The two engines differ in one way that shapes this whole library. The FFmpeg engine offers the
/// contract's external-tools facet - it can find and run <c>ffmpeg</c> - so the work only FFmpeg can do
/// (an MP4 export that carries captions and chapters, captions lifted out of an MP4, the synthetic test
/// clips) runs as FFmpeg command lines through it. The Android engine offers no tools, and every route
/// that would have used them takes an in-process answer instead, or says in the run's notes what could
/// not be carried.
/// </para>
/// </remarks>
public static class ProcessingEngine
{
    /// <summary>The registered engine.</summary>
    /// <exception cref="VideoToolProcessingException">No engine was registered at start-up.</exception>
    public static IVideoProcessor Current
    {
        get
        {
            try
            {
                return VideoProcessors.Current;
            }
            catch (VideoProcessorNotRegisteredException exception)
            {
                throw new VideoToolProcessingException(exception.Message, exception);
            }
        }
    }

    /// <summary>
    /// True when the registered engine can run FFmpeg command lines - the desktop's FFmpeg engine.
    /// It says what the engine offers, not that FFmpeg is installed: a missing FFmpeg is found out when
    /// a line is run, and reported then.
    /// </summary>
    public static bool HasExternalTools => TryGetExternalTools(out _);

    /// <summary>The registered engine's external tools, when it offers them.</summary>
    /// <param name="tools">The tools, or null.</param>
    /// <returns>True when there is an engine and it offers tools.</returns>
    internal static bool TryGetExternalTools(out IExternalTools tools)
    {
        tools = null;
        return VideoProcessors.TryGetCurrent(out var engine) && engine.TryGetExternalTools(out tools);
    }
}
