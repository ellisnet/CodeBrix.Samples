using System;

namespace CodeBrixVideoTool.Processing.Containers;

/// <summary>
/// The checksum an Ogg page header carries.
/// </summary>
/// <remarks>
/// This is the DIRECT CRC-32: polynomial 0x04C11DB7 applied most-significant-bit first, with an
/// initial value of zero and no final exclusive-or. It is deliberately not the reflected CRC-32 that
/// zlib, Matroska and the bespoke CBVF container use, and the two produce different answers for the
/// same bytes. The playback core's own Ogg checksum is internal to that assembly, so this
/// application brings its own.
/// </remarks>
public static class OggCrc32
{
    private const uint Polynomial = 0x04C11DB7u;

    private static readonly uint[] Table = BuildTable();

    /// <summary>Computes the checksum of a whole page, whose own checksum field must be zeroed.</summary>
    /// <param name="data">The complete page: header, segment table and body.</param>
    /// <returns>The value to store in the page header's checksum field.</returns>
    public static uint Compute(ReadOnlySpan<byte> data)
    {
        uint crc = 0;
        foreach (var value in data)
        {
            crc = (crc << 8) ^ Table[((crc >> 24) & 0xFF) ^ value];
        }

        return crc;
    }

    private static uint[] BuildTable()
    {
        var table = new uint[256];
        for (uint index = 0; index < 256; index++)
        {
            var remainder = index << 24;
            for (var bit = 0; bit < 8; bit++)
            {
                remainder = (remainder & 0x80000000u) != 0
                    ? (remainder << 1) ^ Polynomial
                    : remainder << 1;
            }

            table[index] = remainder;
        }

        return table;
    }
}
