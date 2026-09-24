using System;
using System.Drawing;
using System.Numerics;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;
using CodeBrix.Platform.GameEngine.Drawing;
using CodeBrix.Platform.GameEngine.Drawing.Direct;
using CodeBrix.Platform.GameEngine.Drawing.Sprites;
using CodeBrix.Platform.GameEngine.Drawing.Tilesheets;
using CodeBrix.Platform.GameEngine.Rendering;
using CodeBrix.Platform.GameEngine.Scenes;
using SpriteHorizontalAlignment = CodeBrix.Platform.GameEngine.Drawing.Sprites.HorizontalAlignment;
using SpriteVerticalAlignment = CodeBrix.Platform.GameEngine.Drawing.Sprites.VerticalAlignment;

namespace BrixInvaders.Game.Hud;

/// <summary>
/// The engine <see cref="HealthBar"/> over a boss: an invisible anchor sprite follows the boss core on the world
/// layer and the bar follows the sprite (the boss itself is drawn by <see cref="PlayfieldPainter"/>). Created when a
/// boss appears, released when it dies or the game goes away. Engine thread only.
/// </summary>
public sealed class BossHealthBar
{
    private const int CellSize = 16;
    private readonly RenderSurfaceHostBase _host;
    private readonly SceneLayer _layer;
    private readonly Frame _anchorFrame;
    private Boss _boss;
    private Sprite _anchor;
    private HealthBar _bar;

    /// <summary>Creates the presenter.</summary>
    /// <param name="host">The render surface host.</param>
    /// <param name="layer">The world layer (cells of 16 world pixels).</param>
    /// <param name="anchorFrame">Any frame, for the invisible anchor sprite.</param>
    public BossHealthBar(RenderSurfaceHostBase host, SceneLayer layer, Frame anchorFrame)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _layer = layer ?? throw new ArgumentNullException(nameof(layer));
        _anchorFrame = anchorFrame;
    }

    /// <summary>The layer cell size the anchor is positioned in (world pixels per grid cell).</summary>
    public static int GridCellSize => CellSize;

    /// <summary>Brings the bar in line with the game: creates, moves, fills or releases it.</summary>
    /// <param name="game">The game on show, or null.</param>
    /// <param name="transform">World to layer transform.</param>
    public void Sync(GameSimulation game, PlayfieldTransform transform)
    {
        var boss = game?.Boss;
        var alive = boss != null && boss.State is BossState.Entering or BossState.Fighting;
        if (!alive || !ReferenceEquals(boss, _boss))
        {
            Release();
        }

        if (!alive)
        {
            return;
        }

        if (_anchor == null)
        {
            Create(boss, transform);
        }

        var x = transform.ToScreenX(boss.X);
        var y = transform.ToScreenY(boss.Y);
        _anchor.SetPosition(new Vector2((float)(x / CellSize), (float)(y / CellSize)));
        _bar.SetValue(boss.RemainingHealth);
    }

    /// <summary>Releases the sprite and its bar.</summary>
    public void Release()
    {
        if (_anchor != null)
        {
            _bar?.Hide();
            _anchor.Dispose();
        }

        _anchor = null;
        _bar = null;
        _boss = null;
    }

    private void Create(Boss boss, PlayfieldTransform transform)
    {
        _boss = boss;
        _anchor = SpriteManager.Instance.CreateSprite(_layer, _anchorFrame, $"brixinvaders-boss-anchor-{boss.Sector}-{Environment.TickCount64}");
        _anchor.RenderSize = new Size((int)(300 * transform.Scale), (int)(120 * transform.Scale));
        _anchor.HorizAlign = SpriteHorizontalAlignment.Center;
        _anchor.VertAlign = SpriteVerticalAlignment.Middle;
        _anchor.Visible = false;
        _bar = new HealthBar(_host, _anchor, Math.Max(1, boss.TotalHealth), new Size((int)(220 * transform.Scale), (int)(10 * transform.Scale)),
                new Point(0, (int)(-6 * transform.Scale)))
            .SetThresholdColors(Color.FromArgb(245, 240, 190, 60), Color.FromArgb(245, 235, 70, 60))
            .SetThresholds(0.66f, 0.33f);
        _bar.SetZOrder(200);
        _bar.Show();
    }
}
