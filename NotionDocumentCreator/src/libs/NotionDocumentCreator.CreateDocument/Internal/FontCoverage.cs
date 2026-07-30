using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// Answers "can this embedded font render this codepoint?" by parsing each
/// embedded TTF's cmap table (formats 4 and 12) once and caching the covered
/// set. Exact coverage — rather than guessed Unicode ranges — is what keeps
/// tofu boxes out of the printed book.
/// </summary>
internal static class FontCoverage
{
    /// <summary>Embedded file name of the book serif regular face.</summary>
    public const string SerifRegular = "EBGaramond-Regular.ttf";

    /// <summary>Embedded file name of the book sans regular face.</summary>
    public const string SansRegular = "SourceSans3-Regular.ttf";

    /// <summary>Embedded file name of the monospace face.</summary>
    public const string MonoRegular = "SourceCodePro-Regular.ttf";

    /// <summary>Embedded file name of the monochrome emoji face.</summary>
    public const string EmojiRegular = "NotoEmoji-Regular.ttf";

    private const string ResourcePrefix = "NotionDocumentCreator.CreateDocument.Fonts.";

    private static readonly ConcurrentDictionary<string, HashSet<int>> _coverageByFile = new();

    /// <summary>Whether the given embedded font file has a glyph for the codepoint.</summary>
    public static bool Covers(string fontFileName, int codepoint)
    {
        var coverage = _coverageByFile.GetOrAdd(fontFileName, LoadCoverage);
        return coverage.Contains(codepoint);
    }

    /// <summary>
    /// Whether an emoji codepoint can actually be PRINTED: it must be in the
    /// emoji face's cmap AND inside the Basic Multilingual Plane — the PDF text
    /// engine addresses glyphs per UTF-16 code unit, so astral-plane emoji
    /// (U+1F300 and friends) would print as tofu even though the font has them.
    /// </summary>
    public static bool EmojiPrintable(int codepoint) =>
        codepoint <= 0xFFFF && Covers(EmojiRegular, codepoint);

    private static HashSet<int> LoadCoverage(string fontFileName)
    {
        try
        {
            var assembly = typeof(FontCoverage).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourcePrefix + fontFileName);
            if (stream is null) { return []; }

            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return ParseCmap(memory.ToArray());
        }
        catch (Exception)
        {
            //An unparseable font just reports empty coverage (callers fall back safely)
            return [];
        }
    }

    private static HashSet<int> ParseCmap(byte[] font)
    {
        //Locate the 'cmap' table in the sfnt table directory
        var numTables = ReadUInt16(font, 4);
        var cmapOffset = -1;
        for (var i = 0; i < numTables; i++)
        {
            var record = 12 + i * 16;
            if (font[record] == 'c' && font[record + 1] == 'm'
                && font[record + 2] == 'a' && font[record + 3] == 'p')
            {
                cmapOffset = (int)ReadUInt32(font, record + 8);
                break;
            }
        }
        if (cmapOffset < 0) { return []; }

        //Pick the best Unicode encoding subtable: prefer a UCS-4 format 12, else BMP format 4
        var encodingCount = ReadUInt16(font, cmapOffset + 2);
        var format12Offset = -1;
        var format4Offset = -1;
        for (var i = 0; i < encodingCount; i++)
        {
            var record = cmapOffset + 4 + i * 8;
            var platformId = ReadUInt16(font, record);
            var encodingId = ReadUInt16(font, record + 2);
            var subtableOffset = cmapOffset + (int)ReadUInt32(font, record + 4);
            var isUnicode = platformId == 0 || (platformId == 3 && (encodingId == 1 || encodingId == 10));
            if (!isUnicode) { continue; }

            var format = ReadUInt16(font, subtableOffset);
            if (format == 12 && format12Offset < 0) { format12Offset = subtableOffset; }
            if (format == 4 && format4Offset < 0) { format4Offset = subtableOffset; }
        }

        if (format12Offset >= 0) { return ParseFormat12(font, format12Offset); }
        if (format4Offset >= 0) { return ParseFormat4(font, format4Offset); }
        return [];
    }

    private static HashSet<int> ParseFormat12(byte[] font, int offset)
    {
        var coverage = new HashSet<int>();
        var groupCount = (int)ReadUInt32(font, offset + 12);
        for (var g = 0; g < groupCount; g++)
        {
            var group = offset + 16 + g * 12;
            var start = (int)ReadUInt32(font, group);
            var end = (int)ReadUInt32(font, group + 4);
            for (var code = start; code <= end; code++)
            {
                coverage.Add(code);
            }
        }
        return coverage;
    }

    private static HashSet<int> ParseFormat4(byte[] font, int offset)
    {
        var coverage = new HashSet<int>();
        var segCount = ReadUInt16(font, offset + 6) / 2;
        var endCodes = offset + 14;
        var startCodes = endCodes + segCount * 2 + 2; //+2 skips reservedPad
        var idDeltas = startCodes + segCount * 2;
        var idRangeOffsets = idDeltas + segCount * 2;

        for (var seg = 0; seg < segCount; seg++)
        {
            int endCode = ReadUInt16(font, endCodes + seg * 2);
            int startCode = ReadUInt16(font, startCodes + seg * 2);
            int idDelta = ReadUInt16(font, idDeltas + seg * 2);
            int idRangeOffset = ReadUInt16(font, idRangeOffsets + seg * 2);
            if (startCode == 0xFFFF) { continue; }

            for (var code = startCode; code <= endCode && code != 0xFFFF; code++)
            {
                int glyph;
                if (idRangeOffset == 0)
                {
                    glyph = (code + idDelta) & 0xFFFF;
                }
                else
                {
                    //The glyph index lives in glyphIdArray, addressed relative to
                    //  this segment's idRangeOffset slot (the classic cmap-4 trick)
                    var glyphAddress = idRangeOffsets + seg * 2 + idRangeOffset + (code - startCode) * 2;
                    if (glyphAddress + 1 >= font.Length) { continue; }
                    glyph = ReadUInt16(font, glyphAddress);
                    if (glyph != 0) { glyph = (glyph + idDelta) & 0xFFFF; }
                }

                if (glyph != 0) { coverage.Add(code); }
            }
        }
        return coverage;
    }

    private static ushort ReadUInt16(byte[] data, int offset) =>
        (ushort)((data[offset] << 8) | data[offset + 1]);

    private static uint ReadUInt32(byte[] data, int offset) =>
        ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16)
        | ((uint)data[offset + 2] << 8) | data[offset + 3];
}
