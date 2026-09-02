using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace CodeBrixVideoTool.Processing.Containers;

/// <summary>
/// Writes an IVF file - the thin, one-codec wrapper an encoder puts around an elementary video
/// stream, and the shape both FFmpeg and the playback core's own muxer accept as a video input.
/// </summary>
/// <remarks>
/// The file is a 32-byte header followed by frames, each a 4-byte payload length, an 8-byte
/// timestamp in the header's time base, and the payload. This writer states a time base of one
/// hundred nanoseconds, so a frame's timestamp is exactly its .NET tick count and no arithmetic is
/// needed on the way in or out. The frame count in the header is back-patched by
/// <see cref="Complete" />, so the stream must be seekable.
/// </remarks>
public sealed class IvfWriter : IDisposable
{
    private const int HeaderLength = 32;
    private const int FrameCountOffset = 24;

    /// <summary>The four-character code for an AV1 elementary stream.</summary>
    public const string Av1FourCharacterCode = "AV01";

    /// <summary>
    /// The time-base denominator this writer states: one timestamp unit is one ten-millionth of a
    /// second, which is one .NET tick.
    /// </summary>
    public const uint TickTimeBaseDenominator = 10_000_000;

    private readonly Stream output;
    private readonly bool leaveOutputOpen;
    private uint frameCount;
    private bool completed;
    private bool disposed;

    /// <summary>Creates a writer over a seekable stream and writes its header.</summary>
    /// <param name="output">Where the file goes. Must be writable and seekable.</param>
    /// <param name="fourCharacterCode">The codec's four-character code, such as "AV01".</param>
    /// <param name="width">The coded width, in pixels.</param>
    /// <param name="height">The coded height, in pixels.</param>
    /// <param name="leaveOutputOpen">True to leave the stream open when this writer is disposed.</param>
    /// <exception cref="ArgumentNullException">The stream is null.</exception>
    /// <exception cref="ArgumentException">The stream cannot seek, or the code is not four characters.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A dimension is outside the 1..65535 an IVF header can state.</exception>
    public IvfWriter(Stream output, string fourCharacterCode, int width, int height, bool leaveOutputOpen = false)
    {
        this.output = output ?? throw new ArgumentNullException(nameof(output));
        this.leaveOutputOpen = leaveOutputOpen;

        if (!output.CanSeek)
        {
            throw new ArgumentException("An IVF file's frame count is written last, so its stream must be seekable.", nameof(output));
        }

        if (fourCharacterCode is not { Length: 4 })
        {
            throw new ArgumentException("An IVF codec code is exactly four ASCII characters.", nameof(fourCharacterCode));
        }

        if (width is <= 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "An IVF header states its width in sixteen bits.");
        }

        if (height is <= 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "An IVF header states its height in sixteen bits.");
        }

        Span<byte> header = stackalloc byte[HeaderLength];
        header.Clear();
        header[0] = (byte)'D';
        header[1] = (byte)'K';
        header[2] = (byte)'I';
        header[3] = (byte)'F';
        BinaryPrimitives.WriteUInt16LittleEndian(header.Slice(4, 2), 0);
        BinaryPrimitives.WriteUInt16LittleEndian(header.Slice(6, 2), HeaderLength);
        Encoding.ASCII.GetBytes(fourCharacterCode, header.Slice(8, 4));
        BinaryPrimitives.WriteUInt16LittleEndian(header.Slice(12, 2), (ushort)width);
        BinaryPrimitives.WriteUInt16LittleEndian(header.Slice(14, 2), (ushort)height);
        BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(16, 4), TickTimeBaseDenominator);
        BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(20, 4), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(24, 4), 0);
        output.Write(header);
    }

    /// <summary>Creates a writer for an AV1 elementary stream, over a new file.</summary>
    /// <param name="path">The file to create, overwriting anything already there.</param>
    /// <param name="width">The coded width, in pixels.</param>
    /// <param name="height">The coded height, in pixels.</param>
    /// <returns>A writer that owns and will close the file.</returns>
    public static IvfWriter CreateAv1(string path, int width, int height) =>
        new(new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None),
            Av1FourCharacterCode, width, height);

    /// <summary>How many frames have been written so far.</summary>
    public uint FrameCount => frameCount;

    /// <summary>Writes one coded frame - for AV1, one temporal unit.</summary>
    /// <param name="data">The frame's bytes.</param>
    /// <param name="timestamp">When the frame is for.</param>
    /// <exception cref="InvalidOperationException">The file has already been completed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The timestamp is negative.</exception>
    public void WriteFrame(ReadOnlySpan<byte> data, TimeSpan timestamp)
    {
        ThrowIfDisposed();
        if (completed)
        {
            throw new InvalidOperationException("This IVF file has already been completed.");
        }

        if (timestamp < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp), timestamp, "An IVF timestamp cannot be negative.");
        }

        Span<byte> frameHeader = stackalloc byte[12];
        BinaryPrimitives.WriteUInt32LittleEndian(frameHeader[..4], (uint)data.Length);
        BinaryPrimitives.WriteInt64LittleEndian(frameHeader.Slice(4, 8), timestamp.Ticks);
        output.Write(frameHeader);
        output.Write(data);
        frameCount++;
    }

    /// <summary>Back-patches the frame count into the header and flushes the file.</summary>
    /// <exception cref="InvalidOperationException">The file has already been completed.</exception>
    public void Complete()
    {
        ThrowIfDisposed();
        if (completed)
        {
            throw new InvalidOperationException("This IVF file has already been completed.");
        }

        var end = output.Position;
        output.Position = FrameCountOffset;
        Span<byte> count = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(count, frameCount);
        output.Write(count);
        output.Position = end;
        output.Flush();
        completed = true;
    }

    /// <summary>Releases the writer and, unless asked not to, the stream underneath it.</summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        if (!leaveOutputOpen)
        {
            output.Dispose();
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}
