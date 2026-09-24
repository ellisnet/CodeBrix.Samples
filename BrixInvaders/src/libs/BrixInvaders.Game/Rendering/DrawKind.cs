namespace BrixInvaders.Game.Rendering;

/// <summary>What a <see cref="DrawCommand"/> draws.</summary>
public enum DrawKind
{
    /// <summary>An image (an atlas frame or a loose picture), centred on a point.</summary>
    Image = 0,

    /// <summary>A rectangle, filled and/or outlined, centred on a point.</summary>
    Rect = 1,

    /// <summary>A line of text in the Kenney future font.</summary>
    Text = 2,

    /// <summary>A filled circle (stars, glows).</summary>
    Circle = 3,
}
