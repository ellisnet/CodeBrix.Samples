using CodeBrix.VideoProcessing.Core.Frames;
using CodeBrix.VideoProcessing.Core.Media;
using CodeBrix.VideoProcessing.Core.Output;
using CodeBrix.VideoProcessing.Core.Tools;
using CodeBrixVideoTool.Processing.Tools;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeBrixVideoTool.Processing.Samples;

/// <summary>
/// Makes the small synthetic clips the tests and the application's scripted smoke run need, so that
/// neither has to carry a media file around with it.
/// </summary>
/// <remarks>
/// <para>
/// The picture and the tone come from FFmpeg's own test sources, run through the registered engine's
/// external tools like everything else this application asks FFmpeg to do. Nothing is copied from
/// anywhere and nothing is left behind: every clip is written where the caller asks.
/// </para>
/// <para>
/// An engine without external tools - Android's - has no FFmpeg test sources to draw on. There the plain
/// MP4 is drawn here instead, frame by frame, and written through the engine's own standard writer: a
/// scrolling gradient with a bar sweeping across it, and the same 440 Hz tone. The clip with an alpha
/// channel and the clip with captions and chapters need FFmpeg's muxers and are refused there, which
/// <see cref="CanWriteRichClips" /> says in advance.
/// </para>
/// </remarks>
public static class SampleClipFactory
{
    private const int ToneSampleRate = 48000;
    private const double ToneFrequency = 440d;

    /// <summary>
    /// True when the registered engine can write the clips that need FFmpeg: the one with an alpha channel
    /// and the one with captions and chapters. The plain MP4 can be written on every engine.
    /// </summary>
    public static bool CanWriteRichClips => ProcessingEngine.HasExternalTools;

    /// <summary>Writes a small H.264 and AAC MP4 file, of the kind an import starts from.</summary>
    /// <param name="path">The file to write, overwriting anything already there.</param>
    /// <param name="width">The picture's width in pixels. Must be even.</param>
    /// <param name="height">The picture's height in pixels. Must be even.</param>
    /// <param name="duration">How long the clip runs.</param>
    /// <param name="frameRate">The picture's frame rate.</param>
    /// <param name="cancellationToken">Stops the generation.</param>
    /// <returns>The path that was written.</returns>
    /// <exception cref="VideoToolProcessingException">The clip could not be written.</exception>
    public static async Task<string> WriteMp4Async(
        string path,
        int width = 320,
        int height = 240,
        TimeSpan duration = default,
        double frameRate = 25d,
        CancellationToken cancellationToken = default)
    {
        var length = duration <= TimeSpan.Zero ? TimeSpan.FromSeconds(3) : duration;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));

        if (!ProcessingEngine.TryGetExternalTools(out var tools))
        {
            await Task.Run(() => DrawMp4(path, width, height, length, frameRate, cancellationToken), CancellationToken.None)
                .ConfigureAwait(false);
            return path;
        }

        var filterGraph = string.Create(CultureInfo.InvariantCulture,
            $"testsrc2=size={width}x{height}:rate={frameRate}[out0]; sine=frequency={ToneFrequency}:sample_rate={ToneSampleRate}[out1]");

        var arguments = FfmpegLine.Start()
            .Add("-f lavfi").Add("-i").AddPath(filterGraph)
            .Add("-t " + Seconds(length))
            .Add("-c:v libx264")
            .Add("-crf 28")
            .Add("-preset ultrafast")
            .Add("-pix_fmt yuv420p")
            .Add("-c:a aac")
            .Add("-b:a 96k")
            .Add("-f mp4")
            .AddPath(path);

        var run = await FfmpegLine.RunAsync(tools, arguments, TimeSpan.Zero, null, cancellationToken).ConfigureAwait(false);
        if (!run.Succeeded || !File.Exists(path))
        {
            throw new VideoToolProcessingException("A sample clip could not be generated. " + run.ErrorText);
        }

        return path;
    }

    /// <summary>
    /// Writes a small QuickTime file whose picture has an ALPHA CHANNEL: FFmpeg's test pattern at half
    /// size in the middle of a fully transparent frame, stored losslessly as RGBA PNG frames, with an
    /// uncompressed tone. It is the kind of source a Mode 3 master is made from.
    /// </summary>
    /// <param name="path">The file to write, overwriting anything already there.</param>
    /// <param name="width">The picture's width in pixels. Must be a multiple of four.</param>
    /// <param name="height">The picture's height in pixels. Must be a multiple of four.</param>
    /// <param name="duration">How long the clip runs.</param>
    /// <param name="frameRate">The picture's frame rate.</param>
    /// <param name="cancellationToken">Stops the generation.</param>
    /// <returns>The path that was written.</returns>
    /// <exception cref="VideoToolProcessingException">
    /// FFmpeg could not write the clip, or the registered engine cannot run FFmpeg.
    /// </exception>
    public static async Task<string> WriteAlphaMovAsync(
        string path,
        int width = 320,
        int height = 240,
        TimeSpan duration = default,
        double frameRate = 25d,
        CancellationToken cancellationToken = default)
    {
        var tools = RequireTools("a sample clip with an alpha channel");
        var length = duration <= TimeSpan.Zero ? TimeSpan.FromSeconds(3) : duration;
        //A fully transparent frame, the pattern at half size laid over its middle, and a tone.
        var filterGraph = string.Join("; ",
            string.Create(CultureInfo.InvariantCulture, $"color=c=black@0.0:size={width}x{height}:rate={frameRate},format=rgba[bg]"),
            string.Create(CultureInfo.InvariantCulture, $"testsrc2=size={width / 2}x{height / 2}:rate={frameRate},format=rgba[fg]"),
            string.Create(CultureInfo.InvariantCulture, $"[bg][fg]overlay=x={width / 4}:y={height / 4}:shortest=1[out0]"),
            string.Create(CultureInfo.InvariantCulture, $"sine=frequency={ToneFrequency}:sample_rate={ToneSampleRate}[out1]"));

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));

        var arguments = FfmpegLine.Start()
            .Add("-f lavfi").Add("-i").AddPath(filterGraph)
            .Add("-t " + Seconds(length))
            .Add("-c:v png")
            .Add("-pix_fmt rgba")
            .Add("-c:a pcm_s16le")
            .Add("-f mov")
            .AddPath(path);

        var run = await FfmpegLine.RunAsync(tools, arguments, TimeSpan.Zero, null, cancellationToken).ConfigureAwait(false);
        if (!run.Succeeded || !File.Exists(path))
        {
            throw new VideoToolProcessingException(
                "A sample clip with an alpha channel could not be generated. " + run.ErrorText);
        }

        return path;
    }

    /// <summary>
    /// Writes a small MP4 that carries a caption track and chapters as well as picture and sound -
    /// the kind of file an import has something to carry across.
    /// </summary>
    /// <param name="folder">The folder to write the clip and its two working files into.</param>
    /// <param name="duration">How long the clip runs.</param>
    /// <param name="cancellationToken">Stops the generation.</param>
    /// <returns>The path of the MP4 that was written.</returns>
    /// <exception cref="VideoToolProcessingException">
    /// FFmpeg could not write the clip, or the registered engine cannot run FFmpeg.
    /// </exception>
    public static async Task<string> WriteRichMp4Async(
        string folder, TimeSpan duration = default, CancellationToken cancellationToken = default)
    {
        var tools = RequireTools("a sample clip with captions and chapters");
        var length = duration <= TimeSpan.Zero ? TimeSpan.FromSeconds(3) : duration;
        Directory.CreateDirectory(folder);

        var plain = Path.Combine(folder, "sample-plain.mp4");
        var captions = Path.Combine(folder, "sample.en.vtt");
        var chapters = Path.Combine(folder, "sample.ffmetadata");
        var rich = Path.Combine(folder, "sample.mp4");

        await WriteMp4Async(plain, 320, 240, length, 25d, cancellationToken).ConfigureAwait(false);
        WriteWebVtt(captions, length);
        WriteChapterMetadata(chapters, length);

        var arguments = FfmpegLine.Start()
            .Add("-i").AddPath(plain)
            .Add("-i").AddPath(captions)
            .Add("-i").AddPath(chapters)
            .Add("-map_metadata 2")
            .Add("-map 0:v:0")
            .Add("-map 0:a:0")
            .Add("-map 1:s:0")
            .Add("-c copy")
            .Add("-c:s mov_text")
            .Add("-metadata:s:s:0 language=eng")
            .Add("-f mp4")
            .AddPath(rich);

        var run = await FfmpegLine.RunAsync(tools, arguments, TimeSpan.Zero, null, cancellationToken).ConfigureAwait(false);
        if (!run.Succeeded || !File.Exists(rich))
        {
            throw new VideoToolProcessingException(
                "A sample clip with captions and chapters could not be generated. " + run.ErrorText);
        }

        return rich;
    }

    /// <summary>Writes a small WebVTT caption file with a few cues spread over a duration.</summary>
    /// <param name="path">The file to write, overwriting anything already there.</param>
    /// <param name="duration">The media the cues belong to.</param>
    /// <param name="cueCount">How many cues to write.</param>
    /// <returns>The path that was written.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The cue count is not positive.</exception>
    public static string WriteWebVtt(string path, TimeSpan duration, int cueCount = 3)
    {
        if (cueCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cueCount), cueCount, "A caption file needs at least one cue.");
        }

        var length = duration <= TimeSpan.Zero ? TimeSpan.FromSeconds(3) : duration;
        var slot = length / cueCount;

        var text = new StringBuilder("WEBVTT\n\n");
        for (var index = 0; index < cueCount; index++)
        {
            var start = slot * index;
            var end = start + (slot * 0.8);
            text.Append(CultureInfo.InvariantCulture, $"cue{index + 1}\n")
                .Append(Format(start)).Append(" --> ").Append(Format(end)).Append('\n')
                .Append(CultureInfo.InvariantCulture, $"Sample cue {index + 1}\n\n");
        }

        File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));
        return path;
    }

    /// <summary>Writes a small FFmpeg metadata file describing a few chapters.</summary>
    /// <param name="path">The file to write, overwriting anything already there.</param>
    /// <param name="duration">The media the chapters belong to.</param>
    /// <param name="chapterCount">How many chapters to write.</param>
    /// <returns>The path that was written.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The chapter count is not positive.</exception>
    public static string WriteChapterMetadata(string path, TimeSpan duration, int chapterCount = 3)
    {
        if (chapterCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chapterCount), chapterCount, "A chapter file needs at least one chapter.");
        }

        var length = duration <= TimeSpan.Zero ? TimeSpan.FromSeconds(3) : duration;
        var slot = length / chapterCount;

        var text = new StringBuilder(";FFMETADATA1\n");
        for (var index = 0; index < chapterCount; index++)
        {
            var start = (long)(slot * index).TotalMilliseconds;
            var end = (long)(slot * (index + 1)).TotalMilliseconds;
            text.Append("\n[CHAPTER]\nTIMEBASE=1/1000\n")
                .Append(CultureInfo.InvariantCulture, $"START={start}\n")
                .Append(CultureInfo.InvariantCulture, $"END={end}\n")
                .Append(CultureInfo.InvariantCulture, $"title=Part {index + 1}\n");
        }

        File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));
        return path;
    }

    private static IExternalTools RequireTools(string what)
    {
        if (ProcessingEngine.TryGetExternalTools(out var tools))
        {
            return tools;
        }

        throw new VideoToolProcessingException(
            $"Writing {what} needs FFmpeg, and the '{ProcessingEngine.Current.Name}' engine cannot run it.");
    }

    /// <summary>
    /// The plain MP4 without FFmpeg: the picture drawn here, frame by frame, and written with the tone
    /// through the engine's own standard writer - H.264 and AAC, as FFmpeg would have written it.
    /// </summary>
    private static void DrawMp4(
        string path, int width, int height, TimeSpan length, double frameRate, CancellationToken cancellationToken)
    {
        var rate = new FrameRate((int)Math.Round(frameRate * 1000d), 1000);
        var request = new StandardVideoRequest
        {
            OutputPath = path,
            Width = width,
            Height = height,
            FrameRate = rate,
            Quality = VideoQuality.ConstantQuality(28),
            KeyframeIntervalFrames = (int)Math.Round(rate.FramesPerSecond * 2d),
            HasAudio = true,
            AudioSampleRate = ToneSampleRate,
            AudioChannels = 1,
            AudioBitrateKilobitsPerSecond = 96,
        };

        var chromaWidth = (width + 1) / 2;
        var chromaHeight = (height + 1) / 2;
        var y = new byte[width * height];
        var u = new byte[chromaWidth * chromaHeight];
        var v = new byte[chromaWidth * chromaHeight];
        var frameCount = rate.FrameCountFor(length);
        long samplesWritten = 0;
        var tone = new float[ToneSampleRate];

        using var writer = ProcessingEngine.Current.CreateWriter(request);
        for (long index = 0; index < frameCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DrawFrame(y, u, v, width, height, chromaWidth, chromaHeight, index, frameCount);
            writer.WriteFrame(VideoFrameData.CreateI420(
                width, height,
                VideoPlane.FromArray(y, width, height),
                VideoPlane.FromArray(u, chromaWidth, chromaHeight),
                VideoPlane.FromArray(v, chromaWidth, chromaHeight),
                rate.TimestampOf(index), index));

            //The sound up to the end of this frame, in step with the pictures, from the running sample count.
            var samplesDue = (long)Math.Round(rate.TimestampOf(index + 1).TotalSeconds * ToneSampleRate);
            var count = (int)Math.Min(samplesDue - samplesWritten, tone.Length);
            for (var sample = 0; sample < count; sample++)
            {
                tone[sample] = (float)(0.25d * Math.Sin(2d * Math.PI * ToneFrequency * (samplesWritten + sample) / ToneSampleRate));
            }

            writer.WriteAudio(tone.AsSpan(0, count));
            samplesWritten += count;
        }

        writer.Finish();
    }

    //A diagonal gradient that scrolls, four colour quadrants, and a white bar sweeping across once per clip:
    //enough movement to make a frame that did not change easy to see, and nothing a person has to supply.
    private static void DrawFrame(
        byte[] y, byte[] u, byte[] v, int width, int height, int chromaWidth, int chromaHeight, long index, long frameCount)
    {
        var shift = (int)(index * 4 % 256);
        var barLeft = (int)(index * (width - 16) / Math.Max(1, frameCount - 1));

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var inBar = column >= barLeft && column < barLeft + 16;
                y[(row * width) + column] = inBar ? (byte)235 : (byte)(16 + ((column + row + shift) % 200));
            }
        }

        for (var row = 0; row < chromaHeight; row++)
        {
            for (var column = 0; column < chromaWidth; column++)
            {
                var right = column >= chromaWidth / 2;
                var lower = row >= chromaHeight / 2;
                u[(row * chromaWidth) + column] = (byte)(right ? 180 : 76);
                v[(row * chromaWidth) + column] = (byte)(lower ? 180 : 76);
            }
        }
    }

    private static string Seconds(TimeSpan value) =>
        value.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);

    private static string Format(TimeSpan value) =>
        value.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
}
