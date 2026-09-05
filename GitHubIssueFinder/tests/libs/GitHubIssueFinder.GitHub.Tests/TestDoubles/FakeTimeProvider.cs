using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubIssueFinder.GitHub.Tests;

//A clock the tests own. Asking it for a timer is the same thing as asking it to wait, so it
//simply jumps forward to the moment the timer is due and lets the callback run: a search
//that waits an hour for a quota finishes in a millisecond, and the test can still read how
//long the code believed it waited. Task.Delay(delay, provider, token) goes through
//CreateTimer, so the library's own waiting code is what runs, unchanged.
internal sealed class FakeTimeProvider : TimeProvider
{
    private readonly object _sync = new object();
    private DateTimeOffset _utcNow;

    internal FakeTimeProvider(DateTimeOffset start)
    {
        _utcNow = start;
        Start = start;
    }

    //Where the clock began, so a test can say how far it travelled.
    internal DateTimeOffset Start { get; }

    //Every wait the clock was asked for, in the order it was asked.
    internal List<TimeSpan> Waits { get; } = new List<TimeSpan>();

    internal TimeSpan Elapsed => GetUtcNow() - Start;

    internal DateTimeOffset UtcNow
    {
        get { lock (_sync) { return _utcNow; } }
        set { lock (_sync) { _utcNow = value; } }
    }

    public override DateTimeOffset GetUtcNow()
    {
        lock (_sync) { return _utcNow; }
    }

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public override long GetTimestamp() => GetUtcNow().UtcTicks;

    public override ITimer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period) =>
        new FakeTimer(this, callback, state, dueTime);

    //Moves the clock without anybody waiting, which is how a test says "a minute went by
    //between these two calls".
    internal void Advance(TimeSpan amount)
    {
        lock (_sync) { _utcNow = _utcNow.Add(amount); }
    }

    private void Wait(TimeSpan amount)
    {
        lock (_sync)
        {
            Waits.Add(amount);
            _utcNow = _utcNow.Add(amount);
        }
    }

    private sealed class FakeTimer : ITimer
    {
        private readonly FakeTimeProvider _owner;
        private readonly TimerCallback _callback;
        private readonly object _state;

        internal FakeTimer(FakeTimeProvider owner, TimerCallback callback, object state, TimeSpan dueTime)
        {
            _owner = owner;
            _callback = callback;
            _state = state;
            Change(dueTime, Timeout.InfiniteTimeSpan);
        }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            if (dueTime == Timeout.InfiniteTimeSpan) { return true; }

            _owner.Wait(dueTime < TimeSpan.Zero ? TimeSpan.Zero : dueTime);

            //Handed to the pool rather than called here, so the waiting task is finished
            //being built before it is told the wait is over.
            ThreadPool.QueueUserWorkItem(_ => _callback(_state));
            return true;
        }

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
