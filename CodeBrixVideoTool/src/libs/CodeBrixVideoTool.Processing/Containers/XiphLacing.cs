using System;

namespace CodeBrixVideoTool.Processing.Containers;

/// <summary>
/// Splits the Xiph-laced block a container stores as a Vorbis track's codec-private data back into
/// the three setup headers Vorbis actually wants.
/// </summary>
/// <remarks>
/// The playback core publishes the builder - <c>OggAudioStream.BuildXiphCodecPrivate</c> - but not the
/// splitter, so this application brings its own. The block is a count byte holding the number of
/// packets minus one, then the length of each packet except the last written as a run of 0xFF bytes
/// followed by a remainder below 0xFF, then the packets themselves back to back.
/// </remarks>
public static class XiphLacing
{
    /// <summary>
    /// Splits a Vorbis codec-private block into its identification, comment and setup headers.
    /// </summary>
    /// <param name="codecPrivate">The block as the container stored it.</param>
    /// <param name="identification">The Vorbis identification header.</param>
    /// <param name="comment">The Vorbis comment header.</param>
    /// <param name="setup">The Vorbis setup header.</param>
    /// <exception cref="VideoToolProcessingException">
    /// The block is truncated, or does not describe exactly three packets.
    /// </exception>
    public static void SplitVorbisHeaders(
        ReadOnlySpan<byte> codecPrivate,
        out byte[] identification,
        out byte[] comment,
        out byte[] setup)
    {
        if (codecPrivate.Length < 3)
        {
            throw new VideoToolProcessingException(
                "The Vorbis track's codec-private data is too short to hold its three setup headers.");
        }

        var packetCount = codecPrivate[0] + 1;
        if (packetCount != 3)
        {
            throw new VideoToolProcessingException(
                $"The Vorbis track's codec-private data declares {packetCount} packets; Vorbis has exactly three setup headers.");
        }

        var offset = 1;
        var identificationLength = ReadLacedLength(codecPrivate, ref offset);
        var commentLength = ReadLacedLength(codecPrivate, ref offset);

        var remaining = codecPrivate.Length - offset;
        if (identificationLength + commentLength > remaining)
        {
            throw new VideoToolProcessingException(
                "The Vorbis track's codec-private data declares header lengths longer than the data it carries.");
        }

        identification = codecPrivate.Slice(offset, identificationLength).ToArray();
        offset += identificationLength;

        comment = codecPrivate.Slice(offset, commentLength).ToArray();
        offset += commentLength;

        setup = codecPrivate[offset..].ToArray();

        if (setup.Length == 0)
        {
            throw new VideoToolProcessingException(
                "The Vorbis track's codec-private data carries no setup header, so nothing could decode it.");
        }
    }

    private static int ReadLacedLength(ReadOnlySpan<byte> data, ref int offset)
    {
        var length = 0;
        while (true)
        {
            if (offset >= data.Length)
            {
                throw new VideoToolProcessingException(
                    "The Vorbis track's codec-private data ended inside one of its laced header lengths.");
            }

            var value = data[offset++];
            length += value;
            if (value != 0xFF)
            {
                return length;
            }
        }
    }
}
