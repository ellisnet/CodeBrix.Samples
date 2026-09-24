namespace BrixInvaders.Game.Screens;

/// <summary>
/// One screen: builds its state when it opens, updates it each step, draws it each frame and lets it go when it
/// closes. The <see cref="ScreenDirector"/> keeps exactly one open, following the screen state machine.
/// </summary>
public abstract class ScreenPainter
{
    /// <summary>Seconds since the screen opened.</summary>
    protected double OpenTime { get; private set; }

    /// <summary>The screen opens.</summary>
    /// <param name="context">The context.</param>
    public void Open(PaintContext context)
    {
        OpenTime = 0;
        OnOpen(context);
    }

    /// <summary>Advances the screen's own animation.</summary>
    /// <param name="dt">Seconds.</param>
    public void Update(double dt)
    {
        OpenTime += dt;
        OnUpdate(dt);
    }

    /// <summary>Draws one frame.</summary>
    /// <param name="context">The context.</param>
    public abstract void Paint(PaintContext context);

    /// <summary>The screen closes: drop whatever it built.</summary>
    public virtual void Close()
    {
    }

    /// <summary>Called when the screen opens.</summary>
    /// <param name="context">The context.</param>
    protected virtual void OnOpen(PaintContext context)
    {
    }

    /// <summary>Called every step while open.</summary>
    /// <param name="dt">Seconds.</param>
    protected virtual void OnUpdate(double dt)
    {
    }
}
