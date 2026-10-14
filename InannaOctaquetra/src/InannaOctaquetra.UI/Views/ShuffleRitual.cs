using InannaOctaquetra.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace InannaOctaquetra.Views;

/// <summary>
/// The mystical shuffle: a golden glow wakes beneath the deck, eight card backs riffle apart and
/// interleave three times while magic dust rises from the split and four-pointed sparkles twinkle
/// around it, eight motes circle the deck like the points of Inanna's star, and the deck settles
/// with a burst of light.
///
/// Everything is drawn on a temporary, non-interactive layer and driven by one frame timer that
/// moves Canvas positions, opacity and transforms (the animation paths proven on this head).
/// The layer removes itself when the ritual ends, even if a frame throws.
/// </summary>
internal sealed class ShuffleRitual
{
    // ---- timeline, in seconds
    private const double GlowIn = 0.35;
    private const double RiffleStart = 0.25;
    private const double RiffleEnd = 1.95;
    private const double SettleEnd = 2.25;
    private const double Duration = 2.85;
    private const int Riffles = 3;
    private const int CardCount = 8;
    private const int MaxParticles = 170;

    private static readonly string[] DustColours = { Ornament.GoldPale, Ornament.Gold, "#FFF6DC", "#C9B6FF", "#9FB4FF" };
    private static readonly string[] SparkleColours = { "#FFF8E6", Ornament.GoldPale, Ornament.Gold };

    private readonly Canvas _host;
    private readonly Canvas _layer = new() { IsHitTestVisible = false };
    private readonly Random _rng;
    private readonly double _deckX, _deckY, _deckW, _deckH, _cx, _cy;

    private readonly List<(Border Frame, CompositeTransform T)> _cards = new();
    private readonly List<Particle> _particles = new();
    private readonly List<(Ellipse Dot, CompositeTransform T)> _motes = new();
    private Ellipse _glow;
    private Ellipse _halo;
    private CompositeTransform _glowT;

    private double _dustDebt;
    private double _sparkleDebt;
    private bool _burstDone;
    private double _last;

    private ShuffleRitual(Canvas host, double deckX, double deckY, double deckW, double deckH, Random rng)
    {
        _host = host;
        _rng = rng;
        _deckX = deckX;
        _deckY = deckY;
        _deckW = deckW;
        _deckH = deckH;
        _cx = deckX + deckW / 2;
        _cy = deckY + deckH / 2;
    }

    /// <summary>Play the ritual over the deck's rectangle; completes when every element is gone.</summary>
    public static Task PlayAsync(Canvas host, double deckX, double deckY, double deckW, double deckH, Random rng)
    {
        if (deckW < 10 || deckH < 10) { return Task.CompletedTask; }
        return new ShuffleRitual(host, deckX, deckY, deckW, deckH, rng).RunAsync();
    }

    private Task RunAsync()
    {
        var done = new TaskCompletionSource();
        var queue = _host.DispatcherQueue;

        try
        {
            Canvas.SetZIndex(_layer, 55);
            _host.Children.Add(_layer);
            BuildGlow();
            BuildCards();
            BuildMotes();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Shuffle ritual could not start: {ex.Message}");
            Cleanup();
            return Task.CompletedTask;
        }

        Microsoft.UI.Dispatching.DispatcherQueueTimer timer = null;
        try { timer = queue?.CreateTimer(); }
        catch (Exception) { timer = null; }
        if (timer is null)
        {
            Cleanup();
            return Task.CompletedTask;
        }

        var clock = Stopwatch.StartNew();
        timer.Interval = TimeSpan.FromMilliseconds(16);
        timer.Tick += (t, _) =>
        {
            var now = clock.Elapsed.TotalSeconds;
            var finished = now >= Duration;
            try
            {
                Frame(Math.Min(now, Duration), Math.Clamp(now - _last, 0, 0.05));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Shuffle ritual frame failed: {ex.Message}");
                finished = true;
            }
            _last = now;

            if (finished)
            {
                t.Stop();
                Cleanup();
                done.TrySetResult();
            }
        };
        timer.Start();
        return done.Task;
    }

    private void Cleanup()
    {
        try
        {
            _layer.Children.Clear();
            _host.Children.Remove(_layer);
        }
        catch (Exception) { /* the page may already be gone */ }
        _particles.Clear();
        _cards.Clear();
        _motes.Clear();
    }

    // ==================================================================== construction

    private void BuildGlow()
    {
        var size = _deckH * 1.9;
        _halo = new Ellipse
        {
            Width = size * 1.15,
            Height = size * 1.15,
            Opacity = 0,
            Fill = Radial(Ornament.LapisGlow, 0.30),
        };
        Canvas.SetLeft(_halo, _cx - size * 0.575);
        Canvas.SetTop(_halo, _cy - size * 0.575);
        _layer.Children.Add(_halo);

        _glowT = new CompositeTransform { CenterX = size / 2, CenterY = size / 2 };
        _glow = new Ellipse
        {
            Width = size,
            Height = size,
            Opacity = 0,
            Fill = Radial(Ornament.Gold, 0.42),
            RenderTransform = _glowT,
        };
        Canvas.SetLeft(_glow, _cx - size / 2);
        Canvas.SetTop(_glow, _cy - size / 2);
        _layer.Children.Add(_glow);
    }

    private void BuildCards()
    {
        for (int k = 0; k < CardCount; k++)
        {
            var view = new CardView
            {
                IsFaceUp = false,
                CardWidth = _deckW,
                CardHeight = _deckH,
                EnableHover = false,
                IsHitTestVisible = false,
            };
            view.Rebuild();

            var t = new CompositeTransform { CenterX = _deckW / 2, CenterY = _deckH / 2 };
            var frame = new Border
            {
                Width = _deckW,
                Height = _deckH,
                Child = view,
                RenderTransform = t,
                IsHitTestVisible = false,
            };
            var rest = RestOffset(k);
            Canvas.SetLeft(frame, _deckX + rest);
            Canvas.SetTop(frame, _deckY - rest);
            Canvas.SetZIndex(frame, 10 + k);
            _layer.Children.Add(frame);
            _cards.Add((frame, t));
        }
    }

    private void BuildMotes()
    {
        for (int i = 0; i < 8; i++)
        {
            const double size = 12;
            var t = new CompositeTransform { CenterX = size / 2, CenterY = size / 2 };
            var dot = new Ellipse
            {
                Width = size,
                Height = size,
                Opacity = 0,
                Fill = Radial("#FFF6DC", 1.0),
                RenderTransform = t,
            };
            Canvas.SetZIndex(dot, 40);
            _layer.Children.Add(dot);
            _motes.Add((dot, t));
        }
    }

    private static double RestOffset(int k) => (CardCount - 1 - k) * 1.5;

    // ==================================================================== frames

    private void Frame(double t, double dt)
    {
        UpdateGlow(t);
        UpdateCards(t);
        UpdateMotes(t);
        Emit(t, dt);
        if (!_burstDone && t >= RiffleEnd)
        {
            _burstDone = true;
            Burst();
        }
        UpdateParticles(dt);
    }

    private void UpdateGlow(double t)
    {
        if (_glow is null || _halo is null || _glowT is null) { return; }
        var fadeIn = Smooth(t / GlowIn);
        var fadeOut = 1 - Smooth((t - SettleEnd) / (Duration - SettleEnd));
        var pulse = 1 + 0.08 * Math.Sin(t * Math.PI * 3.2);
        var flare = t > RiffleEnd ? 0.35 * Math.Max(0, 1 - (t - RiffleEnd) / 0.4) : 0;

        _glow.Opacity = Math.Clamp((0.85 + flare) * fadeIn * fadeOut, 0, 1);
        _glowT.ScaleX = _glowT.ScaleY = pulse + flare;
        _halo.Opacity = Math.Clamp(0.7 * fadeIn * fadeOut, 0, 1);
    }

    private void UpdateCards(double t)
    {
        var tr = Math.Clamp((t - RiffleStart) / (RiffleEnd - RiffleStart), 0, 1);
        var local = tr * Riffles;
        var cycle = (int)Math.Min(Math.Floor(local), Riffles - 1);
        var phase = local - cycle;
        var open = t < RiffleStart || t > RiffleEnd ? 0 : Math.Sin(Math.PI * phase);
        var lift = Math.Sin(Math.PI * tr);

        // the settle: a small breath of scale once the riffles close
        var settle = t > RiffleEnd && t < SettleEnd ? Math.Sin(Math.PI * (t - RiffleEnd) / (SettleEnd - RiffleEnd)) : 0;

        for (int k = 0; k < _cards.Count; k++)
        {
            var (frame, tf) = _cards[k];
            var inFirstHalf = k < CardCount / 2;
            var side = inFirstHalf ^ (cycle % 2 == 1) ? -1.0 : 1.0;
            var depth = (k % 4) / 3.0;

            tf.TranslateX = side * _deckW * 0.72 * open * (0.72 + 0.28 * depth);
            tf.TranslateY = -_deckH * 0.09 * open - 16 * lift + depth * 3 * open;
            tf.Rotation = side * (9 + 6 * depth) * open;
            tf.ScaleX = tf.ScaleY = 1 + 0.04 * lift + 0.06 * settle;

            int order;
            if (t < RiffleStart || t > RiffleEnd) { order = k; }
            else { order = Interleave(k, phase < 0.5 ? cycle : cycle + 1); }
            Canvas.SetZIndex(frame, 10 + order);
        }
    }

    /// <summary>A different stacking order for each pass, so the halves really do interleave.</summary>
    private static int Interleave(int k, int pass) => pass == 0 ? k : (k * 3 + pass * 5) % CardCount;

    private void UpdateMotes(double t)
    {
        var visible = Smooth((t - 0.15) / 0.4) * (1 - Smooth((t - RiffleEnd) / 0.5));
        var rx = _deckW * 1.05;
        var ry = _deckH * 0.66;
        for (int i = 0; i < _motes.Count; i++)
        {
            var (dot, tf) = _motes[i];
            var angle = i * Math.PI / 4 + t * Math.PI * 1.6;
            var x = _cx + Math.Cos(angle) * rx;
            var y = _cy + Math.Sin(angle) * ry;
            Canvas.SetLeft(dot, x - dot.Width / 2);
            Canvas.SetTop(dot, y - dot.Height / 2);
            var twinkle = 0.75 + 0.25 * Math.Sin(t * 9 + i * 1.7);
            dot.Opacity = Math.Clamp(visible * twinkle, 0, 1);
            tf.ScaleX = tf.ScaleY = 0.8 + 0.35 * twinkle;
        }
    }

    // ==================================================================== particles

    private void Emit(double t, double dt)
    {
        if (t < RiffleStart || t > RiffleEnd) { return; }
        var tr = (t - RiffleStart) / (RiffleEnd - RiffleStart);
        var phase = tr * Riffles % 1.0;
        var open = Math.Sin(Math.PI * phase);

        // dust pours from the gap between the two halves as they separate
        _dustDebt += dt * (38 + 70 * open);
        while (_dustDebt >= 1)
        {
            _dustDebt -= 1;
            var side = _rng.NextDouble() < 0.5 ? -1 : 1;
            var x = _cx + side * _deckW * (0.15 + 0.55 * open) + Range(-10, 10);
            var y = _cy + Range(-_deckH * 0.45, _deckH * 0.4);
            SpawnDust(x, y, side * Range(8, 46), Range(-78, -22), Range(0.9, 1.9), Range(2.2, 4.6));
        }

        // sparkles blink into being on a loose ring around the deck
        _sparkleDebt += dt * 11;
        while (_sparkleDebt >= 1)
        {
            _sparkleDebt -= 1;
            var angle = Range(0, Math.PI * 2);
            var r = Range(0.55, 1.05);
            var x = _cx + Math.Cos(angle) * _deckW * 1.15 * r;
            var y = _cy + Math.Sin(angle) * _deckH * 0.72 * r;
            SpawnSparkle(x, y, Range(-8, 8), Range(-26, -4), Range(0.45, 0.9), Range(0.55, 1.15));
        }
    }

    private void Burst()
    {
        for (int i = 0; i < 18; i++)
        {
            var angle = i * Math.PI * 2 / 18 + Range(-0.12, 0.12);
            var speed = Range(90, 175);
            SpawnSparkle(_cx + Math.Cos(angle) * _deckW * 0.3, _cy + Math.Sin(angle) * _deckH * 0.25,
                Math.Cos(angle) * speed, Math.Sin(angle) * speed - 20, Range(0.55, 0.95), Range(0.7, 1.35));
        }
        for (int i = 0; i < 56; i++)
        {
            var angle = Range(0, Math.PI * 2);
            var speed = Range(35, 150);
            SpawnDust(_cx + Range(-8, 8), _cy + Range(-8, 8),
                Math.Cos(angle) * speed, Math.Sin(angle) * speed - 25, Range(0.7, 1.4), Range(2.0, 4.8));
        }
    }

    private void SpawnDust(double x, double y, double vx, double vy, double life, double size)
    {
        if (_particles.Count >= MaxParticles) { return; }
        var diameter = size * 3.2;   // the soft edge of the gradient makes the visible core ~size
        var dot = new Ellipse
        {
            Width = diameter,
            Height = diameter,
            Opacity = 0,
            Fill = Radial(DustColours[_rng.Next(DustColours.Length)], 1.0),
        };
        Canvas.SetZIndex(dot, 30);
        _layer.Children.Add(dot);
        _particles.Add(new Particle(dot, null, x, y, vx, vy, life, diameter / 2, false, Range(0, 6.28), 0, 1));
        Place(_particles[^1]);
    }

    private void SpawnSparkle(double x, double y, double vx, double vy, double life, double scale)
    {
        if (_particles.Count >= MaxParticles) { return; }
        var colour = SparkleColours[_rng.Next(SparkleColours.Length)];

        // a soft halo and a four-pointed star, both centred on (0,0) so one transform scales them
        var halo = new Ellipse { Width = 22, Height = 22, Fill = Radial(colour, 0.55) };
        Canvas.SetLeft(halo, -11);
        Canvas.SetTop(halo, -11);
        var star = new Microsoft.UI.Xaml.Shapes.Path
        {
            Data = CardView.ParseGeometry(
                "M 0,-11 C 1.1,-2.4 2.4,-1.1 11,0 C 2.4,1.1 1.1,2.4 0,11 C -1.1,2.4 -2.4,1.1 -11,0 C -2.4,-1.1 -1.1,-2.4 0,-11 Z"),
            Fill = Ornament.Brush(colour),
        };
        var t = new CompositeTransform { ScaleX = 0, ScaleY = 0, Rotation = Range(0, 45) };
        var holder = new Canvas { Width = 0, Height = 0, RenderTransform = t, Opacity = 0 };
        holder.Children.Add(halo);
        holder.Children.Add(star);
        Canvas.SetZIndex(holder, 45);
        _layer.Children.Add(holder);

        _particles.Add(new Particle(holder, t, x, y, vx, vy, life, 0, true, 0, Range(-120, 120), scale));
        Place(_particles[^1]);
    }

    private void UpdateParticles(double dt)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.Age += dt;
            var a = p.Age / p.Life;
            if (a >= 1)
            {
                _layer.Children.Remove(p.Element);
                _particles.RemoveAt(i);
                continue;
            }

            if (p.IsSparkle)
            {
                p.Vx *= 1 - 2.2 * dt;
                p.Vy *= 1 - 2.2 * dt;
                p.X += p.Vx * dt;
                p.Y += p.Vy * dt;
                var twinkle = Math.Sin(Math.PI * a);
                if (p.Transform is not null)
                {
                    p.Transform.ScaleX = p.Transform.ScaleY = twinkle * p.Scale;
                    p.Transform.Rotation += p.Spin * dt;
                }
                p.Element.Opacity = Math.Clamp(twinkle * 1.2, 0, 1);
            }
            else
            {
                // dust is buoyant, drags gently and swirls as it rises
                p.Vy -= 14 * dt;
                p.Vx *= 1 - 0.9 * dt;
                p.X += (p.Vx + Math.Sin(p.Age * 5.5 + p.Seed) * 16) * dt;
                p.Y += p.Vy * dt;
                var fade = a < 0.18 ? a / 0.18 : (1 - a) / 0.82;
                p.Element.Opacity = Math.Clamp(fade * 0.95, 0, 1);
            }

            Place(p);
        }
    }

    private static void Place(Particle p)
    {
        Canvas.SetLeft(p.Element, p.X - p.Radius);
        Canvas.SetTop(p.Element, p.Y - p.Radius);
    }

    // ==================================================================== helpers

    private double Range(double min, double max) => min + _rng.NextDouble() * (max - min);

    private static double Smooth(double x)
    {
        x = Math.Clamp(x, 0, 1);
        return x * x * (3 - 2 * x);
    }

    /// <summary>A glow: the colour at the centre, fading to fully transparent at the rim.</summary>
    private static RadialGradientBrush Radial(string hex, double centreAlpha)
    {
        var centre = Ornament.ToColor(hex);
        centre.A = (byte)Math.Round(255 * Math.Clamp(centreAlpha, 0, 1));
        var mid = centre;
        mid.A = (byte)Math.Round(centre.A * 0.35);
        var rim = centre;
        rim.A = 0;
        var brush = new RadialGradientBrush();
        brush.GradientStops.Add(new GradientStop { Color = centre, Offset = 0 });
        brush.GradientStops.Add(new GradientStop { Color = mid, Offset = 0.45 });
        brush.GradientStops.Add(new GradientStop { Color = rim, Offset = 1 });
        return brush;
    }

    private sealed class Particle
    {
        public Particle(FrameworkElement element, CompositeTransform transform, double x, double y,
                        double vx, double vy, double life, double radius, bool isSparkle,
                        double seed, double spin, double scale)
        {
            Element = element;
            Transform = transform;
            X = x;
            Y = y;
            Vx = vx;
            Vy = vy;
            Life = life;
            Radius = radius;
            IsSparkle = isSparkle;
            Seed = seed;
            Spin = spin;
            Scale = scale;
        }

        public FrameworkElement Element { get; }
        public CompositeTransform Transform { get; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double Age { get; set; }
        public double Life { get; }
        public double Radius { get; }
        public bool IsSparkle { get; }
        public double Seed { get; }
        public double Spin { get; }
        public double Scale { get; }
    }
}
