using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>
/// The whole game rules engine. Advance it with <see cref="Step"/> at the fixed step (<see cref="Playfield.FixedStep"/>);
/// read its public state to render; turn <see cref="Events"/> (cleared at the start of every step) into sprites,
/// sounds and particles. Deterministic: the same <see cref="GameSetup"/> and the same inputs give the same game.
/// </summary>
public sealed class GameSimulation
{
    /// <summary>Seconds of the "WAVE n" intro before the formation starts moving.</summary>
    public const double WaveIntroDuration = 2.0;

    /// <summary>Seconds between waves when there is no meteor shower.</summary>
    public const double IntermissionDuration = 2.0;

    /// <summary>Seconds of the boss warning.</summary>
    public const double BossWarningDuration = 3.0;

    /// <summary>Shortest seconds between UFO appearances.</summary>
    public const double UfoMinInterval = 16.0;

    /// <summary>Longest seconds between UFO appearances.</summary>
    public const double UfoMaxInterval = 24.0;

    /// <summary>Spread-shot fan angles in degrees from straight up.</summary>
    public const double SpreadAngleDegrees = 12.0;

    private static readonly IReadOnlyList<Enemy> NoEnemies = new List<Enemy>();

    private readonly GameRandom _random;
    private readonly Func<int> _nextId;
    private readonly GameEvents _events = new GameEvents();
    private FormationController _formation;
    private EnemyBehaviours _behaviours;
    private int _lastId;
    private bool _bombHeld;
    private double _ufoTimer;

    /// <summary>Creates a new game at the setup's start sector (wave 1 intro).</summary>
    /// <param name="setup">The setup.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="setup"/> is null.</exception>
    public GameSimulation(GameSetup setup)
    {
        Setup = setup ?? throw new ArgumentNullException(nameof(setup));
        Settings = DifficultyTable.For(setup.Difficulty);
        _random = new GameRandom(setup.Seed);
        _nextId = NextId;
        Player = new PlayerShip(Settings.Lives, Settings.StartingBombs);
        Scoring = new Scoring(Settings.ScoreMultiplierPercent);
        Sector = setup.StartSector;
        BeginSector();
    }

    /// <summary>The setup the game started with.</summary>
    public GameSetup Setup { get; }

    /// <summary>The difficulty settings.</summary>
    public DifficultySettings Settings { get; }

    /// <summary>Events of the most recent step.</summary>
    public GameEvents Events => _events;

    /// <summary>The player's ship.</summary>
    public PlayerShip Player { get; }

    /// <summary>Score, chain and multipliers.</summary>
    public Scoring Scoring { get; }

    /// <summary>Power-up drops and active timers.</summary>
    public PowerUpSystem PowerUps { get; } = new PowerUpSystem();

    /// <summary>Projectiles in flight.</summary>
    public Projectiles Projectiles { get; } = new Projectiles();

    /// <summary>Meteor showers and meteors.</summary>
    public MeteorField Meteors { get; } = new MeteorField();

    /// <summary>The current wave's formation, or null during the boss fight and after the sector.</summary>
    public FormationController Formation => _formation;

    /// <summary>The current wave's enemies (alive or not; skip the dead ones), or an empty list.</summary>
    public IReadOnlyList<Enemy> Enemies => _formation == null ? NoEnemies : _formation.Enemies;

    /// <summary>The UFO, or null.</summary>
    public Ufo Ufo { get; private set; }

    /// <summary>The boss, or null outside the boss fight.</summary>
    public Boss Boss { get; private set; }

    /// <summary>Current sector, 1 or more.</summary>
    public int Sector { get; private set; }

    /// <summary>Current wave, 1..6 (6 during the boss).</summary>
    public int Wave { get; private set; }

    /// <summary>Where the simulation is within the sector.</summary>
    public StagePhase Phase { get; private set; }

    /// <summary>Seconds left in a timed phase (intro, intermission, boss warning), else 0.</summary>
    public double PhaseTimeLeft { get; private set; }

    /// <summary>Simulated seconds since the start.</summary>
    public double Time { get; private set; }

    /// <summary>Steps taken since the start.</summary>
    public long StepCount { get; private set; }

    /// <summary>Current score.</summary>
    public long Score => Scoring.Score;

    /// <summary>True once the last life is gone.</summary>
    public bool IsGameOver => Phase == StagePhase.GameOver;

    /// <summary>The sector's active features.</summary>
    public SectorFeatures Features => SectorRules.FeaturesOf(Sector);

    /// <summary>
    /// Advances the game by <paramref name="dt"/> seconds (call it with <see cref="Playfield.FixedStep"/>). Does
    /// nothing after game over or for a non-positive step.
    /// </summary>
    /// <param name="dt">Step in seconds.</param>
    /// <param name="input">The input snapshot.</param>
    public void Step(double dt, GameInput input)
    {
        _events.Clear();
        if (Phase == StagePhase.GameOver || dt <= 0 || double.IsNaN(dt))
        {
            return;
        }

        Time += dt;
        StepCount++;
        var bombPressed = input.Bomb && !_bombHeld;
        _bombHeld = input.Bomb;

        Player.Update(dt, _events);
        Player.Move(input.MoveAxis, dt, PowerUps.IsActive(PowerUpKind.SpeedBoost));
        if (Phase != StagePhase.SectorComplete)
        {
            if (input.Fire)
            {
                TryFire();
            }

            if (bombPressed)
            {
                TryBomb();
            }
        }

        UpdateStage(dt);
        UpdateUfo(dt);
        Meteors.Update(dt, _random, SectorRules.SpeedScale(Sector), _nextId, _events);
        PowerUps.Update(dt, _events);
        var misses = Projectiles.Update(dt, Player.X, Player.Y, _events);
        for (var i = 0; i < misses; i++)
        {
            Scoring.RegisterMiss(_events);
        }

        ResolvePlayerFire();
        ResolveHazardsOnPlayer();
        PowerUps.CollectTouching(Player, Scoring, _events);
        CheckLanding();
        CheckProgress();
        CheckGameOver();
    }

    /// <summary>Starts the next sector after <see cref="StagePhase.SectorComplete"/>.</summary>
    /// <exception cref="InvalidOperationException">When the sector is not complete.</exception>
    public void BeginNextSector()
    {
        if (Phase != StagePhase.SectorComplete)
        {
            throw new InvalidOperationException("The sector is not complete.");
        }

        Sector++;
        BeginSector();
    }

    /// <summary>A hash of the public state (see <see cref="StateHash"/>), for determinism checks and replays.</summary>
    /// <returns>The hash.</returns>
    public ulong ComputeStateHash() => StateHash.Compute(this);

    internal GameRandom Random => _random;

    internal EnemyBehaviours Behaviours => _behaviours;

    internal int NextId() => ++_lastId;

    internal void ForceWaveCleared()
    {
        if (_formation == null)
        {
            return;
        }

        foreach (var enemy in _formation.Enemies)
        {
            enemy.Health = 0;
        }
    }

    private void BeginSector()
    {
        Wave = 1;
        _behaviours = new EnemyBehaviours(Settings, Sector, _random);
        Projectiles.Clear();
        PowerUps.ClearDrops();
        Meteors.ClearAll();
        Ufo = null;
        Boss = null;
        _ufoTimer = _random.NextDouble(UfoMinInterval, UfoMaxInterval);
        StartWave();
    }

    private void StartWave()
    {
        _formation = new FormationController(WaveScript.For(Sector, Wave), Settings, _nextId);
        _behaviours.StartWave(_formation.Enemies);
        SetPhase(StagePhase.WaveIntro, WaveIntroDuration);
        _events.Add(new GameEvent(GameEventKind.WaveStarted, value: Wave));
    }

    private void SetPhase(StagePhase phase, double duration)
    {
        Phase = phase;
        PhaseTimeLeft = duration;
    }

    private void UpdateStage(double dt)
    {
        if (PhaseTimeLeft > 0)
        {
            PhaseTimeLeft = Math.Max(0, PhaseTimeLeft - dt);
        }

        switch (Phase)
        {
            case StagePhase.WaveIntro:
                if (PhaseTimeLeft <= 0)
                {
                    SetPhase(StagePhase.WaveActive, 0);
                }

                break;
            case StagePhase.WaveActive:
                _formation.Update(dt, _events);
                _behaviours.Update(dt, _formation, Player, Projectiles, _nextId, _events);
                break;
            case StagePhase.WaveIntermission:
                if (PhaseTimeLeft <= 0)
                {
                    Wave++;
                    StartWave();
                }

                break;
            case StagePhase.BossWarning:
                if (PhaseTimeLeft <= 0)
                {
                    Boss = Boss.Create(Sector, Settings, _nextId);
                    SetPhase(StagePhase.BossFight, 0);
                    _events.Add(new GameEvent(GameEventKind.BossAppeared, Boss.X, Boss.Y, value: Boss.Design));
                }

                break;
            case StagePhase.BossFight:
                Boss.Update(dt, Player, Projectiles, _nextId, _events);
                break;
        }
    }

    private void UpdateUfo(double dt)
    {
        if (Ufo != null)
        {
            Ufo.X += Ufo.Direction * Ufo.Speed * dt;
            if (Ufo.HasEscaped)
            {
                Ufo = null;
                _events.Add(GameEventKind.UfoEscaped);
            }

            return;
        }

        if (Phase != StagePhase.WaveActive || (Features & SectorFeatures.Ufo) == 0)
        {
            return;
        }

        _ufoTimer -= dt;
        if (_ufoTimer <= 0)
        {
            _ufoTimer = _random.NextDouble(UfoMinInterval, UfoMaxInterval);
            var direction = _random.Next(2) == 0 ? 1 : -1;
            Ufo = new Ufo(NextId(), direction, Scoring.UfoPoints(_random.Next(Scoring.UfoValueCount), Sector));
            _events.Add(new GameEvent(GameEventKind.UfoAppeared, Ufo.X, Ufo.Y, value: direction));
        }
    }

    private void TryFire()
    {
        if (!Player.CanFire())
        {
            return;
        }

        Player.FireCooldown = PowerUps.IsActive(PowerUpKind.RapidFire) ? PlayerShip.RapidFireInterval : PlayerShip.FireInterval;
        var piercing = PowerUps.IsActive(PowerUpKind.PiercingLaser);
        var y = PlayerShip.LineY - (PlayerShip.Height / 2.0) - (Projectile.PlayerBoltHeight / 2.0);
        if (PowerUps.IsActive(PowerUpKind.SpreadShot))
        {
            foreach (var degrees in new[] { -SpreadAngleDegrees, 0.0, SpreadAngleDegrees })
            {
                var radians = degrees * Math.PI / 180.0;
                Projectiles.AddPlayerBolt(NextId(), Player.X, y, Math.Sin(radians) * Projectiles.PlayerBoltSpeed,
                    -Math.Cos(radians) * Projectiles.PlayerBoltSpeed, piercing);
            }

            _events.Add(new GameEvent(GameEventKind.PlayerFired, Player.X, y, value: 3));
        }
        else
        {
            Projectiles.AddPlayerBolt(NextId(), Player.X, y, 0, -Projectiles.PlayerBoltSpeed, piercing);
            _events.Add(new GameEvent(GameEventKind.PlayerFired, Player.X, y, value: 1));
        }
    }

    private void TryBomb()
    {
        if (!Player.IsPresent || Player.Bombs <= 0)
        {
            return;
        }

        Player.Bombs--;
        _events.Add(new GameEvent(GameEventKind.BombDetonated, Player.X, Player.Y));
        Projectiles.ClearEnemyFire();
        Meteors.ClearAll();
        if (_formation != null && (Phase == StagePhase.WaveActive || Phase == StagePhase.WaveIntro))
        {
            foreach (var enemy in _formation.Enemies)
            {
                if (enemy.IsAlive)
                {
                    DamageEnemy(enemy);
                }
            }
        }

        if (Boss != null)
        {
            foreach (var section in Boss.Sections)
            {
                DamageBoss(section, Boss.BombDamage);
            }
        }
    }

    private void DamageEnemy(Enemy enemy)
    {
        if (enemy.HasShield)
        {
            enemy.Health--;
            _events.Add(new GameEvent(GameEventKind.EnemyShieldBroken, enemy.X, enemy.Y, role: enemy.Role, colour: enemy.Colour));
            return;
        }

        enemy.Health = 0;
        var basePoints = Scoring.EnemyBasePoints(enemy.Role, enemy.Colour, Sector, enemy.State != EnemyState.InFormation);
        var points = Scoring.Award(basePoints);
        _events.Add(new GameEvent(GameEventKind.EnemyDestroyed, enemy.X, enemy.Y, points, role: enemy.Role, colour: enemy.Colour));
        PowerUps.TryDrop(enemy.X, enemy.Y, Settings.PowerUpDropPercent, _random, NextId(), _events);
    }

    private void DamageBoss(BossSection section, int damage)
    {
        var result = Boss.ApplyDamage(section, damage, _events);
        if (result == BossDamageResult.SectionDestroyed)
        {
            var box = Boss.BoxOf(section);
            var points = Scoring.Award(Scoring.BossSectionPoints);
            _events.Add(new GameEvent(GameEventKind.BossSectionDestroyed, box.CenterX, box.CenterY, points, section.Index,
                section: section.Kind));
        }
        else if (result == BossDamageResult.Defeated)
        {
            var points = Scoring.Award(Scoring.BossDefeatPoints(Sector));
            _events.Add(new GameEvent(GameEventKind.BossDefeated, Boss.X, Boss.Y, points, Boss.Design));
        }
    }

    private void ResolvePlayerFire()
    {
        foreach (var bolt in Projectiles.PlayerBolts)
        {
            if (!bolt.IsAlive)
            {
                continue;
            }

            var box = bolt.Box;
            foreach (var enemy in Enemies)
            {
                if (bolt.IsAlive && enemy.IsAlive && !bolt.HasHitTarget(enemy.Id) && box.Intersects(enemy.Box))
                {
                    RegisterBoltHit(bolt, enemy.Id);
                    DamageEnemy(enemy);
                }
            }

            if (Boss != null)
            {
                foreach (var section in Boss.Sections)
                {
                    if (bolt.IsAlive && !section.IsDestroyed && !bolt.HasHitTarget(section.Id) && box.Intersects(Boss.BoxOf(section))
                        && Boss.State != BossState.Dying && Boss.State != BossState.Gone)
                    {
                        RegisterBoltHit(bolt, section.Id);
                        if (Boss.State == BossState.Entering || (section.Kind == BossSectionKind.Core && Boss.IsCoreArmoured))
                        {
                            bolt.IsAlive = false;
                        }

                        DamageBoss(section, 1);
                    }
                }
            }

            if (bolt.IsAlive && Ufo != null && !bolt.HasHitTarget(Ufo.Id) && box.Intersects(Ufo.Box))
            {
                RegisterBoltHit(bolt, Ufo.Id);
                var points = Scoring.Award(Ufo.Points);
                _events.Add(new GameEvent(GameEventKind.UfoDestroyed, Ufo.X, Ufo.Y, points));
                PowerUps.Spawn(PowerUpSystem.ChooseKind(_random), Ufo.X, Ufo.Y, NextId(), _events);
                Ufo = null;
            }

            foreach (var missile in Projectiles.Missiles)
            {
                if (bolt.IsAlive && missile.IsAlive && box.Intersects(missile.Box))
                {
                    RegisterBoltHit(bolt, missile.Id);
                    missile.IsAlive = false;
                    var points = Scoring.Award(Scoring.MissilePoints);
                    _events.Add(new GameEvent(GameEventKind.MissileDestroyed, missile.X, missile.Y, points));
                }
            }

            foreach (var meteor in Meteors.Meteors)
            {
                if (bolt.IsAlive && meteor.IsAlive && !bolt.HasHitTarget(meteor.Id) && box.Intersects(meteor.Box))
                {
                    RegisterBoltHit(bolt, meteor.Id);
                    meteor.Health--;
                    if (meteor.Health > 0)
                    {
                        _events.Add(new GameEvent(GameEventKind.MeteorHit, meteor.X, meteor.Y));
                    }
                    else
                    {
                        meteor.IsAlive = false;
                        var big = meteor.Size == MeteorSize.Big;
                        var points = Scoring.Award(big ? Scoring.BigMeteorPoints : Scoring.SmallMeteorPoints);
                        _events.Add(new GameEvent(GameEventKind.MeteorDestroyed, meteor.X, meteor.Y, points, big ? 1 : 0));
                    }
                }
            }
        }

        Projectiles.RemoveDead();
        Meteors.RemoveDead();
    }

    private void RegisterBoltHit(Projectile bolt, int targetId)
    {
        bolt.RecordHit(targetId);
        if (!bolt.IsPiercing)
        {
            bolt.IsAlive = false;
        }

        Scoring.RegisterHit(_events);
    }

    private void ResolveHazardsOnPlayer()
    {
        if (!Player.IsPresent)
        {
            return;
        }

        foreach (var bolt in Projectiles.EnemyBolts)
        {
            if (Player.IsPresent && bolt.IsAlive && bolt.Box.Intersects(Player.Box) && Player.ApplyHit(_events) != PlayerHitResult.Ignored)
            {
                bolt.IsAlive = false;
            }
        }

        foreach (var missile in Projectiles.Missiles)
        {
            if (Player.IsPresent && missile.IsAlive && missile.Box.Intersects(Player.Box) && Player.ApplyHit(_events) != PlayerHitResult.Ignored)
            {
                missile.IsAlive = false;
            }
        }

        foreach (var meteor in Meteors.Meteors)
        {
            if (Player.IsPresent && meteor.IsAlive && meteor.Box.Intersects(Player.Box) && Player.ApplyHit(_events) != PlayerHitResult.Ignored)
            {
                meteor.IsAlive = false;
            }
        }

        foreach (var enemy in Enemies)
        {
            if (Player.IsPresent && enemy.IsAlive && enemy.State != EnemyState.InFormation && enemy.Box.Intersects(Player.Box)
                && Player.ApplyHit(_events) != PlayerHitResult.Ignored)
            {
                enemy.Health = 0;
                _events.Add(new GameEvent(GameEventKind.EnemyDestroyed, enemy.X, enemy.Y, 0, role: enemy.Role, colour: enemy.Colour));
            }
        }

        Projectiles.RemoveDead();
        Meteors.RemoveDead();
    }

    private void CheckLanding()
    {
        if (Phase != StagePhase.WaveActive || _formation == null || !_formation.HasLanded())
        {
            return;
        }

        _events.Add(GameEventKind.FormationLanded);
        Player.Destroy(_events);
        _formation.ResetHeight();
    }

    private void CheckProgress()
    {
        if (Phase == StagePhase.WaveActive || Phase == StagePhase.WaveIntro)
        {
            if (_formation.AliveCount > 0)
            {
                return;
            }

            _events.Add(new GameEvent(GameEventKind.WaveCleared, value: Wave));
            if (Wave < SectorRules.WavesPerSector)
            {
                if ((Features & SectorFeatures.MeteorShowers) != 0)
                {
                    Meteors.StartShower(_events);
                    SetPhase(StagePhase.WaveIntermission, MeteorField.ShowerDuration);
                }
                else
                {
                    SetPhase(StagePhase.WaveIntermission, IntermissionDuration);
                }
            }
            else
            {
                _formation = null;
                SetPhase(StagePhase.BossWarning, BossWarningDuration);
                _events.Add(new GameEvent(GameEventKind.BossIncoming, value: Sector));
            }
        }
        else if (Phase == StagePhase.BossFight && Boss.State == BossState.Gone)
        {
            Boss = null;
            var bonus = Scoring.AwardBonus(Scoring.SectorClearBonus(Sector));
            _events.Add(new GameEvent(GameEventKind.BonusAwarded, points: bonus));
            _events.Add(new GameEvent(GameEventKind.SectorCleared, value: Sector));
            SetPhase(StagePhase.SectorComplete, 0);
        }
    }

    private void CheckGameOver()
    {
        if (Player.Lives <= 0 && !Player.IsPresent)
        {
            SetPhase(StagePhase.GameOver, 0);
            _events.Add(new GameEvent(GameEventKind.GameOver, points: Scoring.Score));
        }
    }
}
