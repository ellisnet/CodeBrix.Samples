using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace CodeBrixVideoTool.Processing.Containers;

/// <summary>
/// Writes an Ogg Vorbis or Ogg Opus file from the coded audio packets a container gave up, so that
/// a stream FFmpeg cannot reach inside its original container can be handed to FFmpeg after all.
/// </summary>
/// <remarks>
/// The three Xiph setup headers a Vorbis stream needs come out of the track's codec-private data;
/// an Opus stream's codec-private data is its OpusHead, and its comment header is synthesised here
/// because no container stores one. Granule positions are computed from the packets' own
/// timestamps: a Vorbis granule counts samples at the track's sample rate, an Opus granule counts
/// samples at 48 kHz and includes the encoder's pre-skip, which is what both formats specify.
/// </remarks>
public sealed class OggAudioWriter : IDisposable
{
    private const uint SerialNumber = 0x43425654;
    private const int OpusGranuleSampleRate = 48_000;
    private const string Vendor = "CodeBrixVideoTool";

    private readonly OggStreamWriter stream;
    private readonly int granuleSampleRate;
    private readonly long granuleOffset;
    private long lastGranule;
    private bool completed;
    private bool disposed;

    private OggAudioWriter(OggStreamWriter stream, int granuleSampleRate, long granuleOffset)
    {
        this.stream = stream;
        this.granuleSampleRate = granuleSampleRate;
        this.granuleOffset = granuleOffset;
        lastGranule = granuleOffset;
    }

    /// <summary>Creates an Ogg Vorbis file and writes its three setup headers.</summary>
    /// <param name="path">The file to create, overwriting anything already there.</param>
    /// <param name="codecPrivate">The track's Xiph-laced codec-private data.</param>
    /// <param name="sampleRate">The track's sample rate in hertz, which its granules count in.</param>
    /// <returns>A writer ready for audio packets.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The sample rate is not positive.</exception>
    /// <exception cref="VideoToolProcessingException">The codec-private data is not three Xiph-laced headers.</exception>
    public static OggAudioWriter CreateVorbis(string path, ReadOnlySpan<byte> codecPrivate, int sampleRate)
    {
        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, "A Vorbis sample rate must be positive.");
        }

        XiphLacing.SplitVorbisHeaders(codecPrivate, out var identification, out var comment, out var setup);

        var pages = new OggStreamWriter(
            new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None), SerialNumber);

        //The identification header must sit alone on the first page; the other two follow on their own.
        pages.WritePacket(identification, 0);
        pages.FlushPage();
        pages.WritePacket(comment, 0);
        pages.WritePacket(setup, 0);
        pages.FlushPage();

        return new OggAudioWriter(pages, sampleRate, 0);
    }

    /// <summary>Creates an Ogg Opus file and writes its identification and comment headers.</summary>
    /// <param name="path">The file to create, overwriting anything already there.</param>
    /// <param name="codecPrivate">The track's OpusHead identification header.</param>
    /// <param name="preSkipSamples">
    /// The encoder's pre-skip in 48 kHz samples, which every Opus granule is offset by. Pass zero to
    /// take it from the OpusHead itself.
    /// </param>
    /// <returns>A writer ready for audio packets.</returns>
    /// <exception cref="VideoToolProcessingException">The codec-private data is not an OpusHead.</exception>
    public static OggAudioWriter CreateOpus(string path, ReadOnlySpan<byte> codecPrivate, int preSkipSamples)
    {
        if (codecPrivate.Length < 19 || !codecPrivate[..8].SequenceEqual("OpusHead"u8))
        {
            throw new VideoToolProcessingException(
                "The Opus track's codec-private data is not an OpusHead identification header.");
        }

        var preSkip = preSkipSamples > 0
            ? preSkipSamples
            : BinaryPrimitives.ReadUInt16LittleEndian(codecPrivate.Slice(10, 2));

        var pages = new OggStreamWriter(
            new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None), SerialNumber);

        pages.WritePacket(codecPrivate, 0);
        pages.FlushPage();
        pages.WritePacket(BuildOpusTags(), 0);
        pages.FlushPage();

        return new OggAudioWriter(pages, OpusGranuleSampleRate, preSkip);
    }

    /// <summary>Writes one coded audio packet.</summary>
    /// <param name="data">The packet's bytes.</param>
    /// <param name="endTimestamp">
    /// Where the packet ends in the media's own timeline. The granule position is computed from
    /// this, and never allowed to move backwards.
    /// </param>
    /// <exception cref="InvalidOperationException">The file has already been completed.</exception>
    public void WritePacket(ReadOnlySpan<byte> data, TimeSpan endTimestamp)
    {
        ThrowIfDisposed();
        if (completed)
        {
            throw new InvalidOperationException("This Ogg audio file has already been completed.");
        }

        var samples = (long)Math.Round(Math.Max(0d, endTimestamp.TotalSeconds) * granuleSampleRate,
            MidpointRounding.AwayFromZero);
        var granule = granuleOffset + samples;
        if (granule < lastGranule)
        {
            granule = lastGranule;
        }

        lastGranule = granule;
        stream.WritePacket(data, granule);
    }

    /// <summary>Writes the last page, with the end-of-stream flag set.</summary>
    /// <exception cref="InvalidOperationException">The file has already been completed.</exception>
    public void Complete()
    {
        ThrowIfDisposed();
        if (completed)
        {
            throw new InvalidOperationException("This Ogg audio file has already been completed.");
        }

        stream.Complete();
        completed = true;
    }

    /// <summary>Releases the writer and the file underneath it.</summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        stream.Dispose();
    }

    private static byte[] BuildOpusTags()
    {
        var vendor = Encoding.UTF8.GetBytes(Vendor);
        var tags = new byte[8 + 4 + vendor.Length + 4];
        "OpusTags"u8.CopyTo(tags);
        BinaryPrimitives.WriteUInt32LittleEndian(tags.AsSpan(8, 4), (uint)vendor.Length);
        vendor.CopyTo(tags, 12);
        BinaryPrimitives.WriteUInt32LittleEndian(tags.AsSpan(12 + vendor.Length, 4), 0);
        return tags;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}
