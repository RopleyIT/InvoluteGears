using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwoDimensionLib;

namespace Plotter;

public enum StrokeType
{
    Move,
    Line,
    Cubic,
    Quadratic,
    Arc,
    Z
};

public class SVGPath : IRenderable
{
    /// <summary>
    /// The pen or line colour for the path or shape
    /// </summary>

    public string Stroke { get; set; } = String.Empty;

    /// <summary>
    /// The thickness of the line
    /// </summary>

    public string StrokeWidth { get; set; } = String.Empty;

    /// <summary>
    /// The fill colour
    /// </summary>

    public string Fill { get; set; } = String.Empty;

    public LineCap Cap { get; set; } = LineCap.None;

    public LineJoin Join { get; set; } = LineJoin.None;

    /// <summary>
    /// The lengths of the dashes and gaps for a line pattern
    /// </summary>

    public IEnumerable<int> Dashes { get; set; }

    /// <summary>
    /// Alternative helper function for setting the dash lengths
    /// </summary>
    /// <param name="dashes">Sequence of integers for dash lengths
    /// </param>

    public void SetDashes(params int[] dashes)
        => Dashes = dashes;

    /// <summary>
    /// Create a closed or open path using a sequence of points
    /// </summary>
    /// <param name="points">The sequence of points to be joined
    /// using straight line segments</param>
    /// <param name="closed">True to close the 
    /// last point back to the first with a line</param>
    /// <returns>The path created</returns>

    public static SVGPath FromPoints(IEnumerable<Coordinate> points, bool closed)
        => new(points, closed);

    public IList<SVGPathElement> Elements { get; private set; }
        = new List<SVGPathElement>();

    public SVGPath() { }

    public SVGPath(IEnumerable<Coordinate> points, bool closed)
    {
        if (points == null || !points.Any())
            throw new ArgumentException("No points for SVG path");

        Coordinate prev = points.First();
        Elements.Add(SVGPathElement.MoveTo(prev));
        foreach (var p in points.Skip(1))
        {
            if (p != prev)
                Elements.Add(SVGPathElement.LineTo(p));
            prev = p;
        }
        if (closed)
            Elements.Add(SVGPathElement.Close());
    }

    public void SetDrawingParams(string strokeColour, double strokeWidth, string fillColour)
    {
        Stroke = strokeColour;
        StrokeWidth = strokeWidth.ToString();
        Fill = fillColour;
    }

    public void MoveTo(Coordinate p)
    => Elements.Add(SVGPathElement.MoveTo(p.X, p.Y));

    public void MoveRel(double dx, double dy)
        => Elements.Add(SVGPathElement.MoveRel(dx, dy));

    public void LineTo(Coordinate p)
        => Elements.Add(SVGPathElement.LineTo(p));

    public void LineTo(double x, double y)
        => Elements.Add(SVGPathElement.LineTo(x, y));

    public void LineRel(double dx, double dy)
        => Elements.Add(SVGPathElement.LineRel(dx, dy));

    public void Close()
        => Elements.Add(SVGPathElement.Close());

    public void Cubic(double cx1, double cy1, double cx2, double cy2, double x, double y)
        => Elements.Add(SVGPathElement.Cubic(cx1, cy1, cx2, cy2, x, y));

    public void CubicRel(double dx1, double dy1, double dx2, double dy2, double dx, double dy)
        => Elements.Add(SVGPathElement.CubicRel(dx1, dy1, dx2, dy2, dx, dy));

    public void Quadratic(double cx1, double cy1, double x, double y)
        => Elements.Add(SVGPathElement.Quadratic(cx1, cy1, x, y));

    public void QuadraticRel(double dx1, double dy1, double dx, double dy)
        => Elements.Add(SVGPathElement.QuadraticRel(dx1, dy1, dx, dy));

    public void Arc(double rx, double ry, double angle, bool largeArc, bool sweep, double x, double y)
        => Elements.Add(SVGPathElement.Arc(rx, ry, angle, largeArc, sweep, x, y));

    public void ArcRel(double rx, double ry, double angle, bool largeArc, bool sweep, double dx, double dy)
        => Elements.Add(SVGPathElement.ArcRel(rx, ry, angle, largeArc, sweep, dx, dy));

    public Rectangle BoundingBox()
    {
        Rectangle bbox = Rectangle.Empty;
        var elementsWithPoints =
            from e in Elements
            where e.Points != null && e.Points.Count > 0
            select e;

        if (elementsWithPoints.Any())
        {
            bbox = elementsWithPoints.First().BoundingBox();
            foreach (SVGPathElement e in elementsWithPoints.Skip(1))
                bbox = SVGPath.Union(bbox, e.BoundingBox());
        }
        return bbox;
    }

    public static Rectangle Union(Rectangle r, Rectangle s)
    {
        double xMax = Math.Max(r.Right, s.Right);
        double xMin = Math.Min(r.Left, s.Left);
        double yMax = Math.Max(r.Bottom, s.Bottom);
        double yMin = Math.Min(r.Top, s.Top);
        return new Rectangle
            (new(xMin, yMin), xMax - xMin, yMax - yMin);
    }

    public override string ToString()
    {
        StringWriter sw = new();
        sw.Write("<path d=\"");
        int i = 0;
        foreach (SVGPathElement pe in Elements)
        {
            sw.Write(pe.ToString());
            if (++i % 10 == 0)
                sw.WriteLine();
        }
        sw.Write("\"");
        sw.Write(RenderStye());
        sw.WriteLine("/>");
        return sw.ToString();
    }

    public string RenderStye()
    {
        StringWriter sw = new();
        if (!string.IsNullOrEmpty(Stroke))
            sw.Write($" stroke=\"{Stroke}\"");
        if (!string.IsNullOrEmpty(Fill))
            sw.Write($" fill=\"{Fill}\"");
        if (!string.IsNullOrEmpty(StrokeWidth))
            sw.Write($" stroke-width=\"{StrokeWidth}\"");
        var strCap = Cap switch
        {
            LineCap.Round => "round",
            LineCap.Square => "square",
            LineCap.Butt => "butt",
            _ => ""
        };
        if (!string.IsNullOrEmpty(strCap))
            sw.Write($" stroke-linecap=\"{strCap}\"");
        var strJoin = Join switch
        {
            LineJoin.Round => "round",
            LineJoin.Mitre => "mitre",
            LineJoin.Bevel => "bevel",
            _ => ""
        };
        if (!string.IsNullOrEmpty(strJoin))
            sw.Write($" stroke-linejoin=\"{strJoin}\"");
        if (Dashes != null && Dashes.Any())
        {
            sw.Write($" stroke-dasharray=\"{Dashes.First()}");
            foreach (int i in Dashes.Skip(1))
                sw.Write($",{i}");
            sw.Write("\"");
        }
        return sw.ToString();
    }
}

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

    public static SVGPathElement MoveTo(Coordinate p)
        => MoveTo(p.X, p.Y);

    public static SVGPathElement MoveTo(double x, double y)
    {
        SVGPathElement element = new(StrokeType.Move, false);
        element.points[0] = new Coordinate(x, y);
        return element;
    }

    public static SVGPathElement MoveRel(double dx, double dy)
    {
        SVGPathElement element = new(StrokeType.Move, true);
        element.points[0] = new Coordinate(dx, dy);
        return element;
    }

    public static SVGPathElement LineTo(Coordinate p)
        => LineTo(p.X, p.Y);

    public static SVGPathElement LineTo(double x, double y)
    {
        SVGPathElement element = new(StrokeType.Line, false);
        element.points[0] = new Coordinate(x, y);
        return element;
    }

    public static SVGPathElement LineRel(double dx, double dy)
    {
        SVGPathElement element = new(StrokeType.Line, true);
        element.points[0] = new Coordinate(dx, dy);
        return element;
    }

    public static SVGPathElement Close() => new(StrokeType.Z, false);

    public static SVGPathElement Cubic
        (double cx1, double cy1, double cx2, double cy2, double x, double y)
    {
        SVGPathElement element = new(StrokeType.Cubic, false);
        element.points[0] = new Coordinate(cx1, cy1);
        element.points[1] = new Coordinate(cx2, cy2);
        element.points[2] = new Coordinate(x, y);
        return element;
    }

    public static SVGPathElement CubicRel
        (double dx1, double dy1, double dx2, double dy2, double dx, double dy)
    {
        SVGPathElement element = new(StrokeType.Cubic, true);
        element.points[0] = new Coordinate(dx1, dy1);
        element.points[1] = new Coordinate(dx2, dy2);
        element.points[2] = new Coordinate(dx, dy);
        return element;
    }

    public static SVGPathElement Quadratic(double cx1, double cy1, double x, double y)
    {
        SVGPathElement element = new(StrokeType.Cubic, false);
        element.points[0] = new Coordinate(cx1, cy1);
        element.points[1] = new Coordinate(x, y);
        return element;
    }

    public static SVGPathElement QuadraticRel(double dx1, double dy1, double dx, double dy)
    {
        SVGPathElement element = new(StrokeType.Cubic, true);
        element.points[0] = new Coordinate(dx1, dy1);
        element.points[1] = new Coordinate(dx, dy);
        return element;
    }

    public static SVGPathElement Arc
        (double rx, double ry, double angle, bool largeArc, bool sweep, double x, double y)
    {
        SVGPathElement element = new(StrokeType.Arc, false);
        element.points[0] = new Coordinate(rx, ry);
        element.points[1] = new Coordinate(angle, (largeArc ? 1.0f : 0.0f) + (sweep ? 2.0f : 0.0f)); // UGH!
        element.points[2] = new Coordinate(x, y);
        return element;
    }

    public static SVGPathElement ArcRel
        (double rx, double ry, double angle, bool largeArc, bool sweep, double dx, double dy)
    {
        SVGPathElement element = new(StrokeType.Arc, true);
        element.points[0] = new Coordinate(rx, ry);
        element.points[1] = new Coordinate(angle, (largeArc ? 1.0f : 0.0f) + (sweep ? 2.0f : 0.0f)); // UGH!
        element.points[2] = new Coordinate(dx, dy);
        return element;
    }

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


    private static string RenderPoint(Coordinate p) => $"{p.X:F2},{p.Y:F2}";

    private static string RenderPoints(IEnumerable<Coordinate> pe, int count)
        => string.Join(",", pe.Take(count).Select(p => RenderPoint(p)));
}
