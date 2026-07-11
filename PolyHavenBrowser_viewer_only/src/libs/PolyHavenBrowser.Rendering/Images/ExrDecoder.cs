using TinyEXR;

namespace PolyHavenBrowser.Rendering;

/// <summary>
/// Decoder for OpenEXR images (<c>.exr</c>) via TinyEXR.NET (pure managed; handles the
/// ZIP, PIZ, and PXR24 compressions Poly Haven uses). Produces linear-light RGB floats.
/// </summary>
public static class ExrDecoder
{
    /// <summary>Decodes an OpenEXR file from a byte buffer.</summary>
    /// <exception cref="InvalidDataException">The data is not a decodable OpenEXR image.</exception>
    public static FloatImage Decode(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var result = Exr.LoadEXRFromMemory(data, out var rgba, out var width, out var height);
        if (result != ResultCode.Success)
        {
            throw new InvalidDataException($"Failed to decode OpenEXR data: {result}.");
        }

        return FloatImage.FromRgba(rgba, width, height);
    }

    /// <summary>Decodes an OpenEXR file from a stream.</summary>
    /// <exception cref="InvalidDataException">The data is not a decodable OpenEXR image.</exception>
    public static FloatImage Decode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var result = Exr.LoadEXRFromStream(stream, out var rgba, out var width, out var height);
        if (result != ResultCode.Success)
        {
            throw new InvalidDataException($"Failed to decode OpenEXR data: {result}.");
        }

        return FloatImage.FromRgba(rgba, width, height);
    }
}
