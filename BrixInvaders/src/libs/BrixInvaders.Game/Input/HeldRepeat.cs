namespace BrixInvaders.Game.Input;

/// <summary>
/// The hold-to-repeat clock of one menu direction: the press acts at once, then - while the direction stays held -
/// it acts again after <c>delay</c> seconds and every <c>interval</c> seconds after that, at a constant rate.
/// Releasing resets it. Driven by elapsed time passed in, never by the wall clock.
/// </summary>
internal sealed class HeldRepeat
{
    private bool _held;
    private bool _suppressed;
    private double _heldTime;
    private double _nextAction;

    /// <summary>
    /// Swallows the current hold: nothing fires (not even a repeat) until the direction has been released once.
    /// Used for whatever is already held when sampling starts.
    /// </summary>
    /// <param name="held">Whether the direction is held now.</param>
    public void Suppress(bool held)
    {
        _suppressed = held;
        _held = false;
    }

    /// <summary>Advances the clock by one sample.</summary>
    /// <param name="held">Whether the direction is held in this sample.</param>
    /// <param name="elapsed">Seconds since the previous sample.</param>
    /// <param name="delay">Seconds from the press to the first repeat.</param>
    /// <param name="interval">Seconds between repeats.</param>
    /// <returns>True when the direction acts in this sample (the press or a repeat).</returns>
    public bool Advance(bool held, double elapsed, double delay, double interval)
    {
        if (!held)
        {
            _held = false;
            _suppressed = false;
            return false;
        }

        if (_suppressed)
        {
            return false;
        }

        if (!_held)
        {
            _held = true;
            _heldTime = 0;
            _nextAction = delay;
            return true;
        }

        _heldTime += elapsed > 0 ? elapsed : 0;
        if (_heldTime < _nextAction)
        {
            return false;
        }

        //One action per due time; after a long stall the next one is a full interval away (no burst)
        _nextAction += interval;
        if (_nextAction <= _heldTime)
        {
            _nextAction = _heldTime + interval;
        }

        return true;
    }
}
