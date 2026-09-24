namespace BrixInvaders.Game.Input;

/// <summary>The two gamepad binding presets offered on the settings screen.</summary>
public enum GamepadProfile
{
    /// <summary>A fires, B drops a bomb (the default).</summary>
    Classic = 0,

    /// <summary>The right shoulder fires, the left shoulder drops a bomb; A and B stay confirm and back.</summary>
    Shoulder = 1,
}
