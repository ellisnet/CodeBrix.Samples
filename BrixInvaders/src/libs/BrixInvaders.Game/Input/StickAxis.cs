namespace BrixInvaders.Game.Input;

/// <summary>
/// One stick axis read as a digital menu direction: -1, 0 or +1. It turns on past
/// <see cref="InputMapper.StickMenuThreshold"/>, stays on until the stick comes back inside
/// <see cref="InputMapper.StickMenuRelease"/> (hysteresis), and after a release it cannot turn on again - either
/// way - until <see cref="InputMapper.StickSettleSeconds"/> have passed, so the spring-back overshoot of a released
/// stick is never read as a push the other way.
/// </summary>
internal sealed class StickAxis
{
    private double _sinceRelease = InputMapper.StickSettleSeconds;

    /// <summary>The direction now: -1, 0 or +1.</summary>
    public int Direction { get; private set; }

    /// <summary>Starts from a first reading: a stick already pushed counts as on (it must come back first).</summary>
    /// <param name="value">The axis value.</param>
    public void Start(double value)
    {
        Direction = value >= InputMapper.StickMenuRelease ? 1 : value <= -InputMapper.StickMenuRelease ? -1 : 0;
        _sinceRelease = InputMapper.StickSettleSeconds;
    }

    /// <summary>Takes one reading.</summary>
    /// <param name="value">The axis value.</param>
    /// <param name="elapsed">Seconds since the previous reading.</param>
    /// <returns>True when the direction turned on in this reading (a new push).</returns>
    public bool Update(double value, double elapsed)
    {
        if ((Direction > 0 && value < InputMapper.StickMenuRelease) || (Direction < 0 && value > -InputMapper.StickMenuRelease))
        {
            Direction = 0;
            _sinceRelease = 0;
            return false;
        }

        if (Direction != 0)
        {
            return false;
        }

        _sinceRelease += elapsed > 0 ? elapsed : 0;
        if (_sinceRelease < InputMapper.StickSettleSeconds)
        {
            return false;
        }

        if (value >= InputMapper.StickMenuThreshold)
        {
            Direction = 1;
            return true;
        }

        if (value <= -InputMapper.StickMenuThreshold)
        {
            Direction = -1;
            return true;
        }

        return false;
    }
}
