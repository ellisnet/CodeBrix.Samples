namespace BrixInvaders.GameLogic;

/// <summary>One destructible part of a boss, positioned relative to the boss centre.</summary>
public sealed class BossSection
{
    internal BossSection(int id, int index, BossSectionKind kind, double offsetX, double offsetY, double width,
        double height, int maxHealth)
    {
        Id = id;
        Index = index;
        Kind = kind;
        OffsetX = offsetX;
        OffsetY = offsetY;
        Width = width;
        Height = height;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    /// <summary>Unique id within the simulation.</summary>
    public int Id { get; }

    /// <summary>Index within the boss's section list (0 is always the core).</summary>
    public int Index { get; }

    /// <summary>Kind.</summary>
    public BossSectionKind Kind { get; }

    /// <summary>Offset x from the boss centre.</summary>
    public double OffsetX { get; }

    /// <summary>Offset y from the boss centre.</summary>
    public double OffsetY { get; }

    /// <summary>Hit-box width.</summary>
    public double Width { get; }

    /// <summary>Hit-box height.</summary>
    public double Height { get; }

    /// <summary>Health at full strength.</summary>
    public int MaxHealth { get; }

    /// <summary>Remaining health.</summary>
    public int Health { get; internal set; }

    /// <summary>True once destroyed (draw the wreck / damage sprite).</summary>
    public bool IsDestroyed => Health <= 0;

    internal double FireTimer { get; set; }

    /// <summary>The section's hit box for a boss centred at (<paramref name="bossX"/>, <paramref name="bossY"/>).</summary>
    /// <param name="bossX">Boss centre x.</param>
    /// <param name="bossY">Boss centre y.</param>
    /// <returns>The hit box.</returns>
    public Box BoxAt(double bossX, double bossY) => Box.FromCenter(bossX + OffsetX, bossY + OffsetY, Width, Height);
}
