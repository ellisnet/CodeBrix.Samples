using NVorbis;

namespace KenneyAssetBrowser.Rendering;

/// <summary>
/// Decodes Ogg Vorbis audio to a 16-bit PCM WAV stream. Needed because the CodeBrix.Platform
/// AudioPlayer add-in plays WAV and MP3 only (its bundled miniaudio is built without a Vorbis
/// decoder), while every Kenney audio pack ships .ogg files.
/// </summary>
public static class OggWaveConverter
{
    /// <summary>
    /// Decodes an Ogg Vorbis file into an in-memory WAV stream, positioned at the start —
    /// ready for the AudioPlayer's <c>SetSourceStream</c>. The caller owns the stream (the
    /// player takes ownership when it is handed over).
    /// </summary>
    /// <param name="oggBytes">The raw bytes of the .ogg file.</param>
    /// <returns>A seekable stream holding a complete RIFF/WAVE file.</returns>
    /// <exception cref="InvalidDataException">The bytes are not decodable Ogg Vorbis audio.</exception>
    public static MemoryStream ToWavStream(byte[] oggBytes)
    {
        ArgumentNullException.ThrowIfNull(oggBytes);

        using var input = new MemoryStream(oggBytes, writable: false);
        VorbisReader reader;
        try
        {
            reader = new VorbisReader(input, closeOnDispose: false);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("The data is not decodable Ogg Vorbis audio.", ex);
        }

        using (reader)
        {
            var channels = reader.Channels;
            var sampleRate = reader.SampleRate;

            var output = new MemoryStream();
            using var writer = new BinaryWriter(output, System.Text.Encoding.ASCII, leaveOpen: true);

            //Reserve the 44-byte RIFF/WAVE header; the sizes are patched in afterwards
            writer.Write(new byte[44]);

            var floatBuffer = new float[4096 * channels];
            int samplesRead;
            while ((samplesRead = reader.ReadSamples(floatBuffer, 0, floatBuffer.Length)) > 0)
            {
                for (var i = 0; i < samplesRead; i++)
                {
                    var sample = Math.Clamp(floatBuffer[i], -1f, 1f);
                    writer.Write((short)(sample * short.MaxValue));
                }
            }

            var dataSize = (int)(output.Length - 44);
            output.Position = 0;
            writer.Write("RIFF"u8);
            writer.Write(36 + dataSize);
            writer.Write("WAVE"u8);
            writer.Write("fmt "u8);
            writer.Write(16);                                   //fmt chunk size
            writer.Write((short)1);                             //PCM
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * 2);            //byte rate
            writer.Write((short)(channels * 2));                //block align
            writer.Write((short)16);                            //bits per sample
            writer.Write("data"u8);
            writer.Write(dataSize);

            output.Position = 0;
            return output;
        }
    }
}
