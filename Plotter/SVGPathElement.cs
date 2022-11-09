using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoDimensionLib;

namespace Plotter;

public class SVGPathElement
{
    public StrokeType StrokeType { get; private set; }
    public bool Relative { get; private set; }

    private readonly Coordinate[] points;

    public IList<Coordinate> Points => points;

    private SVGPathElement(StrokeType type, bool rel)
    {
        StrokeType = type;
        Relative = rel;
        if (type == StrokeType.Cubic)
            points = new Coordinate[3];
        else if (type == StrokeType.Quadratic)
            points = new Coordinate[2];
        else if (type == StrokeType.Arc)
            points = new Coordinate[3];
        else if (type == StrokeType.Z)
            points = Array.Empty<Coordinate>();
        else
            points = new Coordinate[1];
    }

    public static SVGPathElement MoveTo(Coordinate p, bool rel = false)
    {
        SVGPathElement element = new(StrokeType.Move, rel);
        element.points[0] = p;
        return element;
    }

    public static SVGPathElement MoveTo(double x, double y)
        => MoveTo(new Coordinate(x, y));

    public static SVGPathElement MoveRel(Coordinate dp)
        => MoveTo(dp, true);

    public static SVGPathElement MoveRel(double dx, double dy)
        => MoveTo(new Coordinate(dx, dy), true);

    public static SVGPathElement LineTo(Coordinate p, bool rel = false)
    {
        SVGPathElement element = new(StrokeType.Line, rel);
        element.points[0] = p;
        return element;
    }

    public static SVGPathElement LineTo(double x, double y)
        => LineTo(new Coordinate(x, y));

    public static SVGPathElement LineRel(Coordinate dp)
        => LineTo(dp, true);

    public static SVGPathElement LineRel(double dx, double dy)
        => LineTo(new Coordinate(dx, dy), true);

    public static SVGPathElement Close() => new(StrokeType.Z, false);

    public static SVGPathElement Cubic
        (double cx1, double cy1, double cx2, double cy2, double x, double y, bool rel = false)
        => Cubic(new Coordinate(cx1, cy1), new Coordinate(cx2, cy2), new Coordinate(x, y), rel);

    public static SVGPathElement Cubic
        (Coordinate c1, Coordinate c2, Coordinate end, bool rel = false)
    {
        SVGPathElement element = new(StrokeType.Cubic, rel);
        element.points[0] = c1;
        element.points[1] = c2;
        element.points[2] = end;
        return element;
    }

    public static SVGPathElement CubicRel
        (double dx1, double dy1, double dx2, double dy2, double dx, double dy)
        => Cubic(dx1, dy1, dx2, dy2, dx, dy, true);

    public static SVGPathElement CubicRel
        (Coordinate dc1, Coordinate dc2, Coordinate dEnd)
        => Cubic(dc1, dc2, dEnd, true);

    public static SVGPathElement Quadratic
        (double cx1, double cy1, double x, double y, bool rel = false)
        => Quadratic(new Coordinate(cx1, cy1), new Coordinate(x, y), rel);

    public static SVGPathElement Quadratic(Coordinate c1, Coordinate end, bool rel = false)
    {
        SVGPathElement element = new(StrokeType.Cubic, rel);
        element.points[0] = c1;
        element.points[1] = end;
        return element;
    }

    public static SVGPathElement QuadraticRel(Coordinate c1, Coordinate end)
        => Quadratic(c1, end, true);

    public static SVGPathElement QuadraticRel(double dx1, double dy1, double dx, double dy)
        => Quadratic(dx1, dy1, dx, dy, true);

    public static SVGPathElement Arc
        (double rx, double ry, double angle, bool largeArc,
        bool sweep, double x, double y, bool rel = false)
        => Arc(new Coordinate(rx, ry), angle, largeArc, sweep, new Coordinate(x, y), rel);

    public static SVGPathElement Arc
        (Coordinate radii, double angle, bool largeArc, bool sweep, Coordinate end, bool rel = false)
    {
        SVGPathElement element = new(StrokeType.Arc, rel);
        element.points[0] = radii;
        element.points[1] = new Coordinate(angle, (largeArc ? 1.0f : 0.0f) + (sweep ? 2.0f : 0.0f)); // UGH!
        element.points[2] = end;
        return element;
    }
    public static SVGPathElement ArcRel
        (Coordinate radii, double angle, bool largeArc, bool sweep, Coordinate end)
        => Arc(radii, angle, largeArc, sweep, end, true);

    public static SVGPathElement ArcRel
        (double rx, double ry, double angle, bool largeArc, bool sweep, double dx, double dy)
        => Arc(rx, ry, angle, largeArc, sweep, dx, dy, true);

    private const string TypeStrings = "MLCQAZ";
    private const string RelTypeStrings = "mlcqaz";
    private static readonly string[] ArcStrings = { "0,0", "1,0", "0,1", "1,1" };

    public override string ToString()
    {
        string typeString = Relative ? RelTypeStrings : TypeStrings;
        string result = typeString[(int)StrokeType].ToString();
        switch (StrokeType)
        {
            case StrokeType.Move:
            case StrokeType.Line:
                result += RenderPoint(points[0]);
                break;
            case StrokeType.Quadratic:
                result += RenderPoints(points, 2);
                break;
            case StrokeType.Cubic:
                result += RenderPoints(points, 3);
                break;
            case StrokeType.Arc:
                result += RenderPoint(points[0]);
                result += $",{points[1].X:F2},{ArcStrings[(int)points[1].Y]},";
                result += RenderPoint(points[2]);
                break;
        }
        return result;
    }

    public Rectangle BoundingBox()
    {
        double xMax = points.Select(p => p.X).Max();
        double xMin = points.Select(p => p.X).Min();
        double yMax = points.Select(p => p.Y).Max();
        double yMin = points.Select(p => p.Y).Min();
        return new(new(xMin, yMin), xMax - xMin, yMax - yMin);
    }

    /// <summary>
    /// Calculate the width and height of one quarter of the bounding box
    /// surrounding a rotated ellipse
    /// </summary>
    /// <param name="rx">X axis radius of unrotated ellipse</param>
    /// <param name="ry">Y axis radius of unrotated ellipse</param>
    /// <param name="angle">The anticlockwise angle through which
    /// the ellipse has been rotated (radians)</param>
    /// <returns>The width and height of one quarter of the bounding box
    /// surrounding the rotated ellipse</returns>

    private static Coordinate EllipseQuarterBounds(float rx, float ry, float angle)
    {
        var cosAngle = Math.Cos(angle);
        var sinAngle = Math.Sin(angle);
        var xr = Math.Sqrt(Geometry.Square(rx * cosAngle) + Geometry.Square(ry * sinAngle));
        var yr = Math.Sqrt(Geometry.Square(rx * sinAngle) + Geometry.Square(ry * cosAngle));
        return new Coordinate(xr, yr);
    }

    private static string RenderPoint(Coordinate p) => $"{p.X:F3},{p.Y:F3}";

    private static string RenderPoints(IEnumerable<Coordinate> pe, int count)
        => string.Join(",", pe.Take(count).Select(p => RenderPoint(p)));
}
