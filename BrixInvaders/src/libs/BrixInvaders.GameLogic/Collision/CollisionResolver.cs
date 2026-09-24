using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>Axis-aligned rectangle collision tests in world units.</summary>
public static class CollisionResolver
{
    /// <summary>True when two boxes overlap with a positive area (touching edges do not count).</summary>
    /// <param name="a">First box.</param>
    /// <param name="b">Second box.</param>
    /// <returns>True on overlap.</returns>
    public static bool Overlaps(Box a, Box b) => a.Intersects(b);

    /// <summary>True when a box is entirely inside the playfield.</summary>
    /// <param name="box">The box.</param>
    /// <returns>True when inside.</returns>
    public static bool IsInsidePlayfield(Box box) => box.IsInside(Playfield.Bounds);

    /// <summary>
    /// Returns the index of the first target (in list order) that overlaps the probe and is accepted by the filter,
    /// or -1. The simulation uses list order so that the result is deterministic.
    /// </summary>
    /// <param name="probe">The moving box (usually a bolt).</param>
    /// <param name="targets">Target boxes.</param>
    /// <returns>Index of the first overlapping target or -1.</returns>
    public static int FirstOverlap(Box probe, IReadOnlyList<Box> targets)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            if (probe.Intersects(targets[i]))
            {
                return i;
            }
        }

        return -1;
    }
}
