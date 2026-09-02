using System;
using System.Collections.Generic;

namespace CodeBrixVideoTool.Processing.Resolution;

/// <summary>
/// Builds the preset resolution ladder a conversion offers: the source's own size, then the
/// standard heights that are strictly smaller than it, each scaled proportionally to even
/// dimensions.
/// </summary>
/// <remarks>
/// Dimensions are kept even because every one of the four supported formats carries AV1 in 4:2:0,
/// whose chroma planes are half-size in each direction; an odd dimension has no 4:2:0 representation
/// and the encoder refuses it.
/// </remarks>
public static class ResolutionLadder
{
    /// <summary>The standard heights the ladder offers, tallest first.</summary>
    public static IReadOnlyList<int> StandardHeights { get; } = [1440, 1080, 720, 480];

    /// <summary>
    /// Builds the ladder for one source size. The first rung is always "Original"; the rest are the
    /// standard heights strictly below the source's height, in descending order.
    /// </summary>
    /// <param name="sourceWidth">The source's coded width, in pixels.</param>
    /// <param name="sourceHeight">The source's coded height, in pixels.</param>
    /// <returns>The rungs to offer, in the order they should be listed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either dimension is not positive.</exception>
    public static IReadOnlyList<ResolutionOption> Build(int sourceWidth, int sourceHeight)
    {
        if (sourceWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceWidth), sourceWidth, "A source width must be positive.");
        }

        if (sourceHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceHeight), sourceHeight, "A source height must be positive.");
        }

        var rungs = new List<ResolutionOption>
        {
            ResolutionOption.Original(MakeEven(sourceWidth), MakeEven(sourceHeight)),
        };

        foreach (var height in StandardHeights)
        {
            //Strictly below: a source that is already 1080 tall is not offered "1080p".
            if (height >= sourceHeight)
            {
                continue;
            }

            rungs.Add(ResolutionOption.Reduced(
                height + "p",
                ProportionalWidth(sourceWidth, sourceHeight, height),
                MakeEven(height)));
        }

        return rungs;
    }

    /// <summary>
    /// The width that keeps the source's aspect ratio at a given height, rounded to the nearest even
    /// number of pixels.
    /// </summary>
    /// <param name="sourceWidth">The source's coded width, in pixels.</param>
    /// <param name="sourceHeight">The source's coded height, in pixels.</param>
    /// <param name="targetHeight">The height the rung asks for, in pixels.</param>
    /// <returns>An even width, never smaller than 2.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Any argument is not positive.</exception>
    public static int ProportionalWidth(int sourceWidth, int sourceHeight, int targetHeight)
    {
        if (sourceWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceWidth), sourceWidth, "A source width must be positive.");
        }

        if (sourceHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceHeight), sourceHeight, "A source height must be positive.");
        }

        if (targetHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetHeight), targetHeight, "A target height must be positive.");
        }

        var exact = sourceWidth * (double)targetHeight / sourceHeight;
        return MakeEven((int)Math.Round(exact, MidpointRounding.AwayFromZero));
    }

    /// <summary>Rounds a dimension to the nearest even number of pixels, never below 2.</summary>
    /// <param name="value">The dimension to round.</param>
    /// <returns>An even value of at least 2.</returns>
    public static int MakeEven(int value)
    {
        if (value <= 2)
        {
            return 2;
        }

        return (value % 2 == 0) ? value : value + 1;
    }
}
