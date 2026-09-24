using System;
using System.Collections.Generic;
using System.Globalization;

namespace BrixInvaders.GameLogic;

/// <summary>
/// Top-10 high scores per difficulty. Ordered by score, highest first; a new score equal to an existing one is
/// placed below it (the earlier score keeps its rank). Serialises to plain text lines
/// ("Difficulty|Name|Score|Sector") so the Game library can store it as one settings value.
/// </summary>
public sealed class HighScoreTable
{
    /// <summary>Entries kept per difficulty.</summary>
    public const int MaxEntries = 10;

    private readonly Dictionary<Difficulty, List<HighScoreEntry>> _tables = new Dictionary<Difficulty, List<HighScoreEntry>>();

    /// <summary>Creates an empty table.</summary>
    public HighScoreTable()
    {
        foreach (var level in DifficultyTable.Levels)
        {
            _tables[level] = new List<HighScoreEntry>();
        }
    }

    /// <summary>The entries of a difficulty, highest first.</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <returns>Up to 10 entries.</returns>
    public IReadOnlyList<HighScoreEntry> EntriesFor(Difficulty difficulty) => TableOf(difficulty);

    /// <summary>The best score of a difficulty, or 0.</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <returns>The best score.</returns>
    public long BestScore(Difficulty difficulty)
    {
        var table = TableOf(difficulty);
        return table.Count == 0 ? 0 : table[0].Score;
    }

    /// <summary>The 0-based rank a score would take, or -1 when it does not make the table (scores of 0 never do).</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <param name="score">The score.</param>
    /// <returns>The rank or -1.</returns>
    public int RankFor(Difficulty difficulty, long score)
    {
        if (score <= 0)
        {
            return -1;
        }

        var table = TableOf(difficulty);
        var rank = 0;
        while (rank < table.Count && table[rank].Score >= score)
        {
            rank++;
        }

        return rank < MaxEntries ? rank : -1;
    }

    /// <summary>True when the score makes the table.</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <param name="score">The score.</param>
    /// <returns>True when it qualifies.</returns>
    public bool Qualifies(Difficulty difficulty, long score) => RankFor(difficulty, score) >= 0;

    /// <summary>Inserts a score, dropping the 11th entry.</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <param name="name">Three-letter name.</param>
    /// <param name="score">Score.</param>
    /// <param name="sector">Sector reached.</param>
    /// <returns>The 0-based rank taken, or -1 when the score did not qualify.</returns>
    public int Insert(Difficulty difficulty, string name, long score, int sector)
    {
        var rank = RankFor(difficulty, score);
        if (rank < 0)
        {
            return -1;
        }

        var table = TableOf(difficulty);
        table.Insert(rank, new HighScoreEntry(name, score, sector));
        if (table.Count > MaxEntries)
        {
            table.RemoveRange(MaxEntries, table.Count - MaxEntries);
        }

        return rank;
    }

    /// <summary>Removes every entry of one difficulty.</summary>
    /// <param name="difficulty">The difficulty.</param>
    public void Clear(Difficulty difficulty) => TableOf(difficulty).Clear();

    /// <summary>Removes every entry of every difficulty.</summary>
    public void ClearAll()
    {
        foreach (var table in _tables.Values)
        {
            table.Clear();
        }
    }

    /// <summary>Serialises the table: one "Difficulty|Name|Score|Sector" line per entry, easiest difficulty first.</summary>
    /// <returns>The lines.</returns>
    public IReadOnlyList<string> ToLines()
    {
        var lines = new List<string>();
        foreach (var level in DifficultyTable.Levels)
        {
            foreach (var entry in TableOf(level))
            {
                lines.Add(string.Join("|", level.ToString(), entry.Name,
                    entry.Score.ToString(CultureInfo.InvariantCulture), entry.Sector.ToString(CultureInfo.InvariantCulture)));
            }
        }

        return lines;
    }

    /// <summary>
    /// Rebuilds a table from <see cref="ToLines"/> output. Malformed lines are skipped; entries are re-sorted and
    /// each difficulty is cut to 10.
    /// </summary>
    /// <param name="lines">The lines (null is treated as empty).</param>
    /// <returns>The table.</returns>
    public static HighScoreTable FromLines(IEnumerable<string> lines)
    {
        var table = new HighScoreTable();
        if (lines == null)
        {
            return table;
        }

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split('|');
            if (parts.Length != 4
                || !Enum.TryParse(parts[0], false, out Difficulty level)
                || !Enum.IsDefined(typeof(Difficulty), level)
                || !long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var score)
                || !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var sector))
            {
                continue;
            }

            table.Insert(level, parts[1], score, Math.Max(1, sector));
        }

        return table;
    }

    private List<HighScoreEntry> TableOf(Difficulty difficulty)
    {
        if (!_tables.TryGetValue(difficulty, out var table))
        {
            throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unknown difficulty.");
        }

        return table;
    }
}
