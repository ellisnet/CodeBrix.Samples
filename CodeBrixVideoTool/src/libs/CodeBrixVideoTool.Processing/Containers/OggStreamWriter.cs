using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace CodeBrixVideoTool.Processing.Containers;

/// <summary>
/// Writes one logical bitstream into an Ogg physical bitstream: packets in, pages out, with the
/// segment table, the page sequence numbers and the checksums looked after.
/// </summary>
/// <remarks>
/// Packets are gathered into a page until the page's 255-segment table is full; a packet longer than
/// the room left is split across pages with the continuation flag set, exactly as the format
/// requires. The last page is written by <see cref="Complete" />, which is what sets the
/// end-of-stream flag, so a writer that is never completed produces a stream no reader will accept.
/// </remarks>
public sealed class OggStreamWriter : IDisposable
{
    private const int MaximumSegments = 255;
    private const int PageHeaderLength = 27;

    private readonly Stream output;
    private readonly bool leaveOutputOpen;
    private readonly uint serialNumber;
    private readonly List<byte> lacing = new(MaximumSegments);
    private readonly List<byte> body = new(64 * 1024);

    private uint pageSequence;
    private long pageGranule = -1;
    private bool isFirstPage = true;
    private bool continuesPreviousPage;
    private bool completed;
    private bool disposed;

    /// <summary>Creates a writer over a stream.</summary>
    /// <param name="output">Where the pages go. Must be writable.</param>
    /// <param name="serialNumber">The logical bitstream's serial number.</param>
    /// <param name="leaveOutputOpen">True to leave the stream open when this writer is disposed.</param>
    /// <exception cref="ArgumentNullException">The stream is null.</exception>
    public OggStreamWriter(Stream output, uint serialNumber, bool leaveOutputOpen = false)
    {
        this.output = output ?? throw new ArgumentNullException(nameof(output));
        this.serialNumber = serialNumber;
        this.leaveOutputOpen = leaveOutputOpen;
    }

    /// <summary>How many pages have been written so far.</summary>
    public long PagesWritten => pageSequence;

    /// <summary>
    /// Adds one packet to the stream.
    /// </summary>
    /// <param name="data">The packet's bytes. A zero-length packet is legal and is written as one zero segment.</param>
    /// <param name="granulePosition">
    /// The codec's own position at the end of this packet. It reaches the file only on the page the
    /// packet finishes in, which is what the format asks for.
    /// </param>
    /// <exception cref="InvalidOperationException">The stream has already been completed.</exception>
    public void WritePacket(ReadOnlySpan<byte> data, long granulePosition)
    {
        ThrowIfDisposed();
        if (completed)
        {
            throw new InvalidOperationException("This Ogg stream has already been completed.");
        }

        var offset = 0;
        var remaining = data.Length;

        while (true)
        {
            if (lacing.Count == MaximumSegments)
            {
                //The previous packet finished exactly at the page boundary, so this page is closed
                //with nothing left over and the next one does not continue anything.
                FlushPage(false, false);
            }

            var room = MaximumSegments - lacing.Count;
            var segmentsNeeded = (remaining / 255) + 1;
            var take = Math.Min(room, segmentsNeeded);

            for (var index = 0; index < take; index++)
            {
                var chunk = Math.Min(255, remaining);
                lacing.Add((byte)chunk);
                for (var byteIndex = 0; byteIndex < chunk; byteIndex++)
                {
                    body.Add(data[offset + byteIndex]);
                }

                offset += chunk;
                remaining -= chunk;
            }

            if (take >= segmentsNeeded)
            {
                pageGranule = granulePosition;
                return;
            }

            //The page filled up part-way through the packet; the rest goes on the next one.
            FlushPage(false, true);
        }
    }

    /// <summary>
    /// Closes the page being built, so that whatever comes next starts a new one. Vorbis and Opus
    /// both require their identification header to sit alone on the first page, which is what this
    /// is for.
    /// </summary>
    /// <exception cref="InvalidOperationException">The stream has already been completed.</exception>
    public void FlushPage()
    {
        ThrowIfDisposed();
        if (completed)
        {
            throw new InvalidOperationException("This Ogg stream has already been completed.");
        }

        if (lacing.Count > 0)
        {
            FlushPage(false, false);
        }
    }

    /// <summary>Writes the last page, with the end-of-stream flag set.</summary>
    /// <exception cref="InvalidOperationException">The stream has already been completed.</exception>
    public void Complete()
    {
        ThrowIfDisposed();
        if (completed)
        {
            throw new InvalidOperationException("This Ogg stream has already been completed.");
        }

        FlushPage(true, false);
        completed = true;
        output.Flush();
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

    private void FlushPage(bool isLastPage, bool packetContinues)
    {
        if (lacing.Count == 0 && !isLastPage)
        {
            return;
        }

        var page = new byte[PageHeaderLength + lacing.Count + body.Count];
        var header = page.AsSpan(0, PageHeaderLength);

        header[0] = (byte)'O';
        header[1] = (byte)'g';
        header[2] = (byte)'g';
        header[3] = (byte)'S';
        header[4] = 0;

        byte headerType = 0;
        if (continuesPreviousPage) { headerType |= 0x01; }
        if (isFirstPage) { headerType |= 0x02; }
        if (isLastPage) { headerType |= 0x04; }
        header[5] = headerType;

        BinaryPrimitives.WriteInt64LittleEndian(header.Slice(6, 8), pageGranule);
        BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(14, 4), serialNumber);
        BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(18, 4), pageSequence);
        BinaryPrimitives.WriteUInt32LittleEndian(header.Slice(22, 4), 0);
        header[26] = (byte)lacing.Count;

        lacing.CopyTo(page, PageHeaderLength);
        body.CopyTo(page, PageHeaderLength + lacing.Count);

        var checksum = OggCrc32.Compute(page);
        BinaryPrimitives.WriteUInt32LittleEndian(page.AsSpan(22, 4), checksum);

        output.Write(page, 0, page.Length);

        pageSequence++;
        isFirstPage = false;
        continuesPreviousPage = packetContinues;
        pageGranule = -1;
        lacing.Clear();
        body.Clear();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}
