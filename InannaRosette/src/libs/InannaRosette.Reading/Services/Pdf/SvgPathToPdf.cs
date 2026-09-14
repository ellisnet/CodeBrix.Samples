using System.Globalization;
using CodeBrix.PdfDocuments.Drawing;

namespace InannaRosette.Reading.Services.Pdf;

/// <summary>
/// Converts the SVG path mini-language used by <c>InannaRosette.Reading.Data.EmblemArt</c> into a
/// <see cref="XGraphicsPath"/>, mapping a source design box onto a destination rectangle.
/// </summary>
/// <remarks>
/// <para>
/// Supports every command of the grammar in both absolute and relative form:
/// <c>M m L l H h V v C c S s Q q T t A a Z z</c>, implicit repeated commands (a command letter
/// followed by several coordinate sets), implicit <c>L</c>/<c>l</c> after <c>M</c>/<c>m</c>, and
/// numbers separated by whitespace, commas, or nothing at all when a sign or a second decimal
/// point begins the next number (<c>"10-5"</c>, <c>".5.5"</c>, <c>"1e-3"</c>).
/// </para>
/// <para>
/// Quadratic segments are raised to cubics exactly; elliptical arcs are converted with the
/// endpoint-to-centre parameterisation of the SVG specification (appendix F.6) and emitted as at
/// most four cubic segments of 90 degrees or less.
/// </para>
/// </remarks>
public static class SvgPathToPdf
{
    /// <summary>The design box the emblem path data is authored in.</summary>
    public const double DefaultDesignBox = 100.0;

    /// <summary>
    /// Builds a path from <paramref name="pathData"/>, scaling the square design box
    /// <c>0..designBox</c> so that it fits centred inside <paramref name="destination"/>.
    /// </summary>
    public static XGraphicsPath Build(string pathData, XRect destination, double designBox = DefaultDesignBox)
    {
        var scale = Math.Min(destination.Width / designBox, destination.Height / designBox);
        var offsetX = destination.X + (destination.Width - designBox * scale) / 2.0;
        var offsetY = destination.Y + (destination.Height - designBox * scale) / 2.0;
        return Build(pathData, scale, scale, offsetX, offsetY);
    }

    /// <summary>Builds a path, mapping source point <c>(x, y)</c> to <c>(x * sx + dx, y * sy + dy)</c>.</summary>
    public static XGraphicsPath Build(string pathData, double sx, double sy, double dx, double dy)
    {
        var path = new XGraphicsPath { FillMode = XFillMode.Winding };
        var builder = new Builder(path, sx, sy, dx, dy);
        Parse(pathData, builder);
        return path;
    }

    /// <summary>
    /// Parses <paramref name="pathData"/> and reports the number of drawing segments produced.
    /// Throws <see cref="FormatException"/> on malformed input; used by the self-test.
    /// </summary>
    public static int CountSegments(string pathData)
    {
        var counter = new SegmentCounter();
        Parse(pathData, counter);
        return counter.Segments;
    }

    /// <summary>Receives the flattened path, in source coordinates.</summary>
    private interface ISink
    {
        void MoveTo(double x, double y);
        void LineTo(double x, double y);
        void CurveTo(double c1X, double c1Y, double c2X, double c2Y, double x, double y);
        void Close();
    }

    private sealed class Builder(XGraphicsPath path, double sx, double sy, double dx, double dy) : ISink
    {
        private bool _open;
        private XPoint _current;
        private XPoint _figureStart;

        private XPoint P(double x, double y) => new(x * sx + dx, y * sy + dy);

        public void MoveTo(double x, double y)
        {
            _current = _figureStart = P(x, y);
            path.AddMove(_current.X, _current.Y);
            _open = true;
        }

        /// <summary>
        /// After a close-path the current point stays at the sub-path start and any following
        /// segment opens a fresh sub-path there, so re-issue the move before drawing.
        /// </summary>
        private void EnsureOpen()
        {
            if (_open) return;
            path.AddMove(_current.X, _current.Y);
            _open = true;
        }

        public void LineTo(double x, double y)
        {
            EnsureOpen();
            var next = P(x, y);
            if (next == _current) return;
            path.AddLine(_current, next);
            _current = next;
        }

        public void CurveTo(double c1X, double c1Y, double c2X, double c2Y, double x, double y)
        {
            EnsureOpen();
            path.AddBezier(_current, P(c1X, c1Y), P(c2X, c2Y), P(x, y));
            _current = P(x, y);
        }

        public void Close()
        {
            if (!_open) return;
            path.CloseFigure();
            _open = false;
            _current = _figureStart;
        }
    }

    private sealed class SegmentCounter : ISink
    {
        public int Segments { get; private set; }

        public void MoveTo(double x, double y) => Check(x, y);

        public void LineTo(double x, double y)
        {
            Check(x, y);
            Segments++;
        }

        public void CurveTo(double c1X, double c1Y, double c2X, double c2Y, double x, double y)
        {
            Check(c1X, c1Y);
            Check(c2X, c2Y);
            Check(x, y);
            Segments++;
        }

        public void Close() => Segments++;

        private static void Check(double x, double y)
        {
            if (double.IsNaN(x) || double.IsNaN(y) || double.IsInfinity(x) || double.IsInfinity(y))
            {
                throw new FormatException("Path produced a non-finite coordinate.");
            }
        }
    }

    // -----------------------------------------------------------------------------------------
    // The parser
    // -----------------------------------------------------------------------------------------

    private static void Parse(string pathData, ISink sink)
    {
        if (string.IsNullOrWhiteSpace(pathData)) return;

        var scanner = new Scanner(pathData);

        double curX = 0, curY = 0;      // current point
        double startX = 0, startY = 0;  // start of the current sub-path
        double lastC1X = 0, lastC1Y = 0; // reflected control point for S / s
        double lastQX = 0, lastQY = 0;   // reflected control point for T / t
        var lastWasCubic = false;
        var lastWasQuadratic = false;
        var started = false;

        char command = '\0';

        while (true)
        {
            scanner.SkipSeparators();
            if (scanner.AtEnd) break;

            var c = scanner.Peek;
            if (IsCommand(c))
            {
                command = c;
                scanner.Advance();
            }
            else if (command == '\0')
            {
                throw new FormatException($"Path data must begin with a command; found '{c}'.");
            }
            else if (command is 'M') command = 'L';      // implicit line-to after a move-to
            else if (command is 'm') command = 'l';
            else if (command is 'Z' or 'z')
            {
                throw new FormatException("Unexpected number after a close-path command.");
            }

            var relative = char.IsLower(command);
            var upper = char.ToUpperInvariant(command);

            if (upper == 'Z')
            {
                sink.Close();
                curX = startX;
                curY = startY;
                lastWasCubic = lastWasQuadratic = false;
                continue;
            }

            if (!started && upper != 'M')
            {
                throw new FormatException($"Path data must begin with a move-to; found '{command}'.");
            }

            switch (upper)
            {
                case 'M':
                {
                    var x = scanner.Number();
                    var y = scanner.Number();
                    if (relative && started) { x += curX; y += curY; }
                    curX = startX = x;
                    curY = startY = y;
                    sink.MoveTo(curX, curY);
                    started = true;
                    lastWasCubic = lastWasQuadratic = false;
                    break;
                }

                case 'L':
                {
                    var x = scanner.Number();
                    var y = scanner.Number();
                    if (relative) { x += curX; y += curY; }
                    sink.LineTo(x, y);
                    curX = x; curY = y;
                    lastWasCubic = lastWasQuadratic = false;
                    break;
                }

                case 'H':
                {
                    var x = scanner.Number();
                    if (relative) x += curX;
                    sink.LineTo(x, curY);
                    curX = x;
                    lastWasCubic = lastWasQuadratic = false;
                    break;
                }

                case 'V':
                {
                    var y = scanner.Number();
                    if (relative) y += curY;
                    sink.LineTo(curX, y);
                    curY = y;
                    lastWasCubic = lastWasQuadratic = false;
                    break;
                }

                case 'C':
                {
                    var x1 = scanner.Number(); var y1 = scanner.Number();
                    var x2 = scanner.Number(); var y2 = scanner.Number();
                    var x = scanner.Number(); var y = scanner.Number();
                    if (relative)
                    {
                        x1 += curX; y1 += curY;
                        x2 += curX; y2 += curY;
                        x += curX; y += curY;
                    }

                    sink.CurveTo(x1, y1, x2, y2, x, y);
                    lastC1X = x2; lastC1Y = y2;
                    curX = x; curY = y;
                    lastWasCubic = true; lastWasQuadratic = false;
                    break;
                }

                case 'S':
                {
                    var x2 = scanner.Number(); var y2 = scanner.Number();
                    var x = scanner.Number(); var y = scanner.Number();
                    if (relative) { x2 += curX; y2 += curY; x += curX; y += curY; }

                    // The first control point is the reflection of the previous one; when the
                    // previous command was not a cubic it coincides with the current point.
                    var x1 = lastWasCubic ? 2 * curX - lastC1X : curX;
                    var y1 = lastWasCubic ? 2 * curY - lastC1Y : curY;

                    sink.CurveTo(x1, y1, x2, y2, x, y);
                    lastC1X = x2; lastC1Y = y2;
                    curX = x; curY = y;
                    lastWasCubic = true; lastWasQuadratic = false;
                    break;
                }

                case 'Q':
                {
                    var qx = scanner.Number(); var qy = scanner.Number();
                    var x = scanner.Number(); var y = scanner.Number();
                    if (relative) { qx += curX; qy += curY; x += curX; y += curY; }

                    EmitQuadratic(sink, curX, curY, qx, qy, x, y);
                    lastQX = qx; lastQY = qy;
                    curX = x; curY = y;
                    lastWasQuadratic = true; lastWasCubic = false;
                    break;
                }

                case 'T':
                {
                    var x = scanner.Number(); var y = scanner.Number();
                    if (relative) { x += curX; y += curY; }

                    var qx = lastWasQuadratic ? 2 * curX - lastQX : curX;
                    var qy = lastWasQuadratic ? 2 * curY - lastQY : curY;

                    EmitQuadratic(sink, curX, curY, qx, qy, x, y);
                    lastQX = qx; lastQY = qy;
                    curX = x; curY = y;
                    lastWasQuadratic = true; lastWasCubic = false;
                    break;
                }

                case 'A':
                {
                    var rx = scanner.Number();
                    var ry = scanner.Number();
                    var angle = scanner.Number();
                    var largeArc = scanner.Flag();
                    var sweep = scanner.Flag();
                    var x = scanner.Number();
                    var y = scanner.Number();
                    if (relative) { x += curX; y += curY; }

                    EmitArc(sink, curX, curY, rx, ry, angle, largeArc, sweep, x, y);
                    curX = x; curY = y;
                    lastWasCubic = lastWasQuadratic = false;
                    break;
                }

                default:
                    throw new FormatException($"Unsupported path command '{command}'.");
            }
        }

        // Deliberately no implicit close: an un-terminated sub-path stays open, so stroked
        // figures are not silently joined back to their starting point. A fill closes it anyway.
    }

    private static bool IsCommand(char c) => c is
        'M' or 'm' or 'L' or 'l' or 'H' or 'h' or 'V' or 'v' or
        'C' or 'c' or 'S' or 's' or 'Q' or 'q' or 'T' or 't' or
        'A' or 'a' or 'Z' or 'z';

    private static void EmitQuadratic(ISink sink, double x0, double y0, double qx, double qy, double x, double y)
    {
        // Exact degree elevation from quadratic to cubic.
        var c1X = x0 + 2.0 / 3.0 * (qx - x0);
        var c1Y = y0 + 2.0 / 3.0 * (qy - y0);
        var c2X = x + 2.0 / 3.0 * (qx - x);
        var c2Y = y + 2.0 / 3.0 * (qy - y);
        sink.CurveTo(c1X, c1Y, c2X, c2Y, x, y);
    }

    /// <summary>
    /// Elliptical arc, converted with the endpoint-to-centre parameterisation of SVG 1.1 F.6.5 and
    /// emitted as up to four cubic segments (F.6.2 out-of-range handling included).
    /// </summary>
    private static void EmitArc(
        ISink sink,
        double x1, double y1,
        double rx, double ry,
        double xAxisRotationDegrees,
        bool largeArc, bool sweep,
        double x2, double y2)
    {
        // F.6.2: an arc with coincident endpoints is omitted entirely.
        if (Math.Abs(x1 - x2) < 1e-12 && Math.Abs(y1 - y2) < 1e-12) return;

        // F.6.2: a zero radius degenerates to a straight line.
        rx = Math.Abs(rx);
        ry = Math.Abs(ry);
        if (rx < 1e-12 || ry < 1e-12)
        {
            sink.LineTo(x2, y2);
            return;
        }

        var phi = xAxisRotationDegrees * Math.PI / 180.0;
        var cosPhi = Math.Cos(phi);
        var sinPhi = Math.Sin(phi);

        // Step 1: compute (x1', y1') - the endpoint delta in the rotated, centred frame.
        var dx2 = (x1 - x2) / 2.0;
        var dy2 = (y1 - y2) / 2.0;
        var x1P = cosPhi * dx2 + sinPhi * dy2;
        var y1P = -sinPhi * dx2 + cosPhi * dy2;

        // F.6.6: scale the radii up if they are too small to span the endpoints.
        var lambda = x1P * x1P / (rx * rx) + y1P * y1P / (ry * ry);
        if (lambda > 1.0)
        {
            var s = Math.Sqrt(lambda);
            rx *= s;
            ry *= s;
        }

        // Step 2: compute (cx', cy').
        var rxSq = rx * rx;
        var rySq = ry * ry;
        var x1PSq = x1P * x1P;
        var y1PSq = y1P * y1P;

        var numerator = rxSq * rySq - rxSq * y1PSq - rySq * x1PSq;
        var denominator = rxSq * y1PSq + rySq * x1PSq;
        var factor = denominator <= 0 ? 0 : Math.Sqrt(Math.Max(0, numerator / denominator));
        if (largeArc == sweep) factor = -factor;

        var cxP = factor * rx * y1P / ry;
        var cyP = -factor * ry * x1P / rx;

        // Step 3: back to the original frame.
        var cx = cosPhi * cxP - sinPhi * cyP + (x1 + x2) / 2.0;
        var cy = sinPhi * cxP + cosPhi * cyP + (y1 + y2) / 2.0;

        // Step 4: the start angle and the sweep.
        var theta1 = Angle(1, 0, (x1P - cxP) / rx, (y1P - cyP) / ry);
        var deltaTheta = Angle((x1P - cxP) / rx, (y1P - cyP) / ry, (-x1P - cxP) / rx, (-y1P - cyP) / ry);

        const double twoPi = Math.PI * 2;
        if (!sweep && deltaTheta > 0) deltaTheta -= twoPi;
        else if (sweep && deltaTheta < 0) deltaTheta += twoPi;

        // Emit in pieces of at most 90 degrees; the cubic approximation of a circular arc is
        // excellent below that and the error grows quickly above it.
        var pieces = Math.Max(1, (int)Math.Ceiling(Math.Abs(deltaTheta) / (Math.PI / 2) - 1e-9));
        var delta = deltaTheta / pieces;
        var t = 4.0 / 3.0 * Math.Tan(delta / 4.0);

        var theta = theta1;
        for (var i = 0; i < pieces; i++)
        {
            var cosT1 = Math.Cos(theta);
            var sinT1 = Math.Sin(theta);
            var theta2 = theta + delta;
            var cosT2 = Math.Cos(theta2);
            var sinT2 = Math.Sin(theta2);

            // Points and derivatives on the unit circle, mapped through the ellipse.
            var e1X = cx + rx * cosPhi * cosT1 - ry * sinPhi * sinT1;
            var e1Y = cy + rx * sinPhi * cosT1 + ry * cosPhi * sinT1;
            var e2X = cx + rx * cosPhi * cosT2 - ry * sinPhi * sinT2;
            var e2Y = cy + rx * sinPhi * cosT2 + ry * cosPhi * sinT2;

            var d1X = -rx * cosPhi * sinT1 - ry * sinPhi * cosT1;
            var d1Y = -rx * sinPhi * sinT1 + ry * cosPhi * cosT1;
            var d2X = -rx * cosPhi * sinT2 - ry * sinPhi * cosT2;
            var d2Y = -rx * sinPhi * sinT2 + ry * cosPhi * cosT2;

            sink.CurveTo(
                e1X + t * d1X, e1Y + t * d1Y,
                e2X - t * d2X, e2Y - t * d2Y,
                e2X, e2Y);

            theta = theta2;
        }
    }

    /// <summary>Signed angle from vector (ux, uy) to vector (vx, vy), in radians.</summary>
    private static double Angle(double ux, double uy, double vx, double vy)
    {
        var dot = ux * vx + uy * vy;
        var len = Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
        if (len <= 0) return 0;
        var value = Math.Clamp(dot / len, -1.0, 1.0);
        var angle = Math.Acos(value);
        return ux * vy - uy * vx < 0 ? -angle : angle;
    }

    /// <summary>
    /// Tokenises the path string. Numbers may be separated by whitespace, commas, or nothing at
    /// all where a sign or a second decimal point unambiguously begins the next number.
    /// </summary>
    private struct Scanner(string text)
    {
        private int _i = 0;

        public readonly bool AtEnd => _i >= text.Length;

        public readonly char Peek => text[_i];

        public void Advance() => _i++;

        public void SkipSeparators()
        {
            while (_i < text.Length && (char.IsWhiteSpace(text[_i]) || text[_i] is ',')) _i++;
        }

        /// <summary>Reads one number, tolerating exponents and omitted separators.</summary>
        public double Number()
        {
            SkipSeparators();
            if (_i >= text.Length) throw new FormatException("Path data ended while a number was expected.");

            var start = _i;
            var sawDigit = false;
            var sawDot = false;

            if (text[_i] is '+' or '-') _i++;

            while (_i < text.Length)
            {
                var c = text[_i];
                if (char.IsAsciiDigit(c)) { sawDigit = true; _i++; }
                else if (c == '.' && !sawDot) { sawDot = true; _i++; }
                else if ((c is 'e' or 'E') && sawDigit &&
                         _i + 1 < text.Length &&
                         (char.IsAsciiDigit(text[_i + 1]) ||
                          (text[_i + 1] is '+' or '-' && _i + 2 < text.Length && char.IsAsciiDigit(text[_i + 2]))))
                {
                    _i += 2;
                    while (_i < text.Length && char.IsAsciiDigit(text[_i])) _i++;
                    break;
                }
                else break;
            }

            if (!sawDigit)
            {
                throw new FormatException($"Expected a number at offset {start} in path data.");
            }

            var span = text.AsSpan(start, _i - start);
            if (!double.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                throw new FormatException($"'{span}' is not a valid number in path data.");
            }

            return value;
        }

        /// <summary>
        /// Reads an arc flag. The grammar allows these to be written without separators
        /// (<c>"a5 5 0 1150 20"</c>), so exactly one character is consumed.
        /// </summary>
        public bool Flag()
        {
            SkipSeparators();
            if (_i >= text.Length) throw new FormatException("Path data ended while an arc flag was expected.");
            var c = text[_i];
            if (c is not ('0' or '1')) throw new FormatException($"Arc flag must be 0 or 1; found '{c}'.");
            _i++;
            return c == '1';
        }
    }
}
