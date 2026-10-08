using System;
using System.Collections.Generic;

namespace GoddessTempleDiscovery.Game.Session;

/// <summary>
/// The pending presentation steps of the table, drained one at a time from the engine thread's update: a step
/// starts (it queues its table animation), then the queue waits until the table has settled and the step's hold
/// time has passed before starting the next. Nothing blocks; steps that start no animation pass at once.
/// </summary>
public sealed class PresentationQueue
{
    private readonly Queue<Step> _steps = new Queue<Step>();
    private readonly Func<bool> _tableBusy;
    private Step _current;
    private double _currentSeconds;
    private double _flushSeconds;
    private int _flushSteps;

    /// <summary>Creates the queue.</summary>
    /// <param name="tableBusy">Whether the table is still animating.</param>
    public PresentationQueue(Func<bool> tableBusy)
    {
        _tableBusy = tableBusy ?? throw new ArgumentNullException(nameof(tableBusy));
    }

    /// <summary>Raised when the queue empties after a flush, with its seconds and step count.</summary>
    public event Action<double, int> Flushed;

    /// <summary>True when no step is running or waiting.</summary>
    public bool IsIdle => _current == null && _steps.Count == 0;

    /// <summary>Steps waiting (not counting the running one).</summary>
    public int Pending => _steps.Count;

    /// <summary>The name of the running step, or null.</summary>
    public string Current => _current?.Name;

    /// <summary>Queues a step.</summary>
    /// <param name="name">A name for the log.</param>
    /// <param name="start">What the step does when it starts (on the engine thread).</param>
    /// <param name="holdSeconds">How long the step lasts at least, after its animation.</param>
    public void Enqueue(string name, Action start, double holdSeconds = 0)
    {
        ArgumentNullException.ThrowIfNull(start);
        _steps.Enqueue(new Step(name, start, Math.Max(0, holdSeconds)));
    }

    /// <summary>Advances the queue; call once per update after the table's own update.</summary>
    /// <param name="seconds">The seconds since the last call.</param>
    public void Update(double seconds)
    {
        if (!IsIdle)
        {
            _flushSeconds += seconds;
        }

        if (_current != null)
        {
            _currentSeconds += seconds;
        }

        //Steps that animate nothing and hold for nothing finish in the same update, so a burst of bookkeeping
        //  steps never costs a frame each
        for (var guard = 0; guard < 64; guard++)
        {
            if (_current != null)
            {
                if (_tableBusy() || _currentSeconds < _current.Hold)
                {
                    return;
                }

                _current = null;
            }

            if (_steps.Count == 0)
            {
                if (_flushSteps > 0)
                {
                    Flushed?.Invoke(_flushSeconds, _flushSteps);
                }

                _flushSeconds = 0;
                _flushSteps = 0;
                return;
            }

            _current = _steps.Dequeue();
            _currentSeconds = 0;
            _flushSteps++;
            _current.Start();
        }
    }

    /// <summary>Drops every waiting step (a new game).</summary>
    public void Clear()
    {
        _steps.Clear();
        _current = null;
        _flushSeconds = 0;
        _flushSteps = 0;
    }

    private sealed record Step(string Name, Action Start, double Hold);
}
