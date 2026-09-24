using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>
/// The classic stepping formation: every step interval each group moves <see cref="StepX"/> sideways; when the
/// next step would carry any live enemy of a group past its bound, that step is a drop instead (exactly one
/// drop of the difficulty's drop height, and the direction reverses). The step interval shrinks as the formation
/// thins, so the formation speeds up monotonically as enemies die.
/// </summary>
public sealed class FormationController
{
    /// <summary>Sideways distance per step.</summary>
    public const int StepX = 12;

    /// <summary>Fastest possible step interval in seconds (one enemy left).</summary>
    public const double MinStepInterval = 0.06;

    /// <summary>Each later wave of a sector steps 3% faster than the previous one.</summary>
    public const double WaveSpeedUpPerWave = 0.03;

    /// <summary>Half the width of the gap between the bounds of the two groups of a split formation.</summary>
    public const int SplitGapHalfWidth = 12;

    /// <summary>
    /// A formation enemy whose bottom edge reaches this y has landed (the player's ship top is at 636).
    /// </summary>
    public const double LandingLine = 620.0;

    private readonly List<Enemy> _enemies = new List<Enemy>();
    private readonly List<FormationGroup> _groups = new List<FormationGroup>();
    private readonly int _dropHeight;
    private double _timer;

    /// <summary>Creates the formation of a wave and its enemies.</summary>
    /// <param name="layout">The wave layout.</param>
    /// <param name="difficulty">Difficulty settings (drop height, base interval).</param>
    /// <param name="nextId">Id source for the enemies.</param>
    public FormationController(WaveLayout layout, DifficultySettings difficulty, Func<int> nextId)
    {
        Layout = layout;
        _dropHeight = difficulty.DropHeight;
        BaseInterval = difficulty.FormationBaseInterval * SectorRules.IntervalScale(layout.Sector)
            * (1.0 - (WaveSpeedUpPerWave * (layout.Wave - 1)));
        StartY = layout.StartY;
        BuildGroups(layout);
        for (var row = 0; row < layout.Rows; row++)
        {
            for (var column = 0; column < layout.Columns; column++)
            {
                var enemy = new Enemy(nextId(), layout.RowRoles[row], WaveScript.ColourOfRow(row), column, row);
                _enemies.Add(enemy);
            }
        }

        TotalCount = _enemies.Count;
        PlaceInFormation();
    }

    /// <summary>The wave layout.</summary>
    public WaveLayout Layout { get; }

    /// <summary>All enemies of the wave, alive or not, in row-major order.</summary>
    public IReadOnlyList<Enemy> Enemies => _enemies;

    /// <summary>The stepping groups (one, or two for a split formation).</summary>
    public IReadOnlyList<FormationGroup> Groups => _groups;

    /// <summary>Enemies at the start of the wave.</summary>
    public int TotalCount { get; }

    /// <summary>Enemies still alive.</summary>
    public int AliveCount
    {
        get
        {
            var count = 0;
            foreach (var enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>Step interval with the whole formation alive.</summary>
    public double BaseInterval { get; }

    /// <summary>The current step interval (depends on how many enemies are alive).</summary>
    public double CurrentInterval => IntervalFor(AliveCount, TotalCount, BaseInterval);

    /// <summary>Top-row centre y at the start of the wave.</summary>
    public int StartY { get; }

    /// <summary>Sideways steps taken so far (drops not included).</summary>
    public int StepCount { get; private set; }

    /// <summary>Drops taken so far (all groups).</summary>
    public int DropCount { get; private set; }

    /// <summary>
    /// The step interval for a number of live enemies: linear from <paramref name="baseInterval"/> (all alive) down
    /// to <see cref="MinStepInterval"/> (one alive). Never increases as <paramref name="alive"/> decreases.
    /// </summary>
    /// <param name="alive">Live enemies.</param>
    /// <param name="total">Enemies at wave start.</param>
    /// <param name="baseInterval">Interval with all alive.</param>
    /// <returns>Seconds per step.</returns>
    public static double IntervalFor(int alive, int total, double baseInterval)
    {
        var minimum = Math.Min(MinStepInterval, baseInterval);
        if (total <= 1 || alive >= total)
        {
            return baseInterval;
        }

        if (alive <= 1)
        {
            return minimum;
        }

        return minimum + ((baseInterval - minimum) * (alive - 1) / (total - 1));
    }

    /// <summary>The home (slot) x of an enemy.</summary>
    /// <param name="enemy">The enemy.</param>
    /// <returns>Slot centre x.</returns>
    public int HomeX(Enemy enemy)
    {
        var group = GroupOf(enemy.Column);
        return group.OriginX + ((enemy.Column - group.FirstColumn) * WaveScript.ColumnSpacing);
    }

    /// <summary>The home (slot) y of an enemy.</summary>
    /// <param name="enemy">The enemy.</param>
    /// <returns>Slot centre y.</returns>
    public int HomeY(Enemy enemy) => GroupOf(enemy.Column).OriginY + (enemy.Row * WaveScript.RowSpacing);

    /// <summary>The bottom edge of the lowest live enemy sitting in formation, or 0 when none is.</summary>
    /// <returns>Lowest bottom edge.</returns>
    public double LowestFormationBottom()
    {
        var lowest = 0.0;
        foreach (var enemy in _enemies)
        {
            if (enemy.IsAlive && enemy.State == EnemyState.InFormation)
            {
                lowest = Math.Max(lowest, HomeY(enemy) + (Enemy.Height / 2.0));
            }
        }

        return lowest;
    }

    /// <summary>True when a live in-formation enemy has reached <see cref="LandingLine"/>.</summary>
    /// <returns>True when landed.</returns>
    public bool HasLanded() => LowestFormationBottom() >= LandingLine;

    /// <summary>Advances the step timer and takes as many steps as are due; places in-formation enemies.</summary>
    /// <param name="dt">Step in seconds.</param>
    /// <param name="events">Receives FormationStepped / FormationDropped.</param>
    internal void Update(double dt, GameEvents events)
    {
        _timer += dt;
        var interval = CurrentInterval;
        while (_timer >= interval && AliveCount > 0)
        {
            _timer -= interval;
            Step(events);
            interval = CurrentInterval;
        }

        PlaceInFormation();
    }

    /// <summary>Takes one step (sideways or a drop) for every group with live enemies.</summary>
    /// <param name="events">Receives FormationStepped / FormationDropped.</param>
    internal void Step(GameEvents events)
    {
        var steppedSideways = false;
        foreach (var group in _groups)
        {
            if (!TryGetExtents(group, out var left, out var right))
            {
                continue;
            }

            var dx = group.Direction * StepX;
            if (right + dx > group.MaxX || left + dx < group.MinX)
            {
                group.OriginY += _dropHeight;
                group.Direction = -group.Direction;
                DropCount++;
                events.Add(GameEventKind.FormationDropped);
            }
            else
            {
                group.OriginX += dx;
                steppedSideways = true;
            }
        }

        if (steppedSideways)
        {
            events.Add(new GameEvent(GameEventKind.FormationStepped, value: StepCount % 4));
            StepCount++;
        }

        PlaceInFormation();
    }

    /// <summary>Pushes every group back up to the wave's start height (after a landing).</summary>
    internal void ResetHeight()
    {
        foreach (var group in _groups)
        {
            group.OriginY = StartY;
        }

        PlaceInFormation();
    }

    /// <summary>Moves every live in-formation enemy onto its slot.</summary>
    internal void PlaceInFormation()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy.State == EnemyState.InFormation)
            {
                enemy.X = HomeX(enemy);
                enemy.Y = HomeY(enemy);
            }
        }
    }

    /// <summary>The live enemies' left and right edges in a group, computed from their home slots.</summary>
    internal bool TryGetExtents(FormationGroup group, out double left, out double right)
    {
        left = double.MaxValue;
        right = double.MinValue;
        var any = false;
        foreach (var enemy in _enemies)
        {
            if (!enemy.IsAlive || !group.Contains(enemy.Column))
            {
                continue;
            }

            var x = HomeX(enemy);
            left = Math.Min(left, x - (Enemy.Width / 2.0));
            right = Math.Max(right, x + (Enemy.Width / 2.0));
            any = true;
        }

        return any;
    }

    private FormationGroup GroupOf(int column)
    {
        foreach (var group in _groups)
        {
            if (group.Contains(column))
            {
                return group;
            }
        }

        return _groups[_groups.Count - 1];
    }

    private void BuildGroups(WaveLayout layout)
    {
        var margin = (int)Playfield.SideMargin;
        var width = (int)Playfield.Width;
        var center = (int)Playfield.CenterX;
        if (!layout.IsSplit)
        {
            var span = (layout.Columns - 1) * WaveScript.ColumnSpacing;
            _groups.Add(new FormationGroup(0, layout.Columns - 1, center - (span / 2), layout.StartY, 1, margin, width - margin));
            return;
        }

        var leftColumns = (layout.Columns + 1) / 2;
        var rightColumns = layout.Columns - leftColumns;
        var leftMax = center - SplitGapHalfWidth;
        var rightMin = center + SplitGapHalfWidth;
        var leftCenter = (margin + leftMax) / 2;
        var rightCenter = (rightMin + width - margin) / 2;
        var leftSpan = (leftColumns - 1) * WaveScript.ColumnSpacing;
        var rightSpan = (rightColumns - 1) * WaveScript.ColumnSpacing;
        _groups.Add(new FormationGroup(0, leftColumns - 1, leftCenter - (leftSpan / 2), layout.StartY, -1, margin, leftMax));
        _groups.Add(new FormationGroup(leftColumns, layout.Columns - 1, rightCenter - (rightSpan / 2), layout.StartY, 1, rightMin, width - margin));
    }
}
