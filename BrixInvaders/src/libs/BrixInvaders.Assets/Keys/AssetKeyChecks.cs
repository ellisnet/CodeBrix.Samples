using System;

namespace BrixInvaders.Assets;

/// <summary>Argument checks shared by the <see cref="AssetKeys"/> helper methods.</summary>
internal static class AssetKeyChecks
{
    /// <summary>Checks that a zero-based index lies in <c>0 .. count - 1</c>.</summary>
    /// <param name="index">The index to check.</param>
    /// <param name="count">How many values there are.</param>
    /// <param name="parameterName">The caller's parameter name, for the exception.</param>
    /// <returns><paramref name="index"/>, unchanged.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    internal static int Index(int index, int count, string parameterName)
    {
        if (index < 0 || index >= count)
        {
            throw new ArgumentOutOfRangeException(parameterName, index, $"Expected a value in 0..{count - 1} (zero-based).");
        }

        return index;
    }
}
