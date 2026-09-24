using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>Meteor showers between waves (sector design 3 onwards) and the meteors still falling afterwards.</summary>
public sealed class MeteorField
{
    /// <summary>Length of a shower in seconds (the intermission lasts this long).</summary>
    public const double ShowerDuration = 6.0;

    /// <summary>Seconds between meteor spawns during a shower.</summary>
    public const double SpawnInterval = 0.4;

    /// <summary>No meteors spawn during the last second of a shower.</summary>
    public const double QuietTail = 1.0;

    /// <summary>Chance in percent that a spawned meteor is big.</summary>
    public const double BigChancePercent = 30.0;

    private readonly List<Meteor> _meteors = new List<Meteor>();
    private double _spawnTimer;

    /// <summary>Meteors in flight.</summary>
    public IReadOnlyList<Meteor> Meteors => _meteors;

    /// <summary>True while a shower is running.</summary>
    public bool IsShowerActive => ShowerTimeLeft > 0;

    /// <summary>Seconds left in the current shower.</summary>
    public double ShowerTimeLeft { get; private set; }

    internal void StartShower(GameEvents events)
    {
        ShowerTimeLeft = ShowerDuration;
        _spawnTimer = 0;
        events.Add(GameEventKind.MeteorShowerStarted);
    }

    internal void Update(double dt, GameRandom random, double speedScale, Func<int> nextId, GameEvents events)
    {
        if (ShowerTimeLeft > 0)
        {
            _spawnTimer -= dt;
            while (_spawnTimer <= 0 && ShowerTimeLeft > QuietTail)
            {
                _spawnTimer += SpawnInterval;
                var size = random.Chance(BigChancePercent) ? MeteorSize.Big : MeteorSize.Small;
                var x = random.NextDouble(40, Playfield.Width - 40);
                var vx = random.NextDouble(-60, 60);
                var vy = random.NextDouble(180, 300) * speedScale;
                var spin = random.NextDouble(-2, 2);
                _meteors.Add(new Meteor(nextId(), size, x, -40, vx, vy, spin));
            }

            ShowerTimeLeft -= dt;
            if (ShowerTimeLeft <= 0)
            {
                ShowerTimeLeft = 0;
                events.Add(GameEventKind.MeteorShowerEnded);
            }
        }

        foreach (var meteor in _meteors)
        {
            meteor.X += meteor.Vx * dt;
            meteor.Y += meteor.Vy * dt;
            meteor.Rotation += meteor.RotationSpeed * dt;
            if (meteor.Box.Top > Playfield.Height || meteor.Box.Right < 0 || meteor.Box.Left > Playfield.Width)
            {
                meteor.IsAlive = false;
            }
        }

        RemoveDead();
    }

    internal void RemoveDead() => _meteors.RemoveAll(m => !m.IsAlive);

    internal int ClearAll()
    {
        var count = _meteors.Count;
        _meteors.Clear();
        return count;
    }
}
