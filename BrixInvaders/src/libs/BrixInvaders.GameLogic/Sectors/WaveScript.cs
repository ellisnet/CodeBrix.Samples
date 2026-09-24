using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>
/// The wave compositions. Each wave is a row code, top row first: G grunt, S shooter, H shielded, D diver,
/// M missile carrier. Four-letter codes are four-row formations. Loops repeat the five designs.
/// </summary>
public static class WaveScript
{
    /// <summary>Horizontal distance between formation columns.</summary>
    public const int ColumnSpacing = 72;

    /// <summary>Vertical distance between formation rows.</summary>
    public const int RowSpacing = 56;

    /// <summary>Centre y of the top row at wave 1; each later wave starts <see cref="StartYPerWave"/> lower.</summary>
    public const int FirstStartY = 120;

    /// <summary>How much lower each later wave of a sector starts.</summary>
    public const int StartYPerWave = 12;

    private static readonly string[][] Codes =
    {
        new[] { "SGGG", "SSGG", "SGGGG", "SSGGG", "SGSGG", "SSSGG" },
        new[] { "SDGG", "SDDGG", "DSGGG", "SDSGG", "DSDGG", "SDDSG" },
        new[] { "HSGG", "SHHGG", "HSDGG", "SHDHG", "HHSDG", "SHHDG" },
        new[] { "MSGG", "SMHGG", "MSDGG", "MHSDG", "SMDHG", "MMSHG" },
        new[] { "MSHD", "MSHDG", "SMDHG", "MHSDD", "MSHHD", "MMSHD" },
    };

    private static readonly int[] ColumnsPerWave = { 8, 9, 10, 11, 11, 11 };

    /// <summary>The row code of a design and wave, e.g. "SDDGG".</summary>
    /// <param name="design">Design 1..5.</param>
    /// <param name="wave">Wave 1..6.</param>
    /// <returns>The row code.</returns>
    public static string CodeOf(int design, int wave)
    {
        CheckWave(wave);
        if (design < 1 || design > SectorRules.SectorsPerLoop)
        {
            throw new ArgumentOutOfRangeException(nameof(design), design, "Design must be 1..5.");
        }

        return Codes[design - 1][wave - 1];
    }

    /// <summary>The layout of a wave of a sector.</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <param name="wave">Wave 1..6.</param>
    /// <returns>The layout.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the wave is not 1..6 or the sector is below 1.</exception>
    public static WaveLayout For(int sector, int wave)
    {
        CheckWave(wave);
        var design = SectorRules.DesignOf(sector);
        var code = CodeOf(design, wave);
        var roles = new List<EnemyRole>(code.Length);
        foreach (var letter in code)
        {
            roles.Add(RoleOf(letter));
        }

        var split = design == 5 && wave >= 2;
        return new WaveLayout(sector, wave, ColumnsPerWave[wave - 1], roles,
            FirstStartY + (StartYPerWave * (wave - 1)), split);
    }

    /// <summary>The colour (point tier) of a formation row: row 0 red, row 1 green, row 2 blue, rows 3+ black.</summary>
    /// <param name="row">Row index, 0 = top.</param>
    /// <returns>The colour.</returns>
    public static EnemyColour ColourOfRow(int row) => row switch
    {
        0 => EnemyColour.Red,
        1 => EnemyColour.Green,
        2 => EnemyColour.Blue,
        _ => EnemyColour.Black,
    };

    /// <summary>Maps a row-code letter to its role.</summary>
    /// <param name="letter">G, S, H, D or M.</param>
    /// <returns>The role.</returns>
    /// <exception cref="ArgumentException">For any other letter.</exception>
    public static EnemyRole RoleOf(char letter) => letter switch
    {
        'G' => EnemyRole.Grunt,
        'S' => EnemyRole.Shooter,
        'H' => EnemyRole.Shielded,
        'D' => EnemyRole.Diver,
        'M' => EnemyRole.MissileCarrier,
        _ => throw new ArgumentException("Unknown row code letter '" + letter + "'.", nameof(letter)),
    };

    private static void CheckWave(int wave)
    {
        if (wave < 1 || wave > SectorRules.WavesPerSector)
        {
            throw new ArgumentOutOfRangeException(nameof(wave), wave, "Wave must be 1..6.");
        }
    }
}
