using InannaOctaquetra.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace InannaOctaquetra.Views;

/// <summary>
/// A living night sky over the velvet: pinpoint stars that twinkle, four-pointed gold sparkles
/// that breathe, glints that bloom and vanish at random, and gold dust drifting upward, all in
/// the same vocabulary as the shuffle ritual. One gentle timer drives every layer it is given;
/// nothing it draws can be clicked.
/// </summary>
internal sealed class Starfield
{
    private const string SparkleGeometry =
        "M 0,-11 C 1.1,-2.4 2.4,-1.1 11,0 C 2.4,1.1 1.1,2.4 0,11 C -1.1,2.4 -2.4,1.1 -11,0 C -2.4,-1.1 -1.1,-2.4 0,-11 Z";

    private readonly Random _rng = new(1224);
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly List<Field> _fields = new();
    private Microsoft.UI.Dispatching.DispatcherQueueTimer _timer;
    private double _last;

    private sealed class Twinkler
    {
        public required FrameworkElement Element { get; init; }
        public CompositeTransform Transform { get; init; }
        public double Base { get; init; }
        public double Depth { get; init; }
        public double Speed { get; init; }
        public double Phase { get; init; }
        public double Scale { get; init; } = 1;
    }

    private sealed class Mote
    {
        public required Ellipse Dot { get; init; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; init; }
        public double Sway { get; init; }
        public double Phase { get; init; }
    }

    private sealed class Glint
    {
        public required Canvas Holder { get; init; }
        public required CompositeTransform Transform { get; init; }
        public double Age { get; set; } = 99;
        public double Life { get; set; }
        public double Size { get; set; }
    }

    private sealed class Field
    {
        public required Canvas Layer { get; init; }
        public required double Density { get; init; }
        public required bool Dust { get; init; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<Twinkler> Twinklers { get; } = new();
        public List<Mote> Motes { get; } = new();
        public List<Glint> Glints { get; } = new();
        public double NextGlint { get; set; }
    }

    // ------------------------------------------------------------------ ducking

    private double _quietUntil;
    private int _holds;

    /// <summary>
    /// When true, the sky steps aside for the animations a person is watching: while it is ducked
    /// (<see cref="Duck"/>) or held (<see cref="Hold"/>) its stars stay as they are and its timer does
    /// no work, so a shuffle, a deal or a card turning over gets the whole UI thread. The sky is
    /// decoration; those are not. Off by default; the page turns it on where the UI thread is
    /// shared with a lot of native drawing (Android).
    /// </summary>
    public bool DuckingEnabled { get; set; }

    /// <summary>Pauses the sky for at least <paramref name="duration"/> from now (ducks overlap; the latest end wins).</summary>
    /// <param name="duration">How long the foreground animation that asked will run.</param>
    public void Duck(TimeSpan duration)
    {
        var until = _clock.Elapsed.TotalSeconds + duration.TotalSeconds;
        if (until > _quietUntil) { _quietUntil = until; }
    }

    /// <summary>
    /// Pauses the sky until the returned token is disposed - for a foreground animation whose length
    /// is not known in advance (a shuffle, a blessing, a drag). Holds nest.
    /// </summary>
    /// <returns>A token; dispose it once the animation is over (disposing twice does nothing).</returns>
    public IDisposable Hold()
    {
        _holds++;
        return new HoldToken(this);
    }

    private bool IsQuiet => DuckingEnabled && (_holds > 0 || _clock.Elapsed.TotalSeconds < _quietUntil);

    private sealed class HoldToken : IDisposable
    {
        private Starfield _owner;

        public HoldToken(Starfield owner) => _owner = owner;

        public void Dispose()
        {
            if (_owner is null) { return; }
            _owner._holds = Math.Max(0, _owner._holds - 1);
            _owner = null;
        }
    }

    /// <summary>Scatter a sky across <paramref name="layer"/>; <paramref name="density"/> 1 is the full sky.</summary>
    public void Attach(Canvas layer, double density = 1.0, bool dust = true)
    {
        var field = new Field { Layer = layer, Density = density, Dust = dust };
        _fields.Add(field);
        layer.SizeChanged += (_, e) => Populate(field, e.NewSize.Width, e.NewSize.Height);
        if (layer.ActualWidth > 0) { Populate(field, layer.ActualWidth, layer.ActualHeight); }
        EnsureTimer(layer);
    }

    /// <summary>Size a field explicitly (for canvases that do not report their own size).</summary>
    public void Resize(Canvas layer, double width, double height)
    {
        foreach (var field in _fields)
        {
            if (ReferenceEquals(field.Layer, layer)) { Populate(field, width, height); }
        }
    }

    /// <summary>
    /// Stop the sky: the timer is stopped and every field is let go of, so nothing keeps ticking
    /// (or keeps the page alive) once the page has unloaded.
    /// </summary>
    public void Stop()
    {
        try { _timer?.Stop(); }
        catch (Exception) { /* the dispatcher may already be gone */ }
        _timer = null;

        foreach (var field in _fields)
        {
            try { field.Layer.Children.Clear(); }
            catch (Exception) { /* the layer may already be out of the tree */ }
        }
        _fields.Clear();
    }

    private void EnsureTimer(Canvas layer)
    {
        if (_timer is not null) { return; }
        try
        {
            _timer = layer.DispatcherQueue?.CreateTimer();
            if (_timer is null) { return; }
            _timer.Interval = TimeSpan.FromMilliseconds(40);
            _timer.Tick += (_, _) => Tick();
            _timer.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Starfield timer unavailable: {ex.Message}");
        }
    }

    private void Populate(Field field, double width, double height)
    {
        if (width < 20 || height < 20) { return; }
        if (Math.Abs(width - field.Width) < 1 && Math.Abs(height - field.Height) < 1 && field.Twinklers.Count > 0) { return; }
        field.Width = width;
        field.Height = height;
        field.Layer.Children.Clear();
        field.Twinklers.Clear();
        field.Motes.Clear();
        field.Glints.Clear();

        var area = width * height / (1440.0 * 900.0);
        int stars = (int)(150 * area * field.Density);
        int sparkles = (int)(30 * area * field.Density);
        int motes = field.Dust ? (int)(26 * area * field.Density) : 0;

        string[] starColours = { "#FFF8E6", "#F3DFA2", "#E6C476", "#E7D2FF", "#C9A8FF" };
        for (int i = 0; i < stars; i++)
        {
            var size = 1.1 + Math.Pow(_rng.NextDouble(), 3) * 2.8;
            var dot = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = Ornament.Brush(starColours[_rng.Next(starColours.Length)]),
                IsHitTestVisible = false,
            };
            Canvas.SetLeft(dot, _rng.NextDouble() * width);
            Canvas.SetTop(dot, _rng.NextDouble() * height);
            field.Layer.Children.Add(dot);
            field.Twinklers.Add(new Twinkler
            {
                Element = dot,
                Base = 0.55 + _rng.NextDouble() * 0.4,
                Depth = 0.3 + _rng.NextDouble() * 0.5,
                Speed = 0.6 + _rng.NextDouble() * 2.4,
                Phase = _rng.NextDouble() * Math.PI * 2,
            });
        }

        for (int i = 0; i < sparkles; i++)
        {
            var scale = 0.34 + Math.Pow(_rng.NextDouble(), 2) * 0.7;
            var (holder, t) = NewSparkle(_rng.NextDouble() < 0.25 ? "#E7D2FF" : "#F3DFA2", scale);
            Canvas.SetLeft(holder, _rng.NextDouble() * width);
            Canvas.SetTop(holder, _rng.NextDouble() * height);
            field.Layer.Children.Add(holder);
            field.Twinklers.Add(new Twinkler
            {
                Element = holder,
                Transform = t,
                Base = 0.62,
                Depth = 0.7,
                Speed = 0.5 + _rng.NextDouble() * 1.3,
                Phase = _rng.NextDouble() * Math.PI * 2,
                Scale = scale,
            });
        }

        for (int i = 0; i < motes; i++)
        {
            var size = 1.6 + _rng.NextDouble() * 2.6;
            var dot = new Ellipse
            {
                Width = size * 3,
                Height = size * 3,
                Fill = Glow(_rng.NextDouble() < 0.7 ? "#F3DFA2" : "#D9B8FF"),
                Opacity = 0,
                IsHitTestVisible = false,
            };
            field.Layer.Children.Add(dot);
            field.Motes.Add(new Mote
            {
                Dot = dot,
                X = _rng.NextDouble() * width,
                Y = _rng.NextDouble() * height,
                Speed = 6 + _rng.NextDouble() * 16,
                Sway = 4 + _rng.NextDouble() * 12,
                Phase = _rng.NextDouble() * Math.PI * 2,
            });
        }

        for (int i = 0; i < Math.Max(1, (int)(3 * field.Density)); i++)
        {
            var (holder, t) = NewSparkle("#FFF8E6", 1);
            holder.Opacity = 0;
            field.Layer.Children.Add(holder);
            field.Glints.Add(new Glint { Holder = holder, Transform = t });
        }
        field.NextGlint = 0.5 + _rng.NextDouble();
    }

    private void Tick()
    {
        var now = _clock.Elapsed.TotalSeconds;
        var dt = Math.Clamp(now - _last, 0, 0.1);
        _last = now;

        // ducked: something the person is watching has the UI thread; the stars wait
        if (IsQuiet) { return; }

        foreach (var field in _fields)
        {
            try { Animate(field, now, dt); }
            catch (Exception) { /* a layer being rebuilt mid-frame; the next tick catches up */ }
        }
    }

    private void Animate(Field field, double now, double dt)
    {
        foreach (var t in field.Twinklers)
        {
            var wave = Math.Sin(now * t.Speed + t.Phase);
            // a sharper peak reads as a twinkle rather than a pulse
            var peak = Math.Pow((wave + 1) / 2, 3);
            t.Element.Opacity = Math.Clamp(t.Base - t.Depth / 2 + t.Depth * peak, 0.04, 1);
            if (t.Transform is not null)
            {
                t.Transform.ScaleX = t.Transform.ScaleY = t.Scale * (0.55 + 0.6 * peak);
                t.Transform.Rotation = (now * 8 + t.Phase * 20) % 360;
            }
        }

        foreach (var m in field.Motes)
        {
            m.Y -= m.Speed * dt;
            if (m.Y < -10)
            {
                m.Y = field.Height + 10;
                m.X = _rng.NextDouble() * field.Width;
            }
            var x = m.X + Math.Sin(now * 0.7 + m.Phase) * m.Sway;
            Canvas.SetLeft(m.Dot, x - m.Dot.Width / 2);
            Canvas.SetTop(m.Dot, m.Y - m.Dot.Height / 2);
            var edgeFade = Math.Clamp(Math.Min(m.Y, field.Height - m.Y) / 80, 0, 1);
            m.Dot.Opacity = edgeFade * (0.35 + 0.35 * Math.Sin(now * 1.3 + m.Phase * 3));
        }

        field.NextGlint -= dt;
        if (field.NextGlint <= 0)
        {
            field.NextGlint = 0.9 + _rng.NextDouble() * 1.8 / Math.Max(0.3, field.Density);
            foreach (var g in field.Glints)
            {
                if (g.Age < g.Life) { continue; }
                g.Age = 0;
                g.Life = 0.8 + _rng.NextDouble() * 0.6;
                g.Size = 0.6 + _rng.NextDouble() * 0.9;
                Canvas.SetLeft(g.Holder, _rng.NextDouble() * field.Width);
                Canvas.SetTop(g.Holder, _rng.NextDouble() * field.Height);
                break;
            }
        }
        foreach (var g in field.Glints)
        {
            if (g.Age >= g.Life) { g.Holder.Opacity = 0; continue; }
            g.Age += dt;
            var a = Math.Clamp(g.Age / g.Life, 0, 1);
            var bloom = Math.Sin(Math.PI * a);
            g.Holder.Opacity = bloom;
            g.Transform.ScaleX = g.Transform.ScaleY = g.Size * bloom;
            g.Transform.Rotation = 45 * a;
        }
    }

    // ==================================================================== sparkle bursts

    /// <summary>
    /// A small celebration of light where a card comes to rest: a ring of sparkles flies out and
    /// fades while gold dust drifts up, in the same style as the shuffle ritual.
    /// </summary>
    public static void Burst(Canvas layer, double x, double y, double radius)
    {
        var queue = layer.DispatcherQueue;
        if (queue is null) { return; }
        var rng = new Random(unchecked((int)(x * 31 + y * 17 + Environment.TickCount)));
        var parts = new List<(FrameworkElement E, CompositeTransform T, double Vx, double Vy, double Life, double Scale, bool Star)>();

        for (int i = 0; i < 10; i++)
        {
            var angle = i * Math.PI * 2 / 10 + rng.NextDouble() * 0.4;
            var speed = radius * (1.2 + rng.NextDouble() * 1.1);
            var scale = 0.35 + rng.NextDouble() * 0.45;
            var (holder, t) = NewSparkle(rng.NextDouble() < 0.3 ? "#E7D2FF" : "#FFF3C4", scale);
            holder.Opacity = 0;
            Canvas.SetLeft(holder, x);
            Canvas.SetTop(holder, y);
            layer.Children.Add(holder);
            parts.Add((holder, t, Math.Cos(angle) * speed, Math.Sin(angle) * speed, 0.7 + rng.NextDouble() * 0.4, scale, true));
        }
        for (int i = 0; i < 18; i++)
        {
            var angle = rng.NextDouble() * Math.PI * 2;
            var speed = radius * (0.3 + rng.NextDouble() * 0.9);
            var size = 3 + rng.NextDouble() * 5;
            var dot = new Ellipse { Width = size, Height = size, Fill = Glow("#F3DFA2"), Opacity = 0, IsHitTestVisible = false };
            Canvas.SetLeft(dot, x);
            Canvas.SetTop(dot, y);
            layer.Children.Add(dot);
            parts.Add((dot, null, Math.Cos(angle) * speed, Math.Sin(angle) * speed - radius * 0.4, 0.8 + rng.NextDouble() * 0.6, size, false));
        }

        var clock = Stopwatch.StartNew();
        Microsoft.UI.Dispatching.DispatcherQueueTimer timer;
        try { timer = queue.CreateTimer(); }
        catch (Exception) { foreach (var p in parts) { layer.Children.Remove(p.E); } return; }
        timer.Interval = TimeSpan.FromMilliseconds(16);
        timer.Tick += (tm, _) =>
        {
            var t = clock.Elapsed.TotalSeconds;
            var alive = false;
            foreach (var p in parts)
            {
                var a = t / p.Life;
                if (a >= 1) { p.E.Opacity = 0; continue; }
                alive = true;
                var ease = 1 - Math.Pow(1 - a, 2);
                var px = x + p.Vx * ease * 0.6;
                var py = y + p.Vy * ease * 0.6 - (p.Star ? 0 : 18 * a);
                if (p.Star)
                {
                    Canvas.SetLeft(p.E, px);
                    Canvas.SetTop(p.E, py);
                    var bloom = Math.Sin(Math.PI * a);
                    p.E.Opacity = bloom;
                    if (p.T is not null)
                    {
                        p.T.ScaleX = p.T.ScaleY = p.Scale * (0.4 + bloom);
                        p.T.Rotation = 90 * a;
                    }
                }
                else
                {
                    Canvas.SetLeft(p.E, px - p.Scale / 2);
                    Canvas.SetTop(p.E, py - p.Scale / 2);
                    p.E.Opacity = Math.Sin(Math.PI * a) * 0.9;
                }
            }
            if (!alive)
            {
                tm.Stop();
                foreach (var p in parts) { layer.Children.Remove(p.E); }
            }
        };
        timer.Start();
    }

    // ==================================================================== shapes

    private static (Canvas Holder, CompositeTransform Transform) NewSparkle(string colour, double scale)
    {
        var halo = new Ellipse { Width = 26, Height = 26, Fill = Glow(colour, 0.55), IsHitTestVisible = false };
        Canvas.SetLeft(halo, -13);
        Canvas.SetTop(halo, -13);
        var star = new Microsoft.UI.Xaml.Shapes.Path
        {
            Data = CardView.ParseGeometry(SparkleGeometry),
            Fill = Ornament.Brush(colour),
            IsHitTestVisible = false,
        };
        var transform = new CompositeTransform { ScaleX = scale, ScaleY = scale };
        var holder = new Canvas { Width = 0, Height = 0, RenderTransform = transform, IsHitTestVisible = false };
        holder.Children.Add(halo);
        holder.Children.Add(star);
        return (holder, transform);
    }

    private static RadialGradientBrush Glow(string hex, double centreAlpha = 1.0)
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
}
